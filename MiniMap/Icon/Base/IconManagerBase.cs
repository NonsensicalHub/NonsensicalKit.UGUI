using System;
using System.Collections;
using System.Collections.Generic;
using NonsensicalKit.Core;
using NonsensicalKit.Core.Service.Config;
using UnityEngine;

public class IconManagerBase<T> : NonsensicalMono where T : ConfigData
{
    [SerializeField] protected bool m_needWaitService = true;
    [SerializeField] protected Transform m_iconPool;

    /// <summary>
    /// 测点字典,配置信息 TO 测点 
    /// </summary>
    protected readonly Dictionary<IconItemConfig, IconItem> _iconItemDic = new Dictionary<IconItemConfig, IconItem>();

    protected T _iconInitData; //图标样式配置数据集合

    public static IconManagerBase<T> Instance { get; protected set; }

    protected virtual void Awake()
    {
        m_iconPool ??= transform;
        Subscribe<IconItemConfig>("createIcon", CreateIconCor);
        AddHandler<IconItemConfig, IconItem>("createIcon", CreateIconWithCallback);
    }

    protected override void OnDestroy()
    {
        Instance = null;
    }

    #region 回收图标

    public virtual void StoreIcon(string id)
    {
        throw new InvalidOperationException();
    }

    public virtual void StoreIcon(IconItemConfig iconItem)
    {
        throw new InvalidOperationException();
    }

    #endregion

    #region 获取图标

    public virtual IconItem GetIconItem(IconItemConfig iconItemConfig)
    {
        throw new InvalidOperationException();
    }

    public virtual IconItem GetIconItem(string id)
    {
        throw new InvalidOperationException();
    }

    #endregion

    public virtual void ChangeIcon(IconItemConfig iconItemConfig)
    {
        throw new InvalidOperationException();
    }

    #region 创建图标

    protected virtual void CreateIconCor(IconItemConfig iconItemConfig)
    {
    }

    private IconItem CreateIconWithCallback(IconItemConfig iconItemConfig)
    {
        return CreateIcon(iconItemConfig);
    }

    protected virtual IconItem CreateIcon(IconItemConfig iconItemConfig)
    {
        throw new InvalidOperationException();
    }

    #endregion
}