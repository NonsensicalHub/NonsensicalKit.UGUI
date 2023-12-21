using NonsensicalKit.Tools;
using UnityEngine;

namespace NonsensicalKit.Editor
{
    [RequireComponent(typeof(Rigidbody2D), typeof(RectTransform))]
    public class SquarePuppy : MonoBehaviour
    {
        [SerializeField] private RectTransform m_master;
        [SerializeField] private RectTransform m_line;
        [SerializeField] private float m_strength = 5f;
        [SerializeField] private float m_minDiastance = 50;
        [SerializeField] private float m_maxDiastance = 100;
        [SerializeField] private float m_speed = 1000;
        [SerializeField] private float m_cd = 0.5f;

        private RectTransform _selfRect;
        private Rigidbody2D _selfRigidbody;

        private float _timer;
        private bool _free;

        private void Awake()
        {
            _selfRect = GetComponent<RectTransform>();
            _selfRigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            _free = _timer < 0;
        }

        private void FixedUpdate()
        {
            UpdateLine();

            if (_free)
            {
                UpdatePos();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<SquarePuppy>(out var v))
            {
                v.Push(collision.transform.position - transform.position);
            }
        }
        private void UpdateLine()
        {
            var center = (Vector2)_selfRect.position + _selfRect.rect.center * _selfRect.lossyScale;
            var max = (Vector2)_selfRect.position + _selfRect.rect.max * _selfRect.lossyScale;
            var min = (Vector2)_selfRect.position + _selfRect.rect.min * _selfRect.lossyScale;
            var ps = VectorTool.GetLineCrossUprightRect(m_master.position, center, min, max);

            if (ps.Count == 0)
            {
                return;
            }
            Vector2 point = ps[0];
            m_line.position = ((Vector2)m_master.position + point) / 2;
            Vector2 offset = point - (Vector2)m_master.position;
            m_line.sizeDelta = new Vector3(offset.magnitude / m_line.lossyScale.x, 5);
            m_line.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(offset.y / offset.x) / Mathf.PI));
        }

        private void UpdatePos()
        {
            if (Vector3.Distance(m_master.position, _selfRect.position) < m_minDiastance)
            {
                _selfRigidbody.MovePosition(transform.position + (_selfRect.position - m_master.position).normalized * m_speed * Time.fixedDeltaTime);
            }
            if (Vector3.Distance(m_master.position, _selfRect.position) > m_maxDiastance)
            {
                _selfRigidbody.MovePosition(transform.position + (m_master.position - _selfRect.position).normalized * m_speed * Time.fixedDeltaTime);
            }
        }

        private void Push(Vector3 pushDir)
        {
            _timer = m_cd;
            _selfRigidbody.AddForce(pushDir * m_strength, ForceMode2D.Impulse);
        }
    }
}
