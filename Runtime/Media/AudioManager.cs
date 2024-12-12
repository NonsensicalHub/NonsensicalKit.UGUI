using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Tools.NetworkTool;
using UnityEngine;
using UnityEngine.Networking;
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
                m_volume = Mathf.Clamp01(value);

                if (m_volume != value)
                {
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
                m_btn_mute.SetState(value);
                if (m_mute != value)
                {
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
                    ChangePlayState(value);
                }
            }
        }

        private readonly Dictionary<string, AudioClip> _clipBuffer = new();
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
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var clipBuffer in _clipBuffer)
            {
                Destroy(clipBuffer.Value);
            }
        }

        public void ChangeUrl(string url)
        {
            if (_crtUrl != url)
            {
                if (IsPlaying)
                {
                    PlayAudio(url);
                }
                else
                {
                    _crtUrl = url;
                    GetClip(_crtUrl);
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
            if (_clipBuffer.ContainsKey(_crtUrl))
            {
                DoPlay();
            }
            else
            {
                GetClip(_crtUrl);
            }
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
            DoPlay();
        }

        public void Resume()
        {
            _isPlaying = true;
            if (_audio is not null)
            {
                if (_audio.clip != null)
                {
                    _audio.UnPause();
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

        private void GetClip(string url)
        {
            _audio.Stop();
            _audio.clip = null;

            if (string.IsNullOrEmpty(url) == false)
            {
                StartCoroutine(GetAudio(url));
            }
        }

        private void OnDragStateChanged(bool dragging)
        {
            if (_audio != null)
            {
                if (IsPlaying)
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

        private IEnumerator GetAudio(string url)
        {
            UnityWebRequest uwr = new UnityWebRequest();

            yield return uwr.GetAudio(url);
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                if (uwr.downloadHandler is DownloadHandlerAudioClip v)
                {
                    var clip = v.audioClip;
                    if (clip is not null)
                    {
                        _clipBuffer.Add(url, clip);
                        if (IsPlaying)
                        {
                            DoPlay();

                            yield break;
                        }
                    }
                }
            }

            LogCore.Error("音频文件加载错误:" + uwr.url);
            Pause();
        }

        private void DoPlay(bool playFromTheBeginning = true)
        {
            if (_audio is not null && _clipBuffer.TryGetValue(_crtUrl, out var value))
            {
                _audio.clip = value;
                if (playFromTheBeginning)
                {
                    _audio.time = 0;
                }

                m_audioPogress.Init(_audio.clip.length);
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
