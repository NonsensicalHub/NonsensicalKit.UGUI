using TMPro;

namespace NonsensicalKit.UGUI.RestrictedInputField
{
    public class RestrictedInputFieldFloat : RestrictedInputFieldBase
    {
        protected override void Awake()
        {
            base.Awake();
#if TEXTMESHPRO_PRESENT
            _ipf_self.contentType = TMP_InputField.ContentType.DecimalNumber;
#else
            _ipf_self.contentType = UnityEngine.UI.InputField.ContentType.DecimalNumber;
#endif
        }

        protected override void Restrict()
        {
            if (!float.TryParse(_ipf_self.text, out var v))
            {
                _ipf_self.text = m_defaultValue;
            }
        }
    }
}
