using System;
using UnityEngine;

namespace NonsensicalKit.UGUI.Table
{
    public enum TableImageType
    {
        ColumnBackground,
        RowBackground,
        ColumnBoardLine,
        RowBoardLine,
    }

    public class ScrollTableImage : ScrollTableObject
    {
        [SerializeField] private TableImageType m_imageType;

        public TableImageType ImageType => m_imageType;

        public virtual void SetState(int columnIndex, int rowIndex)
        {
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;

            switch (m_imageType)
            {
                case TableImageType.ColumnBackground:
                    Rect.SetRect(Table.GetColumnRect(ColumnIndex)); break;
                case TableImageType.RowBackground:
                    Rect.SetRect(Table.GetRowRect(RowIndex)); break;
                case TableImageType.ColumnBoardLine: 
                    Rect.SetRect(Table.GetColumnBoardLineRect(ColumnIndex)); break;
                case TableImageType.RowBoardLine:
                    Rect.SetRect(Table.GetRowBoardLineRect(RowIndex)); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
