using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Media
{
    public class AudioControlPart : MonoBehaviour
    {
        [SerializeField] private AudioManager m_audioManager;

        [Header("可为空的配置项")]
        [SerializeField] private MediaProgress m_progress;

        [SerializeField] private ToggleButton m_btn_play;
        [SerializeField] private Slider m_sld_volume;
        [SerializeField] private ToggleButton m_btn_mute;
        [SerializeField] private GameObject m_loadingMask;

        private void Awake()
        {
            m_audioManager.OnPlayStateChanged.AddListener(OnPlayStateChanged);
            m_audioManager.OnPlayProgressChanged.AddListener(OnPlayProgressChanged);

            m_progress?.OnDragStateChanged.AddListener(OnDragStateChanged);

            m_btn_play?.OnValueChanged.AddListener(OnPlayChanged);
            m_sld_volume?.onValueChanged.AddListener(OnVolumeChanged);
            m_btn_mute?.OnValueChanged.AddListener(OnMuteChanged);
        }

        private void Update()
        {
            if (m_progress && m_progress.Dragging)
            {
                m_audioManager.PlayTime = m_progress.Value;
            }
        }

        private void OnPlayProgressChanged(MediaProgressState progress)
        {
            if (!m_progress || m_progress.Dragging) return;
            m_progress.State = progress;
        }

        private void OnPlayStateChanged(AudioPlayState state)
        {
            m_btn_play?.SetState(state.Playing);
            m_btn_mute?.SetState(state.Mute);
            m_loadingMask?.SetActive(state.Loading);

            if (m_sld_volume)
            {
                m_sld_volume.value = (state.Volume);
            }
        }

        private void OnDragStateChanged(bool dragging)
        {
            m_audioManager.Manual = dragging;
        }

        private void OnPlayChanged(bool value)
        {
            m_audioManager.Playing = value;
        }

        private void OnVolumeChanged(float value)
        {
            m_audioManager.Volume = value;
        }

        private void OnMuteChanged(bool mute)
        {
            m_audioManager.Mute = mute;
        }
    }
}
