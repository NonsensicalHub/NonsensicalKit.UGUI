using System.Collections;
using UnityEngine;

namespace NonsensicalKit.UGUI
{
    public class FullScreenWorldCanvas : MonoBehaviour
    {
        [SerializeField] private Canvas m_canvas;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();

            StartCoroutine(Resize());
        }

        private IEnumerator Resize()
        {
            while (true)
            {
                var frustumHeight = 2.0f * transform.localPosition.z * Mathf.Tan(m_canvas.worldCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                var frustumWidth = frustumHeight * m_canvas.worldCamera.aspect;
                _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, frustumWidth / transform.localScale.x);
                _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, frustumHeight / transform.localScale.y);
                yield return null;
            }
        }
    }
}
