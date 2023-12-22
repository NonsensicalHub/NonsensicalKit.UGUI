using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI
{
    public class SwitchPageController : NonsensicalUI
    {
        [SerializeField] protected Button[] m_buttons;

        [SerializeField] protected GameObject[] m_targets;

        [SerializeField] private int m_initSelect = 0;

        private GameObject[] _selectedImages;

        protected override void Awake()
        {
            base.Awake();

            if (m_buttons.Length != m_targets.Length)
            {
                Debug.LogWarning("数量不正确");
            }
            else
            {
                for (int i = 0; i < m_buttons.Length; i++)
                {
                    int index = i;
                    m_buttons[i].onClick.AddListener(() => { Switch(index); });
                }
                _selectedImages = new GameObject[m_targets.Length];

                for (int i = 0; i < _selectedImages.Length; i++)
                {
                    _selectedImages[i] = m_buttons[i].transform.Find("img_selected").gameObject;
                }

                Switch(m_initSelect);
            }
        }

        protected virtual void Switch(int index)
        {
            for (int i = 0; i < m_targets.Length; i++)
            {
                if (index == i)
                {
                    m_targets[i].SetActive(true);
                    _selectedImages[i].SetActive(true);
                }
                else
                {
                    m_targets[i].SetActive(false);
                    _selectedImages[i].SetActive(false);
                }
            }
        }
    }
}
