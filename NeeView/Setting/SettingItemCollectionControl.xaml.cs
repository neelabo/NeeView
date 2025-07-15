using NeeLaboratory.Generators;
using NeeView.Properties;
using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// SettingItemCollectionControl.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class SettingItemCollectionControl : UserControl, INotifyPropertyChanged, IValueInitializable
    {
        public SettingItemCollectionControl()
        {
            InitializeComponent();
            this.Root.DataContext = this;
            this.AddButton.Content = TextResources.GetString("Word.Add") + "...";
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


        private void SettingItemCollectionControl_CollectionChanged(object? sender, CollectionChangeEventArgs e)
        {
            RaisePropertyChanged(nameof(IsResetEnabled));
        }

        private void AddButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Collection == null) return;

            var dialog = new AddParameterDialog();
            dialog.Title = AddDialogTitle ?? TextResources.GetString("AddParameterDialog.Tile");
            dialog.Header = AddDialogHeader ?? "";
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                var input = this.Collection.Add(dialog.Input);
                RaisePropertyChanged(nameof(Items));
                this.CollectionListBox.Items.Refresh();
                this.CollectionListBox.SelectedItem = input;
                this.CollectionListBox.ScrollIntoView(input);

                CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, input));
            }
        }

        private void RemoveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Collection == null) return;

            if (this.CollectionListBox.SelectedItem is not string item)
            {
                return;
            }

            var index = Collection.Items.IndexOf(item);

            Collection.Remove(item);
            RaisePropertyChanged(nameof(Items));
            this.CollectionListBox.Items.Refresh();

            index = Math.Min(index, Collection.Items.Count - 1);
            if (index >= 0)
            {
                this.CollectionListBox.SelectedIndex = index;
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
