using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// MenuCommandPropertyDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class MenuCommandPropertyDialog : Window
    {
        private readonly MenuCommandPropertyDialogViewModel? _vm;


        public MenuCommandPropertyDialog()
        {
            InitializeComponent();
        }

        public MenuCommandPropertyDialog(MenuElement menuElement) : this()
        {
            _vm = new MenuCommandPropertyDialogViewModel(menuElement);
            this.DataContext = _vm;
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }



    public class MenuCommandPropertyDialogViewModel : BindableBase
    {
        private readonly MenuElement _menuElement;


        public MenuCommandPropertyDialogViewModel(MenuElement menuElement)
        {
            Debug.Assert(menuElement.MenuElementType == MenuElementType.Command, "MenuCommandPropertyDialogViewModel is only for command type.");

            _menuElement = menuElement;
            _menuElement.CommandName ??= CommandNameList.FirstOrDefault();
        }


        public string Name
        {
            get { return _menuElement.Label; }
            set
            {
                if (_menuElement.Label != value)
                {
                    _menuElement.Label = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public string? CommandName
        {
            get { return _menuElement.CommandName; }
            set
            {
                if (_menuElement.CommandName != value)
                {
                    _menuElement.CommandName = value;
                    RaisePropertyChanged(nameof(CommandName));
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public List<string> CommandNameList { get; } = CreateCommandNameList();

        private static List<string> CreateCommandNameList()
        {
            var list = CommandTable.Current.Values
                .OrderBy(e => e.Order)
                .GroupBy(e => e.Group)
                .SelectMany(g => g)
                .Select(e => e.Name)
                .ToList();

            return list;
        }

    }
}
