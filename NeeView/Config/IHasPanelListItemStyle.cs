using System.ComponentModel;

namespace NeeView
{
    public interface IHasPanelListItemStyle : INotifyPropertyChanged
    {
        PanelListItemStyle PanelListItemStyle { get; set; }
    }

}


