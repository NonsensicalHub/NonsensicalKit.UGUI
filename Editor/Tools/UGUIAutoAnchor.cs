using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Editor.Tools
{
    /// <summary>
    /// 将对象的锚点设定为刚好符合大小和位置
    /// </summary>
    public class UGUIAutoAnchor : EditorWindow
    {
        [MenuItem("NonsensicalKit/UGUI/自动自适应锚点")]
        private static void AddComponentToCrtTargetWithChildren()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.Log("未选中任何对象");
            }
            else
            {
                foreach (var t in Selection.gameObjects)
                {
                    AutoSet(t);
                }
            }
        }

        private static void AutoSet(GameObject target)
        {
            RectTransform item = target.GetComponent<RectTransform>();
            if (item == null)
            {
                return;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(item) //跳过预制体对象
                || item.GetComponent<ContentSizeFitter>() != null //跳过自适应尺寸对象
                || item.parent == null || item.parent.GetComponent<LayoutGroup>() != null) //跳过被LayoutGroup管理的对象
            {
                return;
            }

            var partentRT = item.parent.GetComponent<RectTransform>();
            if (partentRT == null)
            {
                return;
            }

            var partentRect = partentRT.rect;

            var v = item.anchorMin * partentRect.size + item.offsetMin;
            var v2 = item.anchorMax * partentRect.size + item.offsetMax;

            if (partentRect.size.x == 0 || partentRect.size.y == 0)
            {
                return;
            }

            Undo.RecordObject(item, item.name);

            item.anchorMin = v / partentRect.size;
            item.anchorMax = v2 / partentRect.size;
            item.offsetMin = Vector2.zero;
            item.offsetMax = Vector2.zero;

            EditorUtility.SetDirty(item);
        }
    }
}
