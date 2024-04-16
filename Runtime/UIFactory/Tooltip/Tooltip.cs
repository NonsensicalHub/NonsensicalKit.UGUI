using NonsensicalKit.Core.Log;
using NonsensicalKit.Tools.ObjectPool;
using UnityEngine;

#if TEXTMESHPRO_PRESENT
using TMPro;
#else
using UnityEngine.UI;
#endif

namespace NonsensicalKit.UGUI.UIFactory
{
    public class TooltipInfo
    {
        public RectTransform.Edge Location;
        public string Text;

        public TooltipInfo(RectTransform.Edge location, string text)
        {
            Location = location;
            Text = text;
        }
    }

    public class Tooltip : MonoBehaviour, IFactoryUI
    {
#if TEXTMESHPRO_PRESENT
        [SerializeField] private TextMeshProUGUI m_text;
#else
        [SerializeField] private Text m_text;
#endif

        [SerializeField] private CanvasGroup m_canvasGroup;

        [SerializeField] private RectTransform m_rectTransform;

        public GameObjectPool Pool { get; set; }

        private float _delay;
        private float _targetAlpha;

        private void Awake()
        {
            if (m_text == null)
            {
#if TEXTMESHPRO_PRESENT
                m_text = GetComponentInChildren<TextMeshProUGUI>();
#else
                m_text = GetComponentInChildren<Text>();
#endif
            }

            if (m_canvasGroup == null)
            {
                m_canvasGroup = GetComponentInChildren<CanvasGroup>();
            }

            if (m_rectTransform == null)
            {
                m_rectTransform = GetComponent<RectTransform>();
            }

            m_canvasGroup.alpha = 0.0f;
            _targetAlpha = 1.0f;
        }

        private void Update()
        {
            if (_delay > 0)
            {
                _delay -= Time.deltaTime;
                return;
            }

            float alpha = m_canvasGroup.alpha;
            alpha = Mathf.Lerp(alpha, _targetAlpha, Time.deltaTime * 5);
            if (alpha < 0.01)
            {
                alpha = 0;
            }
            m_canvasGroup.alpha = alpha;
            if (alpha <= 0 && _targetAlpha == 0)
            {
                Destroy(gameObject);
            }
        }

        public void SetArg(object arg)
        {
            var info = arg as TooltipInfo;
            if (info == null)
            {
                LogCore.Warning("传入FloatMessage的参数有误");
                Pool.Store(gameObject);
                return;
            }

            switch (info.Location)
            {
                case RectTransform.Edge.Left:
                case RectTransform.Edge.Right:
                    m_rectTransform.SetPivot(new Vector2(1, 0));
                    break;
                case RectTransform.Edge.Top:
                case RectTransform.Edge.Bottom:
                    m_rectTransform.SetPivot(new Vector2(0, 0));
                    break;
                default:
                    break;
            }
            m_text.text = info.Text;
            m_rectTransform.SetInsetAndSizeFromParentEdge(info.Location, -3, 0);
        }

        public void ChangeText(string text)
        {
            m_text.text = text;
        }

        public void Show()
        {
            if (m_canvasGroup.alpha < 0.01)
            {
                _delay = 0.5f;
            }
            _targetAlpha = 1;
        }

        public void Hide()
        {
            _targetAlpha = 0;
        }

        public void Close()
        {
            Pool.Store(gameObject);
        }
    }
}
