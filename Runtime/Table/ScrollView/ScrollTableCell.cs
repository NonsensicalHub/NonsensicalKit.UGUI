using TMPro;
using UnityEngine;

namespace NonsensicalKit.UGUI.Table
{
    public class ScrollTableCell : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI m_txt_content;

        protected RectTransform Rect
        {
            get
            {
                if (_rectTransform == null)
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

        public virtual void SetState(string text, int columnIndex, int rowIndex)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            Rect.SetRect(Table.GetCellRect(RowIndex, ColumnIndex));
            
            SetText(text);
        }

        protected virtual void SetText(string text)
        {
            if (m_txt_content is not null)
            {
                m_txt_content.text = text;
            }
        }
    }
}
