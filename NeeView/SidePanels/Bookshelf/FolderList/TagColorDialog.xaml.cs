using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// TagColorDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class TagColorDialog : Window
    {
        private TagColorDialogViewModel? _vm;

        public TagColorDialog()
        {
            InitializeComponent();
        }

        public TagColorDialog(TagColorDialogViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = _vm;
        }

        protected override void OnClosed(EventArgs e)
        {
            _vm?.Decide();

            base.OnClosed(e);
        }
    }
}
