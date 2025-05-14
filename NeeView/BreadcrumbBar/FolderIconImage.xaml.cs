using System;
using System.Collections.Generic;
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
    /// FolderIconImage.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderIconImage : UserControl
    {
        public FolderIconImage()
        {
            InitializeComponent();
        }


        public FolderIcon FolderIcon
        {
            get { return (FolderIcon)GetValue(FolderIconProperty); }
            set { SetValue(FolderIconProperty, value); }
        }

        public static readonly DependencyProperty FolderIconProperty =
            DependencyProperty.Register("FolderIcon", typeof(FolderIcon), typeof(FolderIconImage), new PropertyMetadata(FolderIcon.None, FolderIconProperty_Changed));

        private static void FolderIconProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FolderIconImage control)
            {
                control.Update();
            }
        }

        private void Update()
        {
            var resourceKey = FolderIcon switch
            {
                FolderIcon.Folder => "fic_folder",
                FolderIcon.Archive => "fic_folder_zip",
                FolderIcon.Media => "fic_folder_media",
                _ => ""
            };

            if (string.IsNullOrEmpty(resourceKey))
            {
                this.Image.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Image.Source = (ImageSource)this.Resources[resourceKey];
                this.Image.Visibility = Visibility.Visible;
            }
        }
    }

    public enum FolderIcon
    {
        None,
        Folder,
        Archive,
        Media,
    }
}
