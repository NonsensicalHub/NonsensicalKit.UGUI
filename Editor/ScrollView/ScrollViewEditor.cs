using NonsensicalKit.Editor.Table;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace NonsensicalKit.Editor.UGUI.Table
{
    [CustomEditor(typeof(ScrollView))]
    public class ScrollViewEditor : ScrollRectEditor
    {
        private SerializedProperty _defaultItemSize;
        private SerializedProperty _layoutType;

        private SerializedProperty _useDefaultPool;
        private SerializedProperty _itemTemplate;
        private SerializedProperty _poolSize;

        private GUIStyle _caption;
        private GUIStyle _Caption
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
            _defaultItemSize = serializedObject.FindProperty("m_defaultItemSize");
            _layoutType = serializedObject.FindProperty("m_layoutType");

            _useDefaultPool = serializedObject.FindProperty("m_useDefaultPool");
            _itemTemplate = serializedObject.FindProperty("m_itemTemplate");
            _poolSize = serializedObject.FindProperty("m_poolSize");

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("<b>新增配置项</b>", _Caption);
            EditorGUILayout.Space(5);
            DrawBaseConfig();
            EditorGUILayout.LabelField("对象池配置", _Caption);
            EditorGUILayout.Space(5);
            DrawPoolConfig();
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();


            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("<b>ScrollRect原始配置项</b>", _Caption);
            EditorGUILayout.Space(5);
            base.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }

        protected virtual void DrawBaseConfig()
        {
            EditorGUILayout.PropertyField(_defaultItemSize);
            EditorGUILayout.PropertyField(_layoutType);
        }

        protected virtual void DrawPoolConfig()
        {
            EditorGUILayout.PropertyField(_useDefaultPool);
            if (_useDefaultPool.boolValue)
            {
                EditorGUILayout.PropertyField(_itemTemplate);
                EditorGUILayout.PropertyField(_poolSize);
            }
        }
    }
}
