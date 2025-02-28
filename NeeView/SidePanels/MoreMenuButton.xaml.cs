using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
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

namespace NeeView
{

    /// <summary>
    /// MoreMenu.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class MoreMenuButton : UserControl, INotifyPropertyChanged
    {
        private ContextMenu? _moreMenu;
        private bool _isMoreButtonChecked;


        public MoreMenuButton()
        {
            InitializeComponent();

            this.MoreButton.DataContext = this;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public ImageSource? ImageSource
        {
            get { return (ImageSource?)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(MoreMenuButton), new PropertyMetadata(null, ImageSource_Changed));

        private static void ImageSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MoreMenuButton control)
            {
                control.UpdateImageSource();
            }
        }


        public MoreMenuDescription Description
        {
            get { return (MoreMenuDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(MoreMenuDescription), typeof(MoreMenuButton), new PropertyMetadata(null, DescriptionChanged));

        private static void DescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MoreMenuButton control)
            {
                control.Reset();
            }
        }


        public ContextMenu? MoreMenu
        {
            get { return _moreMenu; }
            private set { SetProperty(ref _moreMenu, value); }
        }

        public bool IsMoreButtonChecked
        {
            get { return _isMoreButtonChecked; }
            set
            {
                if (SetProperty(ref _isMoreButtonChecked, value))
                {
                    OnMoreButtonCheckedChanged();
                }
            }
        }


        private void UpdateImageSource()
        {
            this.MoreButtonImage.Source = ImageSource ?? this.Resources["ic_more_24px_a"] as DrawingImage;
        }

        private void Reset()
        {
            MoreMenu = Description?.Create();
        }

        private void MoreButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MoreButton.IsChecked = !MoreButton.IsChecked;
            e.Handled = true;
        }


        private void OnMoreButtonCheckedChanged()
        {
            if (Description != null)
            {
                if (MoreMenu is not null)
                {
                    MoreMenu = Description.Update(MoreMenu);
                }
            }

            if (_isMoreButtonChecked)
            {
                ContextMenuWatcher.RaiseContextMenuOpening(this, this.MoreButton);
            }
            else
            {
                ContextMenuWatcher.RaiseContextMenuClosing(this, this.MoreButton);
            }
        }

    }


    public abstract class MoreMenuDescription
    {
        public abstract ContextMenu Create();

        public virtual ContextMenu Update(ContextMenu menu)
        {
            return menu;
        }


        protected MenuItem CreateCheckMenuItem(string header, Binding binding)
        {
            var item = new MenuItem();
            item.Header = header;
            item.IsCheckable = true;
            item.SetBinding(MenuItem.IsCheckedProperty, binding);
            return item;
        }

        protected MenuItem CreateCommandMenuItem(string header, ICommand command, Binding? binding = null)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = command;
            if (binding != null)
            {
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }
            return item;
        }

        protected MenuItem CreateCommandMenuItem(string header, string command)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = RoutedCommandTable.Current.Commands[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            var binding = CommandTable.Current.GetElement(command).CreateIsCheckedBinding();
            if (binding != null)
            {
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }

            return item;
        }
    }
}
