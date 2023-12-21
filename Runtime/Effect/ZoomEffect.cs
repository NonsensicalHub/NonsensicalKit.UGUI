using System;
using System.Collections;
using UnityEngine;

namespace NonsensicalKit.Editor.Effect
{
    /// <summary>
    /// 缩放效果
    /// </summary>
    public class ZoomEffect : UGUIEffectBase
    {
        [SerializeField] private float m_effectTime = 0.5f;

        private Vector3 _originSize;
        private Vector3 _originPos;

        protected override void Awake()
        {
            base.Awake();
            _originSize = m_target.localScale;
            _originPos = _rt.anchoredPosition;
        }

        protected override void DoEffect(string command)
        {
            StopAllCoroutines();
            switch (command)
            {
                default:
                case "0":
                    StartCoroutine(DoEnlarge());
                    break;
                case "1":
                    StartCoroutine(DoReduced());
                    break;
            }
        }

        /// <summary>
        /// 先瞬间缩小到看不见的大小并移动到鼠标的位置，随后在短时间内插值放大到原始大小并移动到原点位置
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoEnlarge()
        {
            m_target.localScale = Vector3.one * 0.001f;
            m_target.position = Input.mousePosition;
            float timer = 0;

            while (timer < m_effectTime)
            {
                yield return null;
                timer += Time.deltaTime;

                m_target.localScale = _originSize * (timer / m_effectTime);
                _rt.anchoredPosition = Vector3.Lerp(_rt.anchoredPosition, _originPos, timer / m_effectTime);
            }
            m_target.localScale = _originSize;
            _rt.anchoredPosition = _originPos;
        }

        /// <summary>
        /// 短时间缩小到看不见
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoReduced()
        {
            m_target.localScale = _originPos;
            float timer = 0;

            while (timer < m_effectTime)
            {
                yield return null;
                timer += Time.deltaTime;

                m_target.localScale = _originSize * (1 - timer / m_effectTime);
            }
            m_target.localScale = Vector3.one * 0.001f;
        }
    }
}
