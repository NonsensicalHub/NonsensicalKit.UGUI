namespace NonsensicalKit.Editor.Table
{
    public abstract class ListTableElement<ElementDataClass> : NonsensicalUI where ElementDataClass : class
    {
        public ElementDataClass ElementData { get; set; }

        public virtual void SetValue(ElementDataClass elementData)
        {
            this.ElementData = elementData;
        }
    }
}
