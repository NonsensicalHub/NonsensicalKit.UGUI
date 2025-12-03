using System;
using System.Collections.Generic;
using NaughtyAttributes;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Events;

public class MenuElementStatusReceive : NonsensicalMono
{
    [InfoBox("Button事件接收器,接受主题为m_receiveCommand的事件" +
             "通过m_enableCommand控制是否启用带参事件",EInfoBoxType.Normal)]
    [SerializeField] private string m_receiveCommand = "menuElementClick";

    [SerializeField, Tooltip("当通过m_receiveCommand传入的参数等于Command时启用MenuCommandEvent")]
    private bool m_enableCommand;

    [SerializeField, ShowIf("enableCommand")]
    private MenuCommandEvent[] m_menuCommandEvents;

    private readonly Dictionary<string, MenuCommandEvent> _eventDict = new();

    [SerializeField] private UnityEvent m_onReceive;

    private void Awake()
    {
        if (m_enableCommand)
        {
            Subscribe<string>(m_receiveCommand, OnReceive);
            foreach (var item in m_menuCommandEvents)
            {
                _eventDict.Add(item.m_Command, item);
            }
        }
        else
        {
            Subscribe(m_receiveCommand, OnReceive);
        }
    }

    private void OnReceive()
    {
        m_onReceive?.Invoke();
    }

    private void OnReceive(string obj)
    {
        if (_eventDict.TryGetValue(obj, out var menuCommandEvent))
        {
            menuCommandEvent.OnReceive();
        }
    }
}

[Serializable]
public class MenuCommandEvent
{
    public string m_Command;
    public UnityEvent m_OnReceive;


    public void OnReceive()
    {
        m_OnReceive?.Invoke();
    }
}
