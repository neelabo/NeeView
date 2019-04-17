﻿using NeeView.Text;
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
    public partial class SettingItemCollectionControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        public SettingItemCollectionControl()
        {
            InitializeComponent();

            this.Root.DataContext = this;
            this.AddButton.Content = Properties.Resources.WordAdd + "...";
            this.RemoveButton.Content = Properties.Resources.WordRemove;
            this.ResetButton.Content = Properties.Resources.WordReset;
        }

        #region DependencyProperties

        public StringCollection Collection
        {
            get { return (StringCollection)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(StringCollection), typeof(SettingItemCollectionControl), new PropertyMetadata(null, CollectionChanged));

        private static void CollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

        #endregion DependencyProperties



        public string AddDialogTitle { get; set; }
        public string AddDialogHeader { get; set; }


        public List<string> Items => Collection?.Items;


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (Collection == null) return;

            var dialog = new AddParameterDialog();
            dialog.Title = AddDialogTitle ?? Properties.Resources.DialogAddParameterTile;
            dialog.Header = AddDialogHeader;
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
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Collection == null) return;

            var item = this.CollectionListBox.SelectedItem as string;
            if (item == null)
            {
                return;
            }

            Collection.Remove(item);
            RaisePropertyChanged(nameof(Items));
            this.CollectionListBox.Items.Refresh();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (Collection == null || DefaultCollection == null) return;

            Collection.Restore(DefaultCollection.Items);
            RaisePropertyChanged(nameof(Items));
            this.CollectionListBox.Items.Refresh();
        }
    }
}
