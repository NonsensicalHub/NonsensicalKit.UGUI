using NaughtyAttributes;
using NonsensicalKit.Core;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Tools.LogicNodeTreeSystem;
using UnityEngine;

public class ButtonEventPublisher : NonsensicalMono
{
    [SerializeField, Tooltip("是否同时发送按钮状态切换事件")]
    private bool m_enableSendButtonStatus;

    [SerializeField, Tooltip("是否同时发送节点切换事件")]
    private bool m_enableSwitchNode;

    [SerializeField] private string m_command;

    [SerializeField, ShowIf("m_enableSwitchNode")]
    private string m_targetNodeID;

    private string _buffer;

    private LogicNodeManager _manager;

    private void Awake()
    {
        ServiceCore.SafeGet<LogicNodeManager>(OnGetManager);
        if (string.IsNullOrEmpty(m_targetNodeID))
        {
            LogCore.Debug($"{nameof(LogicNodeSwitcher)}未设置ID", this);
        }
    }

    public void PublishCommand()
    {
        if (string.IsNullOrEmpty(m_command) == false)
        {
            Publish(m_command);
        }

        if (m_enableSendButtonStatus)
        {
            PublishWithID<string>(10001, "buttonStatusChange", m_command);
        }

        if (m_enableSwitchNode)
        {
            Switch();
        }
    }

    public void PublishCommand(string command)
    {
        Publish<string>(m_command, command);
        if (m_enableSendButtonStatus)
        {
            PublishWithID<string>(10001, "buttonStatusChange", m_command);
        }

        if (m_enableSwitchNode)
        {
            Switch();
        }
    }


    public void Switch()
    {
        if (_manager != null)
        {
            _manager.SwitchNode(m_targetNodeID);
        }
        else
        {
            _buffer = m_targetNodeID;
        }
    }

    private void OnGetManager(LogicNodeManager manager)
    {
        _manager = manager;
        if (string.IsNullOrEmpty(_buffer) == false)
        {
            _manager.SwitchNode(_buffer);
        }
    }
}
