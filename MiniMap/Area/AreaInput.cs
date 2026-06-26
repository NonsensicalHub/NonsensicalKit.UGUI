using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class AreaInput : NonsensicalMono, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHover(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnExit(eventData);
    }

    protected virtual void OnClick(PointerEventData eventData)
    {
    }

    protected virtual void OnHover(PointerEventData eventData)
    {
    }

    protected virtual void OnExit(PointerEventData eventData)
    {
    }
}
