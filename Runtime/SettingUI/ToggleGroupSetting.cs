using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Core.Service.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Setting
{
    public class ToggleGroupSetting : MonoBehaviour
    {
        [SerializeField] private string m_settingName = "lightShadow";
        [SerializeField] private string[] m_values = new[] { "无阴影", "硬阴影", "软阴影" };

        [SerializeField] private Toggle[] m_toggles;

        private SettingService _service;

        private bool _dontInvokeFlag;
        
        private int _currentIndex;

        private void Awake()
        {
            if (m_values.Length != m_toggles.Length)
            {
                LogCore.Error("设置配置长度和选项长度不符，请检查", gameObject);
            }

            for (int i = 0; i < m_toggles.Length; i++)
            {
                int j = i;
                m_toggles[i].onValueChanged.AddListener((value) =>
                    {
                        if (value)
                        {
                            OnToggleOn(j);
                        }
                    }
                );
            }

            ServiceCore.SafeGet<SettingService>(OnGetService);
        }

        private void OnToggleOn(int index)
        {
            if (_dontInvokeFlag)
            {
                //初始化或者其他地方修改设置时也会触发，此时不应该调用修改方法
                _dontInvokeFlag = false;
            }
            else
            {
                _currentIndex = index;
                _service.SetSetting(m_settingName, index.ToString());
            }
        }

        private void OnGetService(SettingService service)
        {
            _service = service;
            _service.AddSettingListener(m_settingName, OnSettingChanged);
            OnSettingChanged(_service.GetSettingValue(m_settingName));
        }
        
        private void OnSettingChanged(string value)
        {
            if (int.TryParse(value, out int i))
            {
                if (i >= 0 && i <= m_values.Length && i != _currentIndex)
                {
                    _dontInvokeFlag = true;
                    for (int j = 0; j < m_toggles.Length; j++)
                    {
                        m_toggles[j].isOn = i == j;
                    }
                }
            }
        }
    }
}
