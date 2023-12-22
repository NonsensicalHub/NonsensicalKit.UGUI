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

        private MultilevelMenuNode _MultilevelMenuNode;

        private IMultilevelMenu _MultilevelMenu;

        private MultilevelMenuPanel _childsPanel;

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
            if (_childsPanel!=null)
            {
                _childsPanel.gameObject.SetActive(false);
            }
        }

        public void Init(MultilevelMenuNode MultilevelMenuNode, IMultilevelMenu MultilevelMenu)
        {
            StopAllCoroutines();
            m_btn_click.interactable = MultilevelMenuNode.CanClick;
            if (_childsPanel != null)
            {
                _childsPanel.gameObject.SetActive(false);
            }

            _MultilevelMenu = MultilevelMenu;
            m_txt_describe.text = MultilevelMenuNode.Name;
            _MultilevelMenuNode = MultilevelMenuNode;
            m_rightArrow.SetActive(MultilevelMenuNode.Deployable);
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
            if (!_MultilevelMenuNode.MenuInfo.AlwayCanClick&& _MultilevelMenuNode.Deployable)
            {
                ShowChilds();
            }
            else
            {
                _MultilevelMenuNode.MenuInfo.ClickAction?.Invoke(_MultilevelMenuNode.Context);
                if (_MultilevelMenuNode.MenuInfo.AutoClose)
                {
                    _MultilevelMenu.Close();
                }
            }
        }


        private void ShowChilds()
        {
            if (_childsPanel == null)
            {
                _childsPanel = _MultilevelMenu.InstantiatePanel(transform);
            }
            _childsPanel.gameObject.SetActive(true);
            _childsPanel.Init(_MultilevelMenuNode.Childs, _MultilevelMenu);
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

            if (_MultilevelMenuNode.Deployable)
            {
                StartCoroutine(AutoDeploy());
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _mouseHover = false;
            m_hover.gameObject.SetActive(false);
            if (_childsPanel != null)
            {
                _childsPanel.gameObject.SetActive(false);
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
