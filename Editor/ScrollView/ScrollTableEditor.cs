using NonsensicalKit.UGUI.Table;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace NonsensicalKit.UGUI.Editor.Table
{
    [CustomEditor(typeof(ScrollTable))]
    public class ScrollTableEditor : ScrollRectEditor
    {
        private SerializedProperty m_columnWidth;
        private SerializedProperty m_rowHeight;
        private SerializedProperty m_cellPrefab;
        private SerializedProperty m_defaultWidth;
        private SerializedProperty m_defaultHeight;
        private SerializedProperty m_borderSize;
        private SerializedProperty m_borderLineRect;
        private SerializedProperty m_padding;

        private GUIStyle _caption;

        private GUIStyle Caption
        {
            get
            {
                if (_caption == null)
                {
                    _caption = new GUIStyle { richText = true, alignment = TextAnchor.MiddleCenter };
                    _caption.normal.textColor = Color.green;
                }

                return _caption;
            }
        }

        protected override void OnEnable()
        {
            m_columnWidth = serializedObject.FindProperty("m_columnWidth");
            m_rowHeight = serializedObject.FindProperty("m_rowHeight");
            m_cellPrefab = serializedObject.FindProperty("m_cellPrefab");
            m_defaultWidth = serializedObject.FindProperty("m_defaultWidth");
            m_defaultHeight = serializedObject.FindProperty("m_defaultHeight");
            m_borderSize = serializedObject.FindProperty("m_borderSize");
            m_borderLineRect = serializedObject.FindProperty("m_borderLineRect");
            m_padding = serializedObject.FindProperty("m_padding");

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("<b>新增配置项</b>", Caption);
            EditorGUILayout.Space(5);
            DrawBaseConfig();
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("<b>ScrollRect原始配置项</b>", Caption);
            EditorGUILayout.Space(5);
            base.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }

        protected virtual void DrawBaseConfig()
        {

            EditorGUILayout.PropertyField(m_columnWidth);
            EditorGUILayout.PropertyField(m_rowHeight);

            EditorGUILayout.PropertyField(m_cellPrefab);
            EditorGUILayout.PropertyField(m_defaultWidth);
            EditorGUILayout.PropertyField(m_defaultHeight);
            EditorGUILayout.PropertyField(m_borderSize);
            EditorGUILayout.PropertyField(m_borderLineRect);
            EditorGUILayout.PropertyField(m_padding);
        }
    }
}
