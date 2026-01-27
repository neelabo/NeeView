
using NeeLaboratory.ComponentModel;
using NeeView.IO;
using NeeView.Properties;

namespace NeeView
{
    /// <summary>
    /// AddressBar : Model
    /// </summary>
    public class AddressBar : BindableBase
    {
        private string _address = "";


        public AddressBar()
        {
            BookHub.Current.AddressChanged +=
                (s, e) => SetAddress(BookHub.Current.Address ?? "");

            BookHub.Current.BookChanged +=
                (s, e) => SetAddress(BookHub.Current.Address ?? "");

            BookHub.Current.BookmarkChanged +=
                (s, e) => RaisePropertyChanged(nameof(IsBookmark));

            BookHub.Current.SubscribePropertyChanged(nameof(BookHub.IsBookLocked),
                (s, e) => RaisePropertyChanged(nameof(IsBookLocked)));
        }


        public string Address
        {
            get { return _address; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                if (_address != value)
                {
                    SetAddress(value);
                    if (_address != BookHub.Current.Address)
                    {
                        Load(value);
                    }
                }
            }
        }

        public bool IsBookLocked
        {
            get => BookHub.Current.IsBookLocked;
            set => BookHub.Current.IsBookLocked = value;
        }

        public bool CanBookmark => BookTools.CanBookmark(_address);

        public bool IsBookmark => BookmarkCollection.Current.Contains(_address);

        public string BookName => LoosePath.GetFileName(_address);

        public bool IsBookEnabled => BookHub.Current.GetCurrentBook() != null;

        public string BookDetail
        {
            get
            {
                var text = BookHub.Current.GetCurrentBook()?.Source.GetDetail();
                if (text is null)
                {
                    var query = new QueryPath(_address);
                    if (query.Scheme == QueryScheme.Bookmark)
                    {
                        text = TextResources.GetString("BookAddressInfo.Bookmark");
                    }
                }
                return text ?? TextResources.GetString("BookAddressInfo.Invalid");
            }
        }




        private void SetAddress(string address)
        {
            _address = address;
            RaisePropertyChanged(null);
        }

        // フォルダー読み込み
        // TODO: BookHubへ？
        public void Load(string path, BookLoadOption option = BookLoadOption.None)
        {
            if (FileShortcut.IsShortcut(path) && (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)))
            {
                var shortcut = new FileShortcut(path);
                var target = shortcut.TargetPath;
                if (target is null) return;
                path = target;
            }

            BookHub.Current.RequestLoad(this, path, null, option, true);
        }
    }
}
