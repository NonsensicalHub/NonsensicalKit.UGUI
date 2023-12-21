using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Editor
{
    /// <summary>
    /// 自适应大小工具类，当大小大于一定数值时才开启大小自适应
    /// </summary>
    [RequireComponent(typeof(ContentSizeFitter))]
    [RequireComponent(typeof(RectTransform))]
    public class AutoSizeController : MonoBehaviour
    {

        [SerializeField] private float m_verticalArg = -1;
        [SerializeField] private float m_horizontalArg = -1;

        private ContentSizeFitter _contentSizeFitter;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _contentSizeFitter = GetComponent<ContentSizeFitter>();
        }

        private void Update()
        {
            if (m_horizontalArg > 0)
            {
                _contentSizeFitter.horizontalFit = _rectTransform.rect.width < m_horizontalArg ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;
                _rectTransform.sizeDelta = new Vector2(m_horizontalArg - 1, _rectTransform.sizeDelta.y);
            }
            if (m_verticalArg > 0)
            {
                _contentSizeFitter.verticalFit = _rectTransform.rect.height < m_verticalArg ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;
                _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, m_verticalArg - 1);
            }
        }
    }
}
