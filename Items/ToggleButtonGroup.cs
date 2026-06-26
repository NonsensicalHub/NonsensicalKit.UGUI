using System;
using System.Collections.Generic;
using NonsensicalKit.Core.Log;
using UnityEngine;

namespace NonsensicalKit.UGUI
{
    public class ToggleButtonGroup : MonoBehaviour
    {
        [SerializeField] private bool m_allowAllOff;

        public bool AllowAllOff => m_allowAllOff;

        private readonly List<ToggleButton> _toggleButtons = new();

        private ToggleButton _crtTb;

        public void AddToGroup(ToggleButton tb)
        {
            _toggleButtons.Add(tb);

            if (tb.IsOn)
            {
                if (_crtTb == null)
                {
                    _crtTb = tb;
                }
                else
                {
                    tb.SetState(false);
                }
            }
        }

        private void Start()
        {
            if (_toggleButtons.Count != 0)
            {
                if (!m_allowAllOff && _crtTb == null)
                {
                    Switch(_toggleButtons[0], true);
                }
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
                        try
                        {
                            item.IsOn = false;
                        }
                        catch (Exception e)
                        {
                            LogCore.Warning("ToggleButtonGroup Switch Error" + e.Message);
                        }
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
