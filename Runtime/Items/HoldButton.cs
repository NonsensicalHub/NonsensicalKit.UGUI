using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace NonsensicalKit.UGUI
{
    /// <summary>
    /// 长按一段时间才会触发的按钮    
    /// </summary>
    public class HoldButton : Selectable, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        public class ButtonHoldEvent : UnityEvent { }

        [FormerlySerializedAs("onHold")]
        [SerializeField]
        private ButtonHoldEvent m_OnHold = new ButtonHoldEvent();

        [SerializeField]
        private float m_interval = 0.5f;

        private float _timer;

        private bool _isHold = false;

        public ButtonHoldEvent OnHold
        {
            get { return m_OnHold; }
            set { m_OnHold = value; }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_isHold && _timer > m_interval)
            {
                m_OnHold.Invoke();
                _timer = 0;
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            _timer = m_interval;
            _isHold = true;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            _isHold = false;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            _isHold = false;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HoldButton), true)]
    [CanEditMultipleObjects]
    public class HoldButtonEditor : SelectableEditor
    {
        private SerializedProperty onHoldProperty;
        private SerializedProperty interval;

        protected override void OnEnable()
        {
            base.OnEnable();
            onHoldProperty = serializedObject.FindProperty("m_OnHold");
            interval = serializedObject.FindProperty("m_interval");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(onHoldProperty);
            EditorGUILayout.PropertyField(interval);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
