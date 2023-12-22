using UnityEngine;

namespace NonsensicalKit.UGUI
{
    /// <summary>
    /// 与目标rect保持相同大小
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SameSize : MonoBehaviour
    {
        [SerializeField] private RectTransform m_target;
        [SerializeField] private bool m_sameWidth;
        [SerializeField] private bool m_clampWidth;
        [SerializeField] private float m_minWidth;
        [SerializeField] private float m_maxWidth;
        [SerializeField] private bool m_sameHeight;
        [SerializeField] private bool m_clampHeight;
        [SerializeField] private float m_minHeight;
        [SerializeField] private float m_maxHeight;

        private RectTransform _self;

        private void Awake()
        {
            _self = GetComponent<RectTransform>();
        }

        private void Update()
        {
            Vector2 targetSize = new Vector2();
            if (m_sameWidth)
            {
                if (m_clampWidth)
                {
                    targetSize.x = Mathf.Clamp(m_target.sizeDelta.x, m_minWidth, m_maxWidth);
                }
                else
                {
                    targetSize.x = m_target.sizeDelta.x;
                }
            }
            else
            {
                targetSize.x = _self.sizeDelta.x;
            }
            if (m_sameHeight)
            {
                if (m_clampHeight)
                {
                    targetSize.y = Mathf.Clamp(m_target.sizeDelta.y, m_minHeight, m_maxHeight);
                }
                else
                {
                    targetSize.y = m_target.sizeDelta.y;
                }
            }
            else
            {
                targetSize.y = _self.sizeDelta.y;
            }

            _self.sizeDelta = targetSize;
        }
    }
}
