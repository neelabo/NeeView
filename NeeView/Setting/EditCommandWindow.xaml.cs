using NeeLaboratory.Generators;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView.Setting
{
    public enum EditCommandWindowTab
    {
        Default,
        General,
        InputGesture,
        MouseGesture,
        InputTouch,
        Parameter,
    }

    /// <summary>
    /// EditCommandWindow.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class EditCommandWindow : Window, INotifyPropertyChanged, INotifyMouseHorizontalWheelChanged
    {
        private CommandCollection _memento;
        private string _key;
        private bool _isShowMessage;


        public EditCommandWindow(string key, EditCommandWindowTab start)
        {
            InitializeComponent();
            this.DataContext = this;

            var mouseHorizontalWheel = new MouseHorizontalWheelService(this);
            mouseHorizontalWheel.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(s, e);

            this.Loaded += EditCommandWindow_Loaded;
            this.Closed += EditCommandWindow_Closed;

            Initialize(key, start);
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public event MouseWheelEventHandler? MouseHorizontalWheelChanged;


        public bool IsShowMessage
        {
            get => _isShowMessage;
            set => SetProperty(ref _isShowMessage, value);
        }

        public string CommandName => _key;

        public string Note { get; private set; }


        private void EditCommandWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            var tabItem = this.TabControl.ItemContainerGenerator.ContainerFromItem(this.TabControl.SelectedItem) as TabItem;
            tabItem?.Focus();
        }

        private void EditCommandWindow_Closed(object? sender, EventArgs e)
        {
            if (this.DialogResult == true)
            {
                Flush();
            }
            else
            {
                CommandTable.Current.RestoreCommandCollection(_memento, false);
            }
        }

        [MemberNotNull(nameof(_memento), nameof(_key), nameof(Note))]
        private void Initialize(string key, EditCommandWindowTab start)
        {
            _memento = CommandTable.Current.CreateCommandCollectionMemento(false);
            _key = key;

            var commandMap = CommandTable.Current;

            this.Title = $"{commandMap[key].Text} - {Properties.TextResources.GetString("EditCommandWindow.Title")}";

            this.Note = commandMap[key].Remarks;
            this.IsShowMessage = commandMap[key].IsShowMessage;

            this.InputGesture.Initialize(commandMap, key);
            this.MouseGesture.Initialize(commandMap, key);
            this.InputTouch.Initialize(commandMap, key);
            this.Parameter.Initialize(commandMap, key);

            switch (start)
            {
                case EditCommandWindowTab.General:
                    this.GeneralTab.IsSelected = true;
                    break;
                case EditCommandWindowTab.InputGesture:
                    this.InputGestureTab.IsSelected = true;
                    break;
                case EditCommandWindowTab.MouseGesture:
                    this.MouseGestureTab.IsSelected = true;
                    break;
                case EditCommandWindowTab.InputTouch:
                    this.InputTouchTab.IsSelected = true;
                    break;
                case EditCommandWindowTab.Parameter:
                    this.ParameterTab.IsSelected = true;
                    break;
            }

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Flush()
        {
            this.InputGesture.Flush();
            this.MouseGesture.Flush();
            this.InputTouch.Flush();
            this.Parameter.Flush();

            CommandTable.Current.GetElement(_key).IsShowMessage = this.IsShowMessage;
            CommandTable.Current.RaiseChanged();
        }

    }
}
