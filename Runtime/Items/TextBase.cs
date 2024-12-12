using UnityEngine;
using NonsensicalKit.Core;

#if TEXTMESHPRO_PRESENT
using TMPro;
#else
using UnityEngine.UI;
#endif

namespace NonsensicalKit.UGUI
{
#if TEXTMESHPRO_PRESENT
    [RequireComponent(typeof(TextMeshProUGUI))]
#else
    [RequireComponent(typeof(Text))]
#endif
    public class TextBase : NonsensicalMono
    {
#if TEXTMESHPRO_PRESENT
        protected TextMeshProUGUI _txt_self;
        
        protected virtual void Awake()
        {
            _txt_self = GetComponent<TextMeshProUGUI>();
        }
#else
        protected Text _txt_self;

        protected virtual void Awake()
        {
            _txt_self = GetComponent<Text>();
        }
#endif
    }
}
