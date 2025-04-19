using NeeLaboratory.ComponentModel;
using System.Collections.Generic;

namespace NeeView
{
    public class SelectableArchiverList : BindableBase
    {
        static SelectableArchiverList() => Current = new SelectableArchiverList();
        public static SelectableArchiverList Current { get; }


        private List<ArchiverIdentifier> _archivers = new();
        private bool _isDirty = true;


        public SelectableArchiverList()
        {
            BookHub.Current.BookChanged +=
                (s, e) => _isDirty = true;
        }


        public List<ArchiverIdentifier> Archivers
        {
            get { return _archivers; }
            set
            {
                if (SetProperty(ref _archivers, value))
                {
                    RaisePropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public bool IsEnabled
        {
            get { return Archivers.Count > 0; }
        }


        public List<ArchiverIdentifier> GetLatestArchivers()
        {
            Update();
            return Archivers;
        }

        public void Update()
        {
            if (!_isDirty) return;
            _isDirty = true;

            var address = BookHub.Current.Address;
            if (address is not null)
            {
                Archivers = ArchiveManager.Current.GetSupportedArchiverList(address);
            }
            else
            {
                Archivers = new();
            }
        }
    }
}
