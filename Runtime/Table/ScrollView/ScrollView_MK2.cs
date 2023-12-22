using NonsensicalKit.Tools.ObjectPool;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Core.Table
{
    /// <summary>
    /// ！注意！：如果只在开头一次，则需要等待至少一帧使ScrollRect将ViewPort和Content的Rect配置好
    /// 参考： 
    /// https://github.com/aillieo/UnityDynamicScrollView
    /// https://blog.csdn.net/linxinfa/article/details/122019054
    /// 
    /// 如果使用分页，则会导致滚动条无法正常使用
    /// 故使用固定大小来优化计算性能，可通过忽略开头第一个对象和结尾最后一个对象来自定义开头和结尾的尺寸非标准部分
    /// </summary>
    public class ScrollView_MK2 : ScrollRect
    {
        /// <summary>
        /// item布局模式
        /// </summary>
        public enum ItemLayoutType
        {
            // 最后一位表示滚动方向
            Vertical = 1,                   // 0001
            Horizontal = 2,                 // 0010
            HorizontalThenVertical = 3,     // 0011
            VerticalThenHorizontal = 4,     // 0100
        }

        /// <summary>
        /// 关键items
        /// const int 代替 enum 减少 (int)和(CriticalItemType)转换
        /// </summary>
        protected static class CriticalItemType
        {
            public const int FIRST_SHOW = 0;    //第一个显示的item
            public const int LAST_SHOW = 1;     //最后一个显示的item
            public const int FIRST_HIDE = 2;    //第一个显示的item的前一个item，即其最接近的不显示的item
            public const int LAST_HIDE = 3;     //最后一个显示的item的后一个item，即其最接近的不显示的item
        }


        public const int DIRECTION_FLAG = 1;  // 0001

        [SerializeField][Tooltip("默认item尺寸")] protected Vector2 m_itemSize;
        [SerializeField][Tooltip("方向")] protected ItemLayoutType m_layoutType = ItemLayoutType.Vertical;
        [SerializeField][Tooltip("忽略开头对象")] protected bool m_ignoreHead = false;
        [SerializeField][Tooltip("忽略结尾对象")] protected bool m_ignoretail = false;

        [SerializeField][Tooltip("是否使用默认对象池")] protected bool m_useDefaultPool = true;
        [SerializeField][Tooltip("对象池大小")] protected int m_poolSize = 20;
        [SerializeField][Tooltip("item模板")] protected RectTransform m_itemTemplate;


        protected Action<int, RectTransform> UpdateFunc;   //更新对应索引的item的Action
        protected Func<int> ItemCountFunc;                 //item的数量获取的func,这个方法必须赋值
        protected Func<int, RectTransform> ItemGetFunc;    //分配对应索引item的Func
        protected Action<int, RectTransform> ItemRecycleFunc;   //回收对应索引item的Func

        protected int[] _criticalItemIndex = new int[4];     // 只保存4个临界index

        private int _dataCount = 0; //缓存的当前数据数量，用于减少获取数量方法的调用次数
        private RectTransform[] _managedItems; //存储对应索引的item
        private Rect _viewRectInContent; //viewPort一开始在content坐标系中的相对rect，用于计算item是否应该被显示
        private ComponentPool_MK2<RectTransform> _itemPool = null;   //对象池

        private bool _initialized = false;   //是否进行过初始化
        private bool _willUpdateData = false; //是否将要更新数据

        private RectTransform _head;
        private float _headSize;
        private RectTransform _tail;
        private float _tailSize;

        private int _verticalCount;    //纵向能放几个item
        private int _horizontalCount;  //横向能放几个item

        protected override void OnDestroy()
        {
            if (_itemPool != null)
            {
                _itemPool.Clear();
            }
        }

        public virtual void SetUpdateFunc(Action<int, RectTransform> func)
        {
            UpdateFunc = func;
        }

        public virtual void SetItemCountFunc(Func<int> func)
        {
            ItemCountFunc = func;
        }

        public virtual void SetItemSize(Vector2 size)
        {
            m_itemSize = size;
        }

        public void SetItemGetAndRecycleFunc(Func<int, RectTransform> getFunc, Action<int, RectTransform> recycleFunc)
        {
            if (getFunc != null && recycleFunc != null)
            {
                ItemGetFunc = getFunc;
                ItemRecycleFunc = recycleFunc;
            }
        }

        /// <summary>
        /// 更新数据,强制更新所有item的rect
        /// </summary>
        /// <param name="immediately"></param>
        public void UpdateData(bool immediately = true)
        {
            if (!_initialized)
            {
                InitScrollView();
            }
            if (immediately)
            {
                _willUpdateData = true;
                InternalUpdateData();
            }
            else
            {
                if (!_willUpdateData)
                {
                    StartCoroutine(DelayUpdateData());
                }
                _willUpdateData = true;
            }
        }


        /// <summary>
        /// 滚动至目标item
        /// </summary>
        /// <param name="index"></param>
        public void ScrollTo(int index)
        {
            InternalScrollTo(index);
        }

        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            base.SetContentAnchoredPosition(position);
            UpdateCriticalItems();
        }

        protected override void SetNormalizedPosition(float value, int axis)
        {
            base.SetNormalizedPosition(value, axis);
            ResetCriticalItems();
        }

        /// <summary>
        /// 滚动至目标item具体实现
        /// </summary>
        /// <param name="index"></param>
        protected virtual void InternalScrollTo(int index)
        {
            index = Mathf.Clamp(index, 0, _dataCount - 1);
            Rect r = GetItemRectByIndex(index);
            int dir = (int)m_layoutType & DIRECTION_FLAG;
            if (dir == 1)
            {
                // vertical
                float value = 1 - (-r.yMax / (content.sizeDelta.y - _viewRectInContent.height));
                //value = Mathf.Clamp01(value);
                SetNormalizedPosition(value, 1);
            }
            else
            {
                // horizontal
                float value = r.xMin / (content.sizeDelta.x - _viewRectInContent.width);
                //value = Mathf.Clamp01(value);
                SetNormalizedPosition(value, 0);
            }
        }

        /// <summary>
        /// 等待一帧执行，当存在一帧多次调用的情况时，使用延时可将更新操作合并为一次
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayUpdateData()
        {
            yield return null;
            InternalUpdateData();
        }

        /// <summary>
        /// 更新数据具体实现
        /// </summary>
        private void InternalUpdateData()
        {
            int newDataCount = ItemCountFunc();

            if (_managedItems != null)
            {
                if (newDataCount != _managedItems.Length)
                {
                    if (_managedItems.Length < newDataCount) //增加
                    {
                        var temp = new RectTransform[newDataCount];
                        _managedItems.CopyTo(temp, 0);
                    }
                    else //减少 
                    {
                        for (int i = newDataCount, count = _managedItems.Length; i < count; ++i)
                        {
                            if (_managedItems[i] != null)
                            {
                                RecycleOldItem(i, _managedItems[i]);
                                _managedItems[i] = null;
                            }
                        }
                        var temp = new RectTransform[newDataCount];
                        _managedItems.CopyTo(temp, 0);
                    }
                }
            }
            else
            {
                _managedItems = new RectTransform[newDataCount];
            }
            _dataCount = newDataCount;

            if (((int)m_layoutType&DIRECTION_FLAG)==1)
            {
                _head?.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, _headSize);
                _tail?.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, _tailSize);
            }
            else
            {
                _head?.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, _headSize);
                _tail?.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, _tailSize);
            }
            switch (m_layoutType)
            {
                case ItemLayoutType.Vertical:
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _viewRectInContent.width);
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _headSize + _tailSize + newDataCount * m_itemSize.y);
                
                    break;
                case ItemLayoutType.Horizontal:
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _headSize + _tailSize + newDataCount * m_itemSize.x);
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _viewRectInContent.height); 
                    break;
                case ItemLayoutType.HorizontalThenVertical:
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _viewRectInContent.width);
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _headSize + _tailSize + ((int)((newDataCount - 1) / (float)_horizontalCount)+1) * m_itemSize.y);
                    break;
                case ItemLayoutType.VerticalThenHorizontal:
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _headSize + _tailSize + ((int)((newDataCount - 1) / (float)_verticalCount)+1) * m_itemSize.x);
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _viewRectInContent.height);
                    break;
                default:
                    break;
            }

            ResetCriticalItems();

            _willUpdateData = false;
        }
        /// <summary>
        /// 重新计算关键items
        /// </summary>
        private void ResetCriticalItems()
        {
            if (_managedItems == null || _managedItems.Length == 0)
            {
                return;
            }
            //清理之前显示的部分
            for (int i = _criticalItemIndex[CriticalItemType.FIRST_SHOW], count = _criticalItemIndex[CriticalItemType.LAST_SHOW]; i <= count; i++)
            {
                if (_managedItems[i] != null)
                {
                    RecycleOldItem(i, _managedItems[i]);
                    _managedItems[i] = null;
                }
            }

            //算出当前中心的index,之后向两侧依次判断是否能显示
            int midIndex;

            switch (m_layoutType)
            {
                case ItemLayoutType.Vertical:
                    midIndex = (int)(content.anchoredPosition.y / m_itemSize.y);
                    break;
                case ItemLayoutType.Horizontal:
                    midIndex = -(int)(content.anchoredPosition.x / m_itemSize.x);
                    break;
                case ItemLayoutType.HorizontalThenVertical:
                    midIndex = (int)(content.anchoredPosition.y / m_itemSize.y) * _horizontalCount;
                    break;
                case ItemLayoutType.VerticalThenHorizontal:
                    midIndex = -(int)(content.anchoredPosition.x / m_itemSize.x) * _verticalCount;
                    break;
                default:
                    midIndex = 0;
                    break;
            }
            midIndex = Mathf.Clamp(midIndex,0,_dataCount-1);
            //向前判断
            bool canSeen = true;
            int crtIndex = midIndex;
            while (canSeen)
            {
                if (canSeen = ShouldItemSeenAtIndex(crtIndex))
                {
                    RectTransform item = GetNewItem(crtIndex);
                    OnGetItemForDataIndex(item, crtIndex);
                    _managedItems[crtIndex] = item;
                }
                crtIndex--;
                if (crtIndex < 0)
                {
                    break;
                }
            }
            _criticalItemIndex[CriticalItemType.FIRST_HIDE] = Mathf.Max(crtIndex, 0);
            _criticalItemIndex[CriticalItemType.FIRST_SHOW] = crtIndex + 1;

            //向后判断
            canSeen = true;
            crtIndex = midIndex;
            while (canSeen)
            {
                if (canSeen = ShouldItemSeenAtIndex(crtIndex))
                {
                    if (_managedItems[crtIndex] == null)//防止midIndex位置item重复生成
                    {
                        RectTransform item = GetNewItem(crtIndex);
                        OnGetItemForDataIndex(item, crtIndex);
                        _managedItems[crtIndex] = item;
                    }
                }
                crtIndex++;
                if (crtIndex >= _dataCount)
                {
                    break;
                }
            }

            _criticalItemIndex[CriticalItemType.LAST_HIDE] = Mathf.Min(crtIndex, _dataCount - 1);
            _criticalItemIndex[CriticalItemType.LAST_SHOW] = crtIndex - 1;
        }

        /// <summary>
        /// 获取对应关键RectTransform
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private RectTransform GetCriticalItem(int type)
        {
            int index = _criticalItemIndex[type];
            if (index >= 0 && index < _dataCount)
            {
                return _managedItems[index];
            }
            return null;
        }

        /// <summary>
        /// 不断刷新关键item直到没有更新为止
        /// </summary>
        private void UpdateCriticalItems()
        {
            bool dirty = true;

            while (dirty)
            {
                dirty = false;

                for (int i = CriticalItemType.FIRST_SHOW; i <= CriticalItemType.LAST_HIDE; i++)
                {
                    if (i <= CriticalItemType.LAST_SHOW) //隐藏离开可见区域的item
                    {
                        dirty = dirty || CheckAndHideItem(i);
                    }
                    else  //显示进入可见区域的item
                    {
                        dirty = dirty || CheckAndShowItem(i);
                    }
                }
            }
        }

        /// <summary>
        /// 检测之前能被看到的关键item是否仍然能被看到
        /// 有变化则应用变化并返回true，否则返回false
        /// </summary>
        /// <param name="criticalItemType"></param>
        /// <returns></returns>
        private bool CheckAndHideItem(int criticalItemType)
        {
            RectTransform item = GetCriticalItem(criticalItemType);
            int criticalIndex = _criticalItemIndex[criticalItemType];
            if (item != null && !ShouldItemSeenAtIndex(criticalIndex))
            {
                RecycleOldItem(criticalIndex, item);
                _managedItems[criticalIndex] = null;

                if (criticalItemType == CriticalItemType.FIRST_SHOW)
                {
                    // 原本第一个被显示的对象下移
                    _criticalItemIndex[criticalItemType + 2] = Mathf.Max(criticalIndex, _criticalItemIndex[criticalItemType + 2]);
                    _criticalItemIndex[criticalItemType]++;
                }
                else
                {
                    // 原本最后一个被显示的对象上移
                    _criticalItemIndex[criticalItemType + 2] = Mathf.Min(criticalIndex, _criticalItemIndex[criticalItemType + 2]);
                    _criticalItemIndex[criticalItemType]--;
                }
                //确保改变过后仍在合理范围内
                _criticalItemIndex[criticalItemType] = Mathf.Clamp(_criticalItemIndex[criticalItemType], 0, _dataCount - 1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检测之前看不到但是现在能看到的关键item
        /// 有变化则应用变化并返回true，否则返回false
        /// </summary>
        /// <param name="criticalItemType"></param>
        /// <returns></returns>
        private bool CheckAndShowItem(int criticalItemType)
        {
            RectTransform item = GetCriticalItem(criticalItemType);
            int criticalIndex = _criticalItemIndex[criticalItemType];

            if (item == null && ShouldItemSeenAtIndex(criticalIndex))
            {
                RectTransform newItem = GetNewItem(criticalIndex);
                OnGetItemForDataIndex(newItem, criticalIndex);
                _managedItems[criticalIndex] = newItem;

                if (criticalItemType == CriticalItemType.FIRST_HIDE)
                {
                    _criticalItemIndex[criticalItemType - 2] = Mathf.Min(criticalIndex, _criticalItemIndex[criticalItemType - 2]);
                    _criticalItemIndex[criticalItemType]--;
                }
                else
                {
                    _criticalItemIndex[criticalItemType - 2] = Mathf.Max(criticalIndex, _criticalItemIndex[criticalItemType - 2]);
                    _criticalItemIndex[criticalItemType]++;
                }
                _criticalItemIndex[criticalItemType] = Mathf.Clamp(_criticalItemIndex[criticalItemType], 0, _dataCount - 1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 对应索引对象是否能被看到
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool ShouldItemSeenAtIndex(int index)
        {
            if (index < 0 || index >= _dataCount)
            {
                return false;
            }
            //Debug.Log("_____________________________________________________");
            //Debug.Log(index);
            //Debug.Log(new Rect(_viewRectInContent.position - content.anchoredPosition, _viewRectInContent.size));
            //Debug.Log(GetItemRectByIndex(index));
            //Debug.Log(new Rect(_viewRectInContent.position - content.anchoredPosition, _viewRectInContent.size).Overlaps(GetItemRectByIndex(index)));
            //Debug.Log("_____________________________________________________");
            return new Rect(_viewRectInContent.position - content.anchoredPosition, _viewRectInContent.size).Overlaps(GetItemRectByIndex(index));
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void InitPool()
        {
            if (m_itemTemplate == null)
            {
                Debug.LogError("未配置模板");
            }
            GameObject poolNode = new GameObject("POOL");
            poolNode.SetActive(false);
            poolNode.transform.SetParent(transform, false);
            _itemPool = new ComponentPool_MK2<RectTransform>(
              m_itemTemplate,
                (RectTransform item) =>
                {
                    item.transform.SetParent(poolNode.transform, false);
                },
               (rect) =>
               {
                   rect.transform.SetParent(poolNode.transform, false);

                   rect.anchorMin = Vector2.up;
                   rect.anchorMax = Vector2.up;
                   rect.pivot = Vector2.zero;

                   rect.gameObject.SetActive(true);
               });
        }

        /// <summary>
        /// 调用更新方法后应用Rect,最后将item从对象池父物体放入content中
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        private void OnGetItemForDataIndex(RectTransform item, int index)
        {
            SetDataForItemAtIndex(item, index);
            item.transform.SetParent(content, false);
        }

        /// <summary>
        /// 调用更新方法后应用Rect
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        private void SetDataForItemAtIndex(RectTransform item, int index)
        {
            if (UpdateFunc != null)
                UpdateFunc(index, item);

            SetPosForItemAtIndex(item, index);
        }

        /// <summary>
        /// 将对应索引的Rect值应用到RectTransform组件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        private void SetPosForItemAtIndex(RectTransform item, int index)
        {
            Rect r = GetItemRectByIndex(index);
            item.localPosition = r.position;
            item.sizeDelta = r.size;
        }

        /// <summary>
        /// 生成新对象
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private RectTransform GetNewItem(int index)
        {
            RectTransform item;
            if (ItemGetFunc != null)
            {
                item = ItemGetFunc(index);
            }
            else
            {
                item = _itemPool.New();
            }
            return item;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="item"></param>
        private void RecycleOldItem(int index, RectTransform item)
        {
            if (ItemRecycleFunc != null)
            {
                ItemRecycleFunc(index, item);
            }
            else
            {
                _itemPool.Store(item);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitScrollView()
        {
            _initialized = true;

            // 根据设置来控制原ScrollRect的滚动方向
            int dir = (int)m_layoutType & DIRECTION_FLAG;
            vertical = (dir == 1);
            horizontal = (dir == 0);

            if (m_useDefaultPool)
            {
                InitPool();
            }
            if (content.childCount > 0)
            {
                if (m_ignoreHead)
                {
                    _head = content.GetChild(0).GetComponent<RectTransform>();
                    if (_head != null)
                    {
                        _headSize = dir == 1 ? _head.rect.height : _head.rect.width;
                    }
                }
                if (m_ignoretail)
                {
                    _tail = content.GetChild(content.childCount - 1).GetComponent<RectTransform>();
                    if (_tail != null)
                    {
                        _tailSize = dir == 1 ? _tail.rect.height : _tail.rect.width;
                    }

                    
                }
            }
            InitViewRect();
            switch (m_layoutType)
            {
                case ItemLayoutType.Vertical:
                    _horizontalCount = 1;
                    break;
                case ItemLayoutType.Horizontal:
                    _verticalCount = 1;
                    break;
                case ItemLayoutType.HorizontalThenVertical:
                    _horizontalCount = (int)(_viewRectInContent.width / m_itemSize.x);
                    break;
                case ItemLayoutType.VerticalThenHorizontal:
                    _verticalCount = (int)(_viewRectInContent.height / m_itemSize.y);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// 更新_viewRectInContent字段
        /// </summary>
        private void InitViewRect()
        {
            content.pivot = Vector2.up; //(0,1),左上角

            /*
             *  WorldCorners
             * 
             *    1 ------- 2     
             *    |         |
             *    |         |
             *    0 ------- 3
             * 
             */
            Vector3[] viewWorldConers = new Vector3[4];

            viewRect.GetWorldCorners(viewWorldConers);

            Vector3[] rectCorners = new Vector3[2];

            rectCorners[0] = content.transform.InverseTransformPoint(viewWorldConers[0]);   //左下角
            rectCorners[1] = content.transform.InverseTransformPoint(viewWorldConers[2]);   //右上角

            _viewRectInContent = new Rect((Vector2)rectCorners[0] - content.anchoredPosition, rectCorners[1] - rectCorners[0]);
        }

        private Rect GetItemRectByIndex(int index)
        {
            Vector2 pos = Vector2.zero;
            switch (m_layoutType)
            {
                case ItemLayoutType.Vertical:
                    pos.y -= _headSize + (index + 1) * m_itemSize.y;
                    break;
                case ItemLayoutType.Horizontal:
                    pos.x += _headSize + index * m_itemSize.x;
                    pos.y -= 1 * m_itemSize.y;
                    break;
                case ItemLayoutType.VerticalThenHorizontal:
                    pos.x += _headSize + (int)(index / _verticalCount) * m_itemSize.x;
                    pos.y -= ((int)(index % _verticalCount) + 1) * m_itemSize.y;
                    break;
                case ItemLayoutType.HorizontalThenVertical:
                    pos.x += (int)(index % _horizontalCount) * m_itemSize.x;
                    pos.y -= _headSize + ((int)(index / _horizontalCount) + 1) * m_itemSize.y;
                    break;
                default:
                    break;
            }
            return new Rect(pos, m_itemSize);
        }
    }
}
