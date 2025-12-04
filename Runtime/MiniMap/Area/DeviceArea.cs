using System;
using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeviceArea : AreaInput
{
    [SerializeField] private PointerEventData.InputButton m_clickButton = PointerEventData.InputButton.Left;
    [SerializeField] private Image m_image;
    [SerializeField] private Color m_defaultColor;
    [SerializeField] private Color m_hoverColor;

    protected override void OnClick(PointerEventData eventData)
    {
        if (eventData.button != m_clickButton) return;
        Publish("minimapMoveAreaClick", this, eventData);
    }

    protected override void OnHover(PointerEventData eventData)
    {
         m_image?.DoColor(m_hoverColor, 0.3f); 
    }

    protected override void OnExit(PointerEventData eventData)
    {
        m_image?.DoColor(m_defaultColor, 0.3f);
    }

    private void Awake()
    {
        if (m_image != null)
            m_image.color = m_defaultColor;
    }
}
