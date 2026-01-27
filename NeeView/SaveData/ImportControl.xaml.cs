using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ImportControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ImportControl : UserControl
    {
        public ImportControl()
        {
            InitializeComponent();
        }

        public ImportControl(ImportControlViewModel vm) : this()
        {
            this.DataContext = vm;
        }
    }
}
