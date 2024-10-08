namespace NonsensicalKit.UGUI.Table
{
    public abstract class ListTableElement<ElementDataClass> : NonsensicalUI where ElementDataClass : class
    {
        public ElementDataClass ElementData { get; private set; }
        public int Index { get; private set; }

        private IListTableManager<ElementDataClass> _listTableManager;

        public void SetValue(IListTableManager<ElementDataClass> listTableManager, int index, ElementDataClass elementData)
        {
            _listTableManager = listTableManager;
            Index = index;
            SetValue(elementData);
        }

        public virtual void SetValue(ElementDataClass elementData)
        {
            this.ElementData = elementData;
        }

        public void DeleteSelf()
        {
            _listTableManager.Delete(ElementData);
        }
    }
}
