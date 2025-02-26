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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PreviewContentControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PreviewContentControl : UserControl
    {
        public PreviewContentControl()
        {
            InitializeComponent();
        }


        public PreviewContent? Source
        {
            get { return (PreviewContent)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(PreviewContent), typeof(PreviewContentControl), new PropertyMetadata(null, Source_PropertyChanged));

        private static void Source_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PreviewContentControl control)
            {
                control.OnSourceChanged((PreviewContent)e.NewValue);
            }
        }

        private void OnSourceChanged(PreviewContent newValue)
        {
            if (newValue is null) return;

            this.PreviewImage.Source = newValue.ImageSource;
            this.LastWriteTimeTextBlock.Text = newValue.LastWriteTime == DateTime.MinValue ? "--" : newValue.LastWriteTime.ToString();
            this.LengthTextBlock.Text = newValue.Length < 0 ? "--" : FileSizeToStringConverter.ByteToDisplayString(newValue.Length);
            this.SizeTextBlock.Text = newValue.Width < 0 || newValue.Height < 0 ? "--" : $"{newValue.Width} x {newValue.Height}";
        }
    }
}