using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;


namespace NeeView.Collections.Generic
{
    public partial class TreeCollection<T> : BindableBase
        where T : ITreeListNode
    {
        public TreeCollection(TreeListNode<T> root)
        {
            Root = root;
        }

        [Subscribable]
        public event NotifyCollectionChangedEventHandler? RoutedCollectionChanged
        {
            add => Root.RoutedCollectionChanged += value;
            remove => Root.RoutedCollectionChanged -= value;
        }

        [Subscribable]
        public event PropertyChangedEventHandler? RoutedPropertyChanged
        {
            add => Root.RoutedPropertyChanged += value;
            remove => Root.RoutedPropertyChanged -= value;
        }

        [Subscribable]
        public event PropertyChangedEventHandler? RoutedValuePropertyChanged
        {
            add => Root.RoutedValuePropertyChanged += value;
            remove => Root.RoutedValuePropertyChanged -= value;
        }


        public TreeListNode<T> Root { get; }
    }
}
