using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSwitchSprite : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image m_spriteImg;

    [SerializeField] private Sprite m_spriteSelect;
    [SerializeField] private Sprite m_spriteNormal;

    private void Reset()
    {
        m_spriteImg = GetComponent<Image>();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        m_spriteImg.sprite = m_spriteSelect;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_spriteImg.sprite = m_spriteNormal;
    }
}
