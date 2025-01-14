using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Media
{
    public class VideoControlPart : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private CanvasGroup m_canvasGroup;
        [SerializeField] private VideoManager m_videoManager;
        [SerializeField] private bool m_showBegin;
        [SerializeField] private float m_hideTime = 0.5f;

        [Header("可为空的配置项")]
        [SerializeField] private MediaProgress m_progress;

        [SerializeField] private ToggleButton m_btn_fixControl;
        [SerializeField] private ToggleButton m_btn_loop;
        [SerializeField] private ToggleButton m_btn_play;
        [SerializeField] private ToggleButton m_btn_fullScreen;
        [SerializeField] private ToggleButton m_btn_mute;
        [SerializeField] private Slider m_sld_sound;

        private bool _isFixing;
        private bool _isMouseHover;
        private float _timer;

        private void Reset()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            if (m_canvasGroup == null)
            {
                m_canvasGroup = GetComponent<CanvasGroup>();
            }

            m_videoManager.OnPlayStateChanged.AddListener(OnPlayStateChanged);
            m_videoManager.OnPlayProgressChanged.AddListener(OnPlayProgressChanged);

            m_progress?.OnDragStateChanged.AddListener(OnDragStateChanged);

            m_btn_play?.OnValueChanged.AddListener(OnPlayChanged);
            m_btn_fullScreen?.OnValueChanged.AddListener(OnFullScreenChanged);
            m_btn_loop?.OnValueChanged.AddListener(OnLoopChanged);
            m_btn_fixControl?.OnValueChanged.AddListener(OnFixedChanged);
            m_btn_mute?.OnValueChanged.AddListener(OnMuteStateChanged);
            m_sld_sound?.onValueChanged.AddListener(OnSoundValueChanged);
        }

        private void Start()
        {
            if (m_showBegin)
            {
                _timer = m_hideTime;
                OpenSelf();
            }
            else
            {
                CloseSelf();
            }
        }

        private void Update()
        {
            if (!_isFixing && !_isMouseHover && _timer > 0)
            {
                _timer -= Time.deltaTime;

                if (_timer <= 0)
                {
                    CloseSelf();
                }
            }

            if (m_progress && m_progress.Dragging)
            {
                m_videoManager.PlayTime = m_progress.Value;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OpenSelf();
            _timer = m_hideTime;
            _isMouseHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseHover = false;
        }

        private void OnPlayProgressChanged(MediaProgressState progress)
        {
            if (!m_progress || m_progress.Dragging) return;
            
            m_progress.State = progress;
        }

        private void OnPlayStateChanged(VideoPlayState state)
        {
            m_btn_play?.SetState(state.Playing);
            m_btn_fullScreen?.SetState(state.FullScreen);
            m_btn_loop?.SetState(state.Loop);
            m_btn_fixControl?.SetState(state.Fixed);
            m_btn_mute?.SetState(state.Mute);
            if (m_sld_sound)
            {
                m_sld_sound.value = state.Volume;
            }

            if (_isFixing != state.Fixed)
            {
                _isFixing = state.Fixed;
                if (_isFixing)
                {
                    _isFixing = true;
                    OpenSelf();
                }
                else
                {
                    _isFixing = false;
                    OpenSelf();
                    _timer = m_hideTime;
                }
            }
        }

        private void OnDragStateChanged(bool dragging)
        {
            m_videoManager.Manual = dragging;
        }

        #region UIEvent

        private void OnPlayChanged(bool isPlaying)
        {
            m_videoManager.Playing = isPlaying;
        }

        private void OnFullScreenChanged(bool value)
        {
            m_videoManager.FullScreen = value;
        }

        private void OnLoopChanged(bool value)
        {
            m_videoManager.Loop = value;
        }

        private void OnFixedChanged(bool isFixed)
        {
            m_videoManager.Fixed = isFixed;
        }

        private void OnMuteStateChanged(bool isMute)
        {
            m_videoManager.Mute = isMute;
        }

        private void OnSoundValueChanged(float value)
        {
            m_videoManager.Volume = value;
        }

        #endregion

        private void OpenSelf()
        {
            m_canvasGroup.alpha = 1;
        }

        private void CloseSelf()
        {
            m_canvasGroup.alpha = 0;
        }
    }
}
