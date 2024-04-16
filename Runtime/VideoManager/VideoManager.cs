using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NonsensicalKit.UGUI.VideoManager
{
    /// <summary>
    /// 视频播放管理器
    /// </summary>
    public class VideoManager : NonsensicalMono
    {
        [SerializeField] private Canvas m_fullscreenCanvas;
        [SerializeField] private VideoControlSpace m_controlSpace;
        [SerializeField] private VideoProgressSlider m_videoProgressSlider;
        [SerializeField] private PlayPart m_playPart;
        [SerializeField] private RectTransform m_controlPart;
        [SerializeField] private RawImage m_rimg_video;

        [SerializeField] private Button m_btn_fixedControl;
        [SerializeField] private Button m_btn_unfixedControl;

        [SerializeField] private Button m_btn_play;
        [SerializeField] private Button m_btn_pause;
        [SerializeField] private Button m_btn_fullScreen;
        [SerializeField] private Button m_btn_reset;
        [SerializeField] private Slider m_sld_progress;

        [SerializeField] private Button m_btn_sound;
        [SerializeField] private Button m_btn_soundMute;
        [SerializeField] private Slider m_sld_sound;

        [SerializeField] private VideoAspectRatio m_aspectRatio = VideoAspectRatio.FitInside;
        [SerializeField] private bool m_loop = false;
        [SerializeField] private bool m_muteOnInit = false;
        [SerializeField] private bool m_fixedOnInit = false;
        [SerializeField] private bool m_logInfo = false;
        [SerializeField] private UnityEvent m_onPlayEnd;
        [SerializeField] private UnityEvent<bool> m_onPlayStateChanged;

        public UnityEvent OnPlayEnd => m_onPlayEnd;
        public UnityEvent<bool> OnPlayStateChanged => m_onPlayStateChanged;
        public bool IsPlaying => _isPlaying;

        public bool IsFullScreen => _fullScreen;

        private RectTransform _videoRect;
        private Transform _oldParent;
        private VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;
        private bool _fullScreen;
        private bool _isDrag;
        private bool _needWait;
        private bool _isPlaying;
        private float _soundVolume = 1;
        private bool _isMute;
        private bool _isControlFixed;
        private bool _initialized;
        private bool _waitBuffer = false;

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            if (_videoPlayer != null)
            {
                if (_isDrag == true)
                {
                    _videoPlayer.frame = (long)(m_sld_progress.value * _videoPlayer.frameCount);
                }
                else
                {
                    m_sld_progress.value = (float)_videoPlayer.frame / _videoPlayer.frameCount;
                }
            }
        }

        public void SetFullscreenCanvas(Canvas canvas)
        {
            m_fullscreenCanvas = canvas;
        }

        public void PlayVideo(string url, bool needwait = true)
        {
            LogInfo("播放视频：" + url);

            PlayReady();

            _needWait = needwait;
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = url;

            PlayIt();
        }

        public void PlayVideo(VideoClip clip, bool needwait = true)
        {
            LogInfo("播放视频：" + clip.name);

            PlayReady();

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

        public void PlayButton()
        {
            OnPlayVideoClick();
        }

        public void PauseButton()
        {
            OnPauseVideoClick();
        }

        private void Init()
        {
            if (!_initialized)
            {
                _initialized = true;

                m_controlSpace.Init();
                _oldParent = transform.parent;
                if (m_fullscreenCanvas == null)
                {
                    m_fullscreenCanvas = GetComponentInParent<Canvas>();
                }
                _videoRect = m_rimg_video.GetComponent<RectTransform>();
                m_videoProgressSlider.OnProgressSliderDrag += OnDragChanged;

                m_btn_play.onClick.AddListener(OnPlayVideoClick);
                m_btn_pause.onClick.AddListener(OnPauseVideoClick);

                m_btn_sound.onClick.AddListener(SoundNoMute);
                m_btn_soundMute.onClick.AddListener(SoundMute);
                m_sld_sound.onValueChanged.AddListener(OnSoundValueChanged);

                m_btn_fullScreen.onClick.AddListener(OnFullScreenChange);
                m_btn_reset.onClick.AddListener(OnFullScreenChange);

                m_btn_fixedControl.onClick.AddListener(OnFixedClick);
                m_btn_unfixedControl.onClick.AddListener(OnUnfiexedClick);

                _isMute = m_muteOnInit;
                _isControlFixed = m_fixedOnInit;
                _soundVolume = m_sld_sound.value;
                PlayStateChanged();

                m_btn_sound.gameObject.SetActive(_isMute);
                m_btn_soundMute.gameObject.SetActive(!_isMute);
                m_btn_fullScreen.gameObject.SetActive(!_fullScreen);
                m_btn_reset.gameObject.SetActive(_fullScreen);
                UpdateFixed();

                m_playPart.Init(this);
            }
        }

        private void PlayReady()
        {
            Init();
            if (_videoPlayer != null)
            {
                OnPauseVideoClick();
                _videoPlayer.frame = 0;
                _renderTexture?.Release();
            }
            else
            {
                _videoPlayer = gameObject.AddComponent<VideoPlayer>();
                _videoPlayer.playOnAwake = false;
                _videoPlayer.sendFrameReadyEvents = true;
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
            OnPlayVideoClick();
            if (_needWait)
            {
                _isPlaying = false;
            }
            PlayStateChanged();
        }

        private void PlayStateChanged()
        {
            m_btn_play.gameObject.SetActive(!_isPlaying);
            m_btn_pause.gameObject.SetActive(_isPlaying);
            m_onPlayStateChanged?.Invoke(_isPlaying);
        }

        private void OnNewFrame(VideoPlayer source, long frameIdx)
        {
            if (_needWait)
            {
                if (_waitBuffer == false)
                {
                    _waitBuffer = true;
                }
                else
                {
                    _waitBuffer = false;
                    _needWait = false;
                    OnPauseVideoClick();
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
                videoPlayer.frame = 0;
                videoPlayer.Play();
            }
            else
            {
                OnVideoEnd();
            }
        }

        private void OnDragChanged(bool isDrag)
        {
            _isDrag = isDrag;

            if (_videoPlayer == null)
            {
                return;
            }
            if (_isPlaying && isDrag)
            {
                _videoPlayer.Pause();
            }
            else
            {
                PlayStateChanged();
                _videoPlayer.Play();
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

            OnPauseVideoClick();
        }

        private void OnPlayVideoClick()
        {
            LogInfo("视频播放");
            if (_videoPlayer != null)
            {
                _isPlaying = true;
                PlayStateChanged();
                _videoPlayer.Play();
            }
        }

        private void OnPauseVideoClick()
        {
            LogInfo("视频暂停");
            if (_videoPlayer != null)
            {
                _isPlaying = false;
                PlayStateChanged();
                _videoPlayer.Pause();
            }
        }

        public void NotFullScreen()
        {
            if (_fullScreen)
            {
                OnFullScreenChange();
            }
        }

        private void OnFullScreenChange()
        {
            _fullScreen = !_fullScreen;
            m_btn_fullScreen.gameObject.SetActive(!_fullScreen);
            m_btn_reset.gameObject.SetActive(_fullScreen);
            if (_fullScreen)
            {
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

        private void SoundNoMute()
        {
            _isMute = false;
            UpdateSound();
        }

        private void SoundMute()
        {
            _isMute = true;
            UpdateSound();
        }

        private void OnSoundValueChanged(float value)
        {
            if (value != _soundVolume)
            {
                _soundVolume = value;
                if (!_isMute)
                {
                    UpdateSound();
                }
            }
        }

        private void UpdateSound()
        {
            if (_isMute)
            {
                m_btn_sound.gameObject.SetActive(true);
                m_btn_soundMute.gameObject.SetActive(false);
                _videoPlayer?.SetDirectAudioVolume(0, 0);
            }
            else
            {
                m_btn_sound.gameObject.SetActive(false);
                m_btn_soundMute.gameObject.SetActive(true);
                _videoPlayer?.SetDirectAudioVolume(0, _soundVolume);
            }
        }

        private void OnFixedClick()
        {
            _isControlFixed = true;
            UpdateFixed();
        }

        private void OnUnfiexedClick()
        {
            _isControlFixed = false;
            UpdateFixed();
        }

        private void UpdateFixed()
        {
            m_btn_fixedControl.gameObject.SetActive(!_isControlFixed);
            m_btn_unfixedControl.gameObject.SetActive(_isControlFixed);
            var playPartRect = m_playPart.GetComponent<RectTransform>();
            if (_isControlFixed)
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
