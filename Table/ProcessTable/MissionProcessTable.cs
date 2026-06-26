using System.Collections.Generic;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.Table
{
    public class MissionProcessTable : ListTableManager<MissionProcessElement, MissionProcessInfo>
    {
        [SerializeField] private Button m_btn_close;
        [SerializeField] private bool m_coverMode;  // 是否覆盖模式，覆盖模式下，会根据任务状态自动控制显隐

        private readonly Dictionary<string, MissionProcessInfo> _mission = new();

        private bool _updated;

        protected override void Awake()
        {
            base.Awake();

            Subscribe("SwitchMissionProcessTableWindow", () => SwitchSelf(true));
            Subscribe<string>("transferStart", OnTransferStart);
            Subscribe<string, string>("transferStart", OnTransferStart);
            Subscribe<string>("transferEnd", OnTransferEnd);
            Subscribe<string, float>("transferProcess", OnTransferProcess);

            if (!m_coverMode)
            {
                m_btn_close.onClick.AddListener(CloseSelf);
            }
        }

        private void Update()
        {
            if (_updated)
            {
                _updated = false;
                UpdateUI();
            }
        }

        private void OnTransferStart(string key)
        {
            OnTransferStart(key,key);
        }
        private void OnTransferStart(string key, string missionName)
        {
            if (key is null||_mission.ContainsKey(key))
            {
                return;
            }

            var v = new MissionProcessInfo() { MissionName = missionName };
            _mission.Add(key, v);
            ElementData.Add(v);
            IOCC.Set("missionProcessState",true);
            if (m_coverMode && !IsShow)
            {
                OpenSelf();
            }

            _updated = true;
        }

        private void OnTransferEnd(string key)
        {
            if (key is null||_mission.TryGetValue(key, out var value)==false)
            {
                return;
            }
            
            ElementData.Remove(value);
            _mission.Remove(key);
            if (_mission.Count == 0)
            {
                IOCC.Set("missionProcessState",false);
                if (m_coverMode && IsShow)
                {
                    CloseSelf();
                }
            }

            _updated = true;
        }

        private void OnTransferProcess(string key, float process)
        {
            process=Mathf.Clamp01(process);
            if (_mission.TryGetValue(key, out var value))
            {
                value.Process = process;
            }

            _updated = true;
        }
    }

    public class MissionProcessInfo
    {
        public string MissionName;
        public float Process;
    }
}
