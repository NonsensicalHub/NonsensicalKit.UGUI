using NonsensicalKit.Core;
using TMPro;
using UnityEngine;

namespace NonsensicalKit.UGUI.Table
{
    public class ScrollTableCell : ScrollTableObject
    {
        [SerializeField] protected TextMeshProUGUI m_txt_content;

        public virtual void SetState(string text, int columnIndex, int rowIndex)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            Rect.SetRect(Table.GetCellRect( ColumnIndex,RowIndex));
            
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
