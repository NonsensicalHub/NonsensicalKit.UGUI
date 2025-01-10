using NonsensicalKit.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Media
{
    [RequireComponent(typeof(Slider))]
    public class MediaProgress : MonoBehaviour,IPointerDownHandler, IBeginDragHandler, IEndDragHandler,IPointerUpHandler
    {
        [FormerlySerializedAs("m_sld_sound")] [SerializeField] private Slider m_sld_progress;
        [SerializeField] private TextMeshProUGUI m_crtTime;
        [SerializeField] private TextMeshProUGUI m_maxTime;
        [SerializeField] private UnityEvent<bool> m_dragStateChanegd;

        public UnityEvent<bool> OnDragStateChanged => m_dragStateChanegd;

        public float Value
        {
            get
            {
                return m_sld_progress.value;
            }
            set
            {
                m_sld_progress.value = value;
                m_crtTime.text = StringTool.GetFormatTime(value);
            }
        }
        public float MaxValue
        {
            set
            {
                m_sld_progress.maxValue = value;
                m_maxTime.text = StringTool.GetFormatTime(value);
            }
        }

        public bool Dragging { get; private set; }

        private bool _dragging;

        public void Init(float second)
        {
            m_sld_progress.maxValue = second;
            m_sld_progress.value = 0;
            m_crtTime.text = "00:00";
            m_maxTime.text = StringTool.GetFormatTime(second);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Dragging == false)
            {
                Dragging = true;
                OnDragStateChanged?.Invoke(true);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Dragging)
            {
                Dragging = false;
                OnDragStateChanged?.Invoke(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Dragging == false)
            {
                Dragging = true;
                OnDragStateChanged?.Invoke(true);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Dragging)
            {
                Dragging = false;
                OnDragStateChanged?.Invoke(false);
            }
        }
    }
}
