using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NonsensicalKit.UGUI.Media
{
    //除了播放进度外的其他所有播放状态
    public class VideoPlayState
    {
        public bool Playing;
        public bool FullScreen;
        public bool Fixed;
        public bool Loop;
        public bool Mute;
        public float Volume;
    }

    /// <summary>
    /// 视频播放管理器
    /// </summary>
    public class VideoManager : MonoBehaviour
    {
        [SerializeField] private Canvas m_fullscreenCanvas;
        [SerializeField] private RectTransform m_playPart;
        [SerializeField] private RectTransform m_controlPart;
        [SerializeField] private RawImage m_rimg_video;

        [SerializeField] [OnValueChanged("UpdateVideoAspectRatio")]
        private VideoAspectRatio m_aspectRatio = VideoAspectRatio.FitInside;

        [SerializeField] [OnValueChanged("UpdateFixed")]
        private bool m_fixed;

        [SerializeField] [OnValueChanged("UpdateMute")]
        private bool m_mute;

        [SerializeField] [OnValueChanged("UpdateLoop")]
        private bool m_loop;

        [SerializeField] [OnValueChanged("UpdateVolume")] [Range(0, 1)]
        private float m_volume = 0.5f;

        [SerializeField] private UnityEvent<VideoPlayState> m_onPlayStateChanged;
        [SerializeField] private UnityEvent<MediaProgressState> m_onPlayProgressChanged;
        [SerializeField] private UnityEvent m_onPlayEnd;

        [SerializeField] private bool m_initOnAwake;

        public UnityEvent<VideoPlayState> OnPlayStateChanged => m_onPlayStateChanged;
        public UnityEvent<MediaProgressState> OnPlayProgressChanged => m_onPlayProgressChanged;
        public UnityEvent OnPlayEnd => m_onPlayEnd;


        #region public get set property

        public float PlayTime
        {
            get
            {
                if (!_videoPlayer)
                {
                    return 0;
                }

                return (float)_videoPlayer.time;
            }
            set
            {
                if (!_videoPlayer) return;
                _videoPlayer.time = value;

                _progress.CurrentProgress = value;
                InvokeProgressChanged();
            }
        }

        public bool FullScreen
        {
            get => _fullScreen;
            set
            {
                if (_fullScreen == value) return;
                Init();
                _fullScreen = value;
                SetFullScreen();
                InvokePlayStateChanged();
            }
        }

        public bool Playing
        {
            get => _playing;
            set
            {
                if (_playing == value) return;
                Init();
                _playing = value;
                UpdatePlayingState();
                InvokePlayStateChanged();
            }
        }

        public bool Fixed
        {
            get => m_fixed;
            set
            {
                if (m_fixed == value) return;
                Init();
                m_fixed = value;
                SetFixed();
                InvokePlayStateChanged();
            }
        }

        public bool Loop
        {
            get => m_loop;
            set
            {
                if (m_loop == value) return;
                Init();
                m_loop = value;
                SetPlayerLoop();
                InvokePlayStateChanged();
            }
        }

        public float Volume
        {
            get => m_volume;
            set
            {
                if (Mathf.Approximately(m_volume, value)) return;
                value = Mathf.Clamp01(value);
                Init();
                m_volume = value;
                SetPlayerVolume();
                InvokePlayStateChanged();
            }
        }

        public bool Mute
        {
            get => m_mute;
            set
            {
                if (m_mute == value) return;
                Init();
                m_mute = value;
                SetPlayerMute();
                InvokePlayStateChanged();
            }
        }

        public bool Manual
        {
            get => _manual;
            set
            {
                if (_manual == value) return;
                Init();
                _manual = value;
                UpdatePlayingState();
            }
        }

        #endregion

        private bool _fullScreen;
        private bool _playing;
        private bool _manual;

        private bool _inited;

        private RectTransform _videoRect;
        private Transform _oldParent;
        private VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;
        private float _controlPartHeight;

        private VideoPlayState _state;
        private MediaProgressState _progress;

        private bool _fakeFrameFlag;


        private void Awake()
        {
            if (m_initOnAwake)
            {
                Init();
            }
        }

        private void Start()
        {
            if (!m_initOnAwake)
            {
                Init();
            }
        }

        #region Public methods

        public void SetFullscreenCanvas(Canvas canvas)
        {
            m_fullscreenCanvas = canvas;
        }

        public void ChangeUrl(string url)
        {
            Init();
            if (_videoPlayer.url != url)
            {
                PlayVideo(url);
            }
        }

        public void ChangeClip(VideoClip clip)
        {
            Init();
            if (_videoPlayer.clip != clip)
            {
                PlayVideo(clip);
            }
        }


        public void PlayVideo(string url, bool wait = true)
        {
            Init();

            DoPause();

            PlayTime = 0;
            _renderTexture?.Release();
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = url;

            UpdateRenderTextureSize();
            _playing = !wait;
            DoPlay();
            InvokePlayStateChanged();
        }

        public void PlayVideo(VideoClip clip, bool wait = true)
        {
            Init();

            DoPause();

            PlayTime = 0;
            _renderTexture?.Release();
            _videoPlayer.source = VideoSource.VideoClip;
            _videoPlayer.clip = clip;

            UpdateRenderTextureSize();
            _playing = !wait;
            DoPlay();
            InvokePlayStateChanged();
        }

        public void Stop()
        {
            Init();
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
            }
        }

        public void Switch()
        {
            Init();
            if (_playing)
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
            Init();
            _playing = true;
            PlayTime = 0;
            UpdatePlayingState();
            InvokePlayStateChanged();
        }

        public void Play()
        {
            Init();
            _playing = true;
            UpdatePlayingState();
            InvokePlayStateChanged();
        }

        public void PlayAndWait()
        {
            Init();
            _playing = false;
            UpdatePlayingState();
            InvokePlayStateChanged();
        }

        public void Pause()
        {
            Init();
            _playing = false;
            UpdatePlayingState();
            InvokePlayStateChanged();
        }

        #endregion

        #region VideoPlayer Event

        private void OnStarted(VideoPlayer source)
        {
            source.time = _progress.CurrentProgress;
            if (source.frame == 0 && _progress.CurrentProgress == 0)
            {
                source.frame = 1;
            }

            if (source.clip != null)
            {
                _progress.TotalProgress = (float)source.clip.length;
            }
            else
            {
                _progress.TotalProgress = (float)source.length;
            }

            UpdatePlayingState();
        }

        private void OnNewFrame(VideoPlayer source, long frameIdx)
        {
            if (frameIdx == 0 && !_fakeFrameFlag)
            {
                _fakeFrameFlag = true;
                return;
            }

            _fakeFrameFlag = false;
            _progress.CurrentProgress = (float)source.time;
            InvokeProgressChanged();
        }

        private void OnErrorReceived(VideoPlayer source, string message)
        {
            Debug.LogError("视频播放错误:" + message);
        }

        private void OnLoopPoint(VideoPlayer source)
        {
            if (!m_loop)
            {
                m_onPlayEnd?.Invoke();
                source.frame = 1;
            }
        }

        #endregion

        private void UpdatePlayingState()
        {
            var needPlay = _playing && !_manual;
            if (needPlay == _videoPlayer.isPlaying) return;
            if (needPlay)
            {
                DoPlay();
            }
            else
            {
                DoPause();
            }
        }

        private void Init()
        {
            if (_inited) return;

            _oldParent = transform.parent;
            if (m_fullscreenCanvas == null)
            {
                m_fullscreenCanvas = GetComponentInParent<Canvas>(true);
            }

            _videoRect = m_rimg_video.GetComponent<RectTransform>();

            _progress = new MediaProgressState();
            _state = new VideoPlayState();
            InvokePlayStateChanged();
            _controlPartHeight = m_controlPart.rect.height;

            if (TryGetComponent(out _videoPlayer) == false)
            {
                _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }

            _videoPlayer.playOnAwake = false;
            _videoPlayer.sendFrameReadyEvents = true;
            _videoPlayer.started += OnStarted;
            _videoPlayer.frameReady += OnNewFrame;
            _videoPlayer.loopPointReached += OnLoopPoint;
            _videoPlayer.errorReceived += OnErrorReceived;
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.aspectRatio = m_aspectRatio;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

            SetPlayerVolume();
            SetFullScreen();
            SetPlayerLoop();
            SetPlayerMute();
            SetFixed();
            _inited = true;
        }

        private void InvokePlayStateChanged()
        {
            _state.Playing = _playing;
            _state.FullScreen = _fullScreen;
            _state.Loop = m_loop;
            _state.Fixed = m_fixed;
            _state.Mute = m_mute;
            _state.Volume = m_volume;
            m_onPlayStateChanged?.Invoke(_state);
        }

        private void InvokeProgressChanged()
        {
            m_onPlayProgressChanged?.Invoke(_progress);
        }

        private void DoPlay()
        {
            if (_videoPlayer.isActiveAndEnabled)
            {
                _videoPlayer.Play();
            }
        }

        private void DoPause()
        {
            if (_videoPlayer.isActiveAndEnabled)
            {
                _videoPlayer.Pause();
            }
        }

        private void SetFullScreen()
        {
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

        private void SetPlayerLoop()
        {
            _videoPlayer.isLooping = m_loop;
        }

        private void SetPlayerMute()
        {
            for (ushort i = 0; i < _videoPlayer.audioTrackCount; i++)
            {
                _videoPlayer.SetDirectAudioMute(i, m_mute);
            }
        }

        private void SetPlayerVolume()
        {
            for (ushort i = 0; i < _videoPlayer.audioTrackCount; i++)
            {
                _videoPlayer.SetDirectAudioVolume(i, m_volume);
            }
        }

        private void SetFixed()
        {
            if (m_fixed)
            {
                m_playPart.StretchWithBottomInterval(_controlPartHeight);
            }
            else
            {
                m_playPart.Stretch();
            }

            UpdateRenderTextureSize();
        }

        private void UpdateRenderTextureSize()
        {
            if (!_inited) return;

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

        #region Editor Value Changed Event

        private void UpdateVideoAspectRatio()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.aspectRatio = m_aspectRatio;
            }
        }

        private void UpdateFixed()
        {
            Fixed = m_fixed;
        }

        private void UpdateLoop()
        {
            Loop = m_loop;
        }

        private void UpdateMute()
        {
            Mute = m_mute;
        }

        private void UpdateVolume()
        {
            Volume = m_volume;
        }

        #endregion
    }
}
