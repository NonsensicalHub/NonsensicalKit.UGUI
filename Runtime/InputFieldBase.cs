using UnityEngine;
using NonsensicalKit.Editor;

#if TEXTMESHPRO_PRESENT
using TMPro;
#else
using UnityEngine.UI;
#endif

namespace NonsensicalKit.Editor
{
#if TEXTMESHPRO_PRESENT
    [RequireComponent(typeof(TMP_InputField))]
#else
    [RequireComponent(typeof(InputField))]
#endif
    public abstract class InputFieldBase : NonsensicalMono
    {
        [SerializeField] protected string m_defaultValue;

#if TEXTMESHPRO_PRESENT
        protected TMP_InputField _ipf_self;
#else
        protected InputField _ipf_self;
#endif
        protected virtual void Awake()
        {
#if TEXTMESHPRO_PRESENT
            _ipf_self = GetComponent<TMP_InputField>();
#else
            _ipf_self = GetComponent<InputField>();
#endif
        }
    }
}
