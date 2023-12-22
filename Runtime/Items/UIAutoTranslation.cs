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

        private RectTransform _rect_self;
        private float _offset;

        private float _crtPos;

        private Vector2 _tempPos = Vector2.zero;

        private void Awake()
        {
            Subscribe<float>(m_signal, OnTranslation);
            _rect_self = GetComponent<RectTransform>();

            GameObject go2 = Instantiate(m_control.GetChild(0).gameObject, m_control);
            GameObject go3 = Instantiate(m_control.GetChild(0).gameObject, m_control);
            if (m_horizon)
            {
                _offset = _rect_self.rect.width;
                go2.GetComponent<RectTransform>().anchoredPosition -= new Vector2(m_control.rect.width, 0);
                go3.GetComponent<RectTransform>().anchoredPosition += new Vector2(m_control.rect.width, 0);
            }
            else
            {
                _offset = _rect_self.rect.height;
                go2.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, m_control.rect.height);
                go3.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, m_control.rect.height);
            }
        }

        private void OnTranslation(float value)
        {
            if (value == 0)
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
