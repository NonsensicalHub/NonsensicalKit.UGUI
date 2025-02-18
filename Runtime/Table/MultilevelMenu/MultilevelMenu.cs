using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.UGUI.Table
{
    public interface IMultilevelMenu
    {
        public MultilevelMenuPanel InstantiatePanel(Transform parent);
        public MultilevelMenuElement InstantiateElement(Transform parent);
        public void Close();
    }

    public class MultilevelMenu : MonoBehaviour, IMultilevelMenu
    {
        [SerializeField] private MultilevelMenuPanel m_panelTemplate;
        [SerializeField] private MultilevelMenuElement m_elementTemplate;

        private MultilevelMenuPanel _panel;

        public MultilevelMenuElement InstantiateElement(Transform parent)
        {
            var newPanel = Instantiate(m_elementTemplate, parent, false);
            var rect = newPanel.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0, 1);
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(0, 0);
            return newPanel;
        }

        public MultilevelMenuPanel InstantiatePanel(Transform parent)
        {
            var newElement = Instantiate(m_panelTemplate, parent, false);
            return newElement;
        }

        public void Close()
        {
            _panel.Close();
        }

        public void Init(List<MultilevelMenuNode> tops)
        {
            if (_panel == null)
            {
                _panel = InstantiatePanel(transform);
            }

            _panel.RootPanel = true;
            _panel.Init(tops, this);
        }

        public void Init(List<MultilevelMenuInfo> infos, char splitChar = '/')
        {
            List<MultilevelMenuNode> tops = new List<MultilevelMenuNode>();

            foreach (var info in infos)
            {
                if (string.IsNullOrEmpty(info.Path))
                {
                    continue;
                }

                string[] paths = info.Path.Split(splitChar);
                var crtList = tops;

                foreach (var p in paths)
                {
                    MultilevelMenuNode next = null;
                    foreach (var item in crtList)
                    {
                        if (string.Equals(p, item.Name))
                        {
                            next = item;
                            break;
                        }
                    }

                    if (next == null)
                    {
                        next = new MultilevelMenuNode(p, info);
                        crtList.Add(next);
                    }

                    crtList = next.Children;
                }
            }

            Sort(tops);

            Init(tops);

            Close();
        }

        public void Open()
        {
            _panel.Open();
        }

        public void Switch()
        {
            if (_panel.gameObject.activeSelf)
            {
                _panel.Close();
            }
            else
            {
                _panel.Open();
            }
        }

        private void Sort(List<MultilevelMenuNode> list)
        {
            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (list[j].MenuInfo.Priority > list[j + 1].MenuInfo.Priority)
                    {
                        // 交换 arr[j] 和 arr[j + 1]
                        (list[j], list[j + 1]) = (list[j + 1], list[j]);
                        swapped = true;
                    }
                }

                // 如果某一趟没有发生交换，说明数组已经有序
                if (!swapped)
                {
                    break;
                }
            }

            foreach (var node in list)
            {
                Sort(node.Children);
            }
        }
    }
}
