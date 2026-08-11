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

        public ComponentPoolMk3<T> Pool;
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
            Pool = new ComponentPoolMk3<T>(prefab, OnReset, OnInit, OnReinit, OnFirstInit);

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

        public void OnFirstInit(ComponentPool<T> pool, T obj)
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

    /// <summary>
    /// 表格行主序
    /// </summary>
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
        public Vector2 BorderSize { set => m_borderSize = value; get => m_borderSize;}

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
            return _tableData.CopyToNewArray(_tableData.m_Length0, _tableData.m_Length1);
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="tableData"></param>
        /// <param name="columnWidth"></param>
        /// <param name="rowHeight"></param>
        public void SetTableData(Array2<string> tableData, List<float> columnWidth = null, List<float> rowHeight = null)
        {
            ClearTable();
            _tableData = tableData;
            m_columnWidth = columnWidth ?? new List<float> { { m_defaultWidth, tableData.m_Length0 } };
            m_rowHeight = rowHeight ?? new List<float> { { m_defaultHeight, tableData.m_Length1 } };
            EnsureConsistentSizes();
            ReSize();
        }

        /// <summary>
        /// 设置默认单元格大小
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetDefaultSize(float width, float height)
        {
            m_defaultWidth = width;
            m_defaultHeight = height;
        }

        /// <summary>
        /// 添加新行
        /// </summary>
        /// <param name="height"></param>
        /// <param name="cellText"></param>
        public void AddRow(float height = 0, string cellText = "new cell")
        {
            _tableData = _tableData.CopyToNewArray(_tableData.m_Length0, _tableData.m_Length1 + 1);
            for (int i = 0; i < _tableData.m_Length0; i++)
            {
                _tableData[i, _tableData.m_Length1 - 1] = cellText;
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
            _tableData = _tableData.CopyToNewArray(_tableData.m_Length0 + 1, _tableData.m_Length1);
            for (int i = 0; i < _tableData.m_Length1; i++)
            {
                _tableData[_tableData.m_Length0 - 1, i] = cellText;
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
            m_rowHeight = heights ?? new List<float>();
            EnsureConsistentSizes();
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
            m_columnWidth = widths ?? new List<float>();
            EnsureConsistentSizes();
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

            _cells.Reset();
            _columns.Reset();
            _rows.Reset();
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
        /// 拖拽/惯性滚动时 ScrollRect 会走此路径，需同步刷新可见单元格
        /// </summary>
        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            base.SetContentAnchoredPosition(position);
            UpdateTable();
        }

        /// <summary>
        /// 通过滚动条或设置 normalizedPosition 时调用
        /// </summary>
        /// <param name="value"></param>
        /// <param name="axis"></param>
        protected override void SetNormalizedPosition(float value, int axis)
        {
            base.SetNormalizedPosition(value, axis);
            UpdateTable();
        }

        /// <summary>
        /// 保证 _tableData 与列宽/行高列表尺寸一致，避免 UpdateContent 越界
        /// </summary>
        private void EnsureConsistentSizes()
        {
            m_columnWidth ??= new List<float>();
            m_rowHeight ??= new List<float>();

            bool hasData = _tableData.m_Array != null;
            if (hasData)
            {
                AlignListCount(m_columnWidth, _tableData.m_Length0, m_defaultWidth);
                AlignListCount(m_rowHeight, _tableData.m_Length1, m_defaultHeight);
            }
            else if (m_columnWidth.Count > 0 && m_rowHeight.Count > 0)
            {
                _tableData = new Array2<string>(m_columnWidth.Count, m_rowHeight.Count);
            }
        }

        private static void AlignListCount(List<float> list, int count, float fillValue)
        {
            while (list.Count < count)
            {
                list.Add(fillValue);
            }

            while (list.Count > count)
            {
                list.RemoveAt(list.Count - 1);
            }
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

            RowImagePoolSetting = new List<PoolSetting<ScrollTableImage>>();
            if (m_rowImagePrefabs != null)
            {
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
            EnsureConsistentSizes();

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

            int columnImageCount = m_columnImagePrefabs?.Length ?? 0;
            int rowImageCount = m_rowImagePrefabs?.Length ?? 0;
            _columns = new Array2<ScrollTableImage>(columnImageCount, m_columnWidth.Count + 1); //框线需要多算一个
            _rows = new Array2<ScrollTableImage>(rowImageCount, m_rowHeight.Count + 1);

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

            int maxColumn = m_columnWidth.Count - 1;
            int maxRow = m_rowHeight.Count - 1;
            if (maxColumn < 0 || maxRow < 0)
            {
                return;
            }

            GetVisibleCellRange(maxColumn, maxRow, out var newLeftTopCell, out var newRightBottomCell);

            if (newLeftTopCell != _leftTopCell || newRightBottomCell != _rightBottomCell)
            {
                UpdateContent(_leftTopCell, _rightBottomCell, newLeftTopCell, newRightBottomCell);
                _leftTopCell = newLeftTopCell;
                _rightBottomCell = newRightBottomCell;
            }
        }

        /// <summary>
        /// 按行列轴独立探测可见范围，避免以某一格两轴同时判定时在边框间隙漏扩
        /// </summary>
        private void GetVisibleCellRange(int maxColumn, int maxRow, out Vector2Int leftTop, out Vector2Int rightBottom)
        {
            var estimate = GetLeftTopCell();

            int top = estimate.y;
            while (top > 0 && ShouldRowDisplayInView(top - 1))
            {
                top--;
            }

            while (top < maxRow && !ShouldRowDisplayInView(top))
            {
                top++;
            }

            if (!ShouldRowDisplayInView(top))
            {
                leftTop = estimate;
                rightBottom = estimate;
                return;
            }

            int bottom = top;
            while (bottom < maxRow && ShouldRowDisplayInView(bottom + 1))
            {
                bottom++;
            }

            int left = estimate.x;
            while (left > 0 && ShouldColumnDisplayInView(left - 1))
            {
                left--;
            }

            while (left < maxColumn && !ShouldColumnDisplayInView(left))
            {
                left++;
            }

            if (!ShouldColumnDisplayInView(left))
            {
                leftTop = new Vector2Int(estimate.x, top);
                rightBottom = new Vector2Int(estimate.x, bottom);
                return;
            }

            int right = left;
            while (right < maxColumn && ShouldColumnDisplayInView(right + 1))
            {
                right++;
            }

            leftTop = new Vector2Int(left, top);
            rightBottom = new Vector2Int(right, bottom);
        }

        /// <summary>
        /// 获取显示区域左上角的单元格索引
        /// </summary>
        /// <returns></returns>
        private Vector2Int GetLeftTopCell()
        {
            int maxColumn = m_columnWidth.Count - 1;
            int maxRow = m_rowHeight.Count - 1;
            var x = -content.anchoredPosition.x;

            float maxX = _cellX[^1];
            // _cellX 长度为列数+1（包含右边界），合法单元格列索引上限为 Count-1
            int crtX = maxX <= 0
                ? 0
                : Math.Clamp((int)(x / maxX * maxColumn), 0, maxColumn);

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
                    if (crtX >= maxColumn)
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
            int crtY = maxY <= 0
                ? 0
                : Math.Clamp((int)(y / maxY * maxRow), 0, maxRow);

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
                    if (crtY >= maxRow)
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

            return new Vector2Int(Math.Clamp(crtX, 0, maxColumn), Math.Clamp(crtY, 0, maxRow));
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

                    string cellText = null;
                    if (_tableData.m_Array != null && x < _tableData.m_Length0 && y < _tableData.m_Length1)
                    {
                        cellText = _tableData[x, y];
                    }

                    _cells[x, y].SetState(cellText, x, y);
                }
            }

            CellPoolSetting.Pool.Flush();

            if (ColumnImagePoolSetting.Count > 0)
            {
                for (int x = oldTopLeft.x; x <= oldRightBottom.x + 1; x++)
                {
                    int i = x % ColumnImagePoolSetting.Count;
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
                    int i = x % ColumnImagePoolSetting.Count;
                    if (_columns[i, x] == null)
                    {
                        _columns[i, x] = ColumnImagePoolSetting[i].Pool.New();
                    }

                    _columns[i, x].SetState(x, 0);
                }

                foreach (var setting in ColumnImagePoolSetting)
                {
                    setting.Pool.Flush();
                }
            }

            if (RowImagePoolSetting.Count > 0)
            {
                for (int y = oldTopLeft.y; y <= oldRightBottom.y + 1; y++)
                {
                    int i = y % RowImagePoolSetting.Count;
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
                    int i = y % RowImagePoolSetting.Count;
                    if (_rows[i, y] == null)
                    {
                        _rows[i, y] = RowImagePoolSetting[i].Pool.New();
                    }

                    _rows[i, y].SetState(0, y);
                }

                foreach (var setting in RowImagePoolSetting)
                {
                    setting.Pool.Flush();
                }
            }
        }

        /// <summary>
        /// 单元格布局底边（含与下一格之间的边框），与 GetLeftTopCell 使用的 _cellY[i+1] 对齐，
        /// 避免视口落在内容底边与下一格顶边之间的间隙时判定失败、无法向下扩展。
        /// </summary>
        private float GetCellLayoutBottom(int rowIndex)
        {
            if (rowIndex + 1 < _cellY.Length)
            {
                return _cellY[rowIndex + 1];
            }

            return _cellY[rowIndex] + m_rowHeight[rowIndex];
        }

        private float GetCellLayoutRight(int columnIndex)
        {
            if (columnIndex + 1 < _cellX.Length)
            {
                return _cellX[columnIndex + 1];
            }

            return _cellX[columnIndex] + m_columnWidth[columnIndex];
        }

        private bool ShouldRowDisplayInView(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= m_rowHeight.Count)
            {
                return false;
            }

            var viewTop = content.anchoredPosition.y;
            var viewBottom = viewTop + viewRect.rect.height;
            return GetCellLayoutBottom(rowIndex) > viewTop && _cellY[rowIndex] < viewBottom;
        }

        private bool ShouldColumnDisplayInView(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= m_columnWidth.Count)
            {
                return false;
            }

            var viewLeft = -content.anchoredPosition.x;
            var viewRight = viewLeft + viewRect.rect.width;
            return GetCellLayoutRight(columnIndex) > viewLeft && _cellX[columnIndex] < viewRight;
        }

        /// <summary>
        /// 判断单元格是否应当显示
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        private bool ShouldCellDisplayInView(int columnIndex, int rowIndex)
        {
            return ShouldColumnDisplayInView(columnIndex) && ShouldRowDisplayInView(rowIndex);
        }
    }
}
