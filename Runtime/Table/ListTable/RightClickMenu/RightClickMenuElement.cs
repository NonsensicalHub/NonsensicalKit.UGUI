using NonsensicalKit.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Editor.Table
{
    public class RightClickMenuElement : ListTableElement<RightClickMenuItem>
    {
        [SerializeField] private Image m_img_Icon;
        [SerializeField] private Text m_txt_Text;
        [SerializeField] private Button m_btn_Element;

        public override void SetValue(RightClickMenuItem elementData)
        {
            if (this.ElementData != null && this.ElementData.SpriteName != null)
            {
                SpriteManager.Instance.RecoverySprite(this.ElementData.SpriteName);
            }

            base.SetValue(elementData);

            if (elementData.SpriteName != null)
            {
                SpriteManager.Instance.TryGetSprite(elementData.SpriteName, OnGetSprite);
            }
            else
            {
                m_img_Icon.gameObject.SetActive(false);
            }
            m_txt_Text.text = elementData.Text;
            m_btn_Element.onClick.RemoveAllListeners();
            m_btn_Element.onClick.AddListener(OnElementClick);
        }

        private void OnElementClick()
        {
            Publish("CloseRightClickMenu");
            ElementData.ClickAction();
        }

        private void OnGetSprite(Sprite sp)
        {
            if (sp != null)
            {
                m_img_Icon.gameObject.SetActive(true);
                m_img_Icon.sprite = sp;
                m_img_Icon.preserveAspect = true;
            }
            else
            {
                m_img_Icon.gameObject.SetActive(false);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ElementData != null && !NonsensicalInstance.ApplicationIsQuitting)
            {
                SpriteManager.Instance.RecoverySprite(ElementData.SpriteName);
            }
        }
    }
}
