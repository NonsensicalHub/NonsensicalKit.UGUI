
using NonsensicalKit.Core.Service;
using NonsensicalKit.Core.Service.Setting;
using UnityEngine;
#if TEXTMESHPRO_PRESENT
using TMPro;
#else
using UnityEngine.UI;
#endif

namespace NonsensicalKit.UGUI.Setting
{
    public class DropdownSetting : MonoBehaviour
    {
        [SerializeField] private string m_settingName = "lightShadow";
        [SerializeField] private string[] m_values = new[] { "无阴影", "硬阴影", "软阴影" };

#if TEXTMESHPRO_PRESENT
        [SerializeField] private TMP_Dropdown m_dropdown;
#else
        [SerializeField] private Dropdown m_dropdown;
#endif
        private SettingService _service;

        private bool _dontInvokeFlag;

        private void Awake()
        {
            m_dropdown.onValueChanged.AddListener(OnDropDownChanged);
            m_dropdown.InitDropDown(m_values);

            ServiceCore.SafeGet<SettingService>(OnGetService);
        }


        private void OnGetService(SettingService service)
        {
            _service = service;
            _service.AddSettingListener(m_settingName, OnSettingChanged);
            OnSettingChanged(_service.GetSettingValue(m_settingName));
        }

        private void OnDropDownChanged(int index)
        {
            if (_dontInvokeFlag)
            {
                //初始化或者其他地方修改设置时也会触发，此时不应该调用修改方法
                _dontInvokeFlag = false;
            }
            else
            {
                _service.SetSetting(m_settingName, index.ToString());
            }
        }

        private void OnSettingChanged(string value)
        {
            if (int.TryParse(value, out int i))
            {
                if (i >= 0 && i <= m_values.Length && i != m_dropdown.value)
                {
                    _dontInvokeFlag = true;
                    m_dropdown.value = i;
                }
            }
        }
    }
}
