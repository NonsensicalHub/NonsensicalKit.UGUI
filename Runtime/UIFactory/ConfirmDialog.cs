using NonsensicalKit.Core.Log;
using NonsensicalKit.Tools.ObjectPool;
using System;
using UnityEngine;
using UnityEngine.UI;
#if TEXTMESHPRO_PRESENT
using TMPro;
#endif

namespace NonsensicalKit.UGUI.UIFactory
{
    public class ConfirmInfo
    {
        public string Message;
        public string LeftButtonText;
        public string RightButtonText;
        public Func<bool> LeftButtonClick;
        public Func<bool> RightButtonClick;

        public ConfirmInfo(string message, string leftButtonText, string rightButtonText, Func<bool> leftButtonClick, Func<bool> rightButtonClick)
        {
            this.Message = message;
            this.LeftButtonText = leftButtonText;
            this.RightButtonText = rightButtonText;
            this.LeftButtonClick = leftButtonClick;
            this.RightButtonClick = rightButtonClick;
        }
        public ConfirmInfo(string message)
        {
            this.Message = message;
            this.LeftButtonText = "确认";
            this.RightButtonText = "确认";
            this.LeftButtonClick = null;
            this.RightButtonClick = null;
        }
        public ConfirmInfo(string message, Func<bool> leftButtonClick)
        {
            this.Message = message;
            this.LeftButtonText = "确认";
            this.RightButtonText = "取消";
            this.LeftButtonClick = leftButtonClick;
            this.RightButtonClick = null;
        }
        public ConfirmInfo(string message,Action leftButtonClick)
        {
            this.Message = message;
            this.LeftButtonText = "确认";
            this.RightButtonText = "取消";
            this.LeftButtonClick = ()=> { leftButtonClick();return true; };
            this.RightButtonClick = null;
        }
    }

    /// <summary>
    /// 有两个按钮的确认窗口
    /// </summary>
    public class ConfirmDialog : MonoBehaviour, IFactoryUI
    {
#if TEXTMESHPRO_PRESENT
        [SerializeField] private TextMeshProUGUI m_txt_message;
        [SerializeField] private TextMeshProUGUI m_txt_leftButton;
        [SerializeField] private TextMeshProUGUI m_txt_rightButton;
#else
        [SerializeField] private Text m_txt_message;
        [SerializeField] private Text m_txt_leftButton;
        [SerializeField] private Text m_txt_rightButton;
#endif
        [SerializeField] private Button m_btn_leftButton;
        [SerializeField] private Button m_btn_rightButton;

        public GameObjectPool Pool { get; set; }

        private ConfirmInfo _crtConfirmInfo;

        private void Awake()
        {
            m_btn_leftButton.onClick.AddListener(LeftButtonClick);
            m_btn_rightButton.onClick.AddListener(RightButtonClick);
        }

        public void SetArg(object arg)
        {
            _crtConfirmInfo = arg as ConfirmInfo;
            if (_crtConfirmInfo == null)
            {
                LogCore.Warning($"传入{nameof(ConfirmDialog)}的参数有误");
                Pool.Store(gameObject);
                return;
            }

            m_btn_leftButton.gameObject.SetActive(_crtConfirmInfo.LeftButtonClick != null);

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
            else if (_crtConfirmInfo.LeftButtonClick())
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
            else if (_crtConfirmInfo.RightButtonClick())
            {
                Pool.Store(gameObject);
            }
        }
    }
}
