using NeeView.Collections;
using System;
using System.ComponentModel;

namespace NeeView
{
    public interface IQuickAccessEntry: INotifyPropertyChanged, ICloneable, IHasName, IRenameable
    {
        string? RawName { get; }
        new string? Name { get; set; }
        string? Path { get; set; }
    }

}
