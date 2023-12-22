using UnityEngine;

namespace NonsensicalKit.UGUI
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Box2DSizeUpdater : MonoBehaviour
    {
        [SerializeField] private float m_radio = 1;
        private RectTransform _rect;
        private BoxCollider2D _boxCollider;
        private Vector2 _oldSize = Vector2.zero;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _boxCollider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            if (_oldSize != _rect.rect.size)
            {
                _oldSize = _rect.rect.size;
                _boxCollider.size = _rect.rect.size * m_radio;
                _boxCollider.offset = -_rect.rect.size * (_rect.pivot - Vector2.one * 0.5f) ;
            }
        }
    }
}
