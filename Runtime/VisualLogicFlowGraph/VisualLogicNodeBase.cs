using NonsensicalKit.Core;
using NonsensicalKit.Tools;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.VisualLogicGraph
{
    /// <summary>
    /// 逻辑节点基类
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class VisualLogicNodeBase : NonsensicalMono, IBeginDragHandler, IDragHandler
    {
        /// <summary>
        /// 逻辑节点类型
        /// </summary>
        [SerializeField] private string m_type;
        /// <summary>
        /// 是否可以编辑名称
        /// </summary>
        [SerializeField] private bool m_canEditName;

        /// <summary>
        /// 删除按钮
        /// </summary>
        [SerializeField] private Button m_btn_delete;
        /// <summary>
        /// 名称输入框
        /// </summary>
        [SerializeField] private TMP_InputField m_ipf_nodeName;

        /// <summary>
        /// 输入点位
        /// </summary>
        [SerializeField] private List<VisualLogicPointBase> m_inputs;
        /// <summary>
        /// 输出点位
        /// </summary>
        [SerializeField] private List<VisualLogicPointBase> m_outputs;


        public string Type => m_type;
        /// <summary>
        /// 输入点位
        /// </summary>
        public List<VisualLogicPointBase> Inputs => m_inputs;
        /// <summary>
        /// 输出点位
        /// </summary>
        public List<VisualLogicPointBase> Outputs => m_outputs;
        /// <summary>
        /// 存档信息类
        /// </summary>
        public IVisualLogicNodeInfo Info { get; set; }

        /// <summary>
        /// 自身RectTransform
        /// </summary>
        private RectTransform _selfRect;
        /// <summary>
        /// 开始拖拽时鼠标和中心点的偏移量
        /// </summary>
        private Vector3 _startOffset;

        protected virtual void Awake()
        {
            foreach (var item in Inputs)
            {
                item.BelongNode = this;
            }
            foreach (var item in Outputs)
            {
                item.BelongNode = this;
            }

            _selfRect = GetComponent<RectTransform>();
            m_ipf_nodeName.onEndEdit.AddListener(OnNameEndEdit);
            m_btn_delete.onClick.AddListener(OnDelete);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector3 pos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_selfRect, eventData.position, eventData.enterEventCamera, out pos);
            _startOffset = _selfRect.position - pos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //拖拽时移动节点位置，并更新信息类存储的位置信息
            Vector3 pos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_selfRect, eventData.position, eventData.enterEventCamera, out pos);
            _selfRect.position = pos + _startOffset;
            Info.X = _selfRect.anchoredPosition.x;
            Info.Y = _selfRect.anchoredPosition.y;
        }

        /// <summary>
        /// 创建新节点的信息类
        /// </summary>
        /// <param name="createFunc"></param>
        public void NewInfo(Func<string, IVisualLogicNodeInfo> createFunc)
        {
            Info = createFunc(m_type);
            if (string.IsNullOrEmpty(Info.Name))
            {
                Info.Name = "新节点";
            }
            if (string.IsNullOrEmpty(Info.ID))
            {
                Info.ID = Guid.NewGuid().ToString();
            }
            Info.Type = m_type;
            Info.CanEditName = m_canEditName;
            Info.InputPoints = new List<string>();
            foreach (var item in m_inputs)
            {
                Info.InputPoints.Add(item.Info.ID);
            }
            Info.OutputPoints = new List<string>();
            foreach (var item in m_outputs)
            {
                Info.OutputPoints.Add(item.Info.ID);
            }
            AfterNewInfo();
        }

        /// <summary>
        /// 同步显示状态到信息类状态
        /// </summary>
        public void UpdateState()
        {
            m_ipf_nodeName.text = Info.Name;
            m_ipf_nodeName.interactable = Info.CanEditName;
            m_ipf_nodeName.targetGraphic.raycastTarget = Info.CanEditName;
            _selfRect.anchoredPosition = new Vector2(Info.X, Info.Y);
            AfterUpdateState();
        }

        /// <summary>
        /// 被回收时清理点位的信息
        /// </summary>
        public void OnStore()
        {
            foreach (var item in m_inputs)
            {
                item.OnStore();
            }
            foreach (var item in m_outputs)
            {
                item.OnStore();
            }
        }

        /// <summary>
        /// 点位更新后（连接或断开时）
        /// </summary>
        public virtual void AfterPointChanged(VisualLogicPointBase pointBase, bool isConnect)
        {

        }

        /// <summary>
        /// 创建新节点信息后，子类在此初始化其特殊信息
        /// </summary>
        protected virtual void AfterNewInfo()
        {

        }

        /// <summary>
        /// 更新节点状态后
        /// </summary>
        protected virtual void AfterUpdateState()
        {

        }

        /// <summary>
        /// 当删除按钮点击时发送删除事件
        /// </summary>
        private void OnDelete()
        {
            Publish(VisualLogicEnum.DeleteNode, Info.ID);
        }

        /// <summary>
        /// 名称被编辑时更新信息类
        /// </summary>
        /// <param name="value"></param>
        private void OnNameEndEdit(string value)
        {
            Info.Name = value;
        }
    }

    /// <summary>
    /// 节点信息类接口
    /// </summary>
    public interface IVisualLogicNodeInfo
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 节点类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 节点图形的X坐标
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// 节点图形的Y坐标
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// 是否可以编辑名称
        /// </summary>
        public bool CanEditName { get; set; }
        /// <summary>
        /// 节点ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 所有输入点位的ID
        /// </summary>
        public List<string> InputPoints { get; set; }
        /// <summary>
        /// 所有输出点位的ID
        /// </summary>
        public List<string> OutputPoints { get; set; }

        /// <summary>
        /// 克隆方法
        /// </summary>
        /// <returns></returns>
        public IVisualLogicNodeInfo Clone();
    }

    /// <summary>
    /// 基础节点信息类
    /// 用于示例
    /// </summary>
    [System.Serializable]
    public class BasicVisualLogicNodeInfo : IVisualLogicNodeInfo
    {
        public string Name { get { return m_name; } set { m_name = value; } }
        [SerializeField] private string m_name;
        public string Type { get { return m_type; } set { m_type = value; } }
        [SerializeField] private string m_type;
        public float X { get { return m_x; } set { m_x = value; } }
        [SerializeField] private float m_x;
        public float Y { get { return m_y; } set { m_y = value; } }
        [SerializeField] private float m_y;
        public bool CanEditName { get { return m_canEditName; } set { m_canEditName = value; } }
        [SerializeField] private bool m_canEditName;
        public string ID { get { return m_id; } set { m_id = value; } }
        [SerializeField] private string m_id;
        public List<string> InputPoints { get { return m_inputPoints; } set { m_inputPoints = value; } }
        [SerializeField] private List<string> m_inputPoints;
        public List<string> OutputPoints { get { return m_outputPoints; } set { m_outputPoints = value; } }
        [SerializeField] private List<string> m_outputPoints;

        public IVisualLogicNodeInfo Clone()
        {
            return this.CloneByJson();
        }
    }
}
