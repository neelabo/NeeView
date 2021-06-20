﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Windows;

namespace NeeView
{
    public class PdfArchiveConfig : BindableBase
    {
        private static Version MinimumSupportVersion = new Version(10, 0, 10240);
        // NOTE: マニフェストでWindows 10をサポートする様に宣言する必要がある
        public static bool RunningOsIsSupportVersion => System.Environment.OSVersion.Version >= MinimumSupportVersion;

        public static FileTypeCollection DefaultSupportFileTypes { get; } = new FileTypeCollection(".pdf");

        private bool _isEnabled = true;
        private Size _renderSize = new Size(1920, 1080);
        private FileTypeCollection _supportFileTypes = (FileTypeCollection)DefaultSupportFileTypes.Clone();


        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled && RunningOsIsSupportVersion; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember]
        public FileTypeCollection SupportFileTypes
        {
            get { return _supportFileTypes; }
            set { SetProperty(ref _supportFileTypes, value); }
        }

        [PropertyMember]
        public Size RenderSize
        {
            get { return _renderSize; }
            set { SetProperty(ref _renderSize, new Size(Math.Max(value.Width, 256), Math.Max(value.Height, 256))); }
        }
    }
}