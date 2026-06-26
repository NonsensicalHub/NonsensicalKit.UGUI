using System.Collections.Generic;

namespace NonsensicalKit.UGUI.Table
{
    public interface IListTableManager<in T>
    {
        public void SetData(IEnumerable<T> data);
        public bool Delete(T element);
        public void Clear();
        public void Clean();
    }

    public abstract class ListTableManager<TListElement, TElementData> : ListTableManagerCore<TListElement, TElementData>,
        IListTableManager<TElementData>
        where TListElement : ListTableElement<TElementData>
        where TElementData : class
    {
        protected override void SetValue(TListElement element, TElementData elementData, int index)
        {
            element.SetValue(this, index, elementData);
        }
    }
}
