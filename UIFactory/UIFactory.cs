using System;
using System.Collections.Generic;
using NonsensicalKit.Core;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.ObjectPool;
using UnityEngine;

namespace NonsensicalKit.UGUI.UIFactory
{
    [ServicePrefab("Services/UIFactory")]
    public class UIFactory : NonsensicalMono, IMonoService
    {
        [SerializeField] private FactoryUIPrefabSetting[] m_prefabs;
        [SerializeField] private Canvas m_canvas;

        public bool IsReady { get; private set; }

        public Action InitCompleted { get; set; }

        private Dictionary<string, GameObjectPool> _pools;

        private void Awake()
        {
            Init();
            Subscribe<string, object>("OpenUI", OnOpenUI);

            IsReady = true;
            InitCompleted?.Invoke();
        }

        public GameObject OpenUI(string type, object arg)
        {
            if (_pools == null || _pools.Count == 0)
            {
                LogCore.Warning("UIFactory 尚未配置可用 UI 预制体");
                return null;
            }

            if (_pools.ContainsKey(type) == false)
            {
                LogCore.Warning($"未配置类型为{type}的UI");
                return null;
            }

            var v = _pools[type].New();

            IFactoryUI ui = v.GetComponent<IFactoryUI>();
            if (ui == null)
            {
                LogCore.Warning($"类型为{type}的UI实例缺少IFactoryUI实现");
                v.SetActive(false);
                return null;
            }

            ui.SetArg(arg);
            return v;
        }

        private void OnOpenUI(string type, object arg)
        {
            OpenUI(type, arg);
        }

        private void Init()
        {
            _pools = new Dictionary<string, GameObjectPool>();
            if (m_prefabs == null)
            {
                //自动生成时为null
                return;
            }
            foreach (var item in m_prefabs)
            {
                var key = string.IsNullOrEmpty(item.Alias) ? item.Type : item.Alias;
                if (string.IsNullOrEmpty(key))
                {
                    LogCore.Warning("UIFactory 存在空键值配置，已跳过");
                    continue;
                }

                if (_pools.ContainsKey(key))
                {
                    LogCore.Warning($"键值{key} 配置了多次");
                    continue;
                }

                if (item.Prefab == null)
                {
                    LogCore.Warning($"键值为{key}的UI未配置预制体");
                    continue;
                }

                IFactoryUI ui = item.Prefab.GetComponent<IFactoryUI>();
                if (ui == null)
                {
                    LogCore.Warning($"键值为{key}的UI其预制体未挂载实现了IFactoryUI的接口的脚本");
                    continue;
                }

                GameObjectPool newPool = new GameObjectPool(item.Prefab, OnUIReset, OnUIInit, OnUIFirstInit);

                _pools.Add(key, newPool);
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
            if (ui == null)
            {
                LogCore.Warning("UIFactory 创建实例缺少 IFactoryUI，已跳过初始化。");
                return;
            }

            ui.Pool = pool;
            if (m_canvas == null)
            {
                m_canvas = GetComponentInParent<Canvas>(true);
            }

            if (m_canvas != null)
            {
                go.transform.SetParent(m_canvas.transform, false);
            }
            else
            {
                LogCore.Warning("UIFactory 未找到 Canvas，UI 将保留在默认层级。");
            }
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
    [Serializable]
    public class FactoryUIPrefabSetting
    {
        [TypeQualifiedString(typeof(IFactoryUI))]
        public string Type;

        public string Alias;
        public GameObject Prefab;

        public FactoryUIPrefabSetting(string type, GameObject prefab)
        {
            Type = type;
            Alias = null;
            Prefab = prefab;
        }

        public FactoryUIPrefabSetting(string type, string alias, GameObject prefab)
        {
            Type = type;
            Alias = alias;
            Prefab = prefab;
        }
    }

    public interface IFactoryUI
    {
        public void SetArg(object arg);
        public GameObjectPool Pool { get; set; }
    }
}
