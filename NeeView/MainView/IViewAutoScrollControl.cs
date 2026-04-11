using System.ComponentModel;

namespace NeeView
{
    public interface IViewAutoScrollControl : INotifyPropertyChanged
    {
        bool IsAutoScrollMode { get; set; }
        
        bool GetAutoScrollMode();
        void SetAutoScrollMode(bool isAutoScroll);
        void ToggleAutoScrollMode();
    }
}