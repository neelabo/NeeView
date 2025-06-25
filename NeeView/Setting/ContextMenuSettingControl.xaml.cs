using NeeView.Collections.Generic;
using NeeView.ComponentModel;
using NeeView.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
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
    /// <summary>
    /// ContextMenuSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ContextMenuSettingControl : UserControl
    {
        public static readonly RoutedCommand NewRootCommand = new("AddRootCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand NewRootSeparator = new("AddRootSeparator", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand NewRootFolderCommand = new("NewRootFolderCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand NewCommand = new("AddCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand NewSeparator = new("AddSeparator", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand NewFolderCommand = new("NewFolderCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand PropertyCommand = new("PropertyCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand RemoveCommand = new("RemoveCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand RenameCommand = new("RenameCommand", typeof(ContextMenuSettingControl));

        private static void InitializeCommandStatic()
        {
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }


        public ContextMenuSource ContextMenuSetting
        {
            get { return (ContextMenuSource)GetValue(ContextMenuSettingProperty); }
            set { SetValue(ContextMenuSettingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContextMenuSetting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextMenuSettingProperty =
            DependencyProperty.Register("ContextMenuSetting", typeof(ContextMenuSource), typeof(ContextMenuSettingControl), new PropertyMetadata(null, ContextMenuSettingPropertyChanged));

        private static void ContextMenuSettingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ContextMenuSettingControl)?.Initialize();
        }



        private readonly ContextMenuSettingViewModel _vm;
        private readonly ContextMenuTreeViewDropAssist _dropAssist;


        static ContextMenuSettingControl()
        {
            InitializeCommandStatic();
        }

        public ContextMenuSettingControl()
        {
            InitializeComponent();

            _vm = new ContextMenuSettingViewModel();
            this.Root.DataContext = _vm;

            this.CommandBindings.Add(new CommandBinding(NewRootCommand, NewRootCommand_Exec));
            this.CommandBindings.Add(new CommandBinding(NewRootSeparator, NewRootSeparator_Exec));
            this.CommandBindings.Add(new CommandBinding(NewRootFolderCommand, NewRootFolder_Exec));
            this.CommandBindings.Add(new CommandBinding(NewCommand, NewCommand_Exec));
            this.CommandBindings.Add(new CommandBinding(NewSeparator, NewSeparator_Exec));
            this.CommandBindings.Add(new CommandBinding(NewFolderCommand, NewFolder_Exec));
            this.CommandBindings.Add(new CommandBinding(PropertyCommand, Property_Exec));
            this.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec, SelectedItem_CanExec));
            this.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Exec, Rename_CanExec));

            _dropAssist = new ContextMenuTreeViewDropAssist(this.ContextMenuTreeView, _vm);
        }



        private void Initialize()
        {
            if (this.ContextMenuSetting != null)
            {
                _vm.Initialize(ContextMenuSetting);
            }
        }

        private void NewRootCommand_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            var node = _vm.MenuTree?.Root;
            if (node is null) return;
            NewCommandCore(node);
        }

        private void NewCommand_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ContextMenuTreeView.SelectedItem is not TreeListNode<MenuElement> node) return;
            NewCommandCore(node);
        }

        private void NewCommandCore(TreeListNode<MenuElement> node)
        {
            if (node.Value.MenuElementType == MenuElementType.None && node.Parent is not null)
            {
                node = node.Parent;
            }
            if (node.Value.MenuElementType != MenuElementType.Group) return;

            var menuElement = new CommandMenuElement();
            var dialog = new MenuCommandPropertyDialog(menuElement)
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            var result = dialog.ShowDialog();
            if (result == true)
            {
                _vm.Add(new TreeListNode<MenuElement>(menuElement), node);
            }
        }

        private void NewRootSeparator_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            var node = _vm.MenuTree?.Root;
            if (node is null) return;
            NewSeparatorCore(node);
        }

        private void NewSeparator_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ContextMenuTreeView.SelectedItem is not TreeListNode<MenuElement> node) return;
            NewSeparatorCore(node);
        }

        private void NewSeparatorCore(TreeListNode<MenuElement> node)
        {
            if (node.Value.MenuElementType == MenuElementType.None && node.Parent is not null)
            {
                node = node.Parent;
            }

            _vm.Add(new TreeListNode<MenuElement>(new SeparatorMenuElement()), node);
        }

        private async void NewRootFolder_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            var node = _vm.MenuTree?.Root;
            if (node is null) return;
            await NewFoderCore(node);
        }

        private async void NewFolder_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ContextMenuTreeView.SelectedItem is not TreeListNode<MenuElement> node) return;
            await NewFoderCore(node);
        }

        private async ValueTask NewFoderCore(TreeListNode<MenuElement> node)
        { 
            if (node.Value.MenuElementType == MenuElementType.None && node.Parent is not null)
            {
                node = node.Parent;
            }

            var newItem = _vm.Add(new TreeListNode<MenuElement>(new GroupMenuElement() { Name = ResourceService.GetString("@Word.NewFolder") }) , node);
            if (newItem != null)
            {
                this.ContextMenuTreeView.UpdateLayout();
                await RenameAsync(newItem);
            }
        }

        private void Property_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ContextMenuTreeView.SelectedItem is not TreeListNode<MenuElement> node) return;

            if (node.Value.MenuElementType == MenuElementType.Command)
            {
                var menuElement = node.Value.Clone();
                var dialog = new MenuCommandPropertyDialog(menuElement)
                {
                    Owner = Window.GetWindow(this),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    node.Value = menuElement;
                }
            }
        }


        private void SelectedItem_CanExec(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ContextMenuTreeView.SelectedItem is TreeListNode<MenuElement> node && node.Value.MenuElementType != MenuElementType.None;
        }

        private void Remove_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ContextMenuTreeView.SelectedItem is not TreeListNode<MenuElement> node) return;

            _vm.RemoveSelf(node);
        }

        private void Rename_CanExec(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ContextMenuTreeView.SelectedItem is TreeListNode<MenuElement> node && node.Value.MenuElementType != MenuElementType.None && node.Value.MenuElementType != MenuElementType.Separator;
        }

        private async void Rename_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ContextMenuTreeView.SelectedItem is not TreeListNode<MenuElement> node) return;

            await RenameAsync(node);
        }


        private async ValueTask RenameAsync(TreeListNode<MenuElement> item)
        {
            if (!item.CanRename()) return;

            var renamer = new MenuElementRenamer(this.ContextMenuTreeView, null);
            await renamer.RenameAsync(item);
        }


        private void ResetButton_Click(object? sender, RoutedEventArgs e)
        {
            _vm.Reset();
        }


        #region Drag and Drop

        public async ValueTask DragStartBehavior_DragBeginAsync(object? sender, DragStartEventArgs e, CancellationToken token)
        {
            if (e.DragItem is not TreeViewItem data)
            {
                e.Cancel = true;
                return;
            }

            switch (data.DataContext)
            {
                case TreeListNode<MenuElement> node:
                    e.Data.SetData(node);
                    e.AllowedEffects = DragDropEffects.Move | DragDropEffects.Copy;
                    break;

                default:
                    e.Cancel = true;
                    break;
            }

            await ValueTask.CompletedTask;
        }

        private void TreeView_PreviewDragEnter(object? sender, DragEventArgs e)
        {
            _dropAssist.OnDragEnter(sender, e);

            TreeView_PreviewDragOver(sender, e);
        }

        private void TreeView_PreviewDragLeave(object? sender, DragEventArgs e)
        {
            _dropAssist.OnDragLeave(sender, e);
        }

        private void TreeView_PreviewDragOver(object? sender, DragEventArgs e)
        {
            var scrolled = DragDropHelper.AutoScroll(sender, e);
            if (scrolled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }

            if (!e.Handled)
            {
                var target = _dropAssist.OnDragOver(sender, e);
                TreeView_DragDrop(sender, e, target, false);
            }

            if (e.Effects == DragDropEffects.None)
            {
                _dropAssist.HideAdorner();
            }
        }

        private void TreeView_Drop(object? sender, DragEventArgs e)
        {
            var target = _dropAssist.OnDrop(sender, e);

            TreeView_DragDrop(sender, e, target, true);
        }


        private void TreeView_DragDrop(object? sender, DragEventArgs e, DropTargetItem target, bool isDrop)
        {
            if (target.Item is not TreeViewItem treeViewItem || treeViewItem.DataContext is not TreeListNode<MenuElement> targetNode)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            var node = e.Data.GetData<TreeListNode<MenuElement>>();
            if (node is null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (!CanDropToMenuTree(targetNode, target.Delta, node))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            e.Effects = Keyboard.Modifiers == ModifierKeys.Control ? DragDropEffects.Copy : DragDropEffects.Move;

            if (isDrop)
            {
                if (e.Effects == DragDropEffects.Copy)
                {
                    _vm.Copy(node, targetNode, target.Delta);
                }
                else
                {
                    _vm.Move(node, targetNode, target.Delta);
                }
            }

            e.Handled = true;
            return;
        }

        private static bool CanDropToMenuTree(TreeListNode<MenuElement> targetNode, int delta, TreeListNode<MenuElement> node)
        {
            if (targetNode == node) return false;

            if (node.Value.MenuElementType == MenuElementType.None)
            {
                return false;
            }

            if (delta == 0)
            {
                return !targetNode.ParentContains(node) && !targetNode.Children.Contains(node);
            }
            else
            {
                return !targetNode.ParentContains(node);
            }
        }

        #endregion Drag and Drop

        #region ContextMenu

        private void ContextMenuTreeViewItem_MouseRightButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                e.Handled = true;
            }
        }

        private void ContextMenuTreeViewItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is not TreeViewItem viewItem)
            {
                return;
            }

            if (!viewItem.IsSelected)
            {
                return;
            }

            var contextMenu = viewItem.ContextMenu;
            contextMenu.Items.Clear();

            if (viewItem.DataContext is not TreeListNode<MenuElement> node)
            {
                return;
            }

            switch (node.Value.MenuElementType)
            {
                case MenuElementType.None:
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewCommand"), NewCommand));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewSeparator"), NewSeparator));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewFolder"), NewFolderCommand));
                    break;

                case MenuElementType.Group:
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewCommand"), NewCommand));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewSeparator"), NewSeparator));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewFolder"), NewFolderCommand));
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Delete"), RemoveCommand, Key.Delete.ToString()));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Rename"), RenameCommand, Key.F2.ToString()));
                    break;

                case MenuElementType.Command:
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Delete"), RemoveCommand, Key.Delete.ToString()));
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Rename"), RenameCommand, Key.F2.ToString()));
                    contextMenu.Items.Add(new Separator());
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Property"), PropertyCommand));
                    break;

                case MenuElementType.Separator:
                    contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.Delete"), RemoveCommand, Key.Delete.ToString()));
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private static MenuItem CreateMenuItem(string header, ICommand command)
        {
            return new MenuItem() { Header = header, Command = command };
        }

        private static MenuItem CreateMenuItem(string header, ICommand command, string inputGestureText)
        {
            return new MenuItem() { Header = header, Command = command, InputGestureText = inputGestureText };
        }

        private void ContextMenuTreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var contextMenu = this.ContextMenuTreeView.ContextMenu;
            contextMenu.Items.Clear();

            contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewCommand"), NewRootCommand));
            contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewSeparator"), NewRootSeparator));
            contextMenu.Items.Add(CreateMenuItem(Properties.TextResources.GetString("FolderTree.Menu.NewFolder"), NewRootFolderCommand));
        }

        #endregion ContextMenu


    }


    public class ContextMenuTreeViewDropAssist : InsertDropAssist
    {
        public ContextMenuTreeViewDropAssist(TreeView treeView, ContextMenuSettingViewModel vm)
            : base(treeView, new ContextMenuTreeViewDropAssistProfile(vm))
        {
        }
    }

    public class ContextMenuTreeViewDropAssistProfile : TreeViewDropAssistProfile
    {
        private readonly ContextMenuSettingViewModel _vm;

        public ContextMenuTreeViewDropAssistProfile(ContextMenuSettingViewModel vm)
        {
            _vm = vm;
        }

        public override bool IsFolder(FrameworkElement? item)
        {
            return item is TreeViewItem e && (e.DataContext is TreeListNode<MenuElement> node) && node.Value.MenuElementType == MenuElementType.Group;
        }
    }

    public class MenuElementRenamer : TreeViewItemRenamer<TreeListNode<MenuElement>>
    {
        public MenuElementRenamer(TreeView treeView, IToolTipService? toolTipService) : base(treeView, toolTipService)
        {
        }

        protected override RenameControl CreateRenameControl(TreeView treeView, TreeListNode<MenuElement> item)
        {
            var control = base.CreateRenameControl(treeView, item);
            control.IsInvalidSeparatorChars = true;
            return control;
        }
    }

}
