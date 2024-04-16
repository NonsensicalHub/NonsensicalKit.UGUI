using NonsensicalKit.Tools.ObjectPool;
using NonsensicalKit.UGUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NonsensicalKit.Core.Table
{
    public interface TreeNodeTableManager<out NodeElement, ElementData>
        where NodeElement : TreeNodeTableElementBase<ElementData>
        where ElementData : class, ITreeNodeClass<ElementData>
    {
        public void DoFold(TreeNodeTableElementBase<ElementData> ne, bool isFold);
        public void Move(TreeNodeTableElementBase<ElementData> ne, TreeNodeTableElementBase<ElementData> newParent);
        public void MoveSameLevel(TreeNodeTableElementBase<ElementData> ne, TreeNodeTableElementBase<ElementData> newParent, bool isTop);
    }

    /// <summary>
    /// 默认使用Group的第一个子物体作为预制体
    /// 当勾选_specialTop时，使用第一个子物体作为顶节点预制体，第二个子物体作为其余节点预制体
    /// </summary>
    /// <typeparam name="NodeElement"></typeparam>
    /// <typeparam name="ElementData"></typeparam>
    public abstract class TreeNodeTableManagerBase<NodeElement, ElementData> : NonsensicalUI, TreeNodeTableManager<NodeElement, ElementData>
        where NodeElement : TreeNodeTableElementBase<ElementData>
        where ElementData : class, ITreeNodeClass<ElementData>
    {
        [SerializeField] protected Transform m_group;    //用于承载TableElement的父物体，一般会挂载LayoutGroup组件
        [SerializeField] protected Transform m_pool;     //用于缓存未使用对象的对象池

        [SerializeField] protected int _levelDistance = 25; //每一级子节点往右移动多少距离（单位：像素）
        [SerializeField] protected TreeNodeTableElementBase<ElementData>[] m_prefabs = new TreeNodeTableElementBase<ElementData>[1];        //每一级的子物体，如果使用_childPrefab则可以不赋值，但是需要选择数量，超过预制体数量的层级使用最后一个预制体
        [SerializeField] protected bool m_childPrefab = true;     //是否使用_group开头的子节点作为预制体
        [SerializeField] protected bool m_updateWidth;   //是否在没有改变节点时也不停的更新宽度
        [SerializeField] protected bool m_fixedWidth;    //是否固定宽度（会导致_updateWidth选项完全无效）

        protected GameObjectPool_MK2[] _pools;              //对象池组

        protected List<ElementData> _topNode;   //顶节点
        protected List<int> _levels;        //当前所有节点的层级，用于计算宽度

        protected RectTransform _groupRect; //_group的RectTransform
        protected RectTransform _maskRect;  //_group父节点的RectTransform

        protected override void Awake()
        {
            base.Awake();

            m_pool.gameObject.SetActive(false);
            _pools = new GameObjectPool_MK2[m_prefabs.Length];
            if (m_childPrefab)
            {
                for (int i = 0; i < m_prefabs.Length; i++)
                {
                    m_prefabs[i] = m_group.GetChild(i).GetComponent<TreeNodeTableElementBase<ElementData>>();
                    m_prefabs[i].gameObject.SetActive(false);
                    _pools[i] = new GameObjectPool_MK2(m_prefabs[i].gameObject, OnReset, OnInit, OnFirstInit);
                }
            }
            _topNode = new List<ElementData>();
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
            go.GetComponent<TreeNodeTableElementBase<ElementData>>().LevelDistance = _levelDistance;
            go.GetComponent<TreeNodeTableElementBase<ElementData>>().IManager = this;
            go.transform.SetParent(m_pool, false);
        }

        protected virtual void InitTable(IEnumerable<ElementData> topNodes)
        {
            Clear();
            foreach (var item in topNodes)
            {
                AddTopNode(item);
            }
        }

        protected virtual void AddTopNode(ElementData data)
        {
            InsertTopNode(data, _topNode.Count);
        }

        protected virtual void InsertTopNode(ElementData data, int index)
        {
            _topNode.Insert(index, data);
            data.Parent = null;
            data.Level = 0;

            GameObject crtTop = NewElement(0);
            NodeElement crtView = crtTop.GetComponent<NodeElement>();
            crtView.SetValue(data);
            if (data.IsFold == false)
            {
                Unfold(crtView, false);
            }
        }

        protected virtual void RemoveTopNode(NodeElement element)
        {
            Fold(element);

            _topNode.Remove(element.ElementData);
            StoreElement(0, element.gameObject);
        }

        protected virtual void AddNode(ElementData data, NodeElement parent, bool fold = false)
        {
            InsertNode(data, parent, parent.ElementData.Childs.Count, fold);
        }


        protected virtual void InsertNode(ElementData data, NodeElement parent, int childIndex, bool fold = false)
        {
            FocusNode(parent.ElementData, false);
            Unfold(parent);
            var parentElement = parent.ElementData;
            parentElement.Childs.Insert(childIndex, data);
            data.Level = parentElement.Level + 1;
            data.Parent = parentElement;
            data.UpdateInfo();

            int index = parent.transform.GetSiblingIndex();
            index += GetChildOffset(parent.ElementData, childIndex) + 1;
            GameObject crtTop = NewElement(data.Level);
            NodeElement crtView = crtTop.GetComponent<NodeElement>();
            crtView.SetValue(data);
            crtTop.transform.SetSiblingIndex(index);
            parent.UpdateFoldUI();
            if (!fold)
            {
                Unfold(crtView);
            }
        }

        protected virtual void BeforeFold(NodeElement node)
        {

        }

        protected virtual void BeforeUnfold(NodeElement node)
        {

        }

        protected virtual void Fold(NodeElement ne)
        {
            Fold(ne, true);
        }

        protected virtual void Fold(NodeElement ne, bool check = true)
        {
            if (check && ne.ElementData.IsFold == true)
            {
                return;
            }

            ne.ElementData.IsFold = true;
            ne.UpdateFoldUI();

            Stack<ElementData> elements = new Stack<ElementData>();

            elements.Push(ne.ElementData);

            while (elements.Count > 0)
            {
                ElementData crtED = elements.Pop();

                var childs = crtED.Childs;
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

        protected virtual void Unfold(NodeElement ne)
        {
            Unfold(ne, true);
        }

        protected virtual void Unfold(NodeElement ne, bool check = true)
        {
            if (check && ne.ElementData.IsFold == false)
            {
                return;
            }
            ne.ElementData.IsFold = false;
            ne.UpdateFoldUI();

            Stack<ElementData> elements = new Stack<ElementData>();

            elements.Push(ne.ElementData);

            while (elements.Count > 0)
            {
                ElementData crtED = elements.Pop();

                int setIndex = crtED.Belong.transform.GetSiblingIndex();
                foreach (var item in crtED.Childs)
                {
                    GameObject crtChild = NewElement(item.Level);
                    crtChild.transform.SetSiblingIndex(++setIndex);
                    NodeElement childNE = crtChild.GetComponent<NodeElement>();
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

        protected virtual NodeElement FocusNode(ElementData ed, bool focusAction = true)
        {
            ElementData crt = ed;

            Stack<ElementData> datas = new Stack<ElementData>();

            while (crt != null)
            {
                datas.Push(crt);
                crt = crt.Parent;
            }

            while (datas.Count > 1)
            {
                Unfold(datas.Pop().Belong.GetComponent<NodeElement>());
            }

            var ne = ed.Belong.GetComponent<NodeElement>();
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
        private int GetChildOffset(ElementData parent, int index)
        {
            if (index == -1)
            {
                index = parent.Childs.Count;
            }
            int offset = 0;
            for (int i = 0; i < index; i++)
            {
                if (parent.Childs[i].IsFold == false)
                {
                    offset += GetChildOffset(parent.Childs[i], parent.Childs[i].Childs.Count);
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
                _groupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _maskRect.rect.width + maxLevel * _levelDistance);
            }
        }


        public virtual void DoFold(TreeNodeTableElementBase<ElementData> ne, bool isFold)
        {
            if (isFold)
            {
                BeforeFold(ne as NodeElement);
                Fold(ne as NodeElement);
            }
            else
            {
                BeforeUnfold(ne as NodeElement);
                Unfold(ne as NodeElement);
            }
        }

        public virtual void Move(TreeNodeTableElementBase<ElementData> ne, TreeNodeTableElementBase<ElementData> newParent)
        {
            bool fold = ne.ElementData.IsFold;
            Fold(ne as NodeElement);
            var element = ne.ElementData;
            element.Parent.Childs.Remove(element);
            element.Parent.Belong.GetComponent<TreeNodeTableElementBase<ElementData>>().UpdateFoldUI();
            StoreElement(element.Level, ne.gameObject);

            AddNode(element, newParent as NodeElement, fold);
        }

        public virtual void MoveSameLevel(TreeNodeTableElementBase<ElementData> ne, TreeNodeTableElementBase<ElementData> targetElement, bool isTop)
        {
            bool fold = ne.ElementData.IsFold;
            Fold(ne as NodeElement);
            var element = ne.ElementData;
            element.Parent.Childs.Remove(element);
            element.Parent.Belong.GetComponent<TreeNodeTableElementBase<ElementData>>().UpdateFoldUI();
            StoreElement(element.Level, ne.gameObject);
            var parent = targetElement.ElementData.Parent;
            if (parent == null)
            {
                InsertTopNode(element, _topNode.IndexOf(targetElement.ElementData) + (isTop ? 0 : 1));
            }
            else
            {
                InsertNode(element, targetElement.ElementData.Parent.Belong.GetComponent<TreeNodeTableElementBase<ElementData>>() as NodeElement, targetElement.ElementData.Parent.Childs.IndexOf(targetElement.ElementData) + (isTop ? 0 : 1), fold);
            }
        }
    }
}
