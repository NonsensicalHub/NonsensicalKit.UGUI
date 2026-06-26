using NonsensicalKit.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Media
{
    public class MediaProgressState
    {
        public float CurrentProgress;
        public float TotalProgress;
    }

    [RequireComponent(typeof(Slider))]
    public class MediaProgress : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IPointerUpHandler
    {
        [SerializeField] private Slider m_sld_progress;
        [SerializeField] private TextMeshProUGUI m_crtTime;
        [SerializeField] private TextMeshProUGUI m_maxTime;
        [SerializeField] private UnityEvent<bool> m_dragStateChanged;

        public UnityEvent<bool> OnDragStateChanged => m_dragStateChanged;

        public MediaProgressState State
        {
            set
            {
                Value = value.CurrentProgress;
                MaxValue = value.TotalProgress;
            }
        }

        public float Value
        {
            get => m_sld_progress.value;
            set
            {
                if (Mathf.Approximately(m_sld_progress.value, value)) return;
                m_sld_progress.value = value;
                m_crtTime.text = StringTool.GetFormatTime(value);
            }
        }

        public float MaxValue
        {
            set
            {
                if (Mathf.Approximately(m_sld_progress.maxValue, value)) return;
                m_sld_progress.maxValue = value;
                m_maxTime.text = StringTool.GetFormatTime(value);
            }
        }

        public bool Dragging
        {
            get => _dragging;
            private set
            {
                if (_dragging == value) return;
                _dragging = value;
                OnDragStateChanged?.Invoke(value);
            }
        }

        private bool _dragging;

        public void OnBeginDrag(PointerEventData eventData)
        {
            Dragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Dragging = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Dragging = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Dragging = false;
        }
    }
}
