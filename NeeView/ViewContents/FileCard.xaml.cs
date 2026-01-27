using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// FileCard.xaml の相互作用ロジック
    /// </summary>
    public partial class FileCard : UserControl
    {
        public FileCard()
        {
            InitializeComponent();
            this.Root.DataContext = this;
            this.Root.Loaded += (s, e) => UpdatePartLayout();
            this.Root.SizeChanged += (s, e) => UpdatePartLayout();
        }


        public ImageSource? Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(FileCard), new PropertyMetadata(null, OnIconPropertyChanged));

        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileCard control)
            {
                control.FileIcon.Source = e.NewValue as ImageSource;
            }
        }

        public ArchiveEntry ArchiveEntry
        {
            get { return (ArchiveEntry)GetValue(ArchiveEntryProperty); }
            set { SetValue(ArchiveEntryProperty, value); }
        }

        public static readonly DependencyProperty ArchiveEntryProperty =
            DependencyProperty.Register("ArchiveEntry", typeof(ArchiveEntry), typeof(FileCard), new PropertyMetadata(null, OnArchiveEntryPropertyChanged));

        private static void OnArchiveEntryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileCard control && e.NewValue is ArchiveEntry entry)
            {
                control.UpdateFileName();
                control.UpdateFileSize();
                control.UpdateFileTimestamp();
            }
        }


        private void UpdatePartLayout()
        {
            this.IconGrid.Visibility = 256.0 < this.Root.ActualWidth ? Visibility.Visible : Visibility.Collapsed;

            var pos = this.FileSizeTextBlock.TranslatePoint(new Point(FileSizeTextBlock.ActualWidth, FileSizeTextBlock.ActualHeight), this.Root);
            this.FileSizeTextBlock.Visibility = pos.Y < this.Root.ActualHeight ? Visibility.Visible : Visibility.Hidden;

            pos = this.FileTimespampTextBlock.TranslatePoint(new Point(FileTimespampTextBlock.ActualWidth, FileTimespampTextBlock.ActualHeight), this.Root);
            this.FileTimespampTextBlock.Visibility = pos.Y < this.Root.ActualHeight ? Visibility.Visible : Visibility.Hidden;

            UpdateFileTimestamp();
        }

        private void UpdateFileName()
        {
            if (ArchiveEntry is null) return;
            this.FileNameTextBlock.Text = ArchiveEntry.EntryName?.TrimEnd('\\').Replace("\\", " > ", StringComparison.Ordinal);
        }

        private void UpdateFileTimestamp()
        {
            if (ArchiveEntry is null) return;
            var format = 256.0 < this.Root.ActualWidth ? DateTimeTools.DefaultDateTimePattern : DateTimeTools.DefaultDatePattern;
            this.FileTimespampTextBlock.Text = ArchiveEntry.LastWriteTime.ToString(format, CultureInfo.CurrentCulture);
        }

        private void UpdateFileSize()
        {
            if (ArchiveEntry is null) return;
            this.FileSizeTextBlock.Text = FileSizeToStringConverter.ByteToDisplayString(ArchiveEntry.Length);
        }
    }
}
