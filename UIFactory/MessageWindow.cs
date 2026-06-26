using NonsensicalKit.Core.Log;
using NonsensicalKit.Tools.ObjectPool;
using TMPro;
using UnityEngine;

namespace NonsensicalKit.UGUI.UIFactory
{
    public class MessageInfo
    {
        public string Message;
        public float SurviceTime;

        public MessageInfo(string message, float surviceTime)
        {
            this.Message = message;
            this.SurviceTime = surviceTime;
        }
    }

    public class MessageWindow : MonoBehaviour, IFactoryUI
    {
        [SerializeField] private TextMeshProUGUI m_txt_message;

        public GameObjectPool Pool { get; set; }

        private MessageInfo _crtConfirmInfo;


        private float _surviveTime;
        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer > _surviveTime)
            {
                Close();
            }
        }

        public void SetArg(object arg)
        {
            _crtConfirmInfo = arg as MessageInfo;
            if (_crtConfirmInfo == null)
            {
                LogCore.Warning($"传入{nameof(MessageWindow)}的参数有误");
                Close();
                return;
            }

            _timer = 0;
            OpenMessageWindow(_crtConfirmInfo);
        }

        public void Close()
        {
            Pool.Store(gameObject);
        }

        private void OpenMessageWindow(MessageInfo messageInfo)
        {
            m_txt_message.text = messageInfo.Message;

            _surviveTime = messageInfo.SurviceTime;
        }
    }
}
