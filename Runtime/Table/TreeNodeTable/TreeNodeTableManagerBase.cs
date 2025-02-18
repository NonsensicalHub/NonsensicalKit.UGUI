using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Tools.ObjectPool;
using NonsensicalKit.UGUI;
using UnityEngine;
using UnityEngine.Serialization;

namespace NonsensicalKit.UGUI.Table
{
    public interface ITreeNodeTableManager<TElementData> where TElementData : class, ITreeNodeClass<TElementData>
    {
        public void DoFold(TreeNodeTableElementBase<TElementData> ne, bool isFold);
        public void Move(TreeNodeTableElementBase<TElementData> ne, TreeNodeTableElementBase<TElementData> newParent);
        public void MoveSameLevel(TreeNodeTableElementBase<TElementData> ne, TreeNodeTableElementBase<TElementData> newParent, bool isTop);
    }

    /// <summary>
    /// 默认使用Group的第一个子物体作为预制体
    /// 当勾选_specialTop时，使用第一个子物体作为顶节点预制体，第二个子物体作为其余节点预制体
    /// </summary>
    /// <typeparam name="TNodeElement"></typeparam>
    /// <typeparam name="TElementData"></typeparam>
    public abstract class TreeNodeTableManagerBase<TNodeElement, TElementData> : NonsensicalUI, ITreeNodeTableManager<TElementData>
        where TNodeElement : TreeNodeTableElementBase<TElementData>
        where TElementData : class, ITreeNodeClass<TElementData>
    {
        [SerializeField] protected Transform m_group; //用于承载TableElement的父物体，一般会挂载LayoutGroup组件
        [SerializeField] protected Transform m_pool; //用于缓存未使用对象的对象池

        [FormerlySerializedAs("_levelDistance")] [SerializeField]
        protected int m_levelDistance = 25; //每一级子节点往右移动多少距离（单位：像素）

        [SerializeField]
        protected TreeNodeTableElementBase<TElementData>[]
            m_prefabs = new TreeNodeTableElementBase<TElementData>[1]; //每一级的子物体，如果使用_childPrefab则可以不赋值，但是需要选择数量，超过预制体数量的层级使用最后一个预制体

        [SerializeField] protected bool m_childPrefab = true; //是否使用_group开头的子节点作为预制体
        [SerializeField] protected bool m_updateWidth; //是否在没有改变节点时也不停的更新宽度
        [SerializeField] protected bool m_fixedWidth; //是否固定宽度（会导致_updateWidth选项完全无效）

        protected GameObjectPool_MK2[] _pools; //对象池组

        protected List<TElementData> _topNode; //顶节点
        protected List<int> _levels; //当前所有节点的层级，用于计算宽度

        protected RectTransform _groupRect; //_group的RectTransform
        protected RectTransform _maskRect; //_group父节点的RectTransform

        protected override void Awake()
        {
            base.Awake();

            m_pool.gameObject.SetActive(false);
            _pools = new GameObjectPool_MK2[m_prefabs.Length];
            if (m_childPrefab)
            {
                for (int i = 0; i < m_prefabs.Length; i++)
                {
                    m_prefabs[i] = m_group.GetChild(i).GetComponent<TreeNodeTableElementBase<TElementData>>();
                    m_prefabs[i].gameObject.SetActive(false);
                    _pools[i] = new GameObjectPool_MK2(m_prefabs[i].gameObject, OnReset, OnInit, OnFirstInit);
                }
            }

            _topNode = new List<TElementData>();
            _levels = new List<int>() { 0 };
            _groupRect = m_group.GetComponent<RectTransform>();
            _maskRect = m_group.parent.GetComponent<RectTransform>();

            if (m_updateWidth)
            {
                StartCoroutine(UpdateWidthCoroutine());
            }
        }

        private IEnumerator UpdateWidthCoroutine()
        {
            while (true)
            {
                UpdateWidth();
                yield return null;
            }
        }

        protected virtual void OnReset(GameObject go)
        {
            go.SetActive(false);
            go.transform.SetParent(m_pool);
        }

        protected virtual void OnInit(GameObject go)
        {
            go.SetActive(true);
            go.transform.SetParent(m_group);
        }

        protected virtual void OnFirstInit(GameObjectPool_MK2 pool, GameObject go)
        {
            go.GetComponent<TreeNodeTableElementBase<TElementData>>().LevelDistance = m_levelDistance;
            go.GetComponent<TreeNodeTableElementBase<TElementData>>().Manager = this;
            go.transform.SetParent(m_pool, false);
        }

        protected virtual void InitTable(IEnumerable<TElementData> topNodes)
        {
            Clear();
            foreach (var item in topNodes)
            {
                AddTopNode(item);
            }
        }

        protected virtual void AddTopNode(TElementData data)
        {
            InsertTopNode(data, _topNode.Count);
        }

        protected virtual void InsertTopNode(TElementData data, int index)
        {
            _topNode.Insert(index, data);
            data.Parent = null;
            data.Level = 0;

            GameObject crtTop = NewElement(0);
            TNodeElement crtView = crtTop.GetComponent<TNodeElement>();
            crtView.SetValue(data);
            if (data.IsFold == false)
            {
                Unfold(crtView, false);
            }
        }

        protected virtual void RemoveTopNode(TNodeElement element)
        {
            Fold(element);

            _topNode.Remove(element.ElementData);
            StoreElement(0, element.gameObject);
        }

        protected virtual void AddNode(TElementData data, TNodeElement parent, bool fold = false)
        {
            InsertNode(data, parent, parent.ElementData.Children.Count, fold);
        }


        protected virtual void InsertNode(TElementData data, TNodeElement parent, int childIndex, bool fold = false)
        {
            FocusNode(parent.ElementData, false);
            Unfold(parent);
            var parentElement = parent.ElementData;
            parentElement.Children.Insert(childIndex, data);
            data.Level = parentElement.Level + 1;
            data.Parent = parentElement;
            data.UpdateInfo();

            int index = parent.transform.GetSiblingIndex();
            index += GetChildOffset(parent.ElementData, childIndex) + 1;
            GameObject crtTop = NewElement(data.Level);
            TNodeElement crtView = crtTop.GetComponent<TNodeElement>();
            crtView.SetValue(data);
            crtTop.transform.SetSiblingIndex(index);
            parent.UpdateFoldUI();
            if (!fold)
            {
                Unfold(crtView);
            }
        }

        protected virtual void BeforeFold(TNodeElement node)
        {
        }

        protected virtual void BeforeUnfold(TNodeElement node)
        {
        }

        protected virtual void Fold(TNodeElement ne)
        {
            Fold(ne, true);
        }

        protected virtual void Fold(TNodeElement ne, bool check)
        {
            if (check && ne.ElementData.IsFold == true)
            {
                return;
            }

            ne.ElementData.IsFold = true;
            ne.UpdateFoldUI();

            Stack<TElementData> elements = new Stack<TElementData>();

            elements.Push(ne.ElementData);

            while (elements.Count > 0)
            {
                TElementData crtED = elements.Pop();

                var childs = crtED.Children;
                foreach (var item in childs)
                {
                    _levels.Remove(item.Level);
                    StoreElement(item.Level, item.Belong);
                    if (item.IsFold == false)
                    {
                        elements.Push(item);
                    }
                }
            }

            UpdateWidth();
        }

        protected virtual void Unfold(TNodeElement ne)
        {
            Unfold(ne, true);
        }

        protected virtual void Unfold(TNodeElement ne, bool check)
        {
            if (check && ne.ElementData.IsFold == false)
            {
                return;
            }

            ne.ElementData.IsFold = false;
            ne.UpdateFoldUI();

            Stack<TElementData> elements = new Stack<TElementData>();

            elements.Push(ne.ElementData);

            while (elements.Count > 0)
            {
                TElementData crtED = elements.Pop();

                int setIndex = crtED.Belong.transform.GetSiblingIndex();
                foreach (var item in crtED.Children)
                {
                    GameObject crtChild = NewElement(item.Level);
                    crtChild.transform.SetSiblingIndex(++setIndex);
                    TNodeElement childNE = crtChild.GetComponent<TNodeElement>();
                    childNE.SetValue(item);
                    _levels.Add(item.Level);
                    if (item.IsFold == false)
                    {
                        elements.Push(item);
                    }
                }
            }

            UpdateWidth();
        }

        protected virtual TNodeElement FocusNode(TElementData ed, bool focusAction = true)
        {
            TElementData crt = ed;

            Stack<TElementData> datas = new Stack<TElementData>();

            while (crt != null)
            {
                datas.Push(crt);
                crt = crt.Parent;
            }

            while (datas.Count > 1)
            {
                Unfold(datas.Pop().Belong.GetComponent<TNodeElement>());
            }

            var ne = ed.Belong.GetComponent<TNodeElement>();
            if (focusAction)
            {
                ne.OnFocus();
            }

            return ne;
        }

        private GameObject NewElement(int level)
        {
            level = Mathf.Clamp(level, 0, m_prefabs.Length - 1);
            return _pools[level].New();
        }

        private void StoreElement(int level, GameObject go)
        {
            level = Mathf.Clamp(level, 0, m_prefabs.Length - 1);
            _pools[level].Store(go);
        }

        private int GetChildOffset(TElementData parent, int index)
        {
            if (index == -1)
            {
                index = parent.Children.Count;
            }

            int offset = 0;
            for (int i = 0; i < index; i++)
            {
                if (parent.Children[i].IsFold == false)
                {
                    offset += GetChildOffset(parent.Children[i], parent.Children[i].Children.Count);
                }

                offset++;
            }

            return offset;
        }


        protected virtual void Clear()
        {
            foreach (var pool in _pools)
            {
                pool.Clear();
            }

            _topNode.Clear();
            _levels.Clear();
            _levels.Add(0);
        }

        protected void UpdateWidth()
        {
            if (!m_fixedWidth)
            {
                int maxLevel = _levels.Max();
                _groupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _maskRect.rect.width + maxLevel * m_levelDistance);
            }
        }


        public virtual void DoFold(TreeNodeTableElementBase<TElementData> ne, bool isFold)
        {
            if (isFold)
            {
                BeforeFold(ne as TNodeElement);
                Fold(ne as TNodeElement);
            }
            else
            {
                BeforeUnfold(ne as TNodeElement);
                Unfold(ne as TNodeElement);
            }
        }

        public virtual void Move(TreeNodeTableElementBase<TElementData> ne, TreeNodeTableElementBase<TElementData> newParent)
        {
            bool fold = ne.ElementData.IsFold;
            Fold(ne as TNodeElement);
            var element = ne.ElementData;
            element.Parent.Children.Remove(element);
            element.Parent.Belong.GetComponent<TreeNodeTableElementBase<TElementData>>().UpdateFoldUI();
            StoreElement(element.Level, ne.gameObject);

            AddNode(element, newParent as TNodeElement, fold);
        }

        public virtual void Move(TreeNodeTableElementBase<TElementData> ne, TreeNodeTableElementBase<TElementData> newParent, int index)
        {
            bool fold = ne.ElementData.IsFold;
            Fold(ne as TNodeElement);
            var element = ne.ElementData;
            element.Parent.Children.Remove(element);
            element.Parent.Belong.GetComponent<TreeNodeTableElementBase<TElementData>>().UpdateFoldUI();
            StoreElement(element.Level, ne.gameObject);

            if (index < 0)
            {
                AddNode(element, newParent as TNodeElement, fold);
            }
            else
            {
                InsertNode(element, newParent as TNodeElement, index, fold);
            }
        }


        public virtual void MoveSameLevel(TreeNodeTableElementBase<TElementData> ne, TreeNodeTableElementBase<TElementData> targetElement, bool isTop)
        {
            bool fold = ne.ElementData.IsFold;
            Fold(ne as TNodeElement);
            var element = ne.ElementData;
            element.Parent.Children.Remove(element);
            element.Parent.Belong.GetComponent<TreeNodeTableElementBase<TElementData>>().UpdateFoldUI();
            StoreElement(element.Level, ne.gameObject);
            var parent = targetElement.ElementData.Parent;
            if (parent == null)
            {
                InsertTopNode(element, _topNode.IndexOf(targetElement.ElementData) + (isTop ? 0 : 1));
            }
            else
            {
                InsertNode(element, targetElement.ElementData.Parent.Belong.GetComponent<TreeNodeTableElementBase<TElementData>>() as TNodeElement,
                    targetElement.ElementData.Parent.Children.IndexOf(targetElement.ElementData) + (isTop ? 0 : 1), fold);
            }
        }
    }
}
