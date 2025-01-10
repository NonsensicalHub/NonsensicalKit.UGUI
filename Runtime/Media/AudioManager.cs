using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Core;
using NonsensicalKit.Tools.EasyTool;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Media
{
    /// <summary>
    /// 使用url播放音频的UI管理类，会以url为键缓存AudioClip
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private MediaProgress m_audioPogress;
        [SerializeField] private ToggleButton m_btn_play;
        [SerializeField] private Slider m_sld_volume;
        [SerializeField] private ToggleButton m_btn_mute;
        [SerializeField] private GameObject m_downloadingMask;

        [SerializeField] private bool m_loop;
        [SerializeField] private bool m_mute;
        [SerializeField] [Range(0, 1)] private float m_volume = 0.5f;

        public bool Loop
        {
            get => m_loop;
            set
            {
                m_loop = value;
                UpdateLoop();
            }
        }

        public float Volume
        {
            get => m_volume;
            set
            {
                if (m_volume != value)
                {
                    m_volume = Mathf.Clamp01(value);
                    m_sld_volume.value = m_volume;
                    UpdateSound();
                }
            }
        }

        public bool Mute
        {
            get => m_mute;
            set
            {
                if (m_mute != value)
                {
                    m_btn_mute.SetState(value);
                    m_mute = value;
                    UpdateSound();
                }
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    ChangePlayState(value);
                }
            }
        }

        private readonly HashSet<string> _clipLoading = new();
        private bool _isPlaying;
        private AudioSource _audio;
        private string _crtUrl;
        private bool _inited;

        private void Awake()
        {
            Init();
            m_btn_play.OnValueChanged.AddListener(ChangePlayState);
            m_audioPogress.OnDragStateChanged.AddListener(OnDragStateChanged);
        }

        private void Update()
        {
            if (Time.frameCount % 5 == 0
                && _audio is not null)
            {
                if (m_audioPogress.Dragging)
                {
                    _audio.time = m_audioPogress.Value;
                }
                else
                {
                    m_audioPogress.Value = _audio.time;
                    if (_audio.clip != null)
                    {
                        m_audioPogress.MaxValue = _audio.clip.length;
                    }
                }
            }
        }

        public void ChangeUrl(string url)
        {
            if (_crtUrl != url)
            {
                if (_isPlaying)
                {
                    PlayAudio(url);
                }
                else
                {
                    _crtUrl = url;
                    PreheatClip(_crtUrl);
                }
            }
        }

        public void PlayAudio(string url)
        {
            _crtUrl = url;
            PlayAudio();
        }

        public void PlayAudio()
        {
            Init();
            if (_crtUrl == null) return;
            _isPlaying = true;
            m_btn_play.SetState(true);

            DoPlay();
        }

        public void ChangePlayState(bool newState)
        {
            m_btn_play.SetState(newState);
            if (newState)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Replay()
        {
            _isPlaying = true;
            m_btn_play.SetState(_isPlaying);
            DoPlay();
        }

        public void Resume()
        {
            _isPlaying = true;
            m_btn_play.SetState(_isPlaying);
            if (_audio is not null)
            {
                if (_audio.clip != null)
                {
                    _audio.time = m_audioPogress.Value;
                    _audio.UnPause();
                    //_audio.UnPause();
                }
                else
                {
                    DoPlay();
                }
            }
        }

        public void Pause()
        {
            _isPlaying = false;
            if (_audio is not null)
            {
                _audio.Pause();
            }
            m_btn_play.SetState(false);
        }

        public void Switch()
        {
            if (_isPlaying)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        private void Init()
        {
            if (_inited) return;
            _inited = true;
            _audio = GetComponent<AudioSource>();
            m_btn_play.OnValueChanged.AddListener(ChangePlayState);
            m_sld_volume.onValueChanged.AddListener(OnVolumeChanged);
            m_btn_mute.OnValueChanged.AddListener(OnMuteChanged);
        }

        private void PreheatClip(string url)
        {
            _audio.Stop();
            _audio.clip = null;

            if (string.IsNullOrEmpty(url) == false)
            {
                NonsensicalInstance.Instance.StartCoroutine(DoPreheat(url));
            }
        }


        private IEnumerator DoPreheat(string url)
        {
            var v = new DownloadContext<AudioClip>();
            yield return AudioDownloader.Instance.Get(url, v);
            if (url == _crtUrl)
            {
                m_downloadingMask?.SetActive(false);
            }
        }

        private void OnDragStateChanged(bool dragging)
        {
            if (_audio != null)
            {
                if (_isPlaying)
                {
                    if (dragging)
                    {
                        _audio.Pause();
                    }
                    else
                    {
                        _audio.Play();
                    }
                }
            }
        }

        private void DoPlay(bool playFromTheBeginning = true)
        {
            if (_audio is not null && string.IsNullOrEmpty(_crtUrl) == false)
            {
                StartCoroutine(PlayCor(playFromTheBeginning));
            }
        }

        private IEnumerator PlayCor(bool playFromTheBeginning = true)
        {
            m_downloadingMask.SetActive(true);
            var context = new DownloadContext<AudioClip>();
            yield return AudioDownloader.Instance.Get(_crtUrl, context);

            m_downloadingMask.SetActive(false);
            if (context.Resource is null) yield break;

            _audio.clip = context.Resource;
            if (playFromTheBeginning)
            {
                _audio.time = 0;
            }

            //m_audioPogress.Init(_audio.clip.length);
            if (_isPlaying)
            {
                _audio.Play();
            }
        }

        private void OnVolumeChanged(float value)
        {
            Volume = value;
            UpdateSound();
        }

        private void OnMuteChanged(bool mute)
        {
            Mute = mute;
            UpdateSound();
        }

        private void UpdateSound()
        {
            if (_audio is not null)
            {
                if (Mute)
                {
                    _audio.volume = 0;
                }
                else
                {
                    _audio.volume = Volume;
                }
            }
        }

        private void UpdateLoop()
        {
            if (_audio is not null)
            {
                _audio.loop = m_loop;
            }
        }
    }
}
