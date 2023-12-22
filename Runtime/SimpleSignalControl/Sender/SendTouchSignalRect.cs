using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    public class SendTouchSignalRect : NonsensicalMono, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private string m_signal;

        private bool _isHold;

        public void OnPointerDown(PointerEventData eventData)
        {
            _isHold = true;

            Publish(m_signal, true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isHold)
            {
                _isHold = false;
                Publish(m_signal, false);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isHold)
            {
                _isHold = false;
                Publish(m_signal, false);
            }
        }
    }
}
