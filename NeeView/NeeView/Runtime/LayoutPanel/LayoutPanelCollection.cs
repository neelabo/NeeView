using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutPanelCollection : ObservableCollection<LayoutPanel>
    {
        public LayoutPanelCollection() : base()
        {
        }

        public LayoutPanelCollection(IEnumerable<LayoutPanel> collection) : base(collection)
        {
        }

        public Orientation Orientation { get; set; } = Orientation.Vertical;

        internal bool IsStandAlone(LayoutPanel panel)
        {
            return this.Count == 1 && this.First() == panel;
        }
    }
}
