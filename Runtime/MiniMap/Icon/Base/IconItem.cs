using System;
using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 小地图图标基类
/// </summary>
public class IconItem : MonoBehaviour
{
    [SerializeField] private string m_type;
    [SerializeField] private IconItemConfig m_config;
    [SerializeField] protected Image m_icon;

    [FormerlySerializedAs("m_rectTransform")] [SerializeField]
    private RectTransform m_selfRectTransform;

    private Transform _followTarget;
    private string _commandID;

    private void OnValidate()
    {
        m_config.m_Type = m_type;
    }

    private void LateUpdate()
    {
        if (!_followTarget) return;
        var pos = CoordinateTransformation.Instance.WorldToMap(_followTarget.position);
        CoordinateTransformation.Instance.SetCenterPositionToMap(m_selfRectTransform, pos);
        IOCC.PublishWithID("iconItemPositionUpdate", _commandID, pos);
    }

    public void Init(IconItemConfig config)
    {
        SetConfig(config);
        _commandID = m_type.ToString();
    }

    public void Recycling()
    {
        _followTarget = null;
        m_config.Reset();
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    public void ChangeConfig(IconItemConfig config)
    {
        SetConfig(config);
    }

    public void ChangeColor(Color color)
    {
        m_icon.color = color;
        m_config.m_BaseColor = color;
    }

    public virtual void ChangeFollowTarget(Transform target)
    {
        _followTarget = target;
        m_config.m_FollowTarget = target;
    }


    protected virtual void SetConfig(IconItemConfig config)
    {
        m_config = config;
        m_type = config.m_Type;
        m_icon.color = config.m_BaseColor;
        _followTarget = config.m_FollowTarget;
        m_icon.gameObject.SetActive(config.m_Visible);
    }

    public void RefreshIconInMask()
    {
        m_icon.enabled = false;
        m_icon.enabled = true;
    }
}

