using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Editor.RestrictedInputField
{
    public class RestrictedInputFieldFixedValue : RestrictedInputFieldBase
    {
        [SerializeField] private List<string> m_fixedValues;

        protected override void Restrict()
        {
            if (!m_fixedValues.Contains(_ipf_self.text))
            {
                _ipf_self.text = m_defaultValue;
            }
        }
    }
}
