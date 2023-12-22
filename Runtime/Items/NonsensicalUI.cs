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

        protected RectTransform _rectTransform;
        protected CanvasGroup _canvasGroup;

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

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public void Appear(bool immediately = false)
        {
            OpenSelf(immediately);
        }

        public void Disappear(bool immediately = false)
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
            _canvasGroup.blocksRaycasts = true;

            if (immediately)
            {
                _canvasGroup.alpha = 1;
            }
            else
            {
                _canvasGroup.DoFade(1, 0.2f);
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
            _canvasGroup.blocksRaycasts = false;

            if (immediately)
            {
                _canvasGroup.alpha = 0;
            }
            else
            {
                _canvasGroup.DoFade(0, 0.2f);
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

        protected void SwitchSelf()
        {
            SwitchSelf(true);
        }

        protected void SwitchSelf(bool immediately = false)
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
                _rectTransform = transform.GetComponent<RectTransform>();
                _canvasGroup = transform.GetComponent<CanvasGroup>();
            }
        }
    }
}

