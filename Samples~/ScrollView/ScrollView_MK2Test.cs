using NonsensicalKit.Core;
using NonsensicalKit.Core.Table;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NonsensicalKit.UGUI.Samples.Table
{
    public class ScrollView_MK2Test : MonoBehaviour
    {
        [SerializeField] private ScrollView_MK2 m_scrollView_MK2;

        private List<string> _test;

        private void Start()
        {
            NonsensicalInstance.Instance.DelayDoIt(0, S);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_scrollView_MK2.ScrollTo(Random.Range(0, _test.Count));
            }
        }

        private void S()
        {
            // 构造测试数据
            InitData();

            m_scrollView_MK2.SetUpdateFunc((index, rectTransform) =>
            {
                rectTransform.GetComponentInChildren<TextMeshProUGUI>().text = _test[index];
            });

            m_scrollView_MK2.SetItemCountFunc(() =>
            {
                return _test.Count;
            });

            m_scrollView_MK2.UpdateData(false);
        }

        private void InitData()
        {
            _test = new List<string>();
            for (int i = 1; i <= 123456; ++i)
            {
                _test.Add(i.ToString());
            }
        }
    }
}
