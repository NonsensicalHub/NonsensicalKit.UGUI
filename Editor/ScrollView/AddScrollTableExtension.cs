using NonsensicalKit.UGUI.Table;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Editor.Table
{
    public class AddScrollTableExtension
    {
        private const string BG_PATH = "UI/Skin/Background.psd";
        private const string SPRITE_PATH = "UI/Skin/UISprite.psd";
        private const string MASK_PATH = "UI/Skin/UIMask.psd";
        private readonly static Color _panelColor = new Color(1f, 1f, 1f, 0.392f);
        private readonly static Color _borderLineColor = new Color(0f, 0f, 0f, 1f);
        private readonly static Color _defaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        private readonly static Vector2 _thinElementSize = new Vector2(160f, 20f);

        [MenuItem("GameObject/Nonsensical/UI/ScrollTable", false, 90)]
        public static void AddScrollView(MenuCommand menuCommand)
        {
            InternalAddScrollView<ScrollTable>(menuCommand);
        }

        protected static void InternalAddScrollView<T>(MenuCommand menuCommand) where T : ScrollTable
        {
            GameObject root = CreateUIElementRoot(typeof(T).Name, new Vector2(200, 200));
            GameObject viewport = CreateUIObject("Viewport", root);
            GameObject content = CreateUIObject("Content", viewport);
            GameObject borderLine = CreateUIObject("borderLine", content);

            GameObject parent = menuCommand.context as GameObject;
            if (parent != null)
            {
                root.transform.SetParent(parent.transform, false);
            }

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
            contentRect.SetTopLeft(Vector2.zero,new Vector2(200,200));
            
            RectTransform borderLineRect = borderLine.GetComponent<RectTransform>();
            borderLineRect.Stretch();
            
            Image borderLineImage = borderLine.AddComponent<Image>();
            borderLineImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BG_PATH);
            borderLineImage.type = Image.Type.Sliced;
            borderLineImage.color = _borderLineColor;

            ScrollTable scrollTable = root.AddComponent<T>();
            scrollTable.content = contentRect;
            scrollTable.viewport = viewportRect;
            scrollTable.horizontalScrollbar = hScrollbar.GetComponent<Scrollbar>();
            scrollTable.verticalScrollbar = vScrollbar.GetComponent<Scrollbar>();
            scrollTable.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollTable.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollTable.horizontalScrollbarSpacing = -3;
            scrollTable.verticalScrollbarSpacing = -3;
            scrollTable.BorderLineRect = borderLineRect;

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

        static GameObject CreateScrollbar()
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

        static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }
    }
}
