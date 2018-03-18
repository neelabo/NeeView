﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class MediaArchiverProfile : BindableBase
    {
        public static MediaArchiverProfile Current { get; private set; }

        private bool _isEnabled = true;

        public MediaArchiverProfile()
        {
            Current = this;
        }

        [PropertyMember("@ParamArchiverMediaIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamArchiverMediaSupportFileTypes", Tips = "@ParamArchiverMediaSupportFileTypesTips")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".asf;.avi;.mp4;.mkv;.mov;.wmv");

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsEnabled { get; set; }

            [DataMember]
            public string SupportFileTypes { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = this.IsEnabled;
            memento.SupportFileTypes = this.SupportFileTypes.ToString();

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsEnabled = memento.IsEnabled;
            this.SupportFileTypes.FromString(memento.SupportFileTypes.ToString());
        }

        #endregion

    }
}
