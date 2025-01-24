using NonsensicalKit.Core;
using NonsensicalKit.Tools;
using UnityEngine;

namespace NonsensicalKit.UGUI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class NonsensicalUI : NonsensicalMono
    {
        /// <summary>
        /// 场景加载时是否展示
        /// </summary>
        [SerializeField] private bool m_initShow = true;

        /// <summary>
        /// 当前是否展示
        /// </summary>
        public bool IsShow { get; private set; }

        public bool InitShow { get; set; }

        protected RectTransform RectTransform;
        protected CanvasGroup CanvasGroup;

        private bool _init;

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void Start()
        {
            if (m_initShow)
            {
                OpenSelf(true);
            }
            else
            {
                CloseSelf(true);
            }
        }

        public bool CheckAlpha()
        {
            var uis = gameObject.GetComponentsInParent<NonsensicalUI>();
            foreach (var item in uis)
            {
                if (item.IsShow == false)
                {
                    return false;
                }
            }

            return true;
        }

        public void Appear()
        {
            OpenSelf(true);
        }

        public void Appear(bool immediately)
        {
            OpenSelf(immediately);
        }

        public void Disappear()
        {
            CloseSelf(true);
        }

        public void Disappear(bool immediately)
        {
            CloseSelf(immediately);
        }

        public void Switch(bool immediately = false)
        {
            SwitchSelf(immediately);
        }

        protected virtual void OpenSelf()
        {
            OpenSelf(true);
        }

        protected virtual void OpenSelf(bool immediately)
        {
            Init();
            CanvasGroup.blocksRaycasts = true;

            if (immediately)
            {
                CanvasGroup.alpha = 1;
            }
            else
            {
                CanvasGroup.DoFade(1, 0.2f);
            }

            IsShow = true;
            OnOpen();
        }

        protected virtual void CloseSelf()
        {
            CloseSelf(true);
        }

        protected virtual void CloseSelf(bool immediately)
        {
            Init();
            CanvasGroup.blocksRaycasts = false;

            if (immediately)
            {
                CanvasGroup.alpha = 0;
            }
            else
            {
                CanvasGroup.DoFade(0, 0.2f);
            }

            IsShow = false;
            OnClose();
        }

        public void ChangeSelf(bool value)
        {
            if (value)
            {
                OpenSelf();
            }
            else
            {
                CloseSelf();
            }
        }

        protected void SwitchSelf(bool immediately = true)
        {
            if (IsShow)
            {
                CloseSelf(immediately);
            }
            else
            {
                OpenSelf(immediately);
            }
        }

        protected void Open(NonsensicalUI target)
        {
            target.OpenSelf();
        }

        protected void Close(NonsensicalUI target)
        {
            target.CloseSelf();
        }

        protected void Switch(NonsensicalUI target)
        {
            target.SwitchSelf();
        }

        protected virtual void OnOpen()
        {
        }

        protected virtual void OnClose()
        {
        }

        private void Init()
        {
            if (!_init)
            {
                _init = true;
                RectTransform = transform.GetComponent<RectTransform>();
                CanvasGroup = transform.GetComponent<CanvasGroup>();
            }
        }
    }
}
