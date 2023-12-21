using NonsensicalKit.Editor;
using UnityEngine.EventSystems;

namespace NonsensicalKit.Editor
{
    /// <summary>
    ///  EventSystem信息集中处理类
    /// </summary>
    public class EventSystemInfo : MonoSingleton<EventSystemInfo>
    {
        public bool MouseNotInUI
        {
            get
            {
                if (_crtEventSystem == null)
                {
                    return true;
                }

                return _notInUI;
            }
        }

        private bool _notInUI;

        private EventSystem _crtEventSystem;

        private void Update()
        {
            if (_crtEventSystem == null)
            {
                _crtEventSystem = EventSystem.current;
                return;
            }
            _notInUI = !_crtEventSystem.IsPointerOverGameObject();

        }
    }
}
