﻿using System;
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
    /// FilePageControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FilePageControl : UserControl
    {
        public FilePageControl(FileViewData context)
        {
            InitializeComponent();

            this.FileCard.Icon = context.ImageSource;
            this.FileCard.ArchiveEntry = context.Entry;
        }


        public static readonly DependencyProperty DefaultBrushProperty =
            DependencyProperty.Register(
            "DefaultBrush",
            typeof(Brush),
            typeof(FilePageControl),
            new FrameworkPropertyMetadata(Brushes.White, new PropertyChangedCallback(OnDefaultBrushChanged)));

        public Brush DefaultBrush
        {
            get { return (Brush)GetValue(DefaultBrushProperty); }
            set { SetValue(DefaultBrushProperty, value); }
        }

        private static void OnDefaultBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

    }
}
