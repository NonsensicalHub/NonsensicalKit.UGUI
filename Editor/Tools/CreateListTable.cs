using System;
using System.IO;
using NonsensicalKit.Tools;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Editor.Tools
{
    public class CreateListTable : EditorWindow
    {
        [MenuItem("Assets/Create/NonsensicalKit/UGUI/CreateListTable", false, 100)]
        public static void ShowWindow()
        {
            var crtPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            Debug.Log(Path.Combine(Application.dataPath, "..", crtPath));
            var path = FileTool.FileSaveSelector("prefab", Path.Combine(Application.dataPath, "..", crtPath), "prefab");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Unable to create ListTable because no file path has been selected.");
                return;
            }

            path = path.Replace('\\', '/');
            if (path.Contains(Application.dataPath) == false)
            {
                Debug.Log("Unable to create ListTable because the selected file path is not within the Assets folder");
                return;
            }

            DoCreateListTable(path);
        }

        private static string ManagerTemplate =
            @"using NonsensicalKit.UGUI.Table;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class #ManagerName# : ListTableManager<#ElementName#,#InfoName#>
{

}
";

        private static string ElementTemplate =
            @"using NonsensicalKit.UGUI.Table;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class #ElementName# : ListTableElement<#InfoName#>
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void SetValue(#InfoName# elementData)
    {
        base.SetValue(elementData);
    }
}
public class #InfoName#
{

}
";

        private static void DoCreateListTable(string prefabPath)
        {
            string folderPath = Path.GetDirectoryName(prefabPath);
            string name = Path.GetFileNameWithoutExtension(prefabPath);
            string managerPath = Path.Combine(folderPath, name + "Manager.cs");
            string elementPath = Path.Combine(folderPath, name + "Element.cs");

            string managerText = ManagerTemplate
                .Replace("#ManagerName#", name + "Manager")
                .Replace("#ElementName#", name + "Element")
                .Replace("#InfoName#", name + "Info");
            File.WriteAllText(managerPath, managerText);
            string elementText = ElementTemplate
                .Replace("#ElementName#", name + "Element")
                .Replace("#InfoName#", name + "Info");
            File.WriteAllText(elementPath, elementText);

            EditorPrefs.SetString("NonsensicalKit_CreateListTablePath", prefabPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }

        [DidReloadScripts]
        private static void CreateListTablePrefab()
        {
            var prefabPath = EditorPrefs.GetString("NonsensicalKit_CreateListTablePath", string.Empty);

            if (string.IsNullOrEmpty(prefabPath))
            {
                return;
            }

            EditorPrefs.SetString("NonsensicalKit_CreateListTablePath", string.Empty);

            string name = Path.GetFileNameWithoutExtension(prefabPath);
            string folderPath = "Assets" + Path.GetDirectoryName(prefabPath).Substring(Application.dataPath.Length);
            string managerPath = Path.Combine(folderPath, name + "Manager.cs");
            string elementPath = Path.Combine(folderPath, name + "Element.cs");


            MonoScript managerScript = (MonoScript)AssetDatabase.LoadAssetAtPath(managerPath, typeof(MonoScript));
            MonoScript elementScript = (MonoScript)AssetDatabase.LoadAssetAtPath(elementPath, typeof(MonoScript));

            GameObject go = CreateNewListTable(name, managerScript.GetClass(), elementScript.GetClass());
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
        }

        private const string BG_PATH = "UI/Skin/Background.psd";
        private const string SPRITE_PATH = "UI/Skin/UISprite.psd";
        private const string MASK_PATH = "UI/Skin/UIMask.psd";
        private readonly static Color _panelColor = new Color(1f, 1f, 1f, 0.392f);
        private readonly static Color _defaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        private readonly static Vector2 _thinElementSize = new Vector2(160f, 20f);


        private static GameObject CreateNewListTable(string name, Type managerType, Type elementType)
        {
            GameObject root = CreateUIElementRoot(name, new Vector2(200, 200));
            GameObject viewport = CreateUIObject("Viewport", root);
            GameObject content = CreateUIObject("Content", viewport);

            Selection.activeGameObject = root;

            GameObject hScrollbar = CreateScrollbar();
            hScrollbar.name = "Scrollbar Horizontal";
            hScrollbar.transform.SetParent(root.transform, false);
            RectTransform hScrollbarRT = hScrollbar.GetComponent<RectTransform>();
            hScrollbarRT.anchorMin = Vector2.zero;
            hScrollbarRT.anchorMax = Vector2.right;
            hScrollbarRT.pivot = Vector2.zero;
            hScrollbarRT.sizeDelta = new Vector2(0, hScrollbarRT.sizeDelta.y);

            GameObject vScrollbar = CreateScrollbar();
            vScrollbar.name = "Scrollbar Vertical";
            vScrollbar.transform.SetParent(root.transform, false);
            vScrollbar.GetComponent<Scrollbar>().SetDirection(Scrollbar.Direction.BottomToTop, true);
            RectTransform vScrollbarRT = vScrollbar.GetComponent<RectTransform>();
            vScrollbarRT.anchorMin = Vector2.right;
            vScrollbarRT.anchorMax = Vector2.one;
            vScrollbarRT.pivot = Vector2.one;
            vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = Vector2.up;

            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.up;
            contentRect.anchorMax = Vector2.up;
            contentRect.sizeDelta = new Vector2(200, 50);
            contentRect.pivot = Vector2.up;

            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Image rootImage = root.AddComponent<Image>();
            rootImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BG_PATH);
            rootImage.type = Image.Type.Sliced;
            rootImage.color = _panelColor;

            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(MASK_PATH);
            viewportImage.type = Image.Type.Sliced;

            ScrollRect scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.horizontalScrollbar = hScrollbar.GetComponent<Scrollbar>();
            scrollRect.verticalScrollbar = vScrollbar.GetComponent<Scrollbar>();
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            Component manager = root.AddComponent(managerType);
            SerializedObject serializedObject = new SerializedObject(manager);
            SerializedProperty group = serializedObject.FindProperty("m_group");
            group.objectReferenceValue = contentRect;
            serializedObject.ApplyModifiedProperties();

            GameObject elementGO = CreateUIObject("Element", content);
            RectTransform rectTransform = elementGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
            elementGO.AddComponent(elementType);

            return root;
        }


        private static GameObject CreateScrollbar()
        {
            // Create GOs Hierarchy
            GameObject scrollbarRoot = CreateUIElementRoot("Scrollbar", _thinElementSize);
            GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot);
            GameObject handle = CreateUIObject("Handle", sliderArea);

            Image bgImage = scrollbarRoot.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BG_PATH);
            bgImage.type = Image.Type.Sliced;
            bgImage.color = _defaultSelectableColor;

            Image handleImage = handle.AddComponent<Image>();
            handleImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(SPRITE_PATH);
            handleImage.type = Image.Type.Sliced;
            handleImage.color = _defaultSelectableColor;

            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
            sliderAreaRect.sizeDelta = new Vector2(-20, -20);
            sliderAreaRect.anchorMin = Vector2.zero;
            sliderAreaRect.anchorMax = Vector2.one;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            Scrollbar scrollbar = scrollbarRoot.AddComponent<Scrollbar>();
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            SetDefaultColorTransitionValues(scrollbar);

            return scrollbarRoot;
        }

        private static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            child.AddComponent<CanvasRenderer>();
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        private static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        private static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }
    }
}
