using System;
using System.Collections.Generic;
using NonsensicalKit.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI
{
    /// <summary>
    /// UGUI工具类
    /// </summary>
    public static class UGUIHelper
    {
        /// <summary>
        /// 延迟一帧将Scrollbar升至顶部（自下到上时）
        /// </summary>
        public static void DelayTopping(Scrollbar scrollbar)
        {
            NonsensicalInstance.Instance.DelayDoIt(0, () => { scrollbar.value = 1; });
        }

        /// <summary>
        /// 使用可枚举对象初始化下拉菜单
        /// </summary>
        public static void InitDropDown<T>(this TMP_Dropdown dropDown, IEnumerable<T> values)
        {
            List<TMP_Dropdown.OptionData> modelNames = new List<TMP_Dropdown.OptionData>();
            foreach (var item in values)
            {
                modelNames.Add(new TMP_Dropdown.OptionData(item.ToString()));
            }

            dropDown.options = modelNames;
        }

        /// <summary>
        /// 使用枚举初始化下拉菜单
        /// </summary>
        public static void InitDropDown<T>(this TMP_Dropdown dropDown) where T : Enum
        {
            List<TMP_Dropdown.OptionData> modelNames = new List<TMP_Dropdown.OptionData>();
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                modelNames.Add(new TMP_Dropdown.OptionData(item.ToString()));
            }

            dropDown.options = modelNames;
        }

        /// <summary>
        /// 使用可枚举对象初始化下拉菜单
        /// </summary>
        public static void InitDropDown<T>(this Dropdown dropDown, IEnumerable<T> values)
        {
            List<Dropdown.OptionData> modelNames = new List<Dropdown.OptionData>();
            foreach (var item in values)
            {
                modelNames.Add(new Dropdown.OptionData(item.ToString()));
            }

            dropDown.options = modelNames;
        }

        /// <summary>
        /// 使用枚举初始化下拉菜单
        /// </summary>
        public static void InitDropDown<T>(this Dropdown dropDown) where T : Enum
        {
            List<Dropdown.OptionData> modelNames = new List<Dropdown.OptionData>();
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                modelNames.Add(new Dropdown.OptionData(item.ToString()));
            }

            dropDown.options = modelNames;
        }

        /// <summary>
        /// 设置锚点同保持位置不变
        /// </summary>
        public static void SetAnchors(this RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            Vector3 lp = rt.localPosition;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.localPosition = lp;
        }

        /// <summary>
        /// 设置中心点同时保持位置不变
        /// </summary>
        public static void SetPivot(this RectTransform rt, Vector2 pivot)
        {
            Vector3 deltaPosition = rt.pivot - pivot; // get change in pivot
            deltaPosition.Scale(rt.rect.size); // apply sizing
            deltaPosition.Scale(rt.localScale); // apply scaling
            deltaPosition = rt.rotation * deltaPosition; // apply rotation

            rt.pivot = pivot; // change the pivot
            rt.localPosition -= deltaPosition; // reverse the position change
        }

        /// <summary>
        /// 设置成拼接至左上角的样式
        /// </summary>
        public static void SetTopLeft(this RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            SetPivot(rt, new Vector2(0, 1));
        }

        /// <summary>
        /// 设置成以左上角为锚点，固定偏移和大小的样式
        /// </summary>
        public static void SetTopLeft(this RectTransform rt, Vector2 position, Vector2 size)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            SetPivot(rt, new Vector2(0, 1));

            rt.sizeDelta = size;
            rt.anchoredPosition = position;
        }
        
        public static void SetRect(this RectTransform rt, Rect rect)
        {
            rt.sizeDelta = new Vector2(rect.width, rect.height);
            rt.anchoredPosition3D = new Vector3(rect.x, rect.y,0);
        }
        
        /// <summary>
        /// 设置成横向占满，移至底部，高度固定的样式
        /// </summary>
        public static void SetBottomHeight(this RectTransform rt, float height)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1, 0);
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, height);
        }

        /// <summary>
        /// 伸展UI占满父节点
        /// </summary>
        public static void Stretch(this RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void StretchWithBottomInterval(this RectTransform rt, float interval)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(0, interval);
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 获取世界坐标系下的min点和max点坐标
        /// </summary>
        public static void GetWorldMinMax(this RectTransform rt, ref Vector3[] minMaxArray)
        {
            if (minMaxArray == null || minMaxArray.Length < 2)
            {
                Debug.LogError("Calling GetWorldMinMax with an array that is null or has less than 2 elements.");
                return;
            }

            rt.GetLocalMinMax(ref minMaxArray);
            Matrix4x4 matrix4X = rt.transform.localToWorldMatrix;
            for (int i = 0; i < 2; i++)
            {
                minMaxArray[i] = matrix4X.MultiplyPoint(minMaxArray[i]);
            }
        }

        public static void GetLocalMinMax(this RectTransform rt, ref Vector3[] minMaxArray)
        {
            if (minMaxArray == null || minMaxArray.Length < 2)
            {
                Debug.LogError("Calling GetLocalCorners with an array that is null or has less than 2 elements.");
                return;
            }

            Rect rect = rt.rect;
            float x = rect.x;
            float y = rect.y;
            float xMax = rect.xMax;
            float yMax = rect.yMax;
            minMaxArray[0] = new Vector3(x, y, 0f);
            minMaxArray[1] = new Vector3(xMax, yMax, 0f);
        }
    }
}
