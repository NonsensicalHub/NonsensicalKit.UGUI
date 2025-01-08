using System;
using UnityEngine;

namespace NonsensicalKit.Core.Table
{
    /// <summary>
    /// ！注意！：如果只在开头UpdateData一次，则需要等待至少一帧使ScrollRect将ViewPort和Content的Rect配置好
    /// 参考： 
    /// https://github.com/aillieo/UnityDynamicScrollView
    /// https://blog.csdn.net/linxinfa/article/details/122019054
    /// 
    /// 分页优化版本
    /// 分页版本不支持滑动条
    /// </summary>
    public class ScrollViewEx : ScrollView
    {

        [SerializeField] private int m_pageSize = 50;

        private int _startOffset = 0;

        private bool _canNextPage = false;

        private Func<int> _realItemCountFunc;

        private bool _reloadFlag = false;

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnValueChanged);
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(0))
                _canNextPage = true;
        }

        public override void SetUpdateFunc(Action<int, RectTransform> func)
        {
            if (func != null)
            {
                var f = func;
                func = (index, rect) =>
                {
                    f(index + _startOffset, rect);
                };
            }
            base.SetUpdateFunc(func);
        }

        public override void SetItemSizeFunc(Func<int, Vector2> func)
        {
            if (func != null)
            {
                var f = func;
                func = (index) =>
                {
                    return f(index + _startOffset);
                };
            }
            base.SetItemSizeFunc(func);
        }

        public override void SetItemCountFunc(Func<int> func)
        {
            _realItemCountFunc = func;
            if (func != null)
            {
                var f = func;
                func = () => Mathf.Min(f(), m_pageSize);
            }
            base.SetItemCountFunc(func);
        }

        protected override void InternalScrollTo(int index,float pos)
        {
            int count = 0;
            if (_realItemCountFunc != null)
            {
                count = _realItemCountFunc();
            }
            index = Mathf.Clamp(index, 0, count - 1);
            _startOffset = Mathf.Clamp(index - m_pageSize / 2, 0, count - ItemCountFunc());
            UpdateData(true);

            base.InternalScrollTo(index - _startOffset,  pos);
        }

        private void OnValueChanged(Vector2 position)
        {
            if (_reloadFlag)
            {
                UpdateData(true);
                _reloadFlag = false;
            }
            if (Input.GetMouseButton(0) && !_canNextPage) return;
            int toShow;
            int critical;
            bool downward;
            int pin;
            if (((int)LayoutType & DIRECTION_FLAG) == 1)
            {
                // 垂直滚动 只计算y向
                if (velocity.y > 0)
                {
                    // 向上
                    toShow = _criticalItemIndex[CriticalItemType.LAST_HIDE];
                    critical = m_pageSize - 1;
                    if (toShow < critical)
                    {
                        return;
                    }
                    pin = critical - 1;
                    downward = false;
                }
                else
                {
                    // 向下
                    toShow = _criticalItemIndex[CriticalItemType.FIRST_HIDE];
                    critical = 0;
                    if (toShow > critical)
                    {
                        return;
                    }
                    pin = critical + 1;
                    downward = true;
                }
            }
            else // = 0
            {
                // 水平滚动 只计算x向
                if (velocity.x > 0)
                {
                    // 向右
                    toShow = _criticalItemIndex[CriticalItemType.FIRST_HIDE];
                    critical = 0;
                    if (toShow > critical)
                    {
                        return;
                    }
                    pin = critical + 1;
                    downward = true;
                }
                else
                {
                    // 向左
                    toShow = _criticalItemIndex[CriticalItemType.LAST_HIDE];
                    critical = m_pageSize - 1;
                    if (toShow < critical)
                    {
                        return;
                    }
                    pin = critical - 1;
                    downward = false;
                }
            }

            // 翻页
            int old = _startOffset;
            if (downward)
            {
                _startOffset -= m_pageSize / 2;
            }
            else
            {
                _startOffset += m_pageSize / 2;
            }
            _canNextPage = false;


            int realDataCount = 0;
            if (_realItemCountFunc != null)
            {
                realDataCount = _realItemCountFunc();
            }
            _startOffset = Mathf.Clamp(_startOffset, 0, Mathf.Max(realDataCount - m_pageSize, 0));

            if (old != _startOffset)
            {
                _reloadFlag = true;

                // 计算 pin元素的世界坐标
                Rect rect = GetItemLocalRect(pin);
                Vector2 oldWorld = content.TransformPoint(rect.position);
                UpdateData(true);
                int dataCount = 0;
                if (ItemCountFunc != null)
                {
                    dataCount = ItemCountFunc();
                }
                if (dataCount > 0)
                {
                    EnsureItemRect(0);
                    if (dataCount > 1)
                    {
                        EnsureItemRect(dataCount - 1);
                    }
                }

                // 根据 pin元素的世界坐标 计算出content的position
                int pin2 = pin + old - _startOffset;
                Rect rect2 = GetItemLocalRect(pin2);
                Vector2 newWorld = content.TransformPoint(rect2.position);
                Vector2 deltaWorld = newWorld - oldWorld;

                Vector2 deltaLocal = content.InverseTransformVector(deltaWorld);
                SetContentAnchoredPosition(content.anchoredPosition - deltaLocal);

                UpdateData(true);

                // 减速
                velocity /= 50f;
            }
        }
    }
}
