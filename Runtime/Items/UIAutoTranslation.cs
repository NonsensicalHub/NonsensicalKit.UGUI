using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.UGUI
{
    /// <summary>
    /// RectTransform下应当有一个且仅有一个Image，且锚点完全扩张
    /// </summary>
    public class UIAutoTranslation : NonsensicalMono
    {
        [SerializeField] private RectTransform m_control;
        [SerializeField] private string m_signal;
        [SerializeField] private bool m_horizon;

        private RectTransform _rectSelf;
        private float _offset;

        private float _crtPos;

        private Vector2 _tempPos = Vector2.zero;

        private void Awake()
        {
            if (string.IsNullOrEmpty(m_signal))
            {
                Debug.LogWarning($"{nameof(UIAutoTranslation)} 信号名为空，组件不会生效。", this);
                enabled = false;
                return;
            }

            Subscribe<float>(m_signal, OnTranslation);
            _rectSelf = GetComponent<RectTransform>();
            if (_rectSelf == null || m_control == null || m_control.childCount == 0)
            {
                Debug.LogWarning($"{nameof(UIAutoTranslation)} 配置无效，需要绑定 control 且至少存在一个子节点。", this);
                enabled = false;
                return;
            }

            GameObject go2 = Instantiate(m_control.GetChild(0).gameObject, m_control);
            GameObject go3 = Instantiate(m_control.GetChild(0).gameObject, m_control);
            if (!go2.TryGetComponent(out RectTransform go2Rect) || !go3.TryGetComponent(out RectTransform go3Rect))
            {
                Debug.LogWarning($"{nameof(UIAutoTranslation)} 子节点缺少 RectTransform。", this);
                enabled = false;
                return;
            }

            if (m_horizon)
            {
                _offset = _rectSelf.rect.width;
                go2Rect.anchoredPosition -= new Vector2(m_control.rect.width, 0);
                go3Rect.anchoredPosition += new Vector2(m_control.rect.width, 0);
            }
            else
            {
                _offset = _rectSelf.rect.height;
                go2Rect.anchoredPosition -= new Vector2(0, m_control.rect.height);
                go3Rect.anchoredPosition += new Vector2(0, m_control.rect.height);
            }
        }

        private void OnTranslation(float value)
        {
            if (value == 0)
            {
                return;
            }
            if (Mathf.Approximately(_offset, 0f))
            {
                return;
            }

            _crtPos += value;
            if (_crtPos > _offset)
            {
                _crtPos -= _offset;
            }

            if (_crtPos < -_offset)
            {
                _crtPos += _offset;
            }

            float lastPos = _crtPos;
            if (value > 0)
            {
                lastPos -= _offset;
            }

            if (m_horizon)
            {
                _tempPos.x = lastPos;
            }
            else
            {
                _tempPos.y = lastPos;
            }

            m_control.anchoredPosition = _tempPos;
        }
    }
}
