using NaughtyAttributes;
using NonsensicalKit.Core;
using NonsensicalKit.Tools.LogicNodeTreeSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuElementStatusChange : MonoBehaviour
{
    [InfoBox("通过PublishCommand方法发送以menuElementClick为主题,m_command为参数的Button点击事件",EInfoBoxType.Normal)]
    [SerializeField,Tooltip("同时改变本按钮状态")] private bool m_enableStateSwitch;
    [SerializeField] private string m_command;
    [SerializeField] private bool m_autoReset;

    [SerializeField, Tooltip(" 进入到该节点时会触发OnRest事件"), ShowIf("m_autoReset")]
    private string m_autoResetCommand;

    [SerializeField] private Image m_image;
    [SerializeField] private TMP_Text m_text;

    [SerializeField] private Sprite[] m_imagePool;
    [SerializeField] private string[] m_textPool;

    [SerializeField] private string[] m_commandPool;

    [SerializeField, ShowIf("m_autoReset")]
    private UnityEvent m_onResetEvent;


    private int _index = 0;

    private void Awake()
    {
        if (m_autoReset)
        {
            IOCC.Subscribe((int)LogicNodeEnum.NodeEnter, m_autoResetCommand, OnRest);
        }
    }
    public void InitState(int poolIndex, int index)
    {
        this._index = index;
        m_image.sprite = m_imagePool[poolIndex];
        m_text.text = m_textPool[poolIndex];

        m_command = m_commandPool[poolIndex];
    }
    
    public void PublishCommand()
    {
        IOCC.Publish("menuElementClick", m_command);
        if (m_enableStateSwitch)
        {
            StateSwitch();
        }
    }

    private void StateSwitch()
    {
        _index = ++_index % m_imagePool.Length;
        m_image.sprite = m_imagePool[_index];
        m_text.text = m_textPool[_index];

        m_command = m_commandPool[_index];
    }
    public void OnRest()
    {
        if (m_autoReset == false) return;
        _index = 0;
        m_image.sprite = m_imagePool[0];
        m_text.text = m_textPool[0];
        m_command = m_commandPool[0];
        m_onResetEvent?.Invoke();
    }
}
