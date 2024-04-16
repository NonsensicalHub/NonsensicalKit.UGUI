using NonsensicalKit.Core;
using NonsensicalKit.Tools;
using NonsensicalKit.Tools.ObjectPool;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.UGUI.VisualLogicGraph
{
    /// <summary>
    /// 逻辑点位积累
    /// </summary>
    public class VisualLogicPointBase : NonsensicalMono, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        /// <summary>
        /// 用于在加载存档时为连接线提供信息
        /// </summary>
        private static Dictionary<string, VisualLogicPointBase> Points = new Dictionary<string, VisualLogicPointBase>();
        /// <summary>
        /// 用于缓存待连接的点位
        /// </summary>
        private static Dictionary<string, List<VisualLogicPointBase>> Waiting = new Dictionary<string, List<VisualLogicPointBase>>();

        /// <summary>
        /// 点位类型
        /// </summary>
        [SerializeField] private string m_type;
        /// <summary>
        /// 是否允许连接多个点位
        /// </summary>
        [SerializeField] private bool m_allowMulit;
        /// <summary>
        /// 是否是输入点位，为否时代表为输出点位
        /// </summary>
        [SerializeField] private bool m_isInput;

        /// <summary>
        /// 点位信息类
        /// </summary>
        public IVisualLogicPointInfo Info { get; set; }
        /// <summary>
        /// 所属逻辑节点
        /// </summary>
        public VisualLogicNodeBase BelongNode { get; set; }
        /// <summary>
        /// 连接线对象池
        /// </summary>
        public ComponentPool_MK2<VisualLogicLine> LinePool { protected get; set; }
        /// <summary>
        /// 此点位的所有连接线
        /// </summary>
        public List<VisualLogicLine> ConnectLines { get => _connectLines; set { _connectLines = value; } }
        /// <summary>
        /// 此点位的所有连接线
        /// </summary>
        private List<VisualLogicLine> _connectLines = new List<VisualLogicLine>();
        /// <summary>
        /// 当前连接中的线
        /// </summary>
        private VisualLogicLine _flyLine;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Info != null)
            {
                Points.Remove(Info.ID);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IOCC.Set<VisualLogicPointBase>(VisualLogicEnum.ConnectingPoint, this);

            _flyLine = LinePool.New();
            _flyLine.SetObject(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            //必须实现IDragHandler接口，否则不会触发OnBeginDrag接口的方法
        }

        public void OnDrop(PointerEventData eventData)
        {
            var startPoint = IOCC.Get<VisualLogicPointBase>(VisualLogicEnum.ConnectingPoint);
            if (startPoint != null && startPoint != this)
            {
                if (BelongNode != startPoint.BelongNode)
                {
                    if (startPoint.m_isInput != m_isInput)
                    {
                        //由输出点位进行连线，且只有输出点位的信息类会存储连线信息
                        VisualLogicPointBase output = m_isInput ? startPoint : this;
                        output.Connect(m_isInput ? this : startPoint);
                    }
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //OnDrop比OnEndDrag先触发，所以如果正常连线，会先触发OnDrop方法进行连线，随后触发OnEndDrag清空信息
            IOCC.Set<VisualLogicPointBase>(VisualLogicEnum.ConnectingPoint, null);
            if (_flyLine != null)
            {
                LinePool.Store(_flyLine);
                _flyLine = null;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //连续点击两个点位也可以进行连线
            var startPoint = IOCC.Get<VisualLogicPointBase>(VisualLogicEnum.ConnectingPoint);
            if (startPoint != null)
            {
                if (BelongNode != startPoint.BelongNode)
                {
                    if (startPoint != this && startPoint != this)
                    {
                        if (startPoint.m_isInput != m_isInput)
                        {
                            VisualLogicPointBase output = m_isInput ? startPoint : this;
                            output.Connect(m_isInput ? this : startPoint);
                        }
                    }
                }
                startPoint.StoreFlyLine();
                IOCC.Set<VisualLogicPointBase>(VisualLogicEnum.ConnectingPoint, null);
            }
            else
            {
                IOCC.Set<VisualLogicPointBase>(VisualLogicEnum.ConnectingPoint, this);

                _flyLine = LinePool.New();
                _flyLine.SetObject(this);
            }
        }

        /// <summary>
        /// 创建新点位信息类
        /// </summary>
        /// <param name="createFunc"></param>
        public void NewInfo(Func<string, IVisualLogicPointInfo> createFunc)
        {
            Info = createFunc(m_type);
            if (string.IsNullOrEmpty(Info.ID))
            {
                Info.ID = Guid.NewGuid().ToString();
            }
            Info.Type = m_type;
            Info.AllowMulit = m_allowMulit;
            Info.IsInput = m_isInput;
            Info.Connect = new List<string>();
            AfterNewInfo();
        }

        /// <summary>
        /// 读档后更新
        /// </summary>
        public void UpdateState()
        {
            if (Info.Connect == null)
            {
                Info.Connect = new List<string>();
            }
            if (Info.IsInput)
            {
                if (Waiting.ContainsKey(Info.ID))
                {
                    foreach (var item in Waiting[Info.ID])
                    {
                        item.Connect(this, false);
                    }
                    Waiting.Remove(Info.ID);
                }
            }
            else
            {
                foreach (var item in Info.Connect)
                {
                    if (Points.ContainsKey(item))
                    {
                        Connect(Points[item], false);
                    }
                    else
                    {
                        if (Waiting.ContainsKey(item))
                        {
                            Waiting[item].Add(this);
                        }
                        else
                        {
                            Waiting.Add(item, new List<VisualLogicPointBase>() { this });
                        }
                    }
                }
            }
            if (Points.ContainsKey(Info.ID) == false)
            {
                Points.Add(Info.ID, this);
            }
            AfterUpdateState();
        }

        /// <summary>
        /// 连线
        /// </summary>
        /// <param name="input"></param>
        /// <param name="newConnect"></param>
        public void Connect(VisualLogicPointBase input, bool newConnect = true)
        {
            if (m_isInput)
            {
                Debug.LogError("应当只有输出点位调用连线方法");
                return;
            }

            if (newConnect && Info.Connect.Contains(input.Info.ID))
            {
                return;
            }
            if (ConnectVerification(input) == false)
            {
                return;
            }

            VisualLogicLine line = null;
            if (_flyLine != null)
            {
                line = _flyLine;
                _flyLine = null;
            }
            else
            {
                line = LinePool.New();
            }
            _connectLines.Add(line);
            input.ConnectLines.Add(line);
            line.SetObjects(this, input);

            if (newConnect)
            {
                Info.Connect.Add(input.Info.ID);
            }

            BelongNode.AfterPointChanged(this, true);
            input.BelongNode.AfterPointChanged(input, true);
        }

        /// <summary>
        /// 断开
        /// </summary>
        /// <param name="input"></param>
        /// <param name="line"></param>
        public void Disconnect(VisualLogicPointBase input, VisualLogicLine line)
        {
            if (m_isInput)
            {
                Debug.LogError("应当只有输出点位调用断开方法");
                return;
            }

            if (Info.Connect.Contains(input.Info.ID))
            {
                _connectLines.Remove(line);
                input._connectLines.Remove(line);
                Info.Connect.Remove(input.Info.ID);
                LinePool.Store(line);


                BelongNode.AfterPointChanged(this, false);
                input.BelongNode.AfterPointChanged(input, false);
            }
        }

        /// <summary>
        /// 存储连接中的线
        /// </summary>
        public void StoreFlyLine()
        {
            if (_flyLine != null)
            {
                LinePool.Store(_flyLine);
                _flyLine = null;
            }
        }

        /// <summary>
        /// 被回收时清空信息
        /// </summary>
        public void OnStore()
        {
            if (Info != null)
            {
                Points.Remove(Info.ID);
            }
        }

        /// <summary>
        /// 初始化信息类后调用，用于处理子类额外数据的初始化
        /// </summary>
        protected virtual void AfterNewInfo()
        {

        }

        protected virtual void AfterUpdateState()
        {

        }
        /// <summary>
        /// 校验是否能够连接
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected virtual bool ConnectVerification(VisualLogicPointBase input)
        {
            return true;
        }
    }

    /// <summary>
    /// 点位信息类基类
    /// </summary>
    public interface IVisualLogicPointInfo
    {
        /// <summary>
        /// 点位ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 点位类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 是否允许同时连接多个点位
        /// </summary>
        public bool AllowMulit { get; set; }
        /// <summary>
        /// 是否时输入点位，为否时代表是输出点位
        /// </summary>
        public bool IsInput { get; set; }
        /// <summary>
        /// 连接的输入点位
        /// 只有输出点才需要记录连接
        /// </summary>
        public List<string> Connect { get; set; }

        /// <summary>
        /// 克隆方法
        /// </summary>
        /// <returns></returns>
        public IVisualLogicPointInfo Clone();
    }

    /// <summary>
    /// 基础点位信息类
    /// 用于示例
    /// </summary>
    [System.Serializable]
    public class BasicVisualLogicPointInfo : IVisualLogicPointInfo
    {
        public string ID { get { return m_id; } set { m_id = value; } }

        [SerializeField] private string m_id;

        public string Type { get { return m_type; } set { m_type = value; } }
        [SerializeField] private string m_type;

        public bool AllowMulit { get { return m_allowMulit; } set { m_allowMulit = value; } }
        [SerializeField] private bool m_allowMulit;
        public bool IsInput { get { return m_isInput; } set { m_isInput = value; } }
        [SerializeField] private bool m_isInput;

        public List<string> Connect { get { return m_connect; } set { m_connect = value; } }
        [SerializeField] private List<string> m_connect;

        public IVisualLogicPointInfo Clone()
        {
            return this.CloneByJson();
        }
    }
}
