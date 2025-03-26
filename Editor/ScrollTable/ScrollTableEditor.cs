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
        private SerializedProperty m_columnImagePrefabs;
        private SerializedProperty m_rowImagePrefabs;
        private SerializedProperty m_cellParent;
        private SerializedProperty m_rowParent;
        private SerializedProperty m_columnParent;
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
            m_columnImagePrefabs = serializedObject.FindProperty("m_columnImagePrefabs");
            m_rowImagePrefabs = serializedObject.FindProperty("m_rowImagePrefabs");
            m_cellParent = serializedObject.FindProperty("m_cellParent");
            m_rowParent = serializedObject.FindProperty("m_rowParent");
            m_columnParent = serializedObject.FindProperty("m_columnParent");
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
            EditorGUILayout.PropertyField(m_columnImagePrefabs);
            EditorGUILayout.PropertyField(m_rowImagePrefabs);
            EditorGUILayout.PropertyField(m_cellParent);
            EditorGUILayout.PropertyField(m_rowParent);
            EditorGUILayout.PropertyField(m_columnParent);
            EditorGUILayout.PropertyField(m_defaultWidth);
            EditorGUILayout.PropertyField(m_defaultHeight);
            EditorGUILayout.PropertyField(m_borderSize);
            EditorGUILayout.PropertyField(m_borderLineRect);
            EditorGUILayout.PropertyField(m_padding);
        }
    }
}
