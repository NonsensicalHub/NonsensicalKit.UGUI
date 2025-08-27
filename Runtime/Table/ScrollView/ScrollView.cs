using System;
using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Tools.ObjectPool;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Table
{
    /// <summary>
    /// ！注意！：如果只在开头UpdateData一次，则需要等待至少一帧使ScrollRect将ViewPort和Content的Rect配置好
    /// 参考： 
    /// https://github.com/aillieo/UnityDynamicScrollView
    /// https://blog.csdn.net/linxinfa/article/details/122019054
    /// </summary>
    public class ScrollView : ScrollRect
    {
        /// <summary>
        /// item布局模式
        /// </summary>
        public enum ItemLayoutType
        {
            // 最后一位表示滚动方向
            Vertical = 1, // 0001
            Horizontal = 2, // 0010
            VerticalThenHorizontal = 4, // 0100
            HorizontalThenVertical = 5, // 0101
        }

        /// <summary>
        /// 关键items
        /// const int 代替 enum 减少 (int)和(CriticalItemType)转换
        /// </summary>
        protected static class CriticalItemType
        {
            public const int FIRST_SHOW = 0; //第一个显示的item
            public const int LAST_SHOW = 1; //最后一个显示的item
            public const int FIRST_HIDE = 2; //第一个显示的item的前一个item，即其最接近的不显示的item
            public const int LAST_HIDE = 3; //最后一个显示的item的后一个item，即其最接近的不显示的item
        }

        /// <summary>
        /// item的信息
        /// </summary>
        private class ScrollItemInfo
        {
            // scroll item 身上的 RectTransform组件，当为null时代表
            public RectTransform Item;

            // scroll item 在scrollview中的位置
            public Rect Rect;

            // rect 是否需要更新
            public bool IsDirty = true;
        }

        public const int DIRECTION_FLAG = 1; // 0001

        [SerializeField] [Tooltip("默认item尺寸")] protected Vector2 m_defaultItemSize;
        [SerializeField] [Tooltip("方向")] protected ItemLayoutType m_layoutType = ItemLayoutType.Vertical;

        [SerializeField] [Tooltip("是否使用默认对象池")]
        protected bool m_useDefaultPool = true;

        [SerializeField] [Tooltip("对象池大小")] protected int m_poolSize = 20;
        [SerializeField] [Tooltip("item模板")] protected RectTransform m_itemTemplate;

        public Action<int, RectTransform> UpdateFunc; //更新对应索引的item的Action
        public Func<int> ItemCountFunc; //item的数量获取的func,这个方法必须赋值
        public Func<int, Vector2> ItemSizeFunc; //获取对应索引的item的尺寸的Func
        public Func<int, RectTransform> ItemGetFunc; //分配对应索引item的Func
        public Action<int, RectTransform> ItemRecycleFunc; //回收对应索引item的Func

        public bool IsDragging { get; private set; }

        protected ItemLayoutType LayoutType { get { return m_layoutType; } }

        protected int[] _criticalItemIndex = new int[4]; // 只保存4个临界index

        private int _dataCount = 0; //缓存的当前数据数量，用于减少获取数量方法的调用次数
        private List<ScrollItemInfo> _managedItems = new List<ScrollItemInfo>(); //管理所有的信息
        private Rect _viewRectInContent; //viewPort一开始在content坐标系中的相对rect，用于计算item是否应该被显示
        private ComponentPoolMk2<RectTransform> _itemPool = null; //对象池

        private bool _initialized = false; //是否进行过初始化
        private int _willUpdateData = 0; //是否将要更新数据

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            ResetState();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_itemPool != null)
            {
                _itemPool.Clear();
            }
        }

        public void ResetState()
        {
            _initialized = false;
            content.pivot = Vector2.up; //(0,1),左上角
            content.sizeDelta = Vector2.zero;
            content.anchoredPosition = Vector2.zero;
            _managedItems = new List<ScrollItemInfo>();
            _itemPool?.Clear();
        }

        public virtual void SetTemplate(RectTransform itemTemplate)
        {
            m_itemTemplate = itemTemplate;
        }

        public virtual void SetUpdateFunc(Action<int, RectTransform> func)
        {
            UpdateFunc = func;
        }
        public virtual void SetItemCountFunc(Func<int> func)
        {
            ItemCountFunc = func;
        }

        public virtual void SetItemSizeFunc(Func<int, Vector2> func)
        {
            ItemSizeFunc = func;
        }

        public virtual void SetDefaultSize(Vector2 size)
        {
            m_defaultItemSize = size;
        }

        public void SetItemGetAndRecycleFunc(Func<int, RectTransform> getFunc, Action<int, RectTransform> recycleFunc)
        {
            if (getFunc != null && recycleFunc != null)
            {
                ItemGetFunc = getFunc;
                ItemRecycleFunc = recycleFunc;
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            IsDragging = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            IsDragging = false;
        }

        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            base.SetContentAnchoredPosition(position);
            UpdateCriticalItems();
        }

        protected override void SetNormalizedPosition(float value, int axis)
        {
            ResetCriticalItems();
            base.SetNormalizedPosition(value, axis);
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
                _willUpdateData |= 3; // 0011
                InternalUpdateData();
            }
            else
            {
                if (_willUpdateData == 0)
                {
                    StartCoroutine(DelayUpdateData());
                }

                _willUpdateData |= 3;
            }
        }

        /// <summary>
        /// 更新数据,且不会对已经计算过的item的rect重新计算
        /// 当确保item大小固定且不使用分页时使用这个方法
        /// </summary>
        /// <param name="immediately"></param>
        public void UpdateDataIncrementally(bool immediately = true)
        {
            if (!_initialized)
            {
                InitScrollView();
            }

            if (immediately)
            {
                _willUpdateData |= 1; // 0001
                InternalUpdateData();
            }
            else
            {
                if (_willUpdateData == 0)
                {
                    StartCoroutine(DelayUpdateData());
                }

                _willUpdateData |= 1;
            }
        }

        /// <summary>
        /// 滚动至目标item
        /// </summary>
        /// <param name="index"></param>
        public void ScrollTo(int index)
        {
            ScrollTo(index, 0.5f);
        }

        public void ScrollTo(int index, float pos)
        {
            pos = Mathf.Clamp01(pos);
            InternalScrollTo(index, pos);
        }

        /// <summary>
        /// 获取滚动到目标位置时的value
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public float GetScrollValue(int index, float pos)
        {
            index = Mathf.Clamp(index, 0, _dataCount - 1);
            EnsureItemRect(index);
            Rect r = _managedItems[index].Rect;
            int dir = (int)LayoutType & DIRECTION_FLAG;
            if (dir == 1)
            {
                var p = r.yMax + pos * (_viewRectInContent.height - r.height);
                // vertical
                float value = 1 + p / (content.sizeDelta.y - _viewRectInContent.height);
                value = Mathf.Clamp01(value);
                return value;
            }
            else
            {
                var p = r.xMin - pos * (_viewRectInContent.width - r.width);
                // horizontal
                float value = p / (content.sizeDelta.x - _viewRectInContent.width);
                value = Mathf.Clamp01(value);
                return value;
            }
        }

        /// <summary>
        /// 滚动至目标item具体实现
        /// </summary>
        /// <param name="index"></param>
        protected virtual void InternalScrollTo(int index, float pos)
        {
            int dir = (int)LayoutType & DIRECTION_FLAG;
            var value = GetScrollValue(index, pos);
            SetNormalizedPosition(value, dir);
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
            int keepValue = _willUpdateData & 2; //0010
            bool keepOldItems = keepValue == 0;

            if (newDataCount != _managedItems.Count)
            {
                if (_managedItems.Count < newDataCount) //增加
                {
                    if (!keepOldItems)
                    {
                        foreach (var itemWithRect in _managedItems)
                        {
                            // 重置所有rect
                            itemWithRect.IsDirty = true;
                        }
                    }

                    while (_managedItems.Count < newDataCount)
                    {
                        _managedItems.Add(new ScrollItemInfo());
                    }
                }
                else //减少 保留空位 避免GC
                {
                    for (int i = 0, count = _managedItems.Count; i < count; ++i)
                    {
                        if (i < newDataCount)
                        {
                            // 重置所有rect
                            if (!keepOldItems)
                            {
                                _managedItems[i].IsDirty = true;
                            }

                            if (i == newDataCount - 1)
                            {
                                _managedItems[i].IsDirty = true;
                            }
                        }

                        // 超出部分 清理回收item
                        if (i >= newDataCount)
                        {
                            _managedItems[i].IsDirty = true;
                            if (_managedItems[i].Item != null)
                            {
                                RecycleOldItem(i, _managedItems[i].Item);
                                _managedItems[i].Item = null;
                            }
                        }
                    }
                }
            }
            else
            {
                if (!keepOldItems)
                {
                    for (int i = 0, count = _managedItems.Count; i < count; ++i)
                    {
                        // 重置所有rect
                        _managedItems[i].IsDirty = true;
                    }
                }
            }

            _dataCount = newDataCount;

            ResetCriticalItems();
            
            //手动更新时需要刷新现在显示的对象
            for (int i = _criticalItemIndex[CriticalItemType.FIRST_SHOW], count = _criticalItemIndex[CriticalItemType.LAST_SHOW];
                 i <= count;
                 i++)
            {
                if (_managedItems[i].Item!=null)
                {
                    UpdateFunc(i, _managedItems[i].Item);
                }
            }

            _willUpdateData = 0;
        }

        /// <summary>
        /// 重新计算关键items
        /// </summary>
        private void ResetCriticalItems()
        {
            bool hasItem, shouldShow;
            int firstIndex = -1, lastIndex = -1;
            for (int i = 0; i < _dataCount; i++)
            {
                hasItem = _managedItems[i].Item != null;
                shouldShow = ShouldItemSeenAtIndex(i);
                if (shouldShow)
                {
                    if (firstIndex == -1)
                    {
                        firstIndex = i;
                    }

                    lastIndex = i;
                }

                if (hasItem && shouldShow)
                {
                    // 应显示且已显示
                    SetDataForItemAtIndex(_managedItems[i].Item, i);
                    continue;
                }

                if (hasItem == shouldShow)
                {
                    // 不应显示且未显示
                    //if (firstIndex != -1)
                    //{
                    //    // 已经遍历完所有要显示的了 后边的先跳过
                    //    break;
                    //}
                    continue;
                }

                if (hasItem && !shouldShow)
                {
                    // 不该显示 但是有
                    RecycleOldItem(i, _managedItems[i].Item);
                    _managedItems[i].Item = null;
                    continue;
                }

                if (shouldShow && !hasItem)
                {
                    // 需要显示 但是没有
                    RectTransform item = GetNewItem(i);
                    OnGetItemForDataIndex(item, i);
                    _managedItems[i].Item = item;
                    continue;
                }
            }

            // content.localPosition = Vector2.zero;
            _criticalItemIndex[CriticalItemType.FIRST_SHOW] = firstIndex;
            _criticalItemIndex[CriticalItemType.LAST_SHOW] = lastIndex;
            _criticalItemIndex[CriticalItemType.FIRST_HIDE] = Mathf.Max(firstIndex - 1, 0);
            _criticalItemIndex[CriticalItemType.LAST_HIDE] = Mathf.Min(lastIndex + 1, _dataCount - 1);
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
                return _managedItems[index].Item;
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
                    else //显示进入可见区域的item
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
                _managedItems[criticalIndex].Item = null;

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
                _managedItems[criticalIndex].Item = newItem;

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

            EnsureItemRect(index);
            return new Rect(_viewRectInContent.position - content.anchoredPosition, _viewRectInContent.size).Overlaps(_managedItems[index].Rect);
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void InitPool()
        {
            if (m_itemTemplate == null)
            {
                Debug.LogError("未配置模板");
                return;
            }

            GameObject poolNode = new GameObject("POOL");
            poolNode.SetActive(false);
            poolNode.transform.SetParent(transform, false);
            _itemPool = new ComponentPoolMk2<RectTransform>(
                m_itemTemplate,
                (RectTransform item) =>
                {
                    if (item != null)
                    {
                        item.transform.SetParent(poolNode.transform, false);
                    }
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
            EnsureItemRect(index);
            Rect r = _managedItems[index].Rect;
            item.localPosition = r.position;
            item.sizeDelta = r.size;
        }

        /// <summary>
        /// 获取对应索引的item大小，如果没有配置过设定大小func则使用默认大小字段
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Vector2 GetItemSize(int index)
        {
            if (index >= 0 && index <= _dataCount)
            {
                if (ItemSizeFunc != null)
                {
                    return ItemSizeFunc(index);
                }
            }

            return m_defaultItemSize;
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
            int dir = (int)LayoutType & DIRECTION_FLAG;
            vertical = (dir == 1);
            horizontal = (dir == 0);

            if (m_useDefaultPool)
            {
                InitPool();
            }

            UpdateViewRect();
        }

        /// <summary>
        /// 更新_viewRectInContent字段
        /// </summary>
        private void UpdateViewRect()
        {
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
            Vector3[] rectCorners = new Vector3[2];

            viewRect.GetWorldCorners(viewWorldConers);

            rectCorners[0] = content.transform.InverseTransformPoint(viewWorldConers[0]); //左下角
            rectCorners[1] = content.transform.InverseTransformPoint(viewWorldConers[2]); //右上角
            _viewRectInContent = new Rect((Vector2)rectCorners[0] - content.anchoredPosition, rectCorners[1] - rectCorners[0]);
        }

        /// <summary>
        /// 将pos根据方向移动size距离
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        private void MovePos(ref Vector2 pos, Vector2 size)
        {
            switch (LayoutType)
            {
                case ItemLayoutType.Vertical:
                    // 垂直方向 向下移动
                    pos.y -= size.y;
                    break;
                case ItemLayoutType.Horizontal:
                    // 水平方向 向右移动
                    pos.x += size.x;
                    break;
                case ItemLayoutType.VerticalThenHorizontal:
                    pos.y -= size.y;
                    if (pos.y <= -_viewRectInContent.height)
                    {
                        pos.y = 0;
                        pos.x += size.x;
                    }

                    break;
                case ItemLayoutType.HorizontalThenVertical:
                    pos.x += size.x;
                    if (pos.x >= _viewRectInContent.width)
                    {
                        pos.x = 0;
                        pos.y -= size.y;
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 确保对应索引的item的rect参数正确
        /// </summary>
        /// <param name="index"></param>
        protected void EnsureItemRect(int index)
        {
            if (!_managedItems[index].IsDirty)
            {
                // 已经是干净的了
                return;
            }

            //确保至少有一个干净的item
            ScrollItemInfo firstItem = _managedItems[0];
            if (firstItem.IsDirty)
            {
                Vector2 firstSize = GetItemSize(0);
                firstItem.Rect = CreateWithLeftTopAndSize(Vector2.zero, firstSize);
                firstItem.IsDirty = false;
            }

            // 当前item之前的最近的干净的rect
            int nearestClean = 0;
            for (int i = index; i >= 0; --i)
            {
                if (!_managedItems[i].IsDirty)
                {
                    nearestClean = i;
                    break;
                }
            }

            // 需要更新 从 nearestClean 到 index 的尺寸
            Rect nearestCleanRect = _managedItems[nearestClean].Rect;
            Vector2 curPos = GetLeftTopPosition(nearestCleanRect);
            Vector2 size = nearestCleanRect.size;
            MovePos(ref curPos, size);
            for (int i = nearestClean + 1; i <= index; i++)
            {
                size = GetItemSize(i);
                _managedItems[i].Rect = CreateWithLeftTopAndSize(curPos, size);
                _managedItems[i].IsDirty = false;
                MovePos(ref curPos, size);
            }

            //让需要变化的尺寸变为能攘括当前元素的尺寸
            Vector2 range = new Vector2(Mathf.Abs(curPos.x), Mathf.Abs(curPos.y));
            switch (LayoutType)
            {
                case ItemLayoutType.Vertical:
                    range.x = content.sizeDelta.x;
                    break;
                case ItemLayoutType.Horizontal:
                    range.y = content.sizeDelta.y;
                    break;
                case ItemLayoutType.VerticalThenHorizontal:
                    range.x += size.x;
                    range.y = _viewRectInContent.height;
                    break;
                case ItemLayoutType.HorizontalThenVertical:
                    range.x = _viewRectInContent.width;
                    if (curPos.x != 0)
                    {
                        range.y += size.y;
                    }

                    break;
                default:
                    break;
            }

            content.sizeDelta = range;
        }

        /// <summary>
        /// 获取Rect的左上角坐标（position为左下角）
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        private static Vector2 GetLeftTopPosition(Rect rect)
        {
            Vector2 ret = rect.position;
            ret.y += rect.size.y;
            return ret;
        }

        /// <summary>
        /// 获取对应索引的item的Rect
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected Rect GetItemLocalRect(int index)
        {
            if (index >= 0 && index < _dataCount)
            {
                EnsureItemRect(index);
                return _managedItems[index].Rect;
            }

            return new Rect();
        }

        /// <summary>
        /// 创建一个与左上角距离为leftTop且大小为size的Rect
        /// </summary>
        /// <param name="leftTop"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static Rect CreateWithLeftTopAndSize(Vector2 leftTop, Vector2 size)
        {
            Vector2 leftBottom = leftTop - new Vector2(0, size.y);
            return new Rect(leftBottom, size);
        }
    }
}
