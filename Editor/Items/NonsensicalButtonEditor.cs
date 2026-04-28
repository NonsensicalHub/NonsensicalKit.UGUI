using UnityEditor;
using UnityEditor.UI;

namespace NonsensicalKit.UGUI.Editor
{
    [CustomEditor(typeof(NonsensicalButton), true)]
    [CanEditMultipleObjects]
    public class NonsensicalButtonEditor : ButtonEditor
    {
        private SerializedProperty _minimumInteractionInterval;
        private SerializedProperty _doubleClickThreshold;
        private SerializedProperty _onDoubleClick;
        private SerializedProperty _property;

        protected override void OnEnable()
        {
            base.OnEnable();
            _minimumInteractionInterval = serializedObject.FindProperty("m_minimumInteractionInterval");
            _doubleClickThreshold = serializedObject.FindProperty("m_doubleClickInterval");
            _onDoubleClick = serializedObject.FindProperty("m_onDoubleClick");
            _property = serializedObject.FindProperty("m_interactionMouseButton");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            serializedObject.Update();
            EditorGUILayout.PropertyField(_onDoubleClick, true);
            EditorGUILayout.PropertyField(_minimumInteractionInterval);
            EditorGUILayout.PropertyField(_doubleClickThreshold);
            EditorGUILayout.PropertyField(_property);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
