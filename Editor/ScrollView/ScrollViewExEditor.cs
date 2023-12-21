using NonsensicalKit.Editor.Table;
using UnityEditor;

namespace NonsensicalKit.Editor.UGUI.Table
{
    [CustomEditor(typeof(ScrollViewEx))]
    public class ScrollViewExEditor : ScrollViewEditor
    {
       private SerializedProperty _pageSize;

        protected override void OnEnable()
        {
            base.OnEnable();
            _pageSize = serializedObject.FindProperty("m_pageSize");
        }

        protected override void DrawBaseConfig()
        {
            base.DrawBaseConfig();
            EditorGUILayout.PropertyField(_pageSize);
        }
    }
}
