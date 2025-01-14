using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Media
{
    public class VideoPlayPart : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
    {
        [SerializeField] private VideoManager m_manager;

        [SerializeField] private float m_showTime = 1;
        [SerializeField] private Button m_btn_play;
        [SerializeField] private Button m_btn_pause;

        private bool _isHover;
        private bool _isPlaying;
        private float _timer;

        private void Awake()
        {
            m_manager.OnPlayStateChanged.AddListener(OnPlayStateChanged);
            m_btn_play.onClick.AddListener(Play);
            m_btn_pause.onClick.AddListener(Pause);

            m_btn_play.gameObject.SetActive(false);
            m_btn_pause.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_isHover && _timer >= 0)
            {
                _timer -= Time.deltaTime;
                if (_timer < 0)
                {
                    UpdateState();
                }
            }
        }

        private void Play()
        {
            m_manager.Play();
        }

        private void Pause()
        {
            m_manager.Pause();
        }

        public void OnPlayStateChanged(VideoPlayState state)
        {
            if (_isPlaying == state.Playing) return;

            _isPlaying = state.Playing;
            UpdateState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHover = true;
            UpdateState();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (_timer < 0)
            {
                UpdateState();
            }

            _timer = m_showTime;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHover = false;
            UpdateState();
        }

        private void UpdateState()
        {
            if (_isPlaying)
            {
                m_btn_play.gameObject.SetActive(false);
                m_btn_pause.gameObject.SetActive(_isHover && (_timer > 0));
            }
            else
            {
                m_btn_play.gameObject.SetActive(true);
                m_btn_pause.gameObject.SetActive(false);
            }
        }
    }
}
