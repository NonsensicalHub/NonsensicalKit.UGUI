using NonsensicalKit.Core;
using NonsensicalKit.Core.Table;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.UGUI.VisualLogicGraph
{
    /// <summary>
    /// 可视化逻辑图形
    /// 此节点上的RectTransform代表了可视区域
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class VisualLogicGraph : NonsensicalMono
    {
        /// <summary>
        /// 类型字符串匹配节点预制体
        /// 如果需要添加新的节点类型，只需要新建节点预制体，在预制体中进行配置，最后添加到此数组即可
        /// </summary>
        [SerializeField] private VisualNodePrefabsSetting[] m_nodePrefabs;
        /// <summary>
        /// 黑板管理类
        /// </summary>
        [SerializeField] private VisualLogicBoard m_board;
        /// <summary>
        /// 连接线预制体
        /// </summary>
        [SerializeField] private VisualLogicLine m_linePrefab;
        /// <summary>
        /// 放置生成的连接线的父物体，保证线的UI显示在其他元素前面
        /// </summary>
        [SerializeField] private Transform m_lineSpace;
        /// <summary>
        /// 对象池缓存父节点，应处于非激活状态，保证被回收的对象池对象休眠
        /// </summary>
        [SerializeField] private Transform m_pool;
        /// <summary>
        /// 多级菜单
        /// </summary>
        [SerializeField] private MultilevelMenu m_menu;
        /// <summary>
        /// 是否不断的检测尺寸是否改变，检测到改变时会更新黑板的可视化区域大小
        /// </summary>
        [SerializeField] private bool m_checkSize;

        /// <summary>
        /// 当前激活的逻辑节点，键为节点id
        /// </summary>
        private Dictionary<string, VisualLogicNodeBase> _nodes = new Dictionary<string, VisualLogicNodeBase>();
        /// <summary>
        /// 连接线对象池
        /// </summary>
        private ComponentPool_MK2<VisualLogicLine> _linePool;
        /// <summary>
        /// 每种逻辑节点的对象池
        /// </summary>
        private Dictionary<string, ComponentPool_MK2<VisualLogicNodeBase>> _pools = new Dictionary<string, ComponentPool_MK2<VisualLogicNodeBase>>();
        /// <summary>
        /// 创建新的逻辑节点信息的方法
        /// </summary>
        private Func<string, IVisualLogicNodeInfo> _createNodeInfo;
        /// <summary>
        /// 创建新的节点信息的方法
        /// </summary>
        private Func<string, IVisualLogicPointInfo> _createPointInfo;
        /// <summary>
        /// 自身的RectTransform
        /// </summary>
        private RectTransform _selfRect;
        /// <summary>
        /// 是否初始化过创建方法
        /// </summary>
        private bool _initCreateFunc;

        private void Awake()
        {
            _selfRect = gameObject.GetComponent<RectTransform>();

            var menuInfos = new List<MultilevelMenuInfo>();
            foreach (var node in m_nodePrefabs)
            {
                node.Type = node.Prefab.Type;
                if (_pools.ContainsKey(node.Type))
                {
                    Debug.LogWarning("类型重复：" + node.Type);
                    continue;
                }
                _pools.Add(node.Type, new ComponentPool_MK2<VisualLogicNodeBase>(node.Prefab, OnNodeStore, OnNodeInit));
                menuInfos.Add(new MultilevelMenuInfo(node.CreatePath, OnCreateNew));
            }

            _linePool = new ComponentPool_MK2<VisualLogicLine>(m_linePrefab, OnLineStore, OnLineInit);

            m_menu.Init(menuInfos);

            m_board.Init(_selfRect.rect.size, m_menu);

            Subscribe<string>(VisualLogicEnum.DeleteNode, DeleteNode);
        }

        private void OnEnable()
        {
            if (m_checkSize)
            {
                StartCoroutine(CorCheckSize());
            }
        }

        /// <summary>
        /// 初始化创建节点的方法
        /// </summary>
        /// <param name="createNodeInfo"></param>
        /// <param name="createPointInfo"></param>
        public void Init(Func<string, IVisualLogicNodeInfo> createNodeInfo, Func<string, IVisualLogicPointInfo> createPointInfo)
        {
            if (createNodeInfo == null || createPointInfo == null)
            {
                Debug.LogError("创建方法为空");
                return;
            }
            _createNodeInfo = createNodeInfo;
            _createPointInfo = createPointInfo;

            _initCreateFunc = true;
        }

        /// <summary>
        /// 更新逻辑节点状态
        /// </summary>
        /// <param name="id"></param>
        public void UpdateNodeState(string id)
        {
            if (_nodes.ContainsKey(id))
            {
                _nodes[id].UpdateState();
            }
        }

        /// <summary>
        /// 存档，返回存储信息类
        /// </summary>
        /// <typeparam name="Save"></typeparam>
        /// <typeparam name="Node"></typeparam>
        /// <typeparam name="Point"></typeparam>
        /// <returns></returns>
        public Save Save<Save, Node, Point>() where Save : IVisualSaveData<Node, Point>, new() where Node : class, IVisualLogicNodeInfo where Point : class, IVisualLogicPointInfo
        {
            Save data = new Save();
            data.NodeInfos = new List<Node>();
            data.PointInfos = new List<Point>();

            foreach (VisualLogicNodeBase node in _nodes.Values)
            {
                data.NodeInfos.Add(node.Info.Clone() as Node);
                foreach (var item in node.Inputs)
                {
                    data.PointInfos.Add(item.Info.Clone() as Point);
                }
                foreach (var item in node.Outputs)
                {
                    data.PointInfos.Add(item.Info.Clone() as Point);
                }
            }

            var size = m_board.GetSize();
            data.BoardWidth = size.x;
            data.BoardHeight = size.y;
            return data;
        }

        /// <summary>
        /// 读档，输入存储信息类，更新黑板和逻辑节点状态
        /// </summary>
        /// <typeparam name="Save"></typeparam>
        /// <typeparam name="Node"></typeparam>
        /// <typeparam name="Point"></typeparam>
        /// <param name="data"></param>
        public void Load<Save, Node, Point>(Save data) where Save : class, IVisualSaveData<Node, Point> where Node : class, IVisualLogicNodeInfo where Point : class, IVisualLogicPointInfo
        {
            Clear();
            data = data.Clone() as Save;
            m_board.SetSize(data.BoardWidth, data.BoardHeight);

            Dictionary<string, IVisualLogicPointInfo> pointBuffer = new Dictionary<string, IVisualLogicPointInfo>();
            foreach (var item in data.PointInfos)
            {
                if (pointBuffer.ContainsKey(item.ID))
                {
                    Debug.LogWarning("存档点位ID重复：" + item.ID);
                    continue;
                }
                pointBuffer.Add(item.ID, item);
            }

            foreach (var item in data.NodeInfos)
            {
                if (_pools.ContainsKey(item.Type) == false)
                {
                    Debug.LogWarning("存档中存在未配置的节点类型：" + item.Type);
                    continue;
                }
                var visualLogicNodeBase = _pools[item.Type].New();
                NodeInit(visualLogicNodeBase, item);

                if (item.InputPoints.Count != visualLogicNodeBase.Inputs.Count)
                {
                    Debug.LogWarning("存档输入点位长度不匹配：" + item.Type);
                }
                int inputMin = Mathf.Min(item.InputPoints.Count, visualLogicNodeBase.Inputs.Count);
                int inputIndex = 0;
                for (; inputIndex < inputMin; inputIndex++)
                {
                    if (pointBuffer.ContainsKey(item.InputPoints[inputIndex]) == false)
                    {
                        Debug.LogWarning("存档中存在无法匹配的点位ID：" + item.InputPoints[inputIndex]);

                        NewPointInit(visualLogicNodeBase.Inputs[inputIndex]);
                        continue;
                    }
                    PointInit(visualLogicNodeBase.Inputs[inputIndex], pointBuffer[item.InputPoints[inputIndex]]);
                }

                for (; inputIndex < visualLogicNodeBase.Inputs.Count; inputIndex++)
                {
                    NewPointInit(visualLogicNodeBase.Inputs[inputIndex]);
                }

                if (item.OutputPoints.Count != visualLogicNodeBase.Outputs.Count)
                {
                    Debug.LogWarning("存档输出点位长度不匹配：" + item.Type);
                }
                int outputMin = Mathf.Min(item.OutputPoints.Count, visualLogicNodeBase.Outputs.Count);
                int outputIndex = 0;
                for (; outputIndex < outputMin; outputIndex++)
                {
                    if (pointBuffer.ContainsKey(item.OutputPoints[outputIndex]) == false)
                    {
                        Debug.LogWarning("存档中存在无法匹配的点位ID：" + item.OutputPoints[outputIndex]);

                        NewPointInit(visualLogicNodeBase.Outputs[outputIndex]);
                        continue;
                    }
                    PointInit(visualLogicNodeBase.Outputs[outputIndex], pointBuffer[item.OutputPoints[outputIndex]]);
                }

                for (; outputIndex < visualLogicNodeBase.Outputs.Count; outputIndex++)
                {
                    NewPointInit(visualLogicNodeBase.Outputs[outputIndex]);
                }
            }
        }

        /// <summary>
        /// 添加新节点
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string AddNewNode(string type)
        {
            if (!_initCreateFunc)
            {
                Debug.LogError("尚未进行初始化");
                return null;
            }
            if (_pools.ContainsKey(type) == false)
            {
                Debug.LogWarning($"未找到类型为{type}的节点");
                return null;
            }

            var newNode = _pools[type].New();
            NewNodeInit(newNode);
            return newNode.Info.ID;
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="id"></param>
        public void DeleteNode(string id)
        {
            if (_nodes.ContainsKey(id))
            {
                var v = _nodes[id];
                _nodes.Remove(id);
                foreach (var item in v.Inputs)
                {
                    while (item.ConnectLines.Count > 0)
                    {
                        //CutIt方法会从ConnectLine链表中Remove掉被切断的线
                        //所以一直切断第零条线即可
                        item.ConnectLines[0].CutIt();
                    }
                }
                foreach (var item in v.Outputs)
                {
                    while (item.ConnectLines.Count > 0)
                    {
                        item.ConnectLines[0].CutIt();
                    }
                }
                _pools[v.Info.Type].Store(v);
            }
        }

        /// <summary>
        /// 清空所有节点
        /// </summary>
        public void Clear()
        {
            _linePool.Clear();
            _nodes.Clear();
            foreach (var item in _pools)
            {
                item.Value.Clear();
            }
        }

        /// <summary>
        /// 检测尺寸改变协程
        /// </summary>
        /// <returns></returns>
        private IEnumerator CorCheckSize()
        {
            Vector2 lastSize = _selfRect.rect.size;
            while (true)
            {
                yield return null;
                if (lastSize != _selfRect.rect.size)
                {
                    lastSize = _selfRect.rect.size;
                    m_board.ViewportResize(lastSize);
                }
            }
        }

        /// <summary>
        /// 右键菜单创建方法
        /// </summary>
        /// <param name="context"></param>
        private void OnCreateNew(MultilevelContext context)
        {
            foreach (var item in m_nodePrefabs)
            {
                if (item.CreatePath == context.Path)
                {
                    AddNewNode(item.Type);
                    break;
                }
            }
        }

        /// <summary>
        /// 创建新逻辑节点的初始化
        /// </summary>
        /// <param name="node"></param>
        private void NewNodeInit(VisualLogicNodeBase node)
        {
            foreach (var item in node.Inputs)
            {
                NewPointInit(item);
            }
            foreach (var item in node.Outputs)
            {
                NewPointInit(item);
            }

            node.NewInfo(_createNodeInfo);
            node.UpdateState();
            _nodes.Add(node.Info.ID, node);

            Vector3 temp = IOCC.Get<Vector3>(VisualLogicEnum.CreatPos);//将新生成的节点移动至鼠标位置
            node.transform.position = temp;
        }

        /// <summary>
        /// 加载存档逻辑节点的初始化
        /// </summary>
        /// <param name="node"></param>
        /// <param name="info"></param>
        private void NodeInit(VisualLogicNodeBase node, IVisualLogicNodeInfo info)
        {
            node.Info = info;
            node.UpdateState();
            _nodes.Add(node.Info.ID, node);
        }

        /// <summary>
        /// 创建新点位的初始化
        /// </summary>
        /// <param name="point"></param>
        private void NewPointInit(VisualLogicPointBase point)
        {
            point.NewInfo(_createPointInfo);
            point.LinePool = _linePool;
            point.UpdateState();
        }

        /// <summary>
        /// 加载存档点位的初始化
        /// </summary>
        /// <param name="point"></param>
        /// <param name="info"></param>
        private void PointInit(VisualLogicPointBase point, IVisualLogicPointInfo info)
        {
            point.Info = info;
            point.LinePool = _linePool;
            point.UpdateState();
        }

        /// <summary>
        /// 节点对象池的存储方法
        /// </summary>
        /// <param name="node"></param>
        private void OnNodeStore(VisualLogicNodeBase node)
        {
            node.OnStore();
            node.gameObject.SetActive(false);
            node.transform.SetParent(m_pool, false);
        }

        /// <summary>
        /// 节点对象池的初始化方法
        /// </summary>
        /// <param name="node"></param>
        private void OnNodeInit(VisualLogicNodeBase node)
        {
            node.gameObject.SetActive(true);
            node.transform.SetParent(m_board.transform, false);
        }

        /// <summary>
        /// 连接线对象池的存储方法
        /// </summary>
        /// <param name="line"></param>
        private void OnLineStore(VisualLogicLine line)
        {
            line.OnStore();
            line.gameObject.SetActive(false);
            line.transform.SetParent(m_pool, false);
        }

        /// <summary>
        /// 连接线对象池的初始化对象
        /// </summary>
        /// <param name="line"></param>
        private void OnLineInit(VisualLogicLine line)
        {
            line.gameObject.SetActive(true);
            line.transform.SetParent(m_lineSpace, false);
        }
    }

    /// <summary>
    /// 配置节点类型的创建路径和预制体
    /// </summary>
    [System.Serializable]
    public class VisualNodePrefabsSetting
    {
        /// <summary>
        /// 节点预制体
        /// </summary>
        public VisualLogicNodeBase Prefab;
        /// <summary>
        /// 在右键菜单里的创建路径
        /// </summary>
        public string CreatePath;

        public string Type { get; set; }
    }

    /// <summary>
    /// 存档类接口
    /// </summary>
    /// <typeparam name="NodeInfo"></typeparam>
    /// <typeparam name="PointInfo"></typeparam>
    public interface IVisualSaveData<NodeInfo, PointInfo> where NodeInfo : IVisualLogicNodeInfo where PointInfo : IVisualLogicPointInfo
    {
        /// <summary>
        /// 所有节点信息类
        /// </summary>
        public List<NodeInfo> NodeInfos { get; set; }
        /// <summary>
        /// 所有点位信息类
        /// </summary>
        public List<PointInfo> PointInfos { get; set; }

        /// <summary>
        /// 黑板宽度
        /// </summary>
        public float BoardWidth { get; set; }
        /// <summary>
        /// 黑板高度
        /// </summary>
        public float BoardHeight { get; set; }

        /// <summary>
        /// 克隆方法
        /// </summary>
        /// <returns></returns>
        public IVisualSaveData<NodeInfo, PointInfo> Clone();
    }

    /// <summary>
    /// 基础存档类
    /// 用于示例
    /// </summary>
    [System.Serializable]
    public class BasicVisualSaveData : IVisualSaveData<BasicVisualLogicNodeInfo, BasicVisualLogicPointInfo>
    {
        public List<BasicVisualLogicNodeInfo> NodeInfos { get; set; }
        public List<BasicVisualLogicPointInfo> PointInfos { get; set; }

        public float BoardWidth { get; set; }
        public float BoardHeight { get; set; }

        public IVisualSaveData<BasicVisualLogicNodeInfo, BasicVisualLogicPointInfo> Clone()
        {
            return this.CloneByJson();
        }
    }
}
