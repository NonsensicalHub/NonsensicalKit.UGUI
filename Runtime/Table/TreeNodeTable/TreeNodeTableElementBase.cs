using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Editor.Table
{
    public abstract class TreeNodeTableElementBase<ElementDataClass> : NonsensicalUI where ElementDataClass : class, ITreeNodeClass<ElementDataClass>
    {
        [SerializeField] protected Button m_btn_Collapsed;       //已经收起时显示的按钮，点击后展开
        [SerializeField] protected Button m_btn_Expanded;        //已经展开时显示的按钮，点击后收起
        [SerializeField] protected RectTransform m_rt_Box;

        public TreeNodeTableManager<TreeNodeTableElementBase<ElementDataClass>, ElementDataClass> IManager;

        //每一级子节点往右移动多少距离（单位：像素）
        public int LevelDistance { get; set; }

        public ElementDataClass ElementData { get; set; }
        protected bool _IsFold { get { return ElementData.IsFold; } set { ElementData.IsFold = value; } }

        /// <summary>
        /// box一开始坐标值，作为位移的基准值
        /// </summary>
        protected Vector2 _basePosition;

        protected override void Awake()
        {
            base.Awake();
            _basePosition = m_rt_Box.anchoredPosition;

            m_btn_Collapsed.onClick.AddListener(OnUnfoldButtonClick);
            m_btn_Expanded.onClick.AddListener(OnFoldButtonClick);
        }

        public virtual void SetValue(ElementDataClass elementData)
        {
            this.ElementData = elementData;
            elementData.Belong = gameObject;
            m_rt_Box.anchoredPosition = new Vector2(_basePosition.x + elementData.Level * LevelDistance, _basePosition.y);
            UpdateFoldUI();
        }


        protected virtual void OnFoldButtonClick()
        {
            IManager. DoFold(this, true);
        }

        protected virtual void OnUnfoldButtonClick()
        {
            IManager.DoFold(this, false);
        }

        public virtual void OnFocus()
        {

        }

        public virtual void UpdateFoldUI()
        {
            if (ElementData.Childs.Count != 0)
            {
                m_btn_Collapsed.gameObject.SetActive(_IsFold);
                m_btn_Expanded.gameObject.SetActive(!_IsFold);
            }
            else
            {
                m_btn_Collapsed.gameObject.SetActive(false);
                m_btn_Expanded.gameObject.SetActive(false);
            }
        }
    }
}
