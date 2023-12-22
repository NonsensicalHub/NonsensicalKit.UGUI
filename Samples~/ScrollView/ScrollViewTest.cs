using NonsensicalKit.Core;
using NonsensicalKit.Core.Table;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NonsensicalKit.Samples
{
    public class ScrollViewTest : MonoBehaviour
    {
        [SerializeField] private ScrollView m_scrollView;

        private List<string> _test;

        private void Start()
        {
            NonsensicalInstance.Instance.DelayDoIt(0, S);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_scrollView.ScrollTo(Random.Range(0, _test.Count));
            }
        }

        private void S()
        {
            // 构造测试数据
            InitData();

            m_scrollView.SetUpdateFunc((index, rectTransform) =>
            {
                rectTransform.GetComponentInChildren<TextMeshProUGUI>().text = _test[index];
            });

            m_scrollView.SetItemCountFunc(() =>
            {
                return _test.Count;
            });

            m_scrollView.UpdateData(false);
        }

        private void InitData()
        {
            _test = new List<string>();
            for (int i = 1; i <= 10000; ++i)
            {
                _test.Add(i.ToString());
            }
        }
    }
}
