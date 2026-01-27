using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// DebugJobList.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugJobList : UserControl
    {
        public DebugJobList()
        {
            InitializeComponent();
            this.Root.DataContext = new DebugJobListViewModel();
        }
    }

    public class DebugJobListViewModel
    {
        public JobScheduler JobScheduler => JobEngine.Current.Scheduler;
    }
}
