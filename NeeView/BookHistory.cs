﻿using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Data;

namespace NeeView
{
    [DataContract]
    public class BookHistory : BindableBase, IHasPage, IHasName
    {
        private string _place;

        public BookHistory()
        {
        }

        public BookHistory(BookMementoUnit unit, DateTime lastAccessTime)
        {
            Place = unit.Place;
            LastAccessTime = lastAccessTime;
            Unit = unit;
        }

        [DataMember]
        public string Place
        {
            get { return _place; }
            set
            {
                if (SetProperty(ref _place, value))
                {
                    _unit = null;
                    RaisePropertyChanged(null);
                }
            }
        }


        [DataMember]
        public DateTime LastAccessTime { get; set; }


        public override string ToString()
        {
            return Place ?? base.ToString();
        }

        public string Name => Unit.Memento.Name;
        public string Note => Unit.ArchivePage.Content.Entry.RootArchiverName;
        public string Detail => Place + "\n" + LastAccessTime;

        public IThumbnail Thumbnail => Unit.ArchivePage.Thumbnail;

        private BookMementoUnit _unit;
        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Set(Place); }
            private set { _unit = value; }
        }

        public Page GetPage()
        {
            return Unit.ArchivePage;
        }
    }
}
