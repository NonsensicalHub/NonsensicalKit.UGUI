using UnityEngine;

namespace NonsensicalKit.Editor
{
    /// <summary>
    /// 通过修改中心点实现平移运动，用于UI在屏幕边缘隐藏的情况
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class EdgeWalker : MonoBehaviour
    {
        [SerializeField] private Vector2 m_openPivot;
        [SerializeField] private Vector2 m_closePivot;

        private Vector2 _targetPivot;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (Vector2.Distance(_rectTransform.pivot, _targetPivot) < 0.01f)
            {
                _rectTransform.pivot = _targetPivot;
                enabled = false;
            }
            else
            {
                _rectTransform.pivot = Vector2.Lerp(_rectTransform.pivot, _targetPivot, 0.05f);
            }
        }

        public void Open()
        {
            enabled = true;
            _targetPivot = m_openPivot;
        }

        public void Close()
        {
            enabled = true;
            _targetPivot = m_closePivot;
        }
    }
}
