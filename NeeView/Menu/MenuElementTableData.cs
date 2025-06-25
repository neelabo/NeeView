using NeeView.Collections.Generic;

namespace NeeView
{
    public class MenuElementTableData
    {
        public int Depth { get; set; }
        public TreeListNode<MenuElement> Element { get; set; }

        public MenuElementTableData(int depth, TreeListNode<MenuElement> element)
        {
            Depth = depth;
            Element = element;
        }
    }
}
