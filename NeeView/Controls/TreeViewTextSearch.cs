using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// TextSearch behavior for TreeView
    /// </summary>
    public class TreeViewTextSearch : DependencyObject, INavigateControl
    {
        private readonly SimpleTextSearch _textSearch = new();
        private readonly TreeView _attachTo;

        private TreeViewTextSearch(TreeView itemsControl)
        {
            ArgumentNullException.ThrowIfNull(itemsControl);

            _attachTo = itemsControl;
            _textSearch.ResetState();
        }

        #region Attached properties

        private static readonly DependencyPropertyKey TreeViewTextSearchInstancePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("TreeViewTextSearchInstance", typeof(TreeViewTextSearch), typeof(TreeViewTextSearch), new FrameworkPropertyMetadata(null));

        private static readonly DependencyProperty TreeViewTextSearchInstanceProperty =
            TreeViewTextSearchInstancePropertyKey.DependencyProperty;


        public static readonly DependencyProperty IsTextSearchEnabledProperty =
            DependencyProperty.RegisterAttached("IsTextSearchEnabled", typeof(bool), typeof(TreeViewTextSearch), new FrameworkPropertyMetadata(false, IsTextSearchEnabled_PropertyChanged));

        public static void SetIsTextSearchEnabled(DependencyObject element, bool value)
        {
            ArgumentNullException.ThrowIfNull(element);
            element.SetValue(IsTextSearchEnabledProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetIsTextSearchEnabled(DependencyObject element)
        {
            ArgumentNullException.ThrowIfNull(element);
            return (bool)element.GetValue(IsTextSearchEnabledProperty);
        }

        private static void IsTextSearchEnabled_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TreeView treeView) throw new NotSupportedException("TreeViewTextSearch supports only TreeView.");

            if (e.NewValue is bool isEnable && isEnable == true)
            {
                treeView.TextInput += TreeView_TextInput;
            }
            else
            {
                treeView.TextInput -= TreeView_TextInput;
            }
        }

        internal static TreeViewTextSearch EnsureInstance(TreeView itemsControl)
        {
            TreeViewTextSearch instance = (TreeViewTextSearch)itemsControl.GetValue(TreeViewTextSearchInstanceProperty);

            if (instance == null)
            {
                instance = new TreeViewTextSearch(itemsControl);
                itemsControl.SetValue(TreeViewTextSearchInstancePropertyKey, instance);
            }

            return instance;
        }

        #endregion Attached properties


        public object SelectedItem => _attachTo.SelectedItem;


        private static void TreeView_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TreeView treeView) throw new NotSupportedException("TreeViewTextSearch supports only TreeView.");

            var textSearch = EnsureInstance(treeView);
            var source = new TreeViewTextSearchCollection(textSearch, treeView.Items.Cast<ITreeViewNode>());
            textSearch.DoSearch(source, e.Text);
            e.Handled = true;
        }

        private void DoSearch(ITextSearchCollection collection, string nextChar)
        {
            _textSearch.DoSearch(collection, nextChar);
        }

        public void NavigateToItem(object item)
        {
            if (item is not ITreeViewNode itemData) return;

            var node = item as ITreeNode;
            _attachTo.ScrollIntoView(node);
            itemData.IsSelected = true;
        }
    }
}
