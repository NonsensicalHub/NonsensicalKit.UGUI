namespace NonsensicalKit.UGUI.Table
{
    public abstract class ListTableElement<TElementDataClass> : NonsensicalUI where TElementDataClass : class
    {
        public TElementDataClass ElementData { get; private set; }
        public int Index { get; private set; }

        private IListTableManager<TElementDataClass> _listTableManager;

        public void SetValue(IListTableManager<TElementDataClass> listTableManager, int index, TElementDataClass elementData)
        {
            _listTableManager = listTableManager;
            Index = index;
            SetValue(elementData);
        }

        public virtual void SetValue(TElementDataClass elementData)
        {
            this.ElementData = elementData;
        }

        public void DeleteSelf()
        {
            _listTableManager.Delete(ElementData);
        }
    }
}
