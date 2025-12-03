using System.Linq;
using NaughtyAttributes;
using NonsensicalKit.Core;
using NonsensicalKit.Tools.LogicNodeTreeSystem;
using UnityEngine;
using UnityEngine.Events;

public class ButtonStatusChange : NonsensicalMono
{
    [InfoBox("订阅按钮状态改变事件,主要用于按钮图片切换")]
    [SerializeField, Tooltip("订阅参数,根据参数切换按钮的不同显示状态")]
    private string m_subscribeCommand;

    [SerializeField, Tooltip(" 进入到该节点时会触发OnRest事件")]
    private string m_autoResetCommand;

    [SerializeField, Label("按钮状态未选中图片")] private GameObject m_oriObj;
    [SerializeField, Label("按钮状态选中图片")] private GameObject m_selectObj;

    [SerializeField, Tooltip("特殊command,button强制切换为select状态")]
    private string[] m_spOn;

    [SerializeField, Tooltip("特殊command,button强制切换为Unselect状态")]
    private string[] m_spOff;

    [SerializeField] private UnityEvent m_onResetEvent;

    private void Reset()
    {
        m_oriObj = transform.GetChild(0).gameObject;
        m_selectObj = transform.GetChild(1).gameObject;
    }

    private void Awake()
    {
        Subscribe<string>(10001, "buttonStatusChange", ChangeStatus);
        Subscribe((int)LogicNodeEnum.NodeEnter, m_autoResetCommand, OnRest);
    }

    private void OnRest()
    {
        SwitchStatus(false);
        m_onResetEvent?.Invoke();
    }

    private void ChangeStatus(string command)
    {
        SwitchStatus(m_subscribeCommand == command);
        if (m_spOn.Contains(command))
        {
            SwitchStatus(true);
        }

        if (m_spOff.Contains(command))
        {
            SwitchStatus(false);
        }
    }

    private void SwitchStatus(bool target)
    {
        m_oriObj?.SetActive(!target);
        m_selectObj?.SetActive(target);
    }
}
