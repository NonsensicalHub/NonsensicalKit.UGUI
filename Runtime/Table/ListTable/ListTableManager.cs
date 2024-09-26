using NonsensicalKit.Core.Log;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.UGUI.Table
{
    public abstract class ListTableManager<ListElement, ElementData> : NonsensicalUI
            where ListElement : ListTableElement<ElementData>
            where ElementData : class
    {
        /// <summary>
        /// 用于承载tableElement的父物体，一般会挂载LayoutGroup组件
        /// </summary>
        [SerializeField] protected Transform m_group;

        /// <summary>
        /// 是否以group内忽视对象外的预设子物体作为预制体,为否时应当手动设置_prefab参数
        /// </summary>
        [Tooltip("是否使用group内忽视对象外的首个子物体作为表格元素预制体")][SerializeField] protected bool m_childPrefab = true;
        [Tooltip("首行不同独自使用第一个预制体")][SerializeField] protected bool m_differentFirst = false;

        /// <summary>
        /// 用于动态生成子物体的预制体，当_firstPrefab为true时会自动使用group的忽视对象外的子物体作为预制体
        /// 当有超过一个预制体时，会轮流使用每一个预制体
        /// </summary>
        [Tooltip("表格元素的预制体，当勾选_childPrefab时可为空")][SerializeField] protected ListElement[] m_prefabs = new ListElement[1];

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
        protected List<ElementData> _elementDatas = new List<ElementData>();

        /// <summary>
        /// 当前所有元素组件，包含未使用的
        /// </summary>
        protected List<ListElement> _elements = new List<ListElement>();

        /// <summary>
        /// 记录忽视的尾部对象，当新增元素时，由于会自动添加在末尾，需要在添加完成后将所有尾部对象移动到末尾
        /// </summary>
        protected List<Transform> _tail = new List<Transform>();

        protected bool _initFlag = true;

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        public void SetDatas(IEnumerable<ElementData> datas)
        {
            UpdateUI(datas);
        }

        public void Clear()
        {
            _elementDatas.Clear();
            foreach (var item in _elements)
            {
                item.gameObject.SetActive(false);
            }
        }

        public void Clean()
        {
            foreach (var item in _elements)
            {
                Destroy(item.gameObject);
            }
            _elementDatas.Clear();
            _elements.Clear();
        }

        protected virtual void UpdateUI(IEnumerable<ElementData> datas)
        {
            if (datas == null)
            {
                _elementDatas = new List<ElementData>();
            }
            else
            {
                _elementDatas = new List<ElementData>(datas);
            }
            UpdateUI();
        }

        protected virtual void UpdateUI(List<ElementData> datas)
        {
            if (datas == null)
            {
                _elementDatas = new List<ElementData>();
            }
            else
            {
                _elementDatas = datas;
            }

            UpdateUI();
        }

        protected virtual void UpdateUI()
        {
            Init();
            int datasCount = _elementDatas.Count;

            //应用数据链表
            for (int i = 0; i < datasCount; i++)
            {
                ListElement crtElement = GetElement(i);
                crtElement.gameObject.SetActive(true);
                crtElement.SetValue(_elementDatas[i]);
            }

            //隐藏剩余未使用的子物体
            for (int i = datasCount; i < _elements.Count; i++)
            {
                _elements[i].gameObject.SetActive(false);
            }

            UpdateTail();
        }

        protected virtual void Append(ElementData appendElementData)
        {
            Init();
            _elementDatas.Add(appendElementData);

            ListElement crtElement = GetElement(_elementDatas.Count - 1);
            crtElement.gameObject.SetActive(true);
            crtElement.SetValue(appendElementData);

            UpdateTail();
        }

        protected virtual void Delete(ListElement deleteElement)
        {
            Init();
            if (!_elements.Contains(deleteElement))
            {
                return;
            }

            if (m_prefabs.Length < 2)
            {
                //移至最后
                _elements.Remove(deleteElement);
                _elements.Add(deleteElement);
                deleteElement.transform.SetAsLastSibling();
                deleteElement.gameObject.SetActive(false);

                //移除数据
                _elementDatas.Remove(deleteElement.ElementData);

                UpdateTail();
            }
            else
            {
                _elementDatas.Remove(deleteElement.ElementData);

                //当预制体超过一个时，删除元素后其后每个元素使用的预制体都会改变

                int index = _elements.IndexOf(deleteElement);

                for (int i = index; i < _elementDatas.Count; i++)
                {
                    _elements[i].SetValue(_elementDatas[i]);
                }
                _elements[_elementDatas.Count].gameObject.SetActive(false);
            }
        }

        protected virtual void Delete(ElementData deleteElementData)
        {
            if (!_elementDatas.Contains(deleteElementData))
            {
                return;
            }

            int index = _elementDatas.IndexOf(deleteElementData);

            Delete(_elements[index]);
        }

        protected virtual void InitNewElement(ListElement element)
        {

        }

        private void Init()
        {
            if (_initFlag)
            {
                if (m_group==null)
                {
                    LogCore.Error("未设置Group", gameObject);
                    enabled = false;
                    return;
                }

                if (m_childPrefab)
                {
                    for (int i = 0; i < m_prefabs.Length; i++)
                    {
                        m_prefabs[i] = m_group.GetChild(m_ignoreHead + i).GetComponent<ListElement>();
                        m_prefabs[i].gameObject.SetActive(false);
                    }
                }

                if (m_ignoreTail > 0)
                {
                    for (int i = 0; i < m_ignoreTail; i++)
                    {
                        _tail.Add(m_group.GetChild(m_group.childCount - m_ignoreTail));
                    }
                }

                foreach (var item in m_prefabs)
                {
                    if (item==null)
                    {
                        LogCore.Error("预制体为空或挂载组件有误", gameObject);
                        enabled = false;
                        return;
                    }
                }

                if (m_differentFirst)
                {
                    if(m_prefabs.Length<2)
                    {
                        LogCore.Error("首行不同时至少需要两个预制体", gameObject);
                        enabled = false;
                        return;
                    }
                }
                _initFlag = false;
            }
        }

        /// <summary>
        /// 返回对应索引的元素，如果当前元素已经用完，则实例化新元素返回
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ListElement GetElement(int index)
        {
            if (index < _elements.Count)
            {
                return _elements[index];
            }
            else
            {
                var newElement = Instantiate(GetPrefab(index), m_group);
                InitNewElement(newElement);
                _elements.Add(newElement);
                return newElement;
            }
        }

        /// <summary>
        /// 更新尾部元素，将其移至最后
        /// </summary>
        private void UpdateTail()
        {
            foreach (var item in _tail)
            {
                item.SetAsLastSibling();
            }
        }

        /// <summary>
        /// 获取符合当前索引的预制体
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ListElement GetPrefab(int index)
        {
            if (m_differentFirst)
            {
                if ( index == 0)
                {
                    return m_prefabs[0];
                }
                else
                {
                    return m_prefabs[((index-1) %( m_prefabs.Length-1))+1];
                }
            }
            else
            {
                return m_prefabs[index % m_prefabs.Length];
            }
        }
    }
}
