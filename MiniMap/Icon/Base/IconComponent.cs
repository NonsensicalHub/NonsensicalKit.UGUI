using System;
using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Core;
using UnityEngine;

/// <summary>
/// 场景中物体应挂载本脚本,管理图标信息
/// </summary>
public abstract class IconComponent : NonsensicalMono 
{
    [SerializeField] private bool m_autoCreate = true;
    [SerializeField] protected IconItemConfig m_iconItemConfig;

    protected IconItem IconItem;
    
    protected bool IsCreated;
    
    protected virtual void Start()
    {
        if (m_autoCreate)
        {
            if (m_iconItemConfig.m_FollowTarget == null)
            {
                Debug.LogWarning("请设置跟随目标", this.gameObject);
                return;
            }

            Publish("createIcon", m_iconItemConfig);
        }
    }

    public void SetConfig(string id)
    {
        m_iconItemConfig.m_ID = id;
    }

    public void CreateIcon(string id = null, string iconName = null, string type = "MeasurePoint", bool visible = true, Color color = default,
        Transform followTarget = null)
    {
        m_iconItemConfig = new IconItemConfig
        {
            m_ID = id,
            m_Name = iconName,
            m_Type = type,
            m_Visible = visible,
            m_BaseColor = color,
            m_FollowTarget = followTarget
        };
        Publish("createIcon", m_iconItemConfig);
    }

    public void CreateIcon(IconItemConfig iconItemConfig)
    {
        m_iconItemConfig = iconItemConfig;
        IsCreated = true;
        Publish("createIcon", iconItemConfig);
        
    }
    public IconItem CreateIconWithCallback(IconItemConfig iconItemConfig)
    { 
        m_iconItemConfig = iconItemConfig;
        IsCreated = true;
        IconItem= Execute<IconItemConfig,IconItem>("createIcon", iconItemConfig);
        return IconItem;
    }
    

    public virtual void DestroyIcon()
    {
        IsCreated = false;
    }

    public virtual void DestroyIcon(string id)
    {
        IsCreated = false;
    }

    public abstract void ChangeIcon(IconItemConfig iconItemConfig);

    protected bool GetIconItem(out IconItem iconItem)
    {
        throw new InvalidOperationException();
    }
}
public enum IconStatus
{
    Normal,
    Warning,
    Danger
}
