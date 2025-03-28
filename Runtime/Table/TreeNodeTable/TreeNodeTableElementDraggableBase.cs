using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.UGUI.Table
{
    public abstract class TreeNodeTableElementDraggableBase<TElementData> : TreeNodeTableElementBase<TElementData>,
        IDropHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
        where TElementData : class, ITreeNodeClass<TElementData>
    {
        [SerializeField] private bool m_justDroppable = false;
        [SerializeField] private bool m_allowSameLevel = true;
        [SerializeField] private GameObject m_hover;
        [SerializeField] private GameObject m_topHover;
        [SerializeField] private GameObject m_bottomHover;

        private RectTransform _rect_bottomHover;
        private float _topBottomAreaHeight = 0.2f;
        private GameObject[] _hovers;
        private bool _isHover;

        protected override void Awake()
        {
            base.Awake();
            _hovers = new GameObject[] { m_hover, m_topHover, m_bottomHover };
            ChangeHover(-1);
            _rect_bottomHover = m_bottomHover.GetComponent<RectTransform>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            //Debug.Log("OnDrop" + gameObject.GetInstanceID());
            if (IOCC.TryGet<TreeNodeTableElementDraggableBase<TElementData>>("dragItem", out var v))
            {
                if (IsVaild(v))
                {
                    var i = CheckHeight(eventData.position);
                    switch (i)
                    {
                        case 1:
                            Manager.MoveSameLevel(v, this, true);
                            break;
                        case 2:
                            if (base.ElementData.Parent.Children.Last() == base.ElementData)
                            {
                                var offset = CheckWidth(eventData.position);
                                offset = Mathf.Min(offset, base.ElementData.Level - 1);
                                var crt = base.ElementData;
                                for (int j = 0; j < offset; j++)
                                {
                                    crt = crt.Parent;
                                }
                                Manager.MoveSameLevel(v, crt.Belong.GetComponent<TreeNodeTableElementDraggableBase<TElementData>>(), false);
                            }
                            else
                            {
                                Manager.MoveSameLevel(v, this, false);
                            }
                            break;
                        default:
                            Manager.Move(v, this);
                            break;
                    }
                    IOCC.Set<TreeNodeTableElementDraggableBase<TElementData>>("dragItem", null);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!m_justDroppable)
            {
                //Debug.Log("OnBeginDrag" + gameObject.GetInstanceID());
                IOCC.Set<TreeNodeTableElementDraggableBase<TElementData>>("dragItem", this);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log("OnEndDrag" + gameObject.GetInstanceID());
            IOCC.Set<TreeNodeTableElementDraggableBase<TElementData>>("dragItem", null);
        }

        public void OnDrag(PointerEventData eventData)
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IOCC.TryGet<TreeNodeTableElementDraggableBase<TElementData>>("dragItem", out var dragElement))
            {
                if (IsVaild(dragElement))
                {
                    _isHover = true;
                    ChangeHover(CheckHeight(eventData.position));
                }
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (_isHover)
            {
                var pos = CheckHeight(eventData.position);
                ChangeHover(pos);
                if (pos == 2)
                {
                    if (base.ElementData.Parent != null && base.ElementData.Parent.Children.Last() == base.ElementData)
                    {//如果是最后一个子物体
                        var i = CheckWidth(eventData.position);
                        var targetLevel = base.ElementData.Level - i;
                        targetLevel = Mathf.Max(1, targetLevel);
                        _rect_bottomHover.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, _basePosition.x + targetLevel * LevelDistance - m_rt_Box.rect.width * 0.5f, m_rt_Box.rect.width + i * LevelDistance);
                    }
                    else
                    {
                        _rect_bottomHover.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, _basePosition.x + base.ElementData.Level * LevelDistance - m_rt_Box.rect.width * 0.5f, m_rt_Box.rect.width);
                    }
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isHover)
            {
                ChangeHover(-1);
                _isHover = false;
            }
        }

        private int CheckHeight(Vector2 mousePosition)
        {
            if (!m_allowSameLevel)
            {
                return 0;
            }

            var scale = RectTransform.lossyScale.y;
            var min = RectTransform.position.y + RectTransform.rect.min.y * scale + RectTransform.rect.height * scale * _topBottomAreaHeight;
            var max = RectTransform.position.y + RectTransform.rect.max.y * scale - RectTransform.rect.height * scale * _topBottomAreaHeight;

            if (mousePosition.y < min)
            {
                return 2;
            }
            else if (mousePosition.y > max)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private int CheckWidth(Vector2 mousePosition)
        {
            var scale = m_rt_Box.lossyScale.x;
            var min = m_rt_Box.position.x + m_rt_Box.rect.min.x * scale;
            var offset = min - mousePosition.x;
            if (offset < 0)
            {
                return 0;
            }
            else
            {
                var level = (int)(offset / LevelDistance) + 1;
                return level;
            }
        }

        private void ChangeHover(int index)
        {
            for (int i = 0; i < _hovers.Length; i++)
            {
                _hovers[i].SetActive(index == i);
            }
        }

        private bool IsVaild(TreeNodeTableElementDraggableBase<TElementData> target)
        {
            Queue<TElementData> datas = new Queue<TElementData>();
            datas.Enqueue(target.ElementData);
            while (datas.Count > 0)
            {
                var element = datas.Dequeue();
                if (element == base.ElementData)
                {
                    return false;
                }
                foreach (var item in element.Children)
                {
                    datas.Enqueue(item);
                }
            }

            return true;
        }
    }
}
