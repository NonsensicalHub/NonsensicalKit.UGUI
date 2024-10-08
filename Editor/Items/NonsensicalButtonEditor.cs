using UnityEditor;
using UnityEditor.UI;

namespace NonsensicalKit.UGUI.Editor
{
    [CustomEditor(typeof(NonsensicalButton), true)]
    [CanEditMultipleObjects]
    public class NonsensicalButtonEditor : ButtonEditor
    {
        private SerializedProperty m_minimumInteractionInterval;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_minimumInteractionInterval = serializedObject.FindProperty("m_minimumInteractionInterval");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_minimumInteractionInterval);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
