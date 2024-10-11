using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.UGUI
{
    public class ToggleButtonGroup : MonoBehaviour
    {
        [SerializeField] private bool m_allowAllOff;

        public bool AllowAllOff => m_allowAllOff;

        private List<ToggleButton> _toggleButtons = new List<ToggleButton>();

        private ToggleButton _crtTb;

        public void AddToGroup(ToggleButton tb)
        {
            _toggleButtons.Add(tb);
            if (_crtTb != null)
            {
                tb.SetState(false);
            }
            if (!m_allowAllOff && _crtTb == null)
            {
                tb.SetState(true);
                _crtTb = tb;
            }
        }

        public bool Switch(ToggleButton tb, bool newState)
        {
            if (newState)
            {
                _crtTb = tb;
                foreach (var item in _toggleButtons)
                {
                    if (item != tb)
                    {
                        item.IsOn = false;
                    }
                }
                return true;
            }
            else
            {
                if (!m_allowAllOff && _crtTb == tb)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
