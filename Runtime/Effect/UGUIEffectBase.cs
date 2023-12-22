using NonsensicalKit.Core.Log;
using UnityEngine;

namespace NonsensicalKit.UGUI.Effect
{
    public abstract class UGUIEffectBase : MonoBehaviour
    {
        [Tooltip("如果不赋值，则会在Awake时选择挂载目标")]
        [SerializeField] protected Transform m_target;

        protected RectTransform _rt;

        private bool _error = false;
        protected virtual void Awake()
        {
            if (m_target == null)
            {
                m_target = transform;
                _rt = m_target.GetComponent<RectTransform>();
                if (_rt == null)
                {
                    _error = true;
                   LogCore.Warning("目标对象未挂载RectTransform组件");
                }
            }
        }
        public void ShowEffect(string command="")
        {
            if (!_error)
            {
                DoEffect(command);
            }
        }

        protected abstract void DoEffect(string command);
    }
}
