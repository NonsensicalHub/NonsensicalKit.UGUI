using System;
using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Core;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Table
{
    public class PoolSetting<T> where T : ScrollTableObject
    {
        private readonly Vector3 _farPos = new Vector3(10000, 10000, 0);

        public ComponentPool_MK3<T> Pool;
        public Action<T> OverrideReset;
        public Action<T> OverrideInit;
        public Action<T> OverrideReinit;
        public Action<T> OverrideFirstInit;

        private Transform _parent;
        private ScrollTable _table;

        public PoolSetting(ScrollTable table, T prefab, Transform parent)
        {
            _parent = parent;
            _table = table;
            Pool = new ComponentPool_MK3<T>(prefab, OnReset, OnInit, OnReinit, OnFirstInit);

            if (prefab != null && parent != null && prefab.transform.parent == parent)
            {
                prefab.gameObject.SetActive(false);
            }
        }


        public void OnReset(T obj)
        {
            if (OverrideReset != null)
            {
                OverrideReset(obj);
            }
            else
            {
                obj.transform.position = _farPos;
            }
        }

        public void OnInit(T obj)
        {
            if (OverrideInit != null)
            {
                OverrideInit(obj);
            }
        }

        public void OnReinit(T obj)
        {
            if (OverrideReinit != null)
            {
                OverrideReinit(obj);
            }
        }

        public void OnFirstInit(ComponentPool_MK3<T> pool, T obj)
        {
            obj.transform.SetParent(_parent, false);
            obj.gameObject.SetActive(true);
            obj.Init(_table);

            if (OverrideFirstInit != null)
            {
                OverrideFirstInit(obj);
            }
        }
    }

    public class ScrollTable : ScrollRect
    {
        [SerializeField] private List<float> m_columnWidth;
        [SerializeField] private List<float> m_rowHeight;
        [SerializeField] private ScrollTableCell m_cellPrefab;
        [SerializeField] private ScrollTableImage[] m_columnImagePrefabs;
        [SerializeField] private ScrollTableImage[] m_rowImagePrefabs;

        [SerializeField] protected Transform m_cellParent;
        [SerializeField] protected Transform m_rowParent;
        [SerializeField] protected Transform m_columnParent;

        [SerializeField] private float m_defaultWidth = 50;
        [SerializeField] private float m_defaultHeight = 20;

        [SerializeField] protected Vector2 m_borderSize;
        [SerializeField] protected RectTransform m_borderLineRect;
        [SerializeField] protected RectOffset m_padding;

        public RectTransform BorderLineRect { set => m_borderLineRect = value; }
        public Transform CellParent { set => m_cellParent = value; }
        public Transform RowParent { set => m_rowParent = value; }
        public Transform ColumnParent { set => m_columnParent = value; }
        public Vector2 BorderSize { set => m_borderSize = value; }

        public PoolSetting<ScrollTableCell> CellPoolSetting;
        public List<PoolSetting<ScrollTableImage>> ColumnImagePoolSetting;
        public List<PoolSetting<ScrollTableImage>> RowImagePoolSetting;

        private float[] _cellX;
        private float[] _cellY;
        private Array2<string> _tableData;
        private Array2<ScrollTableCell> _cells;
        private Array2<ScrollTableImage> _columns;
        private Array2<ScrollTableImage> _rows;

        private Vector2Int _leftTopCell;
        private Vector2Int _rightBottomCell;

        private bool _tableInitFlag;
        private bool _poolInitFlag;
        private bool _resizeFlag;

        protected override void Awake()
        {
            base.Awake();
            InitPool();
        }

        /// <summary>
        /// 设置某个单元格
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        /// <param name="text"></param>
        public void SetCellData(int columnIndex, int rowIndex, string text)
        {
            _tableData[columnIndex, rowIndex] = text;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public Array2<string> GetTableData()
        {
            return _tableData.CopyToNewArray(_tableData.Length0, _tableData.Length1);
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="tableData"></param>
        public void SetTableData(Array2<string> tableData)
        {
            ClearTable();
            _tableData = tableData;
            m_columnWidth = new List<float>();
            m_columnWidth.Add(m_defaultWidth, tableData.Length0);
            m_rowHeight = new List<float>();
            m_rowHeight.Add(m_defaultHeight, tableData.Length1);
            ReSize();
        }

        /// <summary>
        /// 添加新行
        /// </summary>
        /// <param name="height"></param>
        /// <param name="cellText"></param>
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

        /// <summary>
        /// 添加新列
        /// </summary>
        /// <param name="width"></param>
        /// <param name="cellText"></param>
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

        /// <summary>
        /// 设置所有行高
        /// </summary>
        /// <param name="heights"></param>
        public void SetRowHeights(List<float> heights)
        {
            m_rowHeight = heights;
            ReSize();
        }

        /// <summary>
        /// 设置某一行高
        /// </summary>
        /// <param name="index"></param>
        /// <param name="height"></param>
        public void SetRowHeight(int index, float height)
        {
            if (m_rowHeight.Count > index)
            {
                m_rowHeight[index] = height;
                ReSize();
            }
        }

        /// <summary>
        /// 设置所有行等高
        /// </summary>
        /// <param name="height"></param>
        public void SetSameHeight(float height)
        {
            m_rowHeight.Fill(height);
            ReSize();
        }

        /// <summary>
        /// 设置所有列宽
        /// </summary>
        /// <param name="widths"></param>
        public void SetColumnWidths(List<float> widths)
        {
            m_columnWidth = widths;
            ReSize();
        }

        /// <summary>
        /// 设置某一列宽
        /// </summary>
        /// <param name="index"></param>
        /// <param name="width"></param>
        public void SetColumnWidth(int index, float width)
        {
            if (m_columnWidth.Count > index)
            {
                m_columnWidth[index] = width;
                ReSize();
            }
        }

        /// <summary>
        /// 设置所有列等宽
        /// </summary>
        /// <param name="width"></param>
        public void SetSameWidth(float width)
        {
            m_columnWidth.Fill(width);
            ReSize();
        }

        /// <summary>
        /// 清空表格
        /// </summary>
        public void ClearTable()
        {
            InitPool();
            CellPoolSetting.Pool.Clear();
            foreach (var columnPool in ColumnImagePoolSetting)
            {
                columnPool.Pool.Clear();
            }

            foreach (var rowPool in RowImagePoolSetting)
            {
                rowPool.Pool.Clear();
            }
        }

        /// <summary>
        /// 获取某一单元格rect
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public Rect GetCellRect(int columnIndex, int rowIndex)
        {
            return new Rect(_cellX[columnIndex], -_cellY[rowIndex], m_columnWidth[columnIndex], m_rowHeight[rowIndex]);
        }

        /// <summary>
        /// 获取某一列rect
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public Rect GetColumnRect(int columnIndex)
        {
            if (columnIndex >= m_columnWidth.Count)
            {
                return Rect.zero;
            }

            return new Rect(_cellX[columnIndex], -(m_padding.top + m_borderSize.y), m_columnWidth[columnIndex],
                content.sizeDelta.y - m_borderSize.y * 2 - m_padding.top - m_padding.bottom);
        }

        public Rect GetColumnBoardLineRect(int columnIndex)
        {
            return new Rect(_cellX[columnIndex] - m_borderSize.x, -m_padding.top, m_borderSize.x,
                content.sizeDelta.y - m_padding.top - m_padding.bottom);
        }

        /// <summary>
        /// 获取某一行rect
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public Rect GetRowRect(int rowIndex)
        {
            if (rowIndex >= m_rowHeight.Count)
            {
                return Rect.zero;
            }

            return new Rect(m_padding.left + m_borderSize.x, -_cellY[rowIndex],
                content.sizeDelta.x - m_borderSize.x * 2 - m_padding.left - m_padding.right, m_rowHeight[rowIndex]);
        }

        public Rect GetRowBoardLineRect(int rowIndex)
        {
            return new Rect(m_padding.left, -(_cellY[rowIndex] - m_borderSize.y),
                content.sizeDelta.x - m_padding.left - m_padding.right, m_borderSize.y);
        }

        /// <summary>
        /// 继承方法，在Content被ScrollRect组件修改位置时会被调用
        /// </summary>
        /// <param name="value"></param>
        /// <param name="axis"></param>
        protected override void SetNormalizedPosition(float value, int axis)
        {
            base.SetNormalizedPosition(value, axis);
            UpdateTable();
        }

        private void InitPool()
        {
            if (_poolInitFlag)
            {
                return;
            }

            _poolInitFlag = true;
            CellPoolSetting = new PoolSetting<ScrollTableCell>(this, m_cellPrefab, m_cellParent);

            ColumnImagePoolSetting = new List<PoolSetting<ScrollTableImage>>();
            if (m_columnImagePrefabs != null)
            {
                foreach (var columnPrefab in m_columnImagePrefabs)
                {
                    ColumnImagePoolSetting.Add(new PoolSetting<ScrollTableImage>(this, columnPrefab, m_columnParent));
                }
            }

            if (m_rowImagePrefabs != null)
            {
                RowImagePoolSetting = new List<PoolSetting<ScrollTableImage>>();
                foreach (var rowPrefab in m_rowImagePrefabs)
                {
                    RowImagePoolSetting.Add(new PoolSetting<ScrollTableImage>(this, rowPrefab, m_rowParent));
                }
            }
        }

        private void ReSize()
        {
            if (!_resizeFlag)
            {
                _resizeFlag = true;
                NonsensicalInstance.Instance.StartCoroutine(WaitResize());
            }
        }

        private IEnumerator WaitResize()
        {
            //将计算操作放入帧尾，防止ScrollRect控制的ViewPort未初始化的问题
            yield return new WaitForEndOfFrame();
            _resizeFlag = false;
            DoResize();
        }

        /// <summary>
        /// 根据当前配置初始化所有数据
        /// </summary>
        private void DoResize()
        {
            if (m_rowHeight.Count == 0 || m_columnWidth.Count == 0)
            {
                return;
            }

            if (_tableInitFlag == false)
            {
                _tableInitFlag = true;
            }
            else
            {
                UpdateContent(_leftTopCell, _rightBottomCell, Vector2Int.one, Vector2Int.zero); //清空
                _leftTopCell = Vector2Int.zero;
                _rightBottomCell = Vector2Int.zero;
            }

            _cells = new Array2<ScrollTableCell>(m_columnWidth.Count, m_rowHeight.Count);

            _columns = new Array2<ScrollTableImage>(m_columnImagePrefabs.Length, m_columnWidth.Count + 1); //框线需要多算一个
            _rows = new Array2<ScrollTableImage>(m_rowImagePrefabs.Length, m_rowHeight.Count + 1);

            _cellX = new float[m_columnWidth.Count + 1];
            _cellY = new float[m_rowHeight.Count + 1];

            _cellX[0] = m_padding.left + m_borderSize.x;
            for (int i = 0; i < m_columnWidth.Count; i++)
            {
                _cellX[i + 1] = _cellX[i] + m_columnWidth[i] + m_borderSize.x;
            }

            _cellY[0] = m_padding.top + m_borderSize.y;
            for (int i = 0; i < m_rowHeight.Count; i++)
            {
                _cellY[i + 1] = _cellY[i] + m_rowHeight[i] + m_borderSize.y;
            }

            content.sizeDelta = new Vector2(_cellX[^1] + m_padding.right, _cellY[^1] + m_padding.bottom);

            m_borderLineRect?.SetTopLeft(new Vector2(m_padding.left, -m_padding.top),
                content.sizeDelta - new Vector2(m_padding.left + m_padding.right, m_padding.top + m_padding.bottom));

            UpdateTable();
        }

        /// <summary>
        /// 更新表格
        /// </summary>
        private void UpdateTable()
        {
            if (!_tableInitFlag)
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
                UpdateContent(_leftTopCell, _rightBottomCell, newLeftTopCell, newRightBottomCell);
                _leftTopCell = newLeftTopCell;
                _rightBottomCell = newRightBottomCell;
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
            int crtX = Math.Clamp((int)(x / maxX * _cellX.Length), 0, _cellX.Length - 1);

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
                    if (m_columnWidth.Count <= crtX + 1)
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
            int crtY = Math.Clamp((int)(y / maxY * _cellY.Length), 0, _cellY.Length - 1);

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
                    if (m_rowHeight.Count <= crtY + 1)
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

        /// <summary>
        /// 隐藏看不见的内容并显示新看见的内容
        /// </summary>
        /// <param name="oldTopLeft"></param>
        /// <param name="oldRightBottom"></param>
        /// <param name="newTopLeft"></param>
        /// <param name="newRightBottom"></param>
        private void UpdateContent(Vector2Int oldTopLeft, Vector2Int oldRightBottom, Vector2Int newTopLeft, Vector2Int newRightBottom)
        {
            for (int x = oldTopLeft.x; x <= oldRightBottom.x; x++)
            {
                for (int y = oldTopLeft.y; y <= oldRightBottom.y; y++)
                {
                    if (_cells[x, y] != null)
                    {
                        if (x >= newTopLeft.x && x <= newRightBottom.x && y >= newTopLeft.y && y <= newRightBottom.y)
                        {
                            continue;
                        }

                        CellPoolSetting.Pool.Cache(_cells[x, y]);
                        _cells[x, y] = null;
                    }
                }
            }

            for (int x = newTopLeft.x; x <= newRightBottom.x; x++)
            {
                for (int y = newTopLeft.y; y <= newRightBottom.y; y++)
                {
                    if (_cells[x, y] == null)
                    {
                        _cells[x, y] = CellPoolSetting.Pool.New();
                    }

                    _cells[x, y].SetState(_tableData[x, y], x, y);
                }
            }

            CellPoolSetting.Pool.Flush();

            for (int i = 0; i < ColumnImagePoolSetting.Count; i++)
            {
                for (int x = oldTopLeft.x; x <= oldRightBottom.x + 1; x++)
                {
                    if (_columns[i, x] != null)
                    {
                        if (x >= newTopLeft.x && x <= newRightBottom.x + 1)
                        {
                            continue;
                        }

                        ColumnImagePoolSetting[i].Pool.Cache(_columns[i, x]);
                        _columns[i, x] = null;
                    }
                }

                for (int x = newTopLeft.x; x <= newRightBottom.x + 1; x++)
                {
                    if (_columns[i, x] == null)
                    {
                        _columns[i, x] = ColumnImagePoolSetting[i].Pool.New();
                    }

                    _columns[i, x].SetState(x, 0);
                }

                ColumnImagePoolSetting[i].Pool.Flush();
            }

            for (int i = 0; i < RowImagePoolSetting.Count; i++)
            {
                for (int y = oldTopLeft.y; y <= oldRightBottom.y + 1; y++)
                {
                    if (_rows[i, y] != null)
                    {
                        if (y >= newTopLeft.y && y <= newRightBottom.y + 1)
                        {
                            continue;
                        }

                        RowImagePoolSetting[i].Pool.Cache(_rows[i, y]);
                        _rows[i, y] = null;
                    }
                }

                for (int y = newTopLeft.y; y <= newRightBottom.y + 1; y++)
                {
                    if (_rows[i, y] == null)
                    {
                        _rows[i, y] = RowImagePoolSetting[i].Pool.New();
                    }

                    _rows[i, y].SetState(0, y);
                }

                RowImagePoolSetting[i].Pool.Flush();
            }
        }

        /// <summary>
        /// 判断单元格是否应当显示
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        private bool ShouldCellDisplayInView(int columnIndex, int rowIndex)
        {
            if (columnIndex < 0 || rowIndex < 0 || columnIndex >= m_columnWidth.Count || rowIndex >= m_rowHeight.Count)
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
    }
}
