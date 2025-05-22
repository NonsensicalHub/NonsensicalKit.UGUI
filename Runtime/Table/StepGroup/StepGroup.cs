using System.Collections;
using NaughtyAttributes;
using NonsensicalKit.Tools.InputTool;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Table
{
    /// <summary>
    /// 让滚动栏自动固定在规律的位置
    /// m_scrollRect.content必须挂载unity内置的三种LayoutGroup组件之一
    /// 可使用默认UI预制体ScrollView
    /// </summary>
    public class StepGroup : MonoBehaviour
    {
        private const float STEP_THRESHOLD = 0.618f; //离当前索引多远时移动到另一个索引

        [FormerlySerializedAs("m_scollRect")] [SerializeField]
        private ScrollRect m_scrollRect;

        [SerializeField] [Range(1, 300)] private float m_speedThreshold = 100; //开始控制运动的速度阈值，当惯性运动的速度低于此值时才会开始控制运动
        [SerializeField] [Range(0.001f, 1)] private float m_moveSpeed = 0.05f; //校正速度


        [SerializeField] private Vector2Int m_startCount = new Vector2Int(1, 1); //初始横纵方向的单位数量

        [SerializeField] private bool m_autoCheckCellSize = false;

        [SerializeField] [HideIf("m_autoCheckCellSize")]
        private Vector2 m_startCellSize = new Vector2(100, 100); //初始单位尺寸

        [SerializeField] private Vector2Int m_startIndex = new Vector2Int(0, 0); //初始索引
        [SerializeField] private Vector2 m_viewportAnchors = new Vector2(0.5f, 0.5f); //ScrollRect的Viewport用于锚定点
        [SerializeField] private Vector2 m_cellAnchors = new Vector2(0.5f, 0.5f); //每个单位的锚定点，自动移动会试图将单位的锚定点和Viewport锚定点重合

        [SerializeField] private UnityEvent<Vector2Int, Vector2Int> m_onIndexChanged; //索引改变事件
        [SerializeField] private UnityEvent m_onControl; //交互控制事件
        [SerializeField] private UnityEvent<bool> m_xCanPlus;
        [SerializeField] private UnityEvent<bool> m_xCanMinus;
        [SerializeField] private UnityEvent<bool> m_yCanPlus;
        [SerializeField] private UnityEvent<bool> m_yCanMinus;

        public UnityEvent<Vector2Int, Vector2Int> OnIndexChanged => m_onIndexChanged;
        public UnityEvent OnControl => m_onControl;
        public int X => _crtIndex.x;
        public int Y => _crtIndex.y;
        public Vector2Int Index => _crtIndex;

        private RectTransform _content;

        private Vector2 _spacing = Vector2.zero;
        private Vector2 _fullSize = Vector2.zero;

        private bool _forceScroll;

        private Vector2Int _crtIndex;
        private Vector2Int _crtCount;
        private Vector2 _crtCellSize;
        private float _oneMinusMoveSpeed;
        private float _oneMinusStepThreshold;

        private Vector2 _newPosBuffer;

        private InputHub _input;
        private float _lastVelocity;
        private Coroutine _scrollCoroutine;

        private void Awake()
        {
            _input = InputHub.Instance;
            _oneMinusMoveSpeed = 1 - m_moveSpeed;
            _oneMinusStepThreshold = 1 - STEP_THRESHOLD;

            _crtIndex = m_startIndex;
            _content = m_scrollRect.content;
        }

        private void Start()
        {
            CheckIndex();
            if (m_autoCheckCellSize)
            {
                StartCoroutine(CheckSizeCor());
            }
            else
            {
                Recount(m_startCount);
                Resize(m_startCellSize);
            }
        }

        private IEnumerator CheckSizeCor()
        {
            yield return new WaitForEndOfFrame();
            if (m_scrollRect.content != null && m_scrollRect.content.childCount > 1)
            {
                Recount(m_startCount);
                Resize(m_scrollRect.content.GetChild(0).GetComponent<RectTransform>().sizeDelta);
            }
            else
            {
                Recount(m_startCount);
                Resize(m_startCellSize);
            }
        }

        private void Update()
        {
            if (_forceScroll) return; //强制滚动时不进行任何处理

            var velocity = m_scrollRect.velocity.magnitude;
            if (!Mathf.Approximately(_lastVelocity, velocity))
            {
                if (_lastVelocity == 0)
                {
                    OnControl?.Invoke();
                }

                _lastVelocity = velocity;
            }

            var newPos = UpdateState();
            if (_input.IsMouseLeftButtonHold) return; //鼠标正在操作时不进行主动运动
            if (velocity > m_speedThreshold) return; //惯性滑动时不进行主动运动
            _content.anchoredPosition = newPos;
            m_scrollRect.velocity = Vector2.zero; //将速度归零防止抖动
        }

        public void GoToX(int xIndex)
        {
            GoTo(new Vector2Int(xIndex, _crtIndex.y));
        }

        public void GoToY(int yIndex)
        {
            GoTo(new Vector2Int(_crtIndex.x, yIndex));
        }

        public void GoTo(int xIndex, int yIndex)
        {
            GoTo(new Vector2Int(xIndex, yIndex));
        }

        public void GoTo(Vector2Int index)
        {
            var oldIndex = _crtIndex;
            _crtIndex = index;
            CheckIndex();
            _content.anchoredPosition = new Vector2(-GetCrtHorizontalPos(), GetCrtVerticalPos());
            m_onIndexChanged?.Invoke(oldIndex, _crtIndex);
        }

        public void ScrollToX(int xIndex)
        {
            ScrollTo(new Vector2Int(xIndex, _crtIndex.y));
        }

        public void ScrollToY(int yIndex)
        {
            ScrollTo(new Vector2Int(_crtIndex.x, yIndex));
        }

        public void ScrollTo(int xIndex, int yIndex)
        {
            ScrollTo(new Vector2Int(xIndex, yIndex));
        }

        public void ScrollTo(Vector2Int index)
        {
            var oldIndex = _crtIndex;
            _crtIndex = index;
            CheckIndex();
            if (gameObject.activeSelf)
            {
                if (_scrollCoroutine != null)
                {
                    StopCoroutine(_scrollCoroutine);
                }

                _scrollCoroutine = StartCoroutine(CorScrollTo(index));
            }
            else
            {
                _content.anchoredPosition = new Vector2(-GetCrtHorizontalPos(), GetCrtVerticalPos());
            }

            m_onIndexChanged?.Invoke(oldIndex, _crtIndex);
        }

        public void XPlusOne()
        {
            if (_crtIndex.x + 1 >= _crtCount.x)
            {
                return;
            }

            ScrollTo(new Vector2Int(_crtIndex.x + 1, _crtIndex.y));
        }

        public void XMinusOne()
        {
            if (_crtIndex.x - 1 < 0)
            {
                return;
            }

            ScrollTo(new Vector2Int(_crtIndex.x - 1, _crtIndex.y));
        }

        public void YPlusOne()
        {
            if (_crtIndex.y + 1 >= _crtCount.y)
            {
                return;
            }

            ScrollTo(new Vector2Int(_crtIndex.x, _crtIndex.y + 1));
        }

        public void YMinusOne()
        {
            if (_crtIndex.y - 1 < 0)
            {
                return;
            }

            ScrollTo(new Vector2Int(_crtIndex.x, _crtIndex.y - 1));
        }

        public void Recount(int xCount, int yCount)
        {
            Recount(new Vector2Int(xCount, yCount));
        }

        public void Recount(Vector2Int newCount)
        {
            m_scrollRect.horizontal = newCount.x != 1;
            m_scrollRect.vertical = newCount.y != 1;
            _crtCount = newCount;
            CheckIndex();
            Init();
        }

        public void Resize(float xSize, float ySize)
        {
            Resize(new Vector2(xSize, ySize));
        }

        public void Resize(Vector2 newCellSize)
        {
            _crtCellSize = newCellSize;
            Init();
        }

        private Vector2 UpdateState()
        {
            Vector2Int oldIndex = _crtIndex;

            float crtX = -_content.anchoredPosition.x;
            float crtXTarget = GetCrtHorizontalPos();
            float newX = crtXTarget;
            float xOffset = crtX - crtXTarget;

            if (Mathf.Abs(xOffset) > 1)
            {
                float xTargetFloat = crtX / _fullSize.x;
                int xTargetInt = (int)xTargetFloat;
                float xResidue = xTargetFloat - xTargetInt;

                if (xOffset < 0)
                {
                    if (xResidue > _oneMinusStepThreshold)
                    {
                        xTargetInt++;
                    }
                }
                else
                {
                    if (xResidue > STEP_THRESHOLD)
                    {
                        xTargetInt++;
                    }
                }

                _crtIndex.x = xTargetInt;
                crtXTarget = GetCrtHorizontalPos();
                newX = crtX * _oneMinusMoveSpeed + crtXTarget * m_moveSpeed;
            }


            float crtY = _content.anchoredPosition.y;
            float crtYTarget = GetCrtVerticalPos();
            float newY = crtYTarget;
            float yOffset = crtY - crtYTarget;

            if (Mathf.Abs(yOffset) > 1)
            {
                float yTargetFloat = crtY / _fullSize.y;
                int yTargetInt = (int)yTargetFloat;
                float yResidue = yTargetFloat - yTargetInt;

                if (yOffset < 0)
                {
                    if (yResidue > _oneMinusStepThreshold)
                    {
                        yTargetInt++;
                    }
                }
                else
                {
                    if (yResidue > STEP_THRESHOLD)
                    {
                        yTargetInt++;
                    }
                }

                _crtIndex.y = yTargetInt;
                crtYTarget = GetCrtVerticalPos();
                newY = crtY * _oneMinusMoveSpeed + crtYTarget * m_moveSpeed;
            }

            if (oldIndex != _crtIndex)
            {
                m_onIndexChanged?.Invoke(oldIndex, _crtIndex);
            }

            _newPosBuffer.x = -newX;
            _newPosBuffer.y = newY;
            return _newPosBuffer;
        }

        /// <summary>
        /// 确保索引合法
        /// </summary>
        private void CheckIndex()
        {
            if (_crtCount.x <= 0)
            {
                _crtCount.x = 1;
            }

            if (_crtCount.y <= 0)
            {
                _crtCount.y = 1;
            }

            if (_crtIndex.x < 0)
            {
                _crtIndex.x = 0;
            }
            else if (_crtIndex.x >= _crtCount.x)
            {
                _crtIndex.x = _crtCount.x - 1;
            }

            if (_crtIndex.y < 0)
            {
                _crtIndex.y = 0;
            }
            else if (_crtIndex.y >= _crtCount.y)
            {
                _crtIndex.y = _crtCount.y - 1;
            }

            m_xCanPlus?.Invoke(_crtIndex.x < _crtCount.x - 1);
            m_xCanMinus.Invoke(_crtIndex.x > 0);
            m_yCanPlus?.Invoke(_crtIndex.y < _crtCount.y - 1);
            m_yCanMinus.Invoke(_crtIndex.y > 0);
        }

        private bool _initFlag;

        private void Init()
        {
            if (!_initFlag)
            {
                _initFlag = true;
                StartCoroutine(InitCor());
            }
        }

        private IEnumerator InitCor()
        {
            LayoutGroup group = _content.GetComponent<LayoutGroup>();
            if (group == null)
            {
                Debug.LogError("未挂载布局组组件");
                enabled = false;
                yield break;
            }

            yield return new WaitForEndOfFrame();
            var zeroOne = new Vector2(0, 1); //本组件默认从左上到右下的阅读顺序，默认锚定左上角，此时实际位置的x使用负值，y使用正值
            _content.pivot = zeroOne;
            _content.anchorMin = zeroOne;
            _content.anchorMax = zeroOne;

            //获取_spacing
            _spacing = Vector2.zero;
            GridLayoutGroup tempGrid = group as GridLayoutGroup;
            if (tempGrid != null)
            {
                _spacing = tempGrid.spacing;
            }
            else
            {
                HorizontalLayoutGroup tempHorizontal = group as HorizontalLayoutGroup;
                if (tempHorizontal != null)
                {
                    _spacing.x = tempHorizontal.spacing;
                }
                else
                {
                    VerticalLayoutGroup tempVertical = group as VerticalLayoutGroup;
                    if (tempVertical != null)
                    {
                        _spacing.y = (group as VerticalLayoutGroup).spacing;
                    }
                    else
                    {
                        Debug.LogError("未挂载内置的布局组组件");
                        enabled = false;
                        yield break;
                    }
                }
            }

            //计算_content的填充和尺寸
            float mainWidth = _crtCellSize.x * _crtCount.x + _spacing.x * (_crtCount.x - 1);
            float mainHeight = _crtCellSize.y * _crtCount.y + _spacing.y * (_crtCount.y - 1);

            float viewportWidth = m_scrollRect.viewport.rect.width;
            float viewportHeight = m_scrollRect.viewport.rect.height;

            float newLeft = m_viewportAnchors.x * viewportWidth - m_cellAnchors.x * _crtCellSize.x;
            float newTop = m_viewportAnchors.y * viewportHeight - m_cellAnchors.y * _crtCellSize.y;

            float newRight = (1 - m_viewportAnchors.x) * viewportWidth - (1 - m_cellAnchors.x) * _crtCellSize.x;
            float newBottom = (1 - m_viewportAnchors.y) * viewportHeight - (1 - m_cellAnchors.y) * _crtCellSize.y;

            group.padding.left = (int)newLeft;
            group.padding.right = (int)newRight;
            group.padding.top = (int)newTop;
            group.padding.bottom = (int)newBottom;

            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mainWidth + newLeft + newRight);
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mainHeight + newTop + newBottom);


            _fullSize.x = _crtCellSize.x + _spacing.x;
            _fullSize.y = _crtCellSize.y + _spacing.y;

            //计算新的位置
            _content.anchoredPosition = new Vector2(-GetCrtHorizontalPos(), GetCrtVerticalPos());
            CheckIndex();
            
            _initFlag = false;
        }

        private IEnumerator CorScrollTo(Vector2Int index)
        {
            _forceScroll = true;

            Vector2 crtPos = _content.anchoredPosition;
            Vector2 targetPos = new Vector2(-index.x * _fullSize.x, index.y * _fullSize.y);
            float timer = 0;
            float scrollTime = 0.5f;
            while (timer < scrollTime)
            {
                _content.anchoredPosition = Vector2.Lerp(crtPos, targetPos, timer / scrollTime);
                timer += Time.deltaTime;
                yield return null;
            }

            _content.anchoredPosition = targetPos;

            _forceScroll = false;
        }

        private float GetCrtHorizontalPos()
        {
            return _crtIndex.x * _fullSize.x;
        }

        private float GetCrtVerticalPos()
        {
            return _crtIndex.y * _fullSize.y;
        }
    }
}
