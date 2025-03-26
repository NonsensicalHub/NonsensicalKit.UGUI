using NonsensicalKit.Core;
using TMPro;
using UnityEngine;

namespace NonsensicalKit.UGUI.Table
{
    public class ScrollTableObject : NonsensicalMono
    {
        protected RectTransform Rect
        {
            get
            {
                if (_rectTransform is null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                    _rectTransform.SetTopLeft();
                }

                return _rectTransform;
            }
        }

        private RectTransform _rectTransform;

        protected ScrollTable Table;
        protected int RowIndex;
        protected int ColumnIndex;

        public void Init(ScrollTable table)
        {
            Table = table;
        }
    }
}
