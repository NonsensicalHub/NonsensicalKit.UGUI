using System;

namespace NonsensicalKit.Editor.Table
{
    public class RightClickMenuItem
    {
        public string SpriteName;
        public string Text;
        public Action ClickAction;

        public RightClickMenuItem(string spriteName, string text, Action clickAction)
        {
            this.SpriteName = spriteName;
            this.Text = text;
            this.ClickAction = clickAction;
        }
    }

}