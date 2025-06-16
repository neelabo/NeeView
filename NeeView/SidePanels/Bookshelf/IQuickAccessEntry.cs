using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.ComponentModel;

namespace NeeView
{
    public interface IQuickAccessEntry: ITreeListNode
    {
        string? RawName { get; }
        new string? Name { get; set; }
        string? Path { get; set; }
    }

}
