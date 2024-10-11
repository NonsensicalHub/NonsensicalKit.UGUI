using NonsensicalKit.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Media
{
    [RequireComponent(typeof(Slider))]
    public class MediaProgress : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private Slider m_sld_sound;
        [SerializeField] private TextMeshProUGUI m_crtTime;
        [SerializeField] private TextMeshProUGUI m_maxTime;
        [SerializeField] private UnityEvent<bool> m_dragStateChanegd;

        public UnityEvent<bool> OnDragStateChanged => m_dragStateChanegd;

        public float Value
        {
            get
            {
                return m_sld_sound.value;
            }
            set
            {
                m_sld_sound.value = value;
                m_crtTime.text = StringTool.GetFormatTime(value);
            }
        }

        public bool Dragging { get; private set; }

        private bool _dragging;

        public void Init(float second)
        {
            m_sld_sound.maxValue = second;
            m_sld_sound.value = 0;
            m_crtTime.text = "00:00";
            m_maxTime.text = StringTool.GetFormatTime(second);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Dragging = true;
            OnDragStateChanged?.Invoke(true);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Dragging = false;
            OnDragStateChanged?.Invoke(false);
        }
    }
}
