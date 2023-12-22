using NonsensicalKit.Tools.InputTool;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.UGUI.VisualLogicGraph
{
    /// <summary>
    /// 连接线
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class VisualLogicLine : MonoBehaviour, IPointerClickHandler
    {
        public VisualLogicPointBase Outputpoint=> _output;
        public VisualLogicPointBase Inputpoint => _input;
        /// <summary>
        /// 输出点位
        /// </summary>
        private VisualLogicPointBase _output;
        /// <summary>
        /// 输入点位
        /// </summary>
        private VisualLogicPointBase _input;

        /// <summary>
        /// 渲染相机
        /// </summary>
        private Camera _renderCamera;
        /// <summary>
        /// 自身的RectTransform
        /// </summary>
        private RectTransform _selfRect;
        /// <summary>
        /// 点击计时器，用于双击判定
        /// </summary>
        private float _timer;

        private void Awake()
        {
            _selfRect = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (_output != null)
            {
                UpdatePos();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //双击时删除
            if (Time.time - _timer < 0.5f)
            {
                CutIt();
            }
            _timer = Time.time;
        }

        /// <summary>
        /// 连接两个点，两端跟随两点运动
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input"></param>
        public void SetObjects(VisualLogicPointBase output, VisualLogicPointBase input)
        {
            _renderCamera = GetComponentInParent<Canvas>().worldCamera;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            _output = output;
            _input = input;
            UpdatePos();
        }

        /// <summary>
        /// 开始拖拽或点击第一个点时调用，表现为一段在开始位置，另一端跟随鼠标
        /// </summary>
        /// <param name="output"></param>
        public void SetObject(VisualLogicPointBase output)
        {
            _renderCamera = GetComponentInParent<Canvas>().worldCamera;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            _output = output;   //此时的_output不一定是输出点位
            _input = null;
            UpdatePos();
        }

        /// <summary>
        /// 更新线的位置
        /// </summary>
        private void UpdatePos()
        {
            Vector3 inputPos;
            if (_input != null)
            {
                inputPos = _input.transform.position;
            }
            else
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(_selfRect, InputHub.Instance.CrtMousePos, _renderCamera, out inputPos);
            }

            _selfRect.position = (_output.transform.position + inputPos) / 2;
            Vector3 offseet = inputPos - _output.transform.position;
            _selfRect.sizeDelta = new Vector3(offseet.magnitude / transform.lossyScale.x, 5);
            _selfRect.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(offseet.y / offseet.x) / Mathf.PI));
        }


        /// <summary>
        /// 切断
        /// 由于信息只存储在输出点位上，所以只需要调用输出点位的断开方法
        /// </summary>
        public void CutIt()
        {
            if (_input != null)
            {
                _output.Disconnect(_input, this);
            }
        }

        /// <summary>
        /// 回收时清空数据
        /// </summary>
        public void OnStore()
        {
            _input = null;
            _output = null;
        }
    }
}
