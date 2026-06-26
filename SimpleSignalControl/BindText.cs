using System.Globalization;
using TMPro;
using UnityEngine;

namespace NonsensicalKit.Core.SimpleSignalControl
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class BindText : MonoBehaviour
    {
        [SerializeField] private string m_id;
        [SerializeField] private DataSourceType m_dataType;

        private TextMeshProUGUI _txtSelf;

        private void Awake()
        {
            _txtSelf = GetComponent<TextMeshProUGUI>();

            switch (m_dataType)
            {
                case DataSourceType.String:
                    {
                        IOCC.AddListener<string>(m_id, ChangeText);
                        if (IOCC.TryGet<string>(m_id, out var v))
                        {
                            ChangeText(v);
                        }
                    }
                    break;
                case DataSourceType.Int:
                    {
                        IOCC.AddListener<int>(m_id, ChangeText);
                        if (IOCC.TryGet<int>(m_id, out var v))
                        {
                            ChangeText(v);
                        }
                    }
                    break;
                case DataSourceType.Float:
                    {
                        IOCC.AddListener<float>(m_id, ChangeText);
                        if (IOCC.TryGet<float>(m_id, out var v))
                        {
                            ChangeText(v);
                        }
                    }
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (m_dataType)
            {
                case DataSourceType.String:
                    IOCC.RemoveListener<string>(m_id, ChangeText);
                    break;
                case DataSourceType.Int:
                    IOCC.RemoveListener<int>(m_id, ChangeText);
                    break;
                case DataSourceType.Float:
                    IOCC.RemoveListener<float>(m_id, ChangeText);
                    break;
            }
        }

        private void ChangeText(string value)
        {
            _txtSelf.text = value;
        }

        private void ChangeText(float value)
        {
            _txtSelf.text = value.ToString(CultureInfo.InvariantCulture);
        }

        private void ChangeText(int value)
        {
            _txtSelf.text = value.ToString();
        }
    }
}