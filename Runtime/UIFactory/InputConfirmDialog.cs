using NonsensicalKit.Core.Log;
using NonsensicalKit.Tools.ObjectPool;
using UnityEngine;
using UnityEngine.UI;
#if TEXTMESHPRO_PRESENT
using TMPro;
#endif

namespace NonsensicalKit.UGUI.UIFactory
{
    public delegate bool InputConfirmHandle(string input);

    public class InputConfirmInfo
    {
        public string Message;
        public string OldString;
        public string LeftButtonText;
        public string RightButtonText;
        public InputConfirmHandle LeftButtonClick;
        public InputConfirmHandle RightButtonClick;

        public InputConfirmInfo(string message, string oldString, string leftButtonText, string rightButtonText, InputConfirmHandle leftButtonClick,
            InputConfirmHandle rightButtonClick)
        {
            this.Message = message;
            this.OldString = oldString;
            this.LeftButtonText = leftButtonText;
            this.RightButtonText = rightButtonText;
            this.LeftButtonClick = leftButtonClick;
            this.RightButtonClick = rightButtonClick;
        }

        public InputConfirmInfo(string message, string oldString, InputConfirmHandle leftButtonClick)
        {
            this.Message = message;
            this.OldString = oldString;
            this.LeftButtonText = "确认";
            this.RightButtonText = "取消";
            this.LeftButtonClick = leftButtonClick;
            this.RightButtonClick = null;
        }
    }

    /// <summary>
    /// 有一个输入框和两个按钮的确认窗口
    /// </summary>
    public class InputConfirmDialog : MonoBehaviour, IFactoryUI
    {
#if TEXTMESHPRO_PRESENT
        [SerializeField] private TextMeshProUGUI m_txt_message;
        [SerializeField] private TMP_InputField m_ipf_input;
        [SerializeField] private TextMeshProUGUI m_txt_leftButton;
        [SerializeField] private TextMeshProUGUI m_txt_rightButton;
#else
        [SerializeField] private Text m_txt_message;
        [SerializeField] private InputField m_ipf_input;
        [SerializeField] private Text m_txt_leftButton;
        [SerializeField] private Text m_txt_rightButton;
#endif
        [SerializeField] private Button m_btn_leftButton;
        [SerializeField] private Button m_btn_rightButton;

        private InputConfirmInfo _crtConfirmInfo;

        public GameObjectPool Pool { get; set; }

        private void Awake()
        {
            m_btn_leftButton.onClick.AddListener(LeftButtonClick);
            m_btn_rightButton.onClick.AddListener(RightButtonClick);
        }

        public void SetArg(object arg)
        {
            _crtConfirmInfo = arg as InputConfirmInfo;
            if (_crtConfirmInfo == null)
            {
                LogCore.Warning($"传入{nameof(InputConfirmDialog)}的参数有误");
                Pool.Store(gameObject);
                return;
            }

            m_ipf_input.text = _crtConfirmInfo.OldString;
            m_txt_message.text = _crtConfirmInfo.Message;
            m_txt_leftButton.text = _crtConfirmInfo.LeftButtonText;
            m_txt_rightButton.text = _crtConfirmInfo.RightButtonText;
        }

        private void LeftButtonClick()
        {
            if (_crtConfirmInfo.LeftButtonClick == null)
            {
                Pool.Store(gameObject);
            }
            else if (_crtConfirmInfo.LeftButtonClick(m_ipf_input.text))
            {
                Pool.Store(gameObject);
            }
        }

        private void RightButtonClick()
        {
            if (_crtConfirmInfo.RightButtonClick == null)
            {
                Pool.Store(gameObject);
            }
            else if (_crtConfirmInfo.RightButtonClick(m_ipf_input.text))
            {
                Pool.Store(gameObject);
            }
        }
    }
}
