using System.Collections.Generic;
using NonsensicalKit.Tools.InputTool;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.UGUI.Table
{
    public class RightClickMenu : ListTableManager<RightClickMenuElement, RightClickMenuItem>, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// 此顶点应当是右键菜单的左上角
        /// </summary>
        [SerializeField] private RectTransform m_topNode;

        private bool _isHover;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHover = false;
        }

        protected override void Awake()
        {
            base.Awake();

            Subscribe<List<RightClickMenuItem>>("OpenRightClickMenu", OnOpen);
            Subscribe("CloseRightClickMenu", OnCloseMenu);
            InputHub.Instance.OnMouseLeftButtonDown += OnMouse;
            InputHub.Instance.OnMouseRightButtonDown += OnMouse;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            InputHub.Instance.OnMouseLeftButtonDown -= OnMouse;
            InputHub.Instance.OnMouseRightButtonDown -= OnMouse;
        }

        private void OnMouse()
        {
            if (_isHover == false)
            {
                CloseSelf();
            }
        }

        private void OnOpen(IEnumerable<RightClickMenuItem> datas)
        {
            OpenSelf();
            UpdateUI(datas);
        }

        private void OnCloseMenu()
        {
            CloseSelf();
        }

        protected override void UpdateUI(IEnumerable<RightClickMenuItem> data)
        {
            base.UpdateUI(data);
            m_topNode.position = Input.mousePosition;
        }
    }
}
