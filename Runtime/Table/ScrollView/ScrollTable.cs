using System;
using System.Collections.Generic;
using NonsensicalKit.Core;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Table
{
    public class ScrollTable : ScrollRect
    {
        [SerializeField] private List<float> m_columnWidth;
        [SerializeField] private List<float> m_rowHeight;
        [SerializeField] private ScrollTableCell m_cellPrefab;
        [SerializeField] private float m_defaultWidth = 50;
        [SerializeField] private float m_defaultHeight = 20;

        [SerializeField] protected Vector2 m_borderSize;
        [SerializeField] protected RectTransform m_borderLineRect;
        [SerializeField] protected RectOffset m_padding;

        private List<float> _cellX;
        private List<float> _cellY;
        private ComponentPool_MK3<ScrollTableCell> _pool;
        private Array2<string> _tableData;
        private Array2<ScrollTableCell> _cells;

        private Vector2Int _leftTopCell;
        private Vector2Int _rightBottomCell;

        private bool _initFlag;
        public RectTransform BorderLineRect { set => m_borderLineRect = value; }

        protected override void Awake()
        {
            base.Awake();
            _pool = new ComponentPool_MK3<ScrollTableCell>(m_cellPrefab, OnReset, OnInit, null, OnFirstInit);
        }

        public Array2<string> GetTableData()
        {
            return _tableData.CloneByJson();
        }

        public void SetTableData(Array2<string> tableData)
        {
            ClearCells();
            _tableData = tableData;
            m_columnWidth = new List<float>();
            m_columnWidth.Add(m_defaultWidth, tableData.Length0);
            m_rowHeight = new List<float>();
            m_rowHeight.Add(m_defaultHeight, tableData.Length1);
            ReSize();
        }

        public void AddRow(float height = 0, string cellText = "new cell")
        {
            _tableData = _tableData.CopyToNewArray(_tableData.Length0, _tableData.Length1 + 1);
            for (int i = 0; i < _tableData.Length0; i++)
            {
                _tableData[i, _tableData.Length1 - 1] = cellText;
            }

            if (height <= 0)
            {
                height = m_defaultHeight;
            }

            m_rowHeight.Add(height);
            ReSize();
        }

        public void AddColumn(float width = 0, string cellText = "new cell")
        {
            _tableData = _tableData.CopyToNewArray(_tableData.Length0 + 1, _tableData.Length1);
            for (int i = 0; i < _tableData.Length1; i++)
            {
                _tableData[_tableData.Length0 - 1, i] = cellText;
            }

            if (width <= 0)
            {
                width = m_defaultWidth;
            }

            m_columnWidth.Add(width);
            ReSize();
        }

        public void SetCellWidth(float width)
        {
            m_columnWidth.Fill(width);
            ReSize();
        }

        public void SetCellHeight(float height)
        {
            m_rowHeight.Fill(height);
            ReSize();
        }

        public Rect GetCellRect(int rowIndex, int columnIndex)
        {
            return new Rect(_cellX[columnIndex], -_cellY[rowIndex], m_columnWidth[columnIndex], m_rowHeight[rowIndex]);
        }

        protected override void SetNormalizedPosition(float value, int axis)
        {
            base.SetNormalizedPosition(value, axis);
            UpdateRect();
        }

        private void OnReset(ScrollTableCell cell)
        {
            cell.gameObject.SetActive(false);
        }

        private void OnInit(ScrollTableCell cell)
        {
            cell.gameObject.SetActive(true);
        }

        private void OnFirstInit(ComponentPool_MK3<ScrollTableCell> pool, ScrollTableCell cell)
        {
            cell.transform.SetParent(content.transform);
            cell.Init(this);
        }

        private void UpdateRect()
        {
            if (!_initFlag)
            {
                return;
            }

            var newLeftTopCell = GetLeftTopCell();
            int right = newLeftTopCell.x + 1;
            int bottom = newLeftTopCell.y + 1;
            while (ShouldCellDisplayInView(right, bottom))
            {
                right++;
            }

            right--;
            while (ShouldCellDisplayInView(right, bottom))
            {
                bottom++;
            }

            bottom--;
            var newRightBottomCell = new Vector2Int(right, bottom);

            if (newLeftTopCell != _leftTopCell || newRightBottomCell != _rightBottomCell)
            {
                HideCells();
                _leftTopCell = newLeftTopCell;
                _rightBottomCell = newRightBottomCell;
                _pool.Cache();
                ShowCells();
                _pool.Flush();
            }
        }

        /// <summary>
        /// 获取显示区域左上角的单元格索引
        /// </summary>
        /// <returns></returns>
        private Vector2Int GetLeftTopCell()
        {
            var x = -content.anchoredPosition.x;

            float maxX = _cellX[^1];
            int crtX = Math.Clamp((int)(x / maxX * _cellX.Count), 0, _cellX.Count - 1);

            bool flag;
            do
            {
                flag = false;
                if (_cellX[crtX] > x)
                {
                    if (crtX - 1 < 0)
                    {
                        break;
                    }

                    crtX--;
                    flag = true;
                }
                else
                {
                    if (_cellX.Count <= crtX + 1)
                    {
                        break;
                    }

                    if (_cellX[crtX + 1] < x)
                    {
                        crtX++;
                        flag = true;
                    }
                }
            }
            while (flag);

            float y = content.anchoredPosition.y;
            float maxY = _cellY[^1];
            int crtY = Math.Clamp((int)(y / maxY * _cellY.Count), 0, _cellY.Count - 1);


            int lastY;
            do
            {
                lastY = crtY;
                if (_cellY[crtY] > y)
                {
                    if (crtY - 1 < 0)
                    {
                        break;
                    }

                    crtY--;
                }
                else
                {
                    if (_cellY.Count <= crtY + 1)
                    {
                        break;
                    }

                    if (_cellY[crtY + 1] < y)
                    {
                        crtY++;
                    }
                }
            }
            while (lastY != crtY);

            return new Vector2Int(crtX, crtY);
        }


        private void HideCells()
        {
            if (_leftTopCell.x < 0)
            {
                return;
            }

            for (int x = _leftTopCell.x; x <= _rightBottomCell.x; x++)
            {
                for (int y = _leftTopCell.y; y <= _rightBottomCell.y; y++)
                {
                    _cells[x, y] = null;
                }
            }
        }

        private void ShowCells()
        {
            for (int x = _leftTopCell.x; x <= _rightBottomCell.x; x++)
            {
                for (int y = _leftTopCell.y; y <= _rightBottomCell.y; y++)
                {
                    _cells[x, y] = _pool.New();

                    _cells[x, y].SetState(_tableData[x, y], x, y);
                }
            }
        }

        private bool ShouldCellDisplayInView(int columnIndex, int rowIndex)
        {
            if (columnIndex < 0 || rowIndex < 0 || columnIndex >= _cellX.Count || rowIndex >= _cellY.Count)
            {
                return false;
            }

            var cellLeft = _cellX[columnIndex];
            var cellRight = _cellX[columnIndex] + m_columnWidth[columnIndex];
            var cellTop = _cellY[rowIndex];
            var cellBottom = _cellY[rowIndex] + m_rowHeight[rowIndex];
            var viewLeft = -content.anchoredPosition.x;
            var viewRight = -content.anchoredPosition.x + viewport.rect.width;
            var viewTop = content.anchoredPosition.y;
            var viewBottom = content.anchoredPosition.y + viewport.rect.height;

            if (cellRight > viewLeft
                && cellBottom > viewTop
                && cellLeft < viewRight
                && cellTop < viewBottom)
            {
                return true;
            }

            return false;
        }

        private void ClearCells()
        {
            _pool.Clear();
        }

        private void ReSize()
        {
            if (m_rowHeight.Count == 0 || m_columnWidth.Count == 0)
            {
                return;
            }

            _leftTopCell.x = -1;

            _cells = new Array2<ScrollTableCell>(m_columnWidth.Count, m_rowHeight.Count);
            _cellX = new List<float>();
            _cellY = new List<float>();
            _cellX.Add(m_padding.left + m_borderSize.x);
            for (int i = 0; i < m_columnWidth.Count - 1; i++)
            {
                _cellX.Add(_cellX[i] + m_columnWidth[i] + m_borderSize.x);
            }

            _cellY.Add(m_padding.top + m_borderSize.y);
            for (int i = 0; i < m_rowHeight.Count - 1; i++)
            {
                _cellY.Add(_cellY[i] + m_rowHeight[i] + m_borderSize.y);
            }

            _initFlag = true;

            content.sizeDelta = new Vector2(_cellX[^1] + m_columnWidth[^1] + m_padding.right + m_borderSize.x,
                _cellY[^1] + m_rowHeight[^1] + m_padding.bottom + m_borderSize.y);

            m_borderLineRect?.SetTopLeft(new Vector2(m_padding.left, -m_padding.top),
                content.sizeDelta - new Vector2(m_padding.left + m_padding.right, m_padding.top + m_padding.bottom));
            UpdateRect();
        }
    }
}
