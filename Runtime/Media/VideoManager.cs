using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NonsensicalKit.UGUI.Media
{
    /// <summary>
    /// 视频播放管理器
    /// </summary>
    public class VideoManager : NonsensicalMono
    {
        [SerializeField] private Canvas m_fullscreenCanvas;
        [SerializeField] private MediaControlSpace m_controlSpace;
        [SerializeField] private MediaProgress m_videoProgressSlider;
        [SerializeField] private VideoPlayPart m_playPart;
        [SerializeField] private RectTransform m_controlPart;
        [SerializeField] private RawImage m_rimg_video;

        [SerializeField] private ToggleButton m_btn_fixControl;
        [SerializeField] private ToggleButton m_btn_play;
        [SerializeField] private ToggleButton m_btn_fullScreen;
        [SerializeField] private ToggleButton m_btn_mute;
        [SerializeField] private Slider m_sld_sound;

        [SerializeField] private VideoAspectRatio m_aspectRatio = VideoAspectRatio.FitInside;
        [SerializeField] private bool m_loop = false;
        [SerializeField] private bool m_mute = false;
        [SerializeField] private bool m_fixed = false;
        [SerializeField] private bool m_logInfo = false;
        [SerializeField] [Range(0, 1)] private float m_volume = 0.5f;
        [SerializeField] private UnityEvent m_onPlayEnd;
        [SerializeField] private UnityEvent<bool> m_onPlayStateChanged;

        public UnityEvent<bool> OnPlayStateChanged => m_onPlayStateChanged;
        public UnityEvent OnPlayEnd => m_onPlayEnd;

        public bool IsFullScreen
        {
            get => _fullScreen;
            set
            {
                if (_fullScreen != value) { OnFullScreenChanged(value); }
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value) { OnStateChanged(value); }
            }
        }

        public bool Loop { get { return m_loop; } set { m_loop = value; } }

        public float Volume
        {
            get { return m_volume; }
            set
            {
                m_volume = Mathf.Clamp01(value);
                m_sld_sound.value = m_volume;
            }
        }

        public bool Mute
        {
            get { return m_mute; }
            set
            {
                m_mute = value;
                m_btn_mute.SetState(value);
            }
        }

        private RectTransform _videoRect;
        private Transform _oldParent;
        private VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;
        private bool _fullScreen;
        private bool _needWait;
        private bool _isPlaying;
        private float _soundVolume = 1;

        private bool _inited;
        private bool _waitFlag = false;

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            if (_videoPlayer != null)
            {
                if (Time.frameCount % 5 == 0)
                {
                    if (m_videoProgressSlider.Dragging)
                    {
                        _videoPlayer.time = m_videoProgressSlider.Value;
                    }
                    else if (_videoPlayer.isPlaying)
                    {
                        m_videoProgressSlider.Value = (float)_videoPlayer.time;
                        if (_videoPlayer.clip != null)
                        {
                            m_videoProgressSlider.MaxValue = (float)_videoPlayer.clip.length;
                        }
                        else
                        {
                            m_videoProgressSlider.MaxValue = (float)_videoPlayer.length;
                        }
                    }
                }
            }
        }

        #region Public methods

        public void SetFullscreenCanvas(Canvas canvas)
        {
            m_fullscreenCanvas = canvas;
        }

        public void ChangeUrl(string url)
        {
            if (_videoPlayer != null && _videoPlayer.url != url)
            {
                PlayVideo(url, true);
            }
        }

        public void PlayVideo(string url, bool needwait = true)
        {
            LogInfo("播放视频：" + url);

            PlayReady();
            if (_videoPlayer != null)
            {
                _videoPlayer.time = 0;
            }
            m_videoProgressSlider.Value = 0;
            _needWait = needwait;
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = url;

            PlayIt();
        }

        public void PlayVideo(VideoClip clip, bool needwait = true)
        {
            LogInfo("播放视频：" + clip.name);

            PlayReady();
            if (_videoPlayer != null)
            {
                _videoPlayer.time = 0;
            }
            m_videoProgressSlider.Value = 0;
            _needWait = needwait;
            _videoPlayer.source = VideoSource.VideoClip;
            _videoPlayer.clip = clip;

            PlayIt();
        }

        public void Stop()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
            }
        }

        public void Switch()
        {
            if (_isPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void Replay()
        {
            _needWait = false;
            OnPlay(true);
        }

        public void Play()
        {
            _needWait = false;

            OnPlay();
        }

        public void PlayAndWait()
        {
            _needWait = true;

            OnPlay();
        }

        public void Pause()
        {
            OnPause();
        }

        #endregion

        #region UI Event

        private void OnStateChanged(bool newPlayState)
        {
            if (newPlayState)
            {
                OnPlay();
            }
            else
            {
                OnPause();
            }
        }

        private void OnFullScreenChanged(bool value)
        {
            _fullScreen = value;
            if (_fullScreen)
            {
                if (m_fullscreenCanvas == null)
                {
                    m_fullscreenCanvas = GetComponentInParent<Canvas>(true);
                    if (m_fullscreenCanvas == null) return;
                }

                transform.SetParent(m_fullscreenCanvas.transform);
            }
            else
            {
                transform.SetParent(_oldParent);
            }

            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            GetComponent<RectTransform>().Stretch();
            UpdateRenderTextureSize();
        }


        private void OnMuteStateChanegd(bool mute)
        {
            m_mute = mute;
            UpdateSound();
        }

        private void OnSoundValueChanged(float value)
        {
            if (value != _soundVolume)
            {
                _soundVolume = value;
                if (!m_mute)
                {
                    UpdateSound();
                }
            }
        }

        private void OnFixedStateChanged(bool isFixed)
        {
            m_fixed = isFixed;
            var playPartRect = m_playPart.GetComponent<RectTransform>();
            if (m_fixed)
            {
                playPartRect.anchorMin = new Vector2(0, m_controlPart.anchorMax.y);
                playPartRect.offsetMin = Vector2.zero;
                playPartRect.offsetMax = Vector2.zero;

                m_controlSpace.Fixed();
            }
            else
            {
                playPartRect.Stretch();
                m_controlSpace.Unfixed();
            }

            UpdateRenderTextureSize();
        }

        private void OnDragStateChanged(bool dragging)
        {
            if (_videoPlayer != null)
            {
                if (IsPlaying)
                {
                    if (dragging)
                    {
                        _videoPlayer.Pause();
                    }
                    else
                    {
                        _videoPlayer.Play();
                    }
                }
            }
        }

        #endregion

        #region VideoPlayer Event

        private void OnStarted(VideoPlayer source)
        {
            source.time = m_videoProgressSlider.Value;
        }

        private void OnNewFrame(VideoPlayer source, long frameIdx)
        {
            if (_needWait)
            {
             m_videoProgressSlider.Value=  (float)  source.time ;
                if (_waitFlag == false)
                {
                    _waitFlag = true;
                }
                else
                {
                    _waitFlag = false;
                    _needWait = false;
                    OnPause();
                }
            }
        }

        private void OnErrorReceived(VideoPlayer source, string message)
        {
            Debug.LogError("视频播放错误:" + message);
        }

        private void OnLoopPoint(VideoPlayer videoPlayer)
        {
            if (m_loop)
            {
                m_videoProgressSlider.Value = 0;
                videoPlayer.frame = 0;
                videoPlayer.Play();
            }
            else
            {
                OnVideoEnd();
            }
        }

        private void OnVideoEnd()
        {
            LogInfo("视频结束");
            if (_videoPlayer == null)
            {
                return;
            }

            m_onPlayEnd?.Invoke();
            _videoPlayer.frame = 0;

            OnPause();
        }

        #endregion

        private void Init()
        {
            if (!_inited)
            {
                _inited = true;

                m_controlSpace.Init();
                _oldParent = transform.parent;
                if (m_fullscreenCanvas == null)
                {
                    m_fullscreenCanvas = GetComponentInParent<Canvas>(true);
                }

                _videoRect = m_rimg_video.GetComponent<RectTransform>();

                m_btn_play.OnValueChanged.AddListener(OnStateChanged);

                m_btn_mute.OnValueChanged.AddListener(OnMuteStateChanegd);
                m_sld_sound.onValueChanged.AddListener(OnSoundValueChanged);

                m_btn_fullScreen.OnValueChanged.AddListener(OnFullScreenChanged);
                m_btn_fixControl.OnValueChanged.AddListener(OnFixedStateChanged);

                m_videoProgressSlider.OnDragStateChanged.AddListener(OnDragStateChanged);

                _soundVolume = m_sld_sound.value;
                PlayStateChanged();

                m_btn_mute.IsOn = m_mute;
                m_btn_fullScreen.IsOn = _fullScreen;
                m_btn_fixControl.IsOn = m_fixed;
                m_playPart.Init(this);
            }
        }

        private void PlayReady()
        {
            Init();
            if (_videoPlayer != null)
            {
                OnPause();
                _videoPlayer.frame = 0;
                _renderTexture?.Release();
            }
            else
            {
                _videoPlayer = gameObject.AddComponent<VideoPlayer>();
                _videoPlayer.playOnAwake = false;
                _videoPlayer.sendFrameReadyEvents = true;
                _videoPlayer.started += OnStarted;
                _videoPlayer.frameReady += OnNewFrame;
                _videoPlayer.loopPointReached += OnLoopPoint;
                _videoPlayer.errorReceived += OnErrorReceived;
                _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                _videoPlayer.aspectRatio = m_aspectRatio;
                _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            }
        }

        private void PlayIt()
        {
            UpdateSound();
            UpdateRenderTextureSize();

            _videoPlayer.frame = 0;
            OnPlay();
            if (_needWait)
            {
                _isPlaying = false;
            }

            PlayStateChanged();
        }

        private void PlayStateChanged()
        {
            m_btn_play.IsOn = _isPlaying;
            m_onPlayStateChanged?.Invoke(_isPlaying);
        }

        private void OnPlay(bool playFromTheBeginning = false)
        {
            LogInfo("视频播放");

            if (_videoPlayer != null)
            {
                if (playFromTheBeginning) _videoPlayer.time = 0;
                if (!_isPlaying)
                {
                    _isPlaying = true;
                    PlayStateChanged();
                    //m_videoProgressSlider.Init((float)_videoPlayer.length);
                }

                if (_videoPlayer.isActiveAndEnabled)
                {
                    _videoPlayer.Play();
                }
            }
        }

        private void OnPause()
        {
            LogInfo("视频暂停");

            if (_videoPlayer != null)
            {
                if (_isPlaying)
                {
                    _isPlaying = false;
                    PlayStateChanged();
                }

                if (_videoPlayer.isActiveAndEnabled)
                {
                    _videoPlayer.Pause();
                }
            }
        }

        private void UpdateSound()
        {
            if (m_mute)
            {
                _videoPlayer?.SetDirectAudioVolume(0, 0);
            }
            else
            {
                _videoPlayer?.SetDirectAudioVolume(0, _soundVolume);
            }
        }

        private void UpdateRenderTextureSize()
        {
            if (_videoPlayer)
            {
                var width = (int)_videoRect.rect.width;
                var height = (int)_videoRect.rect.height;
                if (_renderTexture == null || width != _renderTexture.width || height != _renderTexture.height)
                {
                    if (_renderTexture)
                    {
                        Destroy(_renderTexture);
                    }

                    _renderTexture = new RenderTexture(width, height, 8);
                }

                m_rimg_video.texture = _renderTexture;
                _videoPlayer.targetTexture = _renderTexture;
            }
        }

        private void LogInfo(string str)
        {
            if (m_logInfo)
            {
                Debug.Log(str);
            }
        }
    }
}
