using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace NonsensicalKit.UGUI.Editor.Tools
{
    /// <summary>
    /// 批量修改预制体的字体
    /// 目前存在问题，需要修改多次才能够完全修改完成，且重进项目后仍可能出现未修改的问题，似乎只是SetDirty并不能很好的保存代码的修改
    /// 目前只需要重复修改至修改0个对象，然后重启项目后重复操作，直到重启后第一次修改仍为0次即可，理论上重启第二次就能完成
    /// </summary>
    public class PrefabFontModifier : EditorWindow
    {
        [MenuItem("NonsensicalKit/UGUI/预制体字体修改")]
        public static void ShowWindow()
        {
            GetWindow(typeof(PrefabFontModifier));
        }

        private static class PrefabFontModifierPanel
        {
            public static TMP_FontAsset Font;
        }

        private void OnGUI()
        {
            PrefabFontModifierPanel.Font = (TMP_FontAsset)EditorGUILayout.ObjectField("Font", PrefabFontModifierPanel.Font, typeof(TMP_FontAsset),
                true, GUILayout.MinWidth(100f));

            if (GUILayout.Button("修改"))
            {
                string objPath = Application.dataPath;

                List<GameObject> prefabs = new List<GameObject>();

                var absolutePaths = Directory.GetFiles(objPath, "*.prefab", SearchOption.AllDirectories);

                for (int i = 0; i < absolutePaths.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("提示", "获取预制体中...", (float)i / absolutePaths.Length);

                    string path = "Assets" + absolutePaths[i].Remove(0, objPath.Length);
                    path = path.Replace("\\", "/");

                    GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (prefab != null)
                        prefabs.Add(prefab);
                    else
                        Debug.Log("预制体不存在！ " + path);
                }

                EditorUtility.ClearProgressBar();

                ChangeFont(prefabs, PrefabFontModifierPanel.Font);
            }
        }

        private void ChangeFont(List<GameObject> prefabs, TMP_FontAsset font)
        {
            int count = 0;
            foreach (var prefab in prefabs)
            {
                TextMeshProUGUI[] texts = prefab.gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);

                foreach (var text in texts)
                {
                    if (text.font != font)
                    {
                        //只会处理非预制体部分（即预制体中引用的其他预制体不进行处理，只会处理原始部分）
                        if (PrefabUtility.IsPartOfPrefabInstance(text.gameObject) == false)
                        {
                            Debug.Log("修改了" + PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(text.gameObject) + "的Text组件");
                            count++;
                            text.font = font;
                            EditorUtility.SetDirty(prefab);
                        }
                        //如果属于预制体部分但是有修改则仍要进行处理，因为此时修改引用的预制体时不会跟着修改
                        else
                        {
                            var v = PrefabUtility.GetPropertyModifications(text);
                            if (v != null)
                            {
                                bool flag = false;
                                foreach (var item in v)
                                {
                                    if (item.propertyPath == "m_fontAsset")
                                    {
                                        flag = true;
                                        break;
                                    }
                                }

                                if (flag)
                                {
                                    Debug.Log("修改了" + PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(text.gameObject) + "的Text组件");
                                    count++;
                                    text.font = font;
                                    EditorUtility.SetDirty(prefab);
                                }
                            }
                        }
                    }
                }
            }

            EditorUtility.DisplayDialog("", "设置字体完毕,共修改了" + count + "个对象", "OK");
        }
    }
}
