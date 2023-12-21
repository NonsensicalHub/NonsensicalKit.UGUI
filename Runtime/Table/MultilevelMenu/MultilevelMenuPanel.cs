using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.Editor.Table
{
    public class MultilevelMenuPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform m_group;

        private bool _mouseHover;
        public bool RootPanel { get; set; }

        private void OnEnable()
        {
            _mouseHover = false;
        }

        public void Init(List<MultilevelMenuNode> nodes, IMultilevelMenu MultilevelMenu)
        {
            StopAllCoroutines();
            gameObject.SetActive(true);
            if (RootPanel)
            {
                StartCoroutine(CheckInput());
            }
            int crtChildCount = m_group.childCount;
            int index = 0;
            for (; index < nodes.Count; index++)
            {
                MultilevelMenuElement crtElement;
                if (index < crtChildCount)
                {
                    crtElement = m_group.GetChild(index).GetComponent<MultilevelMenuElement>();
                }
                else
                {
                    crtElement = MultilevelMenu.InstantiateElement(m_group);
                }
                crtElement.Init(nodes[index], MultilevelMenu);
                crtElement.gameObject.SetActive(true);
            }
            for (; index < crtChildCount; index++)
            {
                m_group.GetChild(index).gameObject.SetActive(false);
            }
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            _mouseHover = true;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            _mouseHover = false;
            if (!RootPanel)
            {
                gameObject.SetActive(false);
            }
        }
        public void Open()
        {
            gameObject.SetActive(true);
            if (RootPanel)
            {
                StartCoroutine(CheckInput());
            }
        }

        public void Close()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        public IEnumerator CheckInput()
        {
            while (true)
            {
                yield return null;
                if (Input.GetMouseButtonDown(0))
                {
                    if (!_mouseHover)
                    {
                        gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }
    }
}
