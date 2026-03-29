using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    public class ExportImagePreview : BindableBase
    {
        private readonly ExportImage _exportImage;

        private FrameworkElement? _preview;
        private double _previewWidth = double.NaN;
        private double _previewHeight = double.NaN;
        private string _imageFormatNote = "";

        public ExportImagePreview(ExportImage exportImage)
        {
            _exportImage = exportImage;
        }

        public FrameworkElement? Preview
        {
            get { return _preview; }
            set { SetProperty(ref _preview, value); }
        }

        public double PreviewWidth
        {
            get { return _previewWidth; }
            set { SetProperty(ref _previewWidth, value); }
        }

        public double PreviewHeight
        {
            get { return _previewHeight; }
            set { SetProperty(ref _previewHeight, value); }
        }

        public string ImageFormatNote
        {
            get { return _imageFormatNote; }
            set { SetProperty(ref _imageFormatNote, value); }
        }

        public void UpdatePreview()
        {
            try
            {
                var content = _exportImage.Exporter.CreateView(_exportImage);
                if (content is null) throw new InvalidOperationException();
                Preview = content.View;
                PreviewWidth = content.Size.IsEmpty ? double.NaN : content.Size.Width;
                PreviewHeight = content.Size.IsEmpty ? double.NaN : content.Size.Height;
                ImageFormatNote = content.Size.IsEmpty ? "" : $"{(int)content.Size.Width} x {(int)content.Size.Height}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Preview = null;
                PreviewWidth = double.NaN;
                PreviewHeight = double.NaN;
                ImageFormatNote = "Error.";
            }
        }
    }
}