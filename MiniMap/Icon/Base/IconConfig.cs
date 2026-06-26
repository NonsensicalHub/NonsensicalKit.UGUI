using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class Icon
{
    public string Type;
    [JsonIgnore] public GameObject Prefab;
    public string Path;
}


[Serializable]
public class IconItemConfig
{
    public string m_ID;
    public string m_Name;
    public bool m_Visible = true;
    public string m_Type;
    public Color m_BaseColor;
    public Transform m_FollowTarget;

    public void Reset()
    {
        m_ID = "";
        m_Name = "";
        m_Type = "Other";
        m_BaseColor = Color.white;
        m_FollowTarget = null;
    }
}
