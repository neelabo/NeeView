using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
    [INotifyPropertyChanged]
    public partial class EditCommandWindow : Window
    {
        private readonly PropertyDocument _generalDocument;
        private CommandCollection _memento;
        private string _key;
        private bool _isShowMessage;


        public EditCommandWindow(string key, EditCommandWindowTab start)
        {
            InitializeComponent();
            this.DataContext = this;

            _generalDocument = new PropertyDocument();
            _generalDocument.AddProperty(PropertyMemberElement.Create(this, nameof(IsShowMessage)));
            _generalDocument.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);

            MouseHorizontalWheelService.SubscribeHorizontalWheelEvent(this);

            this.Loaded += EditCommandWindow_Loaded;
            this.Closed += EditCommandWindow_Closed;

            Initialize(key, start);
        }


        [PropertyMember(Name = "EditCommandWindow.Visible")]
        public bool IsShowMessage
        {
            get => _isShowMessage;
            set => SetProperty(ref _isShowMessage, value);
        }

        public string CommandName => _key;

        public string Note { get; private set; }

        public PropertyDocument GeneralDocument => _generalDocument;


        private void EditCommandWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            var tabItem = this.TabControl.ItemContainerGenerator.ContainerFromItem(this.TabControl.SelectedItem) as TabItem;
            tabItem?.Focus();
        }

        private void EditCommandWindow_Closed(object? sender, EventArgs e)
        {
            Flush();
        }

        [MemberNotNull(nameof(_memento), nameof(_key), nameof(Note))]
        private void Initialize(string key, EditCommandWindowTab start)
        {
            _memento = CommandTable.Current.CreateCommandCollectionMemento(false);
            _key = key;

            var commandMap = CommandTable.Current;

            this.Title = $"{commandMap[key].Text} - {TextResources.GetString("EditCommandWindow.Title")}";

            this.Note = commandMap[key].Remarks;
            this.IsShowMessage = commandMap[key].IsShowMessage;

            this.InputGesture.Initialize(commandMap, key);
            this.MouseGesture.Initialize(commandMap, key);
            this.InputTouch.Initialize(commandMap, key);
            this.Parameter.Initialize(commandMap, key);

            this.Parameter.ParameterChanged += Parameter_ParameterChanged;
            UpdateReverseNoteVisibility();

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
            this.InputBindings.Add(new KeyBinding(CloseWindowCommand, new KeyGesture(Key.Escape)));
        }

        [RelayCommand]
        private void CloseWindow()
        {
            this.Close();
        }

        private void Parameter_ParameterChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReversibleCommandParameter.IsReverse))
            {
                UpdateReverseNoteVisibility();
            }
        }

        private void UpdateReverseNoteVisibility()
        {
            var command = CommandTable.Current.GetElement(_key);

            if (command != null && command.PairPartner != null && Config.Current.Command.IsReversePageMove)
            {
                if (this.Parameter.Parameter is ReversibleCommandParameter parameter && parameter.IsReverse)
                {
                    this.ReverseNoteTextBlock.Text = TextResources.GetFormatString("EditCommandWindow.ReverseNote", CommandTable.Current.GetElement(command.PairPartner).Text);
                    this.ReverseNote.Visibility = Visibility.Visible;
                }
                else
                {
                    this.ReverseNote.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
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
