using System.Collections;
using NonsensicalKit.Tools.EasyTool;
using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.UGUI.Media
{
    public class AudioPlayState
    {
        public bool Playing;
        public bool Loop;
        public bool Mute;
        public float Volume;
        public bool Loading;
    }

    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private bool m_loop;
        [SerializeField] private bool m_mute;
        [SerializeField] [Range(0, 1)] private float m_volume = 0.5f;
        [SerializeField] private UnityEvent<AudioPlayState> m_onPlayStateChanged;
        [SerializeField] private UnityEvent<MediaProgressState> m_onPlayProgressChanged;
        [SerializeField] private bool m_initOnAwake;

        public UnityEvent<AudioPlayState> OnPlayStateChanged => m_onPlayStateChanged;
        public UnityEvent<MediaProgressState> OnPlayProgressChanged => m_onPlayProgressChanged;

        #region public get set property

        public float PlayTime
        {
            get
            {
                if (!_audio)
                {
                    return 0;
                }

                return _audio.time;
            }
            set
            {
                if (!_audio) return;
                _audio.time = value;
                InvokeProgressChanged();
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
                SetLoop();
                InvokePlayStateChanged();
            }
        }

        public float Volume
        {
            get => m_volume;
            set
            {
                if (Mathf.Approximately(m_volume, value)) return;
                Init();
                m_volume = Mathf.Clamp01(value);
                SetSound();
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
                SetMute();
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

        #endregion

        private bool _playing;
        private bool _inited;
        private AudioSource _audio;
        private string _crtUrl;
        private bool _loading;
        private AudioPlayState _state;
        private MediaProgressState _progress;
        private bool _manual;

        private bool _fromUrl;

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

        private void Update()
        {
            if (_audio && _audio.clip && _audio.isPlaying)
            {
                InvokeProgressChanged();
            }
        }

        #region public methods

        public void ChangeUrl(string url)
        {
            if (_crtUrl == url) return;
            Init();
            _crtUrl = url;
            if (string.IsNullOrEmpty(_crtUrl)) return;
            _fromUrl = true;
            StartCoroutine(GetClipCor(_crtUrl));
        }

        public void PlayAudio(string url)
        {
            Init();
            _crtUrl = url;
            if (string.IsNullOrEmpty(_crtUrl)) return;
            _fromUrl = true;
            _playing = true;
            StartCoroutine(GetClipCor(_crtUrl));
        }

        public void PlayAudio(AudioClip clip)
        {
            Init();
            if (_audio.clip == clip) return;
            if (clip == null) return;
            _playing = true;
            _fromUrl = false;
            PlayClip(clip);
        }

        public void Replay()
        {
            Init();
            _playing = true;
            PlayTime = 0;
            UpdatePlayingState();
            InvokePlayStateChanged();
        }

        public void Resume()
        {
            Init();
            _playing = true;
            _audio.time = _progress.CurrentProgress;
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

        public void Switch()
        {
            if (_playing)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        #endregion

        private void Init()
        {
            if (_inited) return;
            if (TryGetComponent(out _audio) == false)
            {
                _audio = gameObject.AddComponent<AudioSource>();
            }

            _state = new AudioPlayState();
            _progress = new MediaProgressState();

            _loading = true;

            SetLoop();
            SetMute();
            SetSound();
            InvokePlayStateChanged();
            _inited = true;
        }

        private void UpdatePlayingState()
        {
            var needPlay = _playing && !_manual;
            if (needPlay == _audio.isPlaying) return;
            if (needPlay)
            {
                DoPlay();
            }
            else
            {
                DoPause();
            }
        }

        private void DoPlay()
        {
            if (_audio.isActiveAndEnabled)
            {
                _audio.Play();
            }
        }

        private void DoPause()
        {
            if (_audio.isActiveAndEnabled)
            {
                _audio.Pause();
            }
        }

        private IEnumerator GetClipCor(string url)
        {
            var context = new DownloadContext<AudioClip>();
            _loading = true;
            InvokePlayStateChanged();
            yield return AudioDownloader.Instance.Get(url, context);

            if (context.Resource is null) yield break;

            if (_fromUrl && url == _crtUrl)
            {
                PlayClip(context.Resource);
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null) return;
            _loading = false;
            _audio.clip = clip;
            _audio.time = _progress.CurrentProgress;
            InvokeProgressChanged();
            UpdatePlayingState();
            InvokePlayStateChanged();
        }

        private void InvokeProgressChanged()
        {
            _progress.CurrentProgress = _audio.time;
            if (_audio.clip == null)
            {
                _progress.TotalProgress = 0;
            }
            else
            {
                _progress.TotalProgress = _audio.clip.length;
            }

            m_onPlayProgressChanged?.Invoke(_progress);
        }

        private void InvokePlayStateChanged()
        {
            _state.Playing = _playing;
            _state.Loop = m_loop;
            _state.Mute = m_mute;
            _state.Volume = m_volume;
            _state.Loading = _loading;
            m_onPlayStateChanged?.Invoke(_state);
        }

        private void SetMute()
        {
            _audio.mute = m_mute;
        }

        private void SetSound()
        {
            _audio.volume = Volume;
        }

        private void SetLoop()
        {
            _audio.loop = m_loop;
        }
    }
}
