﻿using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class PagemarkList : BindableBase
    {
        public static PagemarkList Current { get; set; }

        private PanelListItemStyle _panelListItemStyle;
        private PagemarkListBoxModel _listBox;



        public PagemarkList()
        {
            Current = this;

            _listBox = new PagemarkListBoxModel();
        }


        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        public bool IsThumbnailVisibled
        {
            get
            {
                switch (_panelListItemStyle)
                {
                    default:
                        return false;
                    case PanelListItemStyle.Content:
                        return ThumbnailProfile.Current.ThumbnailWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return ThumbnailProfile.Current.BannerWidth > 0.0;
                }
            }
        }

        public PagemarkListBoxModel ListBox
        {
            get { return _listBox; }
            set { SetProperty(ref _listBox, value); }
        }

        public void Jump(string place, string entryName)
        {
            ListBox.Jump(place, entryName);
        }

        public void PrevPagemark()
        {
            ListBox?.PrevPagemark();
        }

        public void NextPagemark()
        {
            ListBox?.NextPagemark();
        }

        public void Toggle(string place, string entryName)
        {
            ListBox.Toggle(place, entryName);
        }

        public void Remove(string place, string entryName)
        {
            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node != null)
            {
                ListBox.Remove(node);
            }
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PanelListItemStyle = memento.PanelListItemStyle;
        }

        #endregion
    }
}
