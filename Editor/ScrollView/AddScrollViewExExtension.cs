using NonsensicalKit.UGUI.Table;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Editor.Table
{
    public class AddScrollViewExExtension
    {
        private const string BG_PATH = "UI/Skin/Background.psd";
        private const string MASK_PATH = "UI/Skin/UIMask.psd";
        private static Color _panelColor = new Color(1f, 1f, 1f, 0.392f);

        [MenuItem("GameObject/Nonsensical/UI/ScrollViewEx", false, 90)]
        public static void AddScrollView(MenuCommand menuCommand)
        {
            InternalAddScrollViewEx(menuCommand);
        }

        protected static void InternalAddScrollViewEx(MenuCommand menuCommand)
        {
            GameObject root = CreateUIElementRoot(typeof(ScrollViewEx).Name, new Vector2(200, 200));
            GameObject viewport = CreateUIObject("Viewport", root);
            GameObject content = CreateUIObject("Content", viewport);

            GameObject parent = menuCommand.context as GameObject;
            if (parent != null)
            {
                root.transform.SetParent(parent.transform, false);
            }

            Selection.activeGameObject = root;

            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = Vector2.up;

            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.up;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.pivot = Vector2.up;

            ScrollView scrollRect = root.AddComponent<ScrollViewEx>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;

            Image rootImage = root.AddComponent<Image>();
            rootImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BG_PATH);
            rootImage.type = Image.Type.Sliced;
            rootImage.color = _panelColor;

            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(MASK_PATH);
            viewportImage.type = Image.Type.Sliced;
        }

        static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        static GameObject CreateUIObject(string name, GameObject parent)
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

        static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
    }
}
