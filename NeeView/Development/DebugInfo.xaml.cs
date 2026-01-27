using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// DebugInfo.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugInfo : UserControl
    {
        public static DebugInfo? Current { get; private set; }

        private readonly DevInfoViewModel _vm;

        public DebugInfo()
        {
            Current = this;

            InitializeComponent();
            this.Root.DataContext = _vm = new DevInfoViewModel();

            _vm.WorkersChanged += (s, e) =>
            {
                this.items.Items.Refresh();
            };

            this.Loaded += (s, e) => _vm.OnLoad();
            this.Unloaded += (s, e) => _vm.OnUnload();
        }

        //
        [Conditional("DEBUG")]
        public void SetMessage(string message)
        {
            _vm.Message = message;
        }
    }



    public class DevInfoViewModel : BindableBase
    {
        public DevInfoViewModel()
        {
            JobEngine.Current.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(JobEngine.Workers))
                {
                    WorkersChanged?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        public void OnLoad()
        {
            var mainWindow = Application.Current?.MainWindow;
            if (mainWindow is not null)
            {
                mainWindow.MouseMove += MainWindow_MouseMove;
            }
        }

        public void OnUnload()
        {
            var mainWindow = Application.Current?.MainWindow;
            if (mainWindow is not null)
            {
                mainWindow.MouseMove -= MainWindow_MouseMove;
            }
        }

        private void MainWindow_MouseMove(object? sender, MouseEventArgs e)
        {
            CursorPointWindow = e.GetPosition(MainWindow.Current);
            CursorPointRoot = e.GetPosition(MainWindow.Current.Root);
        }


        public event EventHandler? WorkersChanged;


        public JobEngine JobEngine => JobEngine.Current;
        //public DragTransform DragTransform => MainViewComponent.Current.DragTransform;
        public BookOperation BookOperation => BookOperation.Current;
        public BookHub BookHub => BookHub.Current;
        //public ContentRebuild ContentRebuild => MainViewComponent.Current.ContentRebuild;

        public PageFrameBoxPresenter PageFrameBox => PageFrameBoxPresenter.Current;

        // 開発用：コンテンツ座標
        private Point _contentPosition;
        public Point ContentPosition
        {
            get { return _contentPosition; }
            set { _contentPosition = value; RaisePropertyChanged(); }
        }

        // 開発用：コンテンツ座標情報更新
        public void UpdateContentPosition()
        {
            //var mainContent = MainViewComponent.Current.ContentCanvas.MainContent;
            //ContentPosition = mainContent?.View is not null ? mainContent.View.PointToScreen(new Point(0, 0)) : new Point(double.NegativeInfinity, double.NegativeInfinity);
        }

        /// <summary>
        /// Message property.
        /// </summary>
        private string? _Message;
        public string? Message
        {
            get { return _Message; }
            set { if (_Message != value) { _Message = value; RaisePropertyChanged(); } }
        }


        private Point _CursorPointWindow;
        public Point CursorPointWindow
        {
            get { return _CursorPointWindow; }
            set { SetProperty(ref _CursorPointWindow, value); }
        }


        private Point _CursorPointRoot;
        public Point CursorPointRoot
        {
            get { return _CursorPointRoot; }
            set { SetProperty(ref _CursorPointRoot, value); }
        }


        public ObservableCollection<DevTextElement> TextList => DevTextMap.Current.Items;


        // 開発用：
        ////public Development Development { get; private set; } = new Development();

        /// <summary>
        /// DevUpdateContentPosition command.
        /// </summary>
        private RelayCommand? _DevUpdateContentPosition;
        public RelayCommand DevUpdateContentPosition
        {
            get { return _DevUpdateContentPosition = _DevUpdateContentPosition ?? new RelayCommand(DevUpdateContentPosition_Executed); }
        }


        private void DevUpdateContentPosition_Executed()
        {
            UpdateContentPosition();
        }
    }


    public class PointToDisplayStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point point)
            {
                return $"{(int)point.X,4},{(int)point.Y,4}";
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ByteSizeToFormatString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long number)
            {
                return $"{number / 1024:#,0}";
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    [NotifyPropertyChanged]
    public partial class DevTextElement : INotifyPropertyChanged, IComparable<DevTextElement>
    {
        private string _key;
        private string? _text;

        public DevTextElement(string key, string? text)
        {
            _key = key;
            _text = text;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Key
        {
            get { return _key; }
            set { SetProperty(ref _key, value); }
        }

        public string? Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        public int CompareTo(DevTextElement? other)
        {
            return string.Compare(this.Key, other?.Key, StringComparison.InvariantCulture);
        }

        public override string ToString()
        {
            return $"{Key}: {Text}";
        }
    }


    public class DevTextMap
    {
        public static DevTextMap Current { get; } = new DevTextMap();

        private Dictionary<string, DevTextElement> _map = new();
        private ObservableSortedCollection<DevTextElement> _items = new();


        public ObservableSortedCollection<DevTextElement> Items => _items;


        public void SetText(string key, string? text)
        {
            if (_map.TryGetValue(key, out var value))
            {
                value.Text = text;
            }
            else
            {
                var item = new DevTextElement(key, text);
                _map.Add(key, item);
                _items.Add(item);
            }
        }
    }

    // https://qiita.com/tanitanin/items/65082f0a395659028323
    public class ObservableSortedCollection<T> : ObservableCollection<T> where T : IComparable<T>
    {
        public ObservableSortedCollection() : base() { }

        public ObservableSortedCollection(IEnumerable<T> collection) : base()
        {
            foreach (var item in collection)
            {
                this.Add(item);
            }
        }

        public int FirstIndexOf(T item)
        {
            var e = this.FirstOrDefault(x => x.CompareTo(item) >= 0);
            return e is not null ? this.IndexOf(e) : -1;
        }

        public int LastIndexOf(T item)
        {
            var e = this.LastOrDefault(x => x.CompareTo(item) <= 0);
            return e is not null ? this.IndexOf(e) : -1;
        }

        protected override void InsertItem(int _, T item)
        {
            var index = LastIndexOf(item) + 1;
            base.InsertItem(index, item);
        }

        protected override void MoveItem(int oldIndex, int _)
        {
            var firstIndex = this.FirstIndexOf(this[oldIndex]);
            if (oldIndex < firstIndex)
            {
                base.MoveItem(oldIndex, firstIndex);
            }

            var lastIndex = this.LastIndexOf(this[oldIndex]);
            if (lastIndex < oldIndex)
            {
                base.MoveItem(oldIndex, lastIndex + 1);
            }
        }
    }

}
