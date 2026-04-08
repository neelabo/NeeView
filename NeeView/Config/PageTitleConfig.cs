using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class PageTitleConfig : BindableBase
    {
        private const string PageTitleFormat1Default = "{Name}";
        private const string PageTitleFormat2Default = "{NameL} | {NameR}";
        private const string PageTitleFormatMediaDefault = " ";

        [DefaultEquality] private bool _isEnabled = true;
        [DefaultEquality] private string? _pageTitleFormat1;
        [DefaultEquality] private string? _pageTitleFormat2;
        [DefaultEquality] private string? _pageTitleFormatMedia;


        /// <summary>
        /// ページタイトル表示
        /// </summary>
        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// ページタイトルフォーマット 1P用
        /// </summary>
        [PropertyMember]
        public string PageTitleFormat1
        {
            get { return _pageTitleFormat1 ?? PageTitleFormat1Default; }
            set { SetProperty(ref _pageTitleFormat1, CleanUpTitleFormat(value, PageTitleFormat1Default)); }
        }

        /// <summary>
        /// ページタイトルフォーマット 2P用
        /// </summary>
        [PropertyMember]
        public string PageTitleFormat2
        {
            get { return _pageTitleFormat2 ?? PageTitleFormat2Default; }
            set { SetProperty(ref _pageTitleFormat2, CleanUpTitleFormat(value, PageTitleFormat2Default)); }
        }

        /// <summary>
        /// ページタイトルフォーマット メディア用
        /// </summary>
        [PropertyMember]
        public string PageTitleFormatMedia
        {
            get { return _pageTitleFormatMedia ?? PageTitleFormatMediaDefault; }
            set { SetProperty(ref _pageTitleFormatMedia, CleanUpTitleFormat(value, PageTitleFormatMediaDefault)); }
        }


        private static string CleanUpTitleFormat(string source, string defaultFormat)
        {
            return string.IsNullOrEmpty(source) ? defaultFormat : source;
        }
    }
}


