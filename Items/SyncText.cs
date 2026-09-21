using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class SyncText : MonoBehaviour
{
    [Serializable]
    public class TextItem
    {
        public TMP_Text text;
        [Tooltip("节点改名时，同步修改此文本内容")]
        public bool syncFromName;
    }

    [SerializeField] private List<TextItem> m_textItems = new List<TextItem>();
    [SerializeField, Tooltip("勾选后，修改文本内容会同步到本节点名称")]
    private bool m_syncNameFromText;

#if UNITY_EDITOR
    private void Reset()
    {
        CollectTexts();
        for (int i = 0; i < m_textItems.Count; i++)
            m_textItems[i].syncFromName = i == 0;
    }

    [ContextMenu("重新收集 TMP 文本")]
    public void CollectTexts()
    {
        TMP_Text[] found = GetComponentsInChildren<TMP_Text>(true);
        var oldItems = new List<TextItem>(m_textItems);
        bool wasEmpty = oldItems.Count == 0;

        m_textItems.Clear();
        for (int i = 0; i < found.Length; i++)
        {
            TMP_Text tmp = found[i];
            bool sync = false;
            for (int j = 0; j < oldItems.Count; j++)
            {
                if (oldItems[j] != null && oldItems[j].text == tmp)
                {
                    sync = oldItems[j].syncFromName;
                    break;
                }
            }

            m_textItems.Add(new TextItem { text = tmp, syncFromName = sync });
        }

        if (wasEmpty && m_textItems.Count > 0)
            m_textItems[0].syncFromName = true;

        EditorUtility.SetDirty(this);
    }

    public void ApplyNameToTexts()
    {
        string nodeName = gameObject.name;
        for (int i = 0; i < m_textItems.Count; i++)
        {
            TextItem item = m_textItems[i];
            if (item == null || !item.syncFromName || item.text == null)
                continue;
            if (item.text.text != nodeName)
                item.text.text = nodeName;
        }
    }

    public void TryApplyTextToName(TMP_Text source)
    {
        if (!m_syncNameFromText || source == null || string.IsNullOrEmpty(source.text))
            return;

        for (int i = 0; i < m_textItems.Count; i++)
        {
            TextItem item = m_textItems[i];
            if (item == null || !item.syncFromName || item.text != source)
                continue;

            if (gameObject.name != source.text)
                gameObject.name = source.text;
            return;
        }
    }
#endif
}

#if UNITY_EDITOR
[InitializeOnLoad]
static class SyncTextTracker
{
    static bool s_applying;

    static SyncTextTracker()
    {
        ObjectChangeEvents.changesPublished += OnChanges;
    }

    static void OnChanges(ref ObjectChangeEventStream stream)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || s_applying)
            return;

        s_applying = true;
        try
        {
            for (int i = 0; i < stream.length; i++)
            {
                if (stream.GetEventType(i) != ObjectChangeKind.ChangeGameObjectOrComponentProperties)
                    continue;

                stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var evt);
                HandlePropertyChange(evt.instanceId);
            }
        }
        finally
        {
            s_applying = false;
        }
    }

    static void HandlePropertyChange(int instanceId)
    {
        UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceId);
        if (obj == null)
            return;

        if (obj is GameObject go)
        {
            SyncText sync = go.GetComponent<SyncText>();
            if (sync != null)
                sync.ApplyNameToTexts();
            return;
        }

        if (obj is TMP_Text tmp)
        {
            SyncText sync = tmp.GetComponentInParent<SyncText>();
            if (sync != null)
                sync.TryApplyTextToName(tmp);
        }
    }
}

[CustomEditor(typeof(SyncText))]
[CanEditMultipleObjects]
public class SyncTextEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty syncNameFromText = serializedObject.FindProperty("m_syncNameFromText");
        SerializedProperty items = serializedObject.FindProperty("m_textItems");

        EditorGUILayout.PropertyField(syncNameFromText, new GUIContent("文本修改时同步节点名"));

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("节点下的 TMP 文本", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("勾选「同步」的文本会在节点改名时一起修改。默认勾选第一个。新增文本后请点下方按钮重新收集。", MessageType.None);

        for (int i = 0; i < items.arraySize; i++)
        {
            SerializedProperty item = items.GetArrayElementAtIndex(i);
            SerializedProperty textProp = item.FindPropertyRelative("text");
            SerializedProperty flagProp = item.FindPropertyRelative("syncFromName");

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(textProp, GUIContent.none);
            EditorGUI.EndDisabledGroup();
            flagProp.boolValue = EditorGUILayout.ToggleLeft("同步", flagProp.boolValue, GUILayout.Width(56));
            EditorGUILayout.EndHorizontal();
        }

        if (items.arraySize == 0)
            EditorGUILayout.HelpBox("未找到 TMP 文本组件。", MessageType.Warning);

        EditorGUILayout.Space(4);
        if (GUILayout.Button("重新收集 TMP 文本"))
        {
            foreach (UnityEngine.Object t in targets)
            {
                var syncText = (SyncText)t;
                Undo.RecordObject(syncText, "重新收集 TMP 文本");
                syncText.CollectTexts();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
