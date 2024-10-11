using NonsensicalKit.Tools.NetworkTool;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Media
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private MediaProgress m_audioPogress;
        [SerializeField] private ToggleButton m_btn_play;
        [SerializeField] private Slider m_sld_volume;
        [SerializeField] private ToggleButton m_btn_mute;

        [SerializeField] private bool m_loop = false;
        [SerializeField] private bool m_mute = false;
        [SerializeField][Range(0, 1)] private float m_volume = 0.5f;

        public bool Loop { get { return m_loop; } set { m_loop = value; } }
        public float Volume { get { return m_volume; } set { m_volume = Mathf.Clamp01(value); ; m_sld_volume.value = m_volume; } }
        public bool Mute { get { return m_mute; } set { m_mute = value; } }
        public bool IsPlaying { get { return _isPlaying; } set { if (_isPlaying != value) { ChangePlayState(value); } } }

        private bool _isPlaying;
        private AudioSource _audio;
        private Dictionary<string, AudioClip> _clipBuffer = new Dictionary<string, AudioClip>();
        private string _crtUrl;
        private bool _inited;

        private void Awake()
        {
            Init();
            m_btn_play.OnValueChanged.AddListener(ChangePlayState);
        }

        private void Update()
        {
            if (_audio != null)
            {
                if (Time.frameCount % 5 == 0)
                {
                    if (m_audioPogress.Dragging)
                    {
                        _audio.time = m_audioPogress.Value;
                    }
                    else
                    {
                        m_audioPogress.Value = (float)_audio.time;
                    }
                }
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
            if (newState)
            {
                Replay();
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

        public void Play()
        {
            _isPlaying = true;
            DoPlay(false);
        }

        public void Pause()
        {
            _isPlaying = false;
            if (_audio != null)
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
                Play();
            }
        }

        private void Init()
        {
            _audio = GetComponent<AudioSource>();
            m_btn_play.OnValueChanged.AddListener(ChangePlayState);
            m_sld_volume.onValueChanged.AddListener(OnVolumeChanged);
            m_btn_mute.OnValueChanged.AddListener(OnMuteChanged);
        }

        private void GetClip(string url)
        {
            _audio.Stop();
            _audio.clip = null;

            if (url != null)
            {
                StartCoroutine(HttpUtility.GetAudio(url, OnGetAudio));
            }
        }

        private void OnGetAudio(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                var v = request.downloadHandler as DownloadHandlerAudioClip;
                var clip = v.audioClip;
                if (clip != null)
                {
                    _clipBuffer.Add(request.url, clip);
                    _audio.clip = clip;
                    if (IsPlaying)
                    {
                        DoPlay();
                    }
                    return;
                }
            }

            Debug.LogError("音频文件加载错误" + request.url);
            Pause();
        }

        private void DoPlay(bool PlayFromTheBeginning = true)
        {
            if (_audio != null && _crtUrl != null && _clipBuffer.ContainsKey(_crtUrl))
            {
                if (PlayFromTheBeginning)
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
            if (_audio != null)
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
    }
}
