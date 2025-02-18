using TMPro;
using UnityEngine;

namespace NonsensicalKit.UGUI.Table
{
    public class MissionProcessElement : ListTableElement<MissionProcessInfo>
    {
        [SerializeField] private TextMeshProUGUI m_txt_missionName;
        [SerializeField] private TextMeshProUGUI m_txt_process;
        [SerializeField] private RectTransform m_process;

        public override void SetValue(MissionProcessInfo elementData)
        {
            base.SetValue(elementData);

            m_txt_missionName.text = elementData.MissionName;
            m_txt_process.text = (elementData.Process * 100).ToString("f2") + "%";
            m_process.anchorMax = new Vector2(elementData.Process, 1);
            m_process.offsetMax = Vector2.zero;
        }
    }
}
