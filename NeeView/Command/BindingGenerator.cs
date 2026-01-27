using System.Windows.Data;


namespace NeeView
{
    public static class BindingGenerator
    {
        private static readonly StretchModeToBooleanConverter _stretchModeToBooleanConverter = new();
        private static readonly PageModeToBooleanConverter _pageModeToBooleanConverter = new();
        private static readonly BookReadOrderToBooleanConverter _bookReadOrderToBooleanConverter = new();
        private static readonly BackgroundStyleToBooleanConverter _backgroundStyleToBooleanConverter = new();
        private static readonly FolderOrderToBooleanConverter _folderOrderToBooleanConverter = new();
        private static readonly SortModeToBooleanConverter _sortModeToBooleanConverter = new();
        private static readonly PageFrameOrientationToBooleanConverter _pageFrameOrientationToBooleanConverter = new();

        public static Binding BindingBookHub(string path)
        {
            return new Binding(path) { Source = BookHub.Current };
        }

        public static Binding BindingBookSetting(string path)
        {
            return new Binding(path) { Source = Config.Current.BookSetting };
        }

        public static Binding BindingBookConfig(string path)
        {
            return new Binding(path) { Source = Config.Current.Book };
        }

        public static Binding StretchMode(PageStretchMode mode)
        {
            return new Binding(nameof(ViewConfig.StretchMode))
            {
                Converter = _stretchModeToBooleanConverter,
                ConverterParameter = mode.ToString(),
                Source = Config.Current.View
            };
        }

        public static Binding Background(BackgroundType mode)
        {
            return new Binding(nameof(BackgroundConfig.BackgroundType))
            {
                Converter = _backgroundStyleToBooleanConverter,
                ConverterParameter = mode.ToString(),
                Source = Config.Current.Background
            };
        }

        public static Binding FolderOrder(FolderOrder mode)
        {
            return new Binding(nameof(BookshelfFolderList.FolderOrder))
            {
                Converter = _folderOrderToBooleanConverter,
                ConverterParameter = mode.ToString(),
                Mode = BindingMode.OneWay,
                Source = BookshelfFolderList.Current
            };
        }

        public static Binding PageMode(PageMode mode)
        {
            var binding = BindingBookSetting(nameof(BookMemento.PageMode));
            binding.Converter = _pageModeToBooleanConverter;
            binding.ConverterParameter = mode.ToString();
            return binding;
        }

        public static Binding BookReadOrder(PageReadOrder mode)
        {
            var binding = BindingBookSetting(nameof(BookMemento.BookReadOrder));
            binding.Converter = _bookReadOrderToBooleanConverter;
            binding.ConverterParameter = mode.ToString();
            return binding;
        }

        public static Binding SortMode(PageSortMode mode)
        {
            var binding = BindingBookSetting(nameof(BookMemento.SortMode));
            binding.Converter = _sortModeToBooleanConverter;
            binding.ConverterParameter = mode.ToString();
            return binding;
        }

        public static Binding PageFrameOrientation(PageFrameOrientation orientation)
        {
            var binding = BindingBookConfig(nameof(BookConfig.Orientation));
            binding.Converter = _pageFrameOrientationToBooleanConverter;
            binding.ConverterParameter = orientation.ToString();
            return binding;
        }
    }
}
