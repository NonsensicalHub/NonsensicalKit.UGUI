using NonsensicalKit.Core.Log;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.ObjectPool;
using TMPro;
using UnityEngine;

namespace NonsensicalKit.UGUI.UIFactory
{
    public class FloatMessageInfo
    {
        public string Message;
        public Vector2 StartPos;
        public Vector2 EndPos;
        public Vector2 Speed;
        public float Time;

        public FloatMessageInfo(string message, Vector2 startPos, Vector2 endPos, float time)
        {
            Message = message;
            StartPos = startPos;
            EndPos = endPos;
            Time = time;
        }
    }

    /// <summary>
    /// 悬浮消息
    /// </summary>
    public class FloatMessage : MonoBehaviour, IFactoryUI
    {
        [SerializeField] private TextMeshProUGUI m_txt_message;

        private RectTransform _selfRect;

        public GameObjectPool Pool { get; set; }

        private Vector3 _originPosition;

        private void Awake()
        {
            _selfRect = GetComponent<RectTransform>();
            _originPosition = transform.position;
        }

        public void SetArg(object arg)
        {
            FloatMessageInfo info = arg as FloatMessageInfo;
            if (info == null)
            {
                LogCore.Warning($"传入{nameof(FloatMessage)}的参数有误");
                Pool.Store(gameObject);
                return;
            }

            m_txt_message.text = info.Message;
            m_txt_message.ForceMeshUpdate();
            _selfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_txt_message.textBounds.size.x + 20);

            _selfRect.anchoredPosition = info.StartPos;
            _selfRect.DoMove(info.EndPos, info.Time).OnComplete(OnFloatComplete);
        }

        private void OnFloatComplete()
        {
            Pool.Store(gameObject);
        }
    }
}
