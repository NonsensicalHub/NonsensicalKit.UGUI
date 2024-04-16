using NonsensicalKit.Core.Table;
using NonsensicalKit.Tools.InputTool;
using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.UGUI.Samples.Table
{
    public class MultilevelMenuDemo : MonoBehaviour
    {
        [SerializeField] private MultilevelMenu m_menu;

        private bool _flag = true;

        private void Awake()
        {
            m_menu.Init(new List<MultilevelMenuInfo>() {
            new MultilevelMenuInfo ("1",OnSelect ),
            new MultilevelMenuInfo ("2",OnSelect ),
            new MultilevelMenuInfo ("3", OnSelect),
            new MultilevelMenuInfo ("4/5",OnSelect),
            new MultilevelMenuInfo ("4/6/7",OnSelect),
            new MultilevelMenuInfo ("Step1",OnSelect,Check),
            new MultilevelMenuInfo ("Step2",OnSelect,Check),
        });
        }

        private void OnSelect(MultilevelContext context)
        {
            Debug.Log(context.Path);
            if (context.Path == "STEP1")
            {
                _flag = false;
            }
        }

        private bool Check(MultilevelContext context)
        {
            switch (context.Path)
            {
                case "Step2": return !_flag;
                default: return true;
            }
        }

        private void Start()
        {
            InputHub.Instance.OnMouseRightButtonUp += Demo;
        }

        private void OnDestroy()
        {
            InputHub.Instance.OnMouseRightButtonUp -= Demo;
        }

        private void Demo()
        {
            m_menu.Open();
            m_menu.transform.position = InputHub.Instance.CrtMousePos;
        }
    }
}
