using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace NonsensicalKit.UGUI
{
    /// <summary>
    /// 使UI跟随目标对象移动
    /// </summary>
    public class FollowGameObject : MonoBehaviour
    {
        [SerializeField] private Transform m_target;

        [SerializeField] private float m_scale = 1;

        [SerializeField] private Camera m_mainCamera;

        /// <summary>
        /// 渲染ui的摄像机，当Canvas的渲染模式为Overlay时，这个值应当为null
        /// </summary>
        [FormerlySerializedAs("m_RenderCamera")] [SerializeField]
        private Camera m_renderCamera;

        [SerializeField] private bool m_scaleByDistance;

        [SerializeField] [ShowIf(nameof(m_scaleByDistance))]
        private float m_normalDistance = 1;

        public bool Back { get; private set; }
        public Vector2 Offset { get; set; }

        private RectTransform _rectTransformSelf;

        private Vector3 _lastTargetPostion;
        private Vector3 _lastCameraPostion;
        private Quaternion _lastCameraRotation;

        private Vector3 _targetPosition;
        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation;

        private bool _needRefresh;

        private void Awake()
        {
            _rectTransformSelf = transform.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Canvas.willRenderCanvases += Follow;
        }

        private void OnDisable()
        {
            Canvas.willRenderCanvases -= Follow;
        }

        private void Follow()
        {
            if (m_mainCamera == null)
            {
                m_mainCamera = Camera.main;
            }

            if (m_target != null)
            {
                _targetPosition = m_target.position;
                _cameraPosition = m_mainCamera.transform.position;
                _cameraRotation = m_mainCamera.transform.rotation;
                if (_targetPosition != _lastTargetPostion || _cameraPosition != _lastCameraPostion ||
                    _cameraRotation != _lastCameraRotation)
                {
                    _needRefresh = true;
                }

                if (_needRefresh)
                {
                    if (m_scaleByDistance && m_normalDistance != 0)
                    {
                        float dis = Vector3.Distance(m_target.position, m_mainCamera.transform.position);
                        if (dis > 1f)
                        {
                            transform.localScale = Vector3.one * ((m_normalDistance / dis) * m_scale);
                        }
                    }
                    else
                    {
                        transform.localScale = Vector3.one * m_scale;
                    }

                    Vector3 pos = m_mainCamera.WorldToScreenPoint(m_target.position) +
                                  new Vector3(Offset.x, Offset.y, 0);
                    Back = pos.z < 0;
                    if (!Back)
                    {
                        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransformSelf, pos,
                                m_renderCamera, out Vector3 worldPoint))
                        {
                            transform.position = worldPoint;
                        }
                    }

                    _lastTargetPostion = _targetPosition;
                    _lastCameraPostion = _cameraPosition;
                    _lastCameraRotation = _cameraRotation;

                    _needRefresh = false;
                }
            }
        }

        public void SetTarget(GameObject newTarget)
        {
            _needRefresh = true;
            m_target = newTarget.transform;
        }

        public void SetTarget(Transform newTarget)
        {
            _needRefresh = true;
            m_target = newTarget;
        }

        public void SetMainCamera(Camera cam)
        {
            _needRefresh = true;
            m_mainCamera = cam;
        }

        public void SetRendererCamera(Camera cam)
        {
            _needRefresh = true;
            m_renderCamera = cam;
        }

        public Transform GetTarget()
        {
            return m_target;
        }
    }
}
