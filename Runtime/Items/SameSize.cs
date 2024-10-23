using NaughtyAttributes;
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
        [SerializeField][ShowIf("m_sameWidth")] private bool m_clampWidth;
        [SerializeField][ShowIf("m_clampWidth")] private float m_minWidth;
        [SerializeField][ShowIf("m_clampWidth")] private float m_maxWidth;
        [SerializeField] private bool m_sameHeight;
        [SerializeField][ShowIf("m_sameHeight")] private bool m_clampHeight;
        [SerializeField][ShowIf("m_clampHeight")] private float m_minHeight;
        [SerializeField][ShowIf("m_clampHeight")] private float m_maxHeight;

        private RectTransform _self;

        private void Awake()
        {
            _self = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (m_sameWidth)
            {
                float targetSize;
                if (m_clampWidth)
                {
                    targetSize = Mathf.Clamp(m_target.rect.width, m_minWidth, m_maxWidth);
                }
                else
                {
                    targetSize = m_target.rect.width;
                }
                _self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize);
            }
            if (m_sameHeight)
            {
                float targetSize;
                if (m_clampHeight)
                {
                    targetSize = Mathf.Clamp(m_target.rect.height, m_minHeight, m_maxHeight);
                }
                else
                {
                    targetSize = m_target.rect.height;
                }
                _self.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize);
            }
        }
    }
}
