using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Media
{
    public class VideoPlayPart : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
    {
        [SerializeField] private Button m_btn_play;
        [SerializeField] private Button m_btn_pause;

        private bool _isHover;
        private bool _isPlaying;

        private float _timer;

        private VideoManager _manager;

        private void Awake()
        {
            m_btn_play.gameObject.SetActive(false);
            m_btn_pause.gameObject.SetActive(false);

        }

        private void Update()
        {
            if (_isHover)
            {
                _timer -= Time.deltaTime;
                UpdateState();
            }
        }

        public void Init(VideoManager manager)
        {
            _manager = manager;
            manager.OnPlayStateChanged .AddListener( OnPlayStateChanged);
            m_btn_play.onClick.AddListener(Play);
            m_btn_pause.onClick.AddListener(Pause);
        }

        private void Play()
        {
            _manager.Play();
        }

        private void Pause()
        {
            _manager.Pause();
        }

        public void OnPlayStateChanged(bool isPlaying)
        {
            _isPlaying = isPlaying;

            UpdateState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHover = true;
            UpdateState();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            _timer = 1;
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
