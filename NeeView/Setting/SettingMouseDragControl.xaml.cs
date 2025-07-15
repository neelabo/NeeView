using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingMouseDragControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingMouseDragControl : UserControl, IValueInitializable
    {
        public SettingMouseDragControl()
        {
            InitializeComponent();

            // ドラッグアクション一覧作成
            DragActionCollection = new ObservableCollection<DragActionParam>();
            UpdateDragActionList();

            this.Root.DataContext = this;
        }

        // ドラッグ一覧専用パラメータ
        public class DragActionParam : BindableBase
        {
            public DragActionParam(string key, DragAction dragAction)
            {
                Key = key;
                DragAction = dragAction;
            }

            public string Key { get; set; }
            public DragAction DragAction { get; set; }
            public string Header => DragAction.Note;
            public bool HasParameter => DragAction.Parameter != null;
        }

        // コマンド一覧
        public ObservableCollection<DragActionParam> DragActionCollection { get; set; }

        private Window GetOwner()
        {
            return Window.GetWindow(this);
        }

        private void UpdateDragActionList()
        {
            DragActionCollection.Clear();
            foreach (var element in DragActionTable.Current)
            {
                var item = new DragActionParam(element.Key, element.Value);
                DragActionCollection.Add(item);
            }

            this.DragActionListView.Items.Refresh();

            this.Loaded += SettingMouseDragControl_Loaded;
            this.Unloaded += SettingMouseDragControl_Unloaded;
        }

        private void SettingMouseDragControl_Loaded(object? sender, RoutedEventArgs e)
        {
            DragActionTable.Current.GestureDragActionChanged += DragActionTable_GestureDragActionChanged;
        }

        private void SettingMouseDragControl_Unloaded(object? sender, RoutedEventArgs e)
        {
            DragActionTable.Current.GestureDragActionChanged -= DragActionTable_GestureDragActionChanged;
        }

        private void DragActionTable_GestureDragActionChanged(object? sender, EventArgs e)
        {
            this.DragActionListView.Items.Refresh();
        }

        private void DragActionListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
        }

        private void DragActionListViewItem_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
        {
            if (sender is not ListViewItem targetItem) return;

            var value = (DragActionParam)targetItem.DataContext;
            OpenDragActionSettingDialog(value, MouseDragSettingWindowTab.MouseGesture);
        }


        private void DragActionListViewItem_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is not ListViewItem targetItem) return;

                var value = (DragActionParam)targetItem.DataContext;
                OpenDragActionSettingDialog(value, MouseDragSettingWindowTab.MouseGesture);

                e.Handled = true;
            }
        }

        private void DragActionSettingButton_Click(object? sender, RoutedEventArgs e)
        {
            var value = (DragActionParam)this.DragActionListView.SelectedValue;
            OpenDragActionSettingDialog(value, MouseDragSettingWindowTab.MouseGesture);
        }

        private void OpenDragActionSettingDialog(DragActionParam value, MouseDragSettingWindowTab tab)
        {
            if (value.DragAction.IsLocked)
            {
                var dlg = new MessageDialog("", TextResources.GetString("DragActionLockedDialog.Title"));
                dlg.Owner = GetOwner();
                dlg.ShowDialog();
                return;
            }

            var dialog = new MouseDragSettingWindow(value.Key, tab);
            dialog.Owner = GetOwner();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                this.DragActionListView.Items.Refresh();
            }
        }

        private void ResetDragActionSettingButton_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog(TextResources.GetString("DragActionResetDialog.Message"), TextResources.GetString("DragActionResetDialog.Title"));
            dialog.Commands.Add(UICommands.Yes);
            dialog.Commands.Add(UICommands.No);
            dialog.Owner = GetOwner();
            var answer = dialog.ShowDialog();

            if (answer.Command == UICommands.Yes)
            {
                Reset();
            }
        }

        private void Reset()
        {
            var memento = DragActionTable.Current.CreateDefaultMemento();
            DragActionTable.Current.RestoreDragActionCollection(memento);
            DragActionTable.Current.UpdateGestureDragAction();

            this.DragActionListView.Items.Refresh();
        }

        private void EditCommandParameterButton_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not DragActionParam dragActionParam) return;

            this.DragActionListView.SelectedItem = dragActionParam;
            OpenDragActionSettingDialog(dragActionParam, MouseDragSettingWindowTab.Parameter);
        }

        public void InitializeValue()
        {
            Reset();
        }
    }
}
