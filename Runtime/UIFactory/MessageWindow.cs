using NonsensicalKit.Core;
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

        public void SetArg(object arg)
        {
            _crtConfirmInfo = arg as MessageInfo;
            if (_crtConfirmInfo == null)
            {
                LogCore.Warning($"传入{nameof(MessageWindow)}的参数有误");
                Pool.Store(gameObject);
                return;
            }
            OpenMessageWindow(_crtConfirmInfo);
        }

        private void OpenMessageWindow(MessageInfo messageInfo)
        {
            m_txt_message.text = messageInfo.Message;

            NonsensicalInstance.Instance.DelayDoIt(messageInfo.SurviceTime, StoreSelf);
        }

        private void StoreSelf()
        {
            Pool.Store(gameObject);
        }
    }
}
