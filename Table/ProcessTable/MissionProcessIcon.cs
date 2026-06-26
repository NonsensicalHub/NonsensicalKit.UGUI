using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MissionProcessIcon : NonsensicalMono
{
    [FormerlySerializedAs("m_btn_Icon")] [FormerlySerializedAs("btn_Icon")] [SerializeField] private Button m_btn_icon;
    [FormerlySerializedAs("idle")] [SerializeField] private GameObject m_idle;
    [FormerlySerializedAs("transferring")] [SerializeField] private GameObject m_transferring;

    private void Awake()
    {
        m_btn_icon.onClick.AddListener(OnButtonClick );
        IOCC.AddListener<bool>("missionProcessState",OnMissionProcessStateChanged);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        IOCC.RemoveListener<bool>("missionProcessState",OnMissionProcessStateChanged);
    }

    private void OnMissionProcessStateChanged(bool state)
    {
        if (state)
        {
            m_idle.gameObject.SetActive(false);
            m_transferring.gameObject.SetActive(true);
        }
        else
        {
            m_idle.gameObject.SetActive(true);
            m_transferring.gameObject.SetActive(false);
        }
    }

    private void OnButtonClick()
    {
        Publish("SwitchMissionProcessTableWindow");
    }
}
