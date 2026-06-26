using NonsensicalKit.Core;
using UnityEngine;
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
        protected TextMeshProUGUI TxtSelf;

        protected virtual void Awake()
        {
            TxtSelf = GetComponent<TextMeshProUGUI>();
        }
#else
        protected Text TxtSelf;

        protected virtual void Awake()
        {
            TxtSelf = GetComponent<Text>();
        }
#endif
    }
}
