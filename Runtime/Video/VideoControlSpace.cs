using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.UGUI.Video
{
    [RequireComponent(typeof(CanvasGroup))]
    public class VideoControlSpace : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float m_hideTime = 0.5f;

        private bool _isFixed;

        private CanvasGroup _canvasGroupSelf;
        private bool _mouseHover;
        private float _timer;

        private void Awake()
        {
            _canvasGroupSelf = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            _mouseHover = false;
            _timer = m_hideTime;
        }

        private void Update()
        {
            if (!_isFixed && !_mouseHover)
            {
                _timer -= Time.deltaTime;

                if (_timer < 0)
                {
                    CloseSelf();
                }
            }
        }

        public void Init()
        {
            _canvasGroupSelf = GetComponent<CanvasGroup>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OpenSelf();
            _timer = m_hideTime;
            _mouseHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _mouseHover = false;
        }

        public void Fixed()
        {
            _isFixed = true;
            OpenSelf();
        }
        public void Unfixed()
        {
            _isFixed = false;
            OpenSelf();
            _timer = m_hideTime;
        }


        private void OpenSelf()
        {
            _canvasGroupSelf.alpha = 1;
        }

        private void CloseSelf()
        {
            _canvasGroupSelf.alpha = 0;
        }
    }
}
