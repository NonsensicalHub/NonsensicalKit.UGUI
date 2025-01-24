using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonsensicalKit.Core.Table
{
    public class MultilevelMenuElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI m_txt_describe;
        [SerializeField] private Button m_btn_click;
        [SerializeField] private GameObject m_hover;
        [SerializeField] private GameObject m_rightArrow;

        private MultilevelMenuNode _multilevelMenuNode;

        private IMultilevelMenu _multilevelMenu;

        private MultilevelMenuPanel _childrenPanel;

        private bool _mouseHover;

        private bool _canClick;

        private void Awake()
        {
            m_btn_click.onClick.AddListener(OnButtonClick);
        }

        private void OnEnable()
        {
            _canClick = true;
            _mouseHover = false;
            m_hover.gameObject.SetActive(false);
            if (_childrenPanel != null)
            {
                _childrenPanel.gameObject.SetActive(false);
            }

            if (_multilevelMenuNode != null)
            {
                m_btn_click.interactable = _multilevelMenuNode.CanClick;
            }
        }

        public void Init(MultilevelMenuNode multilevelMenuNode, IMultilevelMenu multilevelMenu)
        {
            StopAllCoroutines();
            m_btn_click.interactable = multilevelMenuNode.CanClick;
            if (_childrenPanel != null)
            {
                _childrenPanel.gameObject.SetActive(false);
            }

            _multilevelMenu = multilevelMenu;
            m_txt_describe.text = multilevelMenuNode.Name;
            _multilevelMenuNode = multilevelMenuNode;
            m_rightArrow.SetActive(multilevelMenuNode.Deployable);
        }

        private void OnButtonClick()
        {
            if (!_canClick)
            {
                return;
            }

            _canClick = false;
            StopAllCoroutines();
            StartCoroutine(ColdDown());
            if (!_multilevelMenuNode.MenuInfo.AlwaysCanClick && _multilevelMenuNode.Deployable)
            {
                ShowChilds();
            }
            else
            {
                _multilevelMenuNode.MenuInfo.ClickAction?.Invoke(_multilevelMenuNode.Context);
                if (_multilevelMenuNode.MenuInfo.AutoClose)
                {
                    _multilevelMenu.Close();
                }
            }
        }

        private void ShowChilds()
        {
            if (_childrenPanel == null)
            {
                _childrenPanel = _multilevelMenu.InstantiatePanel(transform);
            }

            _childrenPanel.gameObject.SetActive(true);
            _childrenPanel.Init(_multilevelMenuNode.Children, _multilevelMenu);
        }

        private IEnumerator ColdDown()
        {
            yield return new WaitForSeconds(0.5f);
            _canClick = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _mouseHover = true;
            m_hover.gameObject.SetActive(true);

            if (_multilevelMenuNode.Deployable)
            {
                StartCoroutine(AutoDeploy());
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _mouseHover = false;
            m_hover.gameObject.SetActive(false);
            if (_childrenPanel != null)
            {
                _childrenPanel.gameObject.SetActive(false);
            }
        }

        private IEnumerator AutoDeploy()
        {
            yield return new WaitForSeconds(0.5f);
            if (_mouseHover)
            {
                ShowChilds();
            }
        }
    }
}
