using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// CommandParameterControl.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandParameterControl : UserControl
    {
        #region DependencyProperties

        public bool IsAny
        {
            get { return (bool)GetValue(IsAnyProperty); }
            private set { SetValue(IsAnyProperty, value); }
        }

        public static readonly DependencyProperty IsAnyProperty =
            DependencyProperty.Register("IsAny", typeof(bool), typeof(CommandParameterControl), new PropertyMetadata(false));

        #endregion

        private CommandParameterViewModel? _vm;

        public CommandParameterControl()
        {
            InitializeComponent();
        }


        public event PropertyChangedEventHandler? ParameterChanged
        {
            add
            {
                if (_vm is null) throw new InvalidOperationException("You must call Initialized()");
                _vm?.ParameterChanged += value;
            }
            remove
            {
                _vm?.ParameterChanged -= value;
            }
        }


        public CommandParameter? Parameter => _vm?.Parameter;


        public void Initialize(IReadOnlyDictionary<string, CommandElement> commandMap, string key)
        {
            InitializeComponent();

            _vm = new CommandParameterViewModel(commandMap, key);
            this.DataContext = _vm;

            this.IsAny = _vm.PropertyDocument != null;

            if (this.IsAny)
            {
                this.EmptyText.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.MainPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void Flush()
        {
            _vm?.Flush();
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            _vm?.Reset();
            this.Inspector.Refresh(); // TODO: MVVM的に更新されるようにする
        }
    }
}
