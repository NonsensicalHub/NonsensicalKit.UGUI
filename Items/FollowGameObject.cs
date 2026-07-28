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

        [Header("不可信时段")]
        [SerializeField] private bool m_useUntrustedPeriod = false;
        [Tooltip("启用后在该时长内强制刷新跟随；每次禁用会重置计时")]
        [SerializeField, Min(0f)] private float m_untrustedDuration = 0.5f;

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
        private int _skip = 6;
        private float _untrustedEndTime = -1f;

        private void Awake()
        {
            _rectTransformSelf = transform.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Canvas.willRenderCanvases += Follow;
            BeginUntrustedPeriod();
        }

        private void OnDisable()
        {
            Canvas.willRenderCanvases -= Follow;
            // 每次禁用重置不可信时间，下次启用重新起算
            ResetUntrustedPeriod();
            _needRefresh = true;
            _skip = 6;
        }

        private void BeginUntrustedPeriod()
        {
            if (!m_useUntrustedPeriod || m_untrustedDuration <= 0f)
            {
                _untrustedEndTime = -1f;
                return;
            }

            _untrustedEndTime = Time.unscaledTime + m_untrustedDuration;
            _needRefresh = true;
        }

        private void ResetUntrustedPeriod()
        {
            _untrustedEndTime = -1f;
        }

        private bool IsInUntrustedPeriod()
        {
            return m_useUntrustedPeriod &&
                   m_untrustedDuration > 0f &&
                   _untrustedEndTime > 0f &&
                   Time.unscaledTime < _untrustedEndTime;
        }

        private void Follow()
        {
            if (_skip > 0)
            {
                //一开始有可能遇到Canvas在初始化，等待数帧后再跟随
                _skip--;
                return;
            }

            if (m_mainCamera == null)
            {
                m_mainCamera = Camera.main;
                if (m_mainCamera == null)
                {
                    return;
                }
            }

            if (m_target == null)
            {
                return;
            }

            _targetPosition = m_target.position;
            _cameraPosition = m_mainCamera.transform.position;
            _cameraRotation = m_mainCamera.transform.rotation;

            // 不可信时段内始终刷新；之外仅在目标/相机变化时刷新
            if (IsInUntrustedPeriod() ||
                _targetPosition != _lastTargetPostion ||
                _cameraPosition != _lastCameraPostion ||
                _cameraRotation != _lastCameraRotation)
            {
                _needRefresh = true;
            }

            if (!_needRefresh)
            {
                return;
            }

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

            // 不可信时段内保持需要刷新；结束后才允许短路
            _needRefresh = IsInUntrustedPeriod();
        }

        public void SetTarget(GameObject newTarget)
        {
            _needRefresh = true;
            m_target = newTarget == null ? null : newTarget.transform;
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
