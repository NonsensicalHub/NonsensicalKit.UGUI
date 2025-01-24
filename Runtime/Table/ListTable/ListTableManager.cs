using System.Collections.Generic;
using NonsensicalKit.Core.Log;
using UnityEngine;

namespace NonsensicalKit.UGUI.Table
{
    public interface IListTableManager<in T>
    {
        public void SetData(IEnumerable<T> data);
        public void Append(T element);
        public bool Delete(T element);
        public void Clear();
        public void Clean();
    }

    public abstract class ListTableManager<TListElement, TElementData> : NonsensicalUI, IListTableManager<TElementData>
        where TListElement : ListTableElement<TElementData>
        where TElementData : class
    {
        /// <summary>
        /// 用于承载tableElement的父物体，一般会挂载LayoutGroup组件
        /// </summary>
        [SerializeField] protected Transform m_group;

        /// <summary>
        /// 是否以group内忽视对象外的预设子物体作为预制体,为否时应当手动设置_prefab参数
        /// </summary>
        [Tooltip("是否使用group内忽视对象外的首个子物体作为表格元素预制体")] [SerializeField]
        protected bool m_childPrefab = true;

        [Tooltip("首个元素独自使用第一个预制体")] [SerializeField]
        protected bool m_differentFirst = false;

        /// <summary>
        /// 用于动态生成子物体的预制体，当_firstPrefab为true时会自动使用group的忽视对象外的子物体作为预制体
        /// 当有超过一个预制体时，会轮流使用每一个预制体
        /// </summary>
        [Tooltip("表格元素的预制体，当勾选_childPrefab时可为空")] [SerializeField]
        protected TListElement[] m_prefabs = new TListElement[1];

        /// <summary>
        /// 忽视头部子物体数量，当_childPrefab为true时获取子物体预制体时会也会忽略这些物体
        /// </summary>
        [SerializeField] protected int m_ignoreHead = 0;

        /// <summary>
        /// 忽视尾部子物体数量，尾部子物体会保持在最后
        /// </summary>
        [SerializeField] protected int m_ignoreTail = 0;

        /// <summary>
        /// 当前元素数据
        /// </summary>
        protected List<TElementData> ElementData = new List<TElementData>();

        /// <summary>
        /// 当前所有元素组件，包含未使用的
        /// </summary>
        protected List<TListElement> Elements = new List<TListElement>();

        /// <summary>
        /// 记录忽视的尾部对象，当新增元素时，由于会自动添加在末尾，需要在添加完成后将所有尾部对象移动到末尾
        /// </summary>
        protected List<Transform> Tail = new List<Transform>();

        protected bool InitFlag = true;

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        public void SetData(IEnumerable<TElementData> data)
        {
            UpdateUI(data);
        }

        public virtual void Append(TElementData appendElementData)
        {
            Init();
            ElementData.Add(appendElementData);

            TListElement crtElement = GetElement(ElementData.Count - 1);
            crtElement.gameObject.SetActive(true);
            crtElement.SetValue(this, ElementData.Count - 1, appendElementData);

            UpdateTail();
        }

        public virtual bool Delete(TElementData deleteElementData)
        {
            if (!ElementData.Contains(deleteElementData))
            {
                return false;
            }

            int index = ElementData.IndexOf(deleteElementData);

            return Delete(Elements[index]);
        }

        /// <summary>
        /// 清空数据，隐藏所有元素
        /// </summary>
        public void Clear()
        {
            ElementData.Clear();
            foreach (var item in Elements)
            {
                item.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 清空数据并销毁元素
        /// </summary>
        public void Clean()
        {
            foreach (var item in Elements)
            {
                Destroy(item.gameObject);
            }

            ElementData.Clear();
            Elements.Clear();
        }

        protected virtual void UpdateUI(IEnumerable<TElementData> data)
        {
            if (data == null)
            {
                ElementData = new List<TElementData>();
            }
            else
            {
                ElementData = new List<TElementData>(data);
            }

            UpdateUI();
        }

        protected virtual void UpdateUI(List<TElementData> data)
        {
            if (data == null)
            {
                ElementData = new List<TElementData>();
            }
            else
            {
                ElementData = data;
            }

            UpdateUI();
        }

        protected virtual void UpdateUI()
        {
            Init();
            int datasCount = ElementData.Count;

            //应用数据链表
            for (int i = 0; i < datasCount; i++)
            {
                TListElement crtElement = GetElement(i);
                crtElement.gameObject.SetActive(true);
                crtElement.SetValue(this, i, ElementData[i]);
            }

            //隐藏剩余未使用的子物体
            for (int i = datasCount; i < Elements.Count; i++)
            {
                Elements[i].gameObject.SetActive(false);
            }

            UpdateTail();
        }

        protected virtual bool Delete(TListElement deleteElement)
        {
            Init();
            if (!Elements.Contains(deleteElement))
            {
                return false;
            }

            if (m_prefabs.Length < 2)
            {
                //移至最后
                Elements.Remove(deleteElement);
                Elements.Add(deleteElement);
                deleteElement.transform.SetAsLastSibling();
                deleteElement.gameObject.SetActive(false);

                //移除数据
                ElementData.Remove(deleteElement.ElementData);

                UpdateTail();
            }
            else
            {
                ElementData.Remove(deleteElement.ElementData);

                //当预制体超过一个时，删除元素后其后每个元素使用的预制体都会改变

                int index = Elements.IndexOf(deleteElement);

                for (int i = index; i < ElementData.Count; i++)
                {
                    Elements[i].SetValue(this, i, ElementData[i]);
                }

                Elements[ElementData.Count].gameObject.SetActive(false);
            }

            return true;
        }

        protected virtual void InitNewElement(TListElement element)
        {
        }

        #region private method

        protected void Init()
        {
            if (InitFlag)
            {
                if (m_group == null)
                {
                    LogCore.Error("未设置Group", gameObject);
                    enabled = false;
                    return;
                }

                if (m_childPrefab)
                {
                    if (m_group.childCount < m_ignoreHead + m_prefabs.Length)
                    {
                        LogCore.Error("Group子节点数量不足", gameObject);
                        enabled = false;
                        return;
                    }

                    for (int i = 0; i < m_prefabs.Length; i++)
                    {
                        m_prefabs[i] = m_group.GetChild(m_ignoreHead + i).GetComponent<TListElement>();
                        if (m_prefabs[i] == null)
                        {
                            LogCore.Error("预制体未挂载组件", m_group.GetChild(m_ignoreHead + i));
                            enabled = false;
                            return;
                        }

                        m_prefabs[i].gameObject.SetActive(false);
                    }
                }

                foreach (var item in m_prefabs)
                {
                    if (item == null)
                    {
                        LogCore.Error("预制体配置为空", gameObject);
                        enabled = false;
                        return;
                    }
                }

                if (m_differentFirst)
                {
                    if (m_prefabs.Length < 2)
                    {
                        LogCore.Error("首行不同时至少需要两个预制体", gameObject);
                        enabled = false;
                        return;
                    }
                }

                if (m_ignoreTail > 0)
                {
                    for (int i = 0; i < m_ignoreTail; i++)
                    {
                        Tail.Add(m_group.GetChild(m_group.childCount - m_ignoreTail));
                    }
                }

                InitFlag = false;
            }
        }

        /// <summary>
        /// 返回对应索引的元素，如果当前元素已经用完，则实例化新元素返回
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected TListElement GetElement(int index)
        {
            if (index < Elements.Count)
            {
                return Elements[index];
            }
            else
            {
                var newElement = Instantiate(GetPrefab(index), m_group);
                InitNewElement(newElement);
                Elements.Add(newElement);
                return newElement;
            }
        }

        /// <summary>
        /// 更新尾部元素，将其移至最后
        /// </summary>
        protected void UpdateTail()
        {
            foreach (var item in Tail)
            {
                item.SetAsLastSibling();
            }
        }

        /// <summary>
        /// 获取符合当前索引的预制体
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected TListElement GetPrefab(int index)
        {
            if (m_differentFirst)
            {
                if (index == 0)
                {
                    return m_prefabs[0];
                }
                else
                {
                    return m_prefabs[((index - 1) % (m_prefabs.Length - 1)) + 1];
                }
            }
            else
            {
                return m_prefabs[index % m_prefabs.Length];
            }
        }

        #endregion
    }
}
