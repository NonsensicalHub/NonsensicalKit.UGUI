using NonsensicalKit.Core;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.ObjectPool;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.UGUI.UIFactory
{
    [ServicePrefab("Services/UIFactory")]
    public class UIFactory : NonsensicalMono, IMonoService
    {
        [SerializeField] private FactoryUIPrefabSetting[] m_prefabs;
        [SerializeField] private Canvas m_canvas;

        public bool IsReady { get; set; }

        public Action InitCompleted { get; set; }

        private Dictionary<string, GameObjectPool> pools;

        private void Awake()
        {
            Init();
            Subscribe<string, object>("OpenUI", OnOpenUI);

            IsReady = true;
            InitCompleted?.Invoke();
        }
        public GameObject OpenUI(string type, object arg)
        {
            if (pools.ContainsKey(type) == false)
            {
                LogCore.Warning($"未配置类型为{type}的UI");
                return null;
            }
            var v = pools[type].New();

            IFactoryUI ui = v.GetComponent<IFactoryUI>();
            ui.SetArg(arg);
            return v;
        }

        private void OnOpenUI(string type, object arg)
        {
            if (pools.ContainsKey(type) == false)
            {
                LogCore.Warning($"未配置类型为{type}的UI");
                return;
            }
            var v = pools[type].New();

            IFactoryUI ui = v.GetComponent<IFactoryUI>();
            ui.SetArg(arg);
        }

        private void Init()
        {
            foreach (var item in m_prefabs)
            {
                if (pools.ContainsKey(item.Type))
                {
                    LogCore.Warning($"UI类型:{item.Type} 配置了多次");
                    return;
                }
                if (item.Prefab == null)
                {
                    LogCore.Warning($"类型为{item.Type}的UI未配置预制体");
                    return;
                }
                IFactoryUI ui = item.Prefab.GetComponent<IFactoryUI>();
                if (ui == null)
                {
                    LogCore.Warning($"类型为{item.Type}的UI其预制体未挂载实现了IFactoryUI的接口");
                    return;
                }

                GameObjectPool newPool = new GameObjectPool(item.Prefab, OnUIReset, OnUIInit, OnUIFirstInit);
            }
        }

        private void OnUIReset(GameObject go)
        {
            go.SetActive(false);
        }

        private void OnUIInit(GameObject go)
        {
            go.SetActive(true);
        }

        private void OnUIFirstInit(GameObjectPool pool, GameObject go)
        {
            go.SetActive(false);
            IFactoryUI ui = go.GetComponent<IFactoryUI>();
            ui.Pool = pool;
            go.transform.SetParent(m_canvas.transform);
        }

        private void Reset()
        {
            var allFactoryUI = ReflectionTool.GetConcreteTypes<IFactoryUI>();

            int uiCount = allFactoryUI.Count;
            m_prefabs = new FactoryUIPrefabSetting[uiCount];
            for (int i = 0; i < uiCount; i++)
            {
                m_prefabs[i] = new FactoryUIPrefabSetting(allFactoryUI[i].Name, null);
            }
        }
    }

    /// <summary>
    /// 配置节点类型的创建路径和预制体
    /// </summary>
    [System.Serializable]
    public class FactoryUIPrefabSetting
    {
        [TypeQualifiedString(typeof(IFactoryUI))]
        public string Type;
        public GameObject Prefab;

        public FactoryUIPrefabSetting(string type, GameObject prefab)
        {
            Type = type;
            Prefab = prefab;
        }
    }

    public interface IFactoryUI
    {
        public void SetArg(object arg);
        public GameObjectPool Pool { get; set; }
    }
}
