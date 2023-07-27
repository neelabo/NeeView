﻿using System;
using System.Threading;

namespace NeeView
{
    public class ViewContentSourceCollectionChangedEventArgs : EventArgs
    {
        public ViewContentSourceCollectionChangedEventArgs()
        {
            BookAddress = "";
            ViewPageCollection = new ViewContentSourceCollection();
        }

        public ViewContentSourceCollectionChangedEventArgs(string bookAddress, ViewContentSourceCollection viewPageCollection)
        {
            BookAddress = bookAddress;
            ViewPageCollection = viewPageCollection ?? throw new ArgumentNullException(nameof(viewPageCollection));
        }

        public string BookAddress { get; set; }
        public ViewContentSourceCollection ViewPageCollection { get; set; }
        public bool IsForceResize { get; set; }
        public bool IsFirst { get; set; }

        public override string ToString()
        {
            return ViewPageCollection.Range.ToString();
        }
    }

}

