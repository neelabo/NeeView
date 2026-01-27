using NeeLaboratory.Generators;
using NeeView.Properties;
using NeeView.Text;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingItemCollectionControl.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class SettingItemCollectionControl : UserControl, INotifyPropertyChanged, IValueInitializable
    {
        public SettingItemCollectionControl()
        {
            InitializeComponent();
            this.Root.DataContext = this;
            this.AddButton.Content = TextResources.GetString("Word.Add");
            this.EditButton.Content = TextResources.GetString("Word.Edit");
            this.RemoveButton.Content = TextResources.GetString("Word.Remove");
            this.ResetButton.Content = TextResources.GetString("Word.Reset");

            this.CollectionChanged += SettingItemCollectionControl_CollectionChanged;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<CollectionChangeEventArgs>? CollectionChanged;


        #region DependencyProperties

        public StringCollection Collection
        {
            get { return (StringCollection)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(StringCollection), typeof(SettingItemCollectionControl), new PropertyMetadata(null, CollectionPropertyChanged));

        private static void CollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingItemCollectionControl control)
            {
                control.RaisePropertyChanged(nameof(Items));
            }
        }


        public StringCollection DefaultCollection
        {
            get { return (StringCollection)GetValue(DefaultCollectionProperty); }
            set { SetValue(DefaultCollectionProperty, value); }
        }

        public static readonly DependencyProperty DefaultCollectionProperty =
            DependencyProperty.Register("DefaultCollection", typeof(StringCollection), typeof(SettingItemCollectionControl), new PropertyMetadata(null));


        public bool IsHeightLocked
        {
            get { return (bool)GetValue(IsHeightLockedProperty); }
            set { SetValue(IsHeightLockedProperty, value); }
        }

        public static readonly DependencyProperty IsHeightLockedProperty =
            DependencyProperty.Register("IsHeightLocked", typeof(bool), typeof(SettingItemCollectionControl), new PropertyMetadata(true, IsHeightLockedChanged));

        private static void IsHeightLockedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingItemCollectionControl control)
            {
                control.Root.Height = control.IsHeightLocked ? 150.0 : double.NaN;
            }
        }


        public bool IsAlwaysResetEnabled
        {
            get { return (bool)GetValue(IsAlwaysResetEnabledProperty); }
            set { SetValue(IsAlwaysResetEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsAlwaysResetEnabledProperty =
            DependencyProperty.Register("IsAlwaysResetEnabled", typeof(bool), typeof(SettingItemCollectionControl), new PropertyMetadata(false, IsAlwaysRsetEnabledChanged));

        private static void IsAlwaysRsetEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingItemCollectionControl control)
            {
                control.RaisePropertyChanged(nameof(IsResetEnabled));
            }
        }


        public ISettingItemCollectionDescription Description
        {
            get { return (ISettingItemCollectionDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(ISettingItemCollectionDescription), typeof(SettingItemCollectionControl), new PropertyMetadata(null));

        #endregion DependencyProperties


        public string? AddDialogTitle { get; set; }
        public string? AddDialogHeader { get; set; }


        public List<string>? Items => Collection?.Items;

        public bool IsResetEnabled => IsAlwaysResetEnabled || (DefaultCollection != null && !DefaultCollection.Equals(Collection));

        public bool IsResetVisible => IsAlwaysResetEnabled || DefaultCollection != null;

        public bool IsRegexRuleEnabled { get; set; }

        public bool IsEditable { get; set; }


        private void SettingItemCollectionControl_CollectionChanged(object? sender, CollectionChangeEventArgs e)
        {
            RaisePropertyChanged(nameof(IsResetEnabled));
        }

        private void AddButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Collection == null) return;

            var dialog = CreateAddParameterDialog(TextResources.GetString("AddParameterDialog.Tile"), string.Empty);
            var result = dialog.ShowDialog();
            var newItem = dialog.Input?.Trim();
            if (result == true)
            {
                AddItem(newItem, true, true);
            }
        }

        private void EditButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!IsEditable) return;
            if (Collection == null) return;

            if (this.CollectionListBox.SelectedItem is not string item)
            {
                return;
            }

            var dialog = CreateAddParameterDialog(TextResources.GetString("AddParameterDialog.Tile.Edit"), item);
            var result = dialog.ShowDialog();
            var newItem = dialog.Input?.Trim();
            if (result == true && newItem != item)
            {
                RemoveItem(item, true, true);
                AddItem(newItem, true, true);
            }
        }

        private void RemoveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Collection == null) return;

            if (this.CollectionListBox.SelectedItem is not string item)
            {
                return;
            }

            RemoveItem(item, true, true);
        }

        private AddParameterDialog CreateAddParameterDialog(string defaultTitle, string input)
        {
            var rule = IsRegexRuleEnabled ? new RegexValidationRule() : null;
            var dialog = new AddParameterDialog(rule);
            dialog.Title = AddDialogTitle ?? defaultTitle;
            dialog.Header = AddDialogHeader ?? ""; ;
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Input = input;
            return dialog;
        }

        public void AddItem(string? item, bool updateView, bool updateSelect)
        {
            if (string.IsNullOrEmpty(item)) return;

            item = this.Collection.Add(item);

            if (updateView)
            {
                RaisePropertyChanged(nameof(Items));
                this.CollectionListBox.Items.Refresh();
            }

            if (updateSelect)
            {
                this.CollectionListBox.SelectedItem = item;
                this.CollectionListBox.ScrollIntoView(item);
            }

            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, item));
        }

        private void RemoveItem(string item, bool updateView, bool updateSelect)
        {
            var index = Collection.Items.IndexOf(item);
            if (index < 0) return;

            this.Collection.Remove(item);

            if (updateView)
            {
                RaisePropertyChanged(nameof(Items));
                this.CollectionListBox.Items.Refresh();
            }

            if (updateSelect)
            {
                index = Math.Min(index, Collection.Items.Count - 1);
                if (index >= 0)
                {
                    this.CollectionListBox.SelectedIndex = index;
                }
            }

            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, item));
        }

        private void ResetButton_Click(object? sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            var defaultCollection = DefaultCollection ?? Description?.GetDefaultCollection();

            if (Collection == null || defaultCollection == null) return;

            Collection.Restore(defaultCollection.Items);
            RaisePropertyChanged(nameof(Items));
            this.CollectionListBox.Items.Refresh();

            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        private void CollectionListBox_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                RemoveButton_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F2)
            {
                EditButton_Click(sender, e);
                e.Handled = true;
            }
        }

        public void InitializeValue()
        {
            Reset();
        }
    }

    public interface ISettingItemCollectionDescription
    {
        StringCollection GetDefaultCollection();
    }
}
