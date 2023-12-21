using System;
using System.Collections.Generic;

namespace NonsensicalKit.Editor.Table
{
    public class MultilevelMenuNode
    {
        public List<MultilevelMenuNode> Childs { get; set; } = new List<MultilevelMenuNode>();

        public bool Deployable => Childs.Count > 0;
        public bool CanClick
        {
            get
            {
                if (MenuInfo.Verification == null)
                {
                    return true;
                }
                else
                {
                    return MenuInfo.Verification(Context);
                }
            }
        }
        public string Name { get; set; }
        public MultilevelMenuInfo MenuInfo { get; set; }

        public MultilevelMenuNode(string name, MultilevelMenuInfo info)
        {
            Name = name;
            MenuInfo= info;
        }

        public MultilevelContext Context
        {
            get
            {
                if (_context==null)
                {
                    _context = new MultilevelContext(MenuInfo.Path, Name, MenuInfo.State);
                }
                return _context;
            }
        }

        private MultilevelContext _context;
    }

    public class MultilevelMenuInfo
    {
        public string Path { get; set; }
        public bool AutoClose { get; set; }
        public int Priority { get; set; }
        public object State { get; set; }
        public bool AlwayCanClick { get; set; }

        public Action<MultilevelContext> ClickAction { get; set; }
        public Func<MultilevelContext, bool> Verification { get; set; }

        public MultilevelMenuInfo(string path, Action<MultilevelContext> clickAction) : this(path, clickAction, null, null, 0, true) { }
        public MultilevelMenuInfo(string path, Action<MultilevelContext> clickAction, Func<MultilevelContext, bool> verification) : this(path, clickAction, verification, null, 0, true) { }

        public MultilevelMenuInfo(string path, Action<MultilevelContext> clickAction, Func<MultilevelContext, bool> verification, object state, int priority, bool autoClose)
        {
            this.Path = path;
            this.ClickAction = clickAction;
            this.Verification = verification;
            this.Priority = priority;
            this.AutoClose = autoClose;
            this.State = state;
        }
    }

    public class MultilevelContext
    {
        public string Path;
        public string Name;
        public object State;

        public MultilevelContext(string path, string name, object state)
        {
            Path = path;
            Name = name;
            State = state;
        }
    }
}
