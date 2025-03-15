using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// 画像ファイル出力
    /// </summary>
    // TODO: スケールをオリジナルにできないか？だがフィルターで求めるサイズにしている可能性も。悩ましい。
    public class ExportImage : BindableBase, IDisposable
    {
        private readonly ExportImageSource _source;
        private IImageExporter _exporter;
        private ExportImageMode _mode;
        private bool _hasBackground;
        private bool _isOriginalSize = true;
        private bool _isDotKeep;
        private FrameworkElement? _preview;
        private double _previewWidth = double.NaN;
        private double _previewHeight = double.NaN;
        private string _imageFormatNote = "";
        private bool _disposedValue;


        public ExportImage(ExportImageSource source)
        {
            _source = source;
            UpdateExporter();
        }


        public string? ExportFolder { get; set; }

        public ExportImageMode Mode
        {
            get { return _mode; }
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    UpdateExporter();
                }
            }
        }

        public bool HasBackground
        {
            get { return _hasBackground; }
            set
            {
                if (SetProperty(ref _hasBackground, value))
                {
                    UpdatePreview();
                }
            }
        }

        public bool IsOriginalSize
        {
            get { return _isOriginalSize; }
            set
            {
                if (SetProperty(ref _isOriginalSize, value))
                {
                    UpdatePreview();
                }
            }
        }

        public bool IsDotKeep
        {
            get { return _isDotKeep; }
            set
            {
                if (SetProperty(ref _isDotKeep, value))
                {
                    UpdatePreview();
                }
            }
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

        public int QualityLevel { get; internal set; }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _exporter?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private static IImageExporter CreateExporter(ExportImageMode mode, ExportImageSource source)
        {
            return mode switch
            {
                ExportImageMode.Original
                    => new OriginalImageExporter(source),
                ExportImageMode.View
                    => new ViewImageExporter(source),
                _
                    => throw new InvalidOperationException(),
            };
        }

        [MemberNotNull(nameof(_exporter))]
        public void UpdateExporter()
        {
            _exporter?.Dispose();
            _exporter = CreateExporter(_mode, _source);
            UpdatePreview();
        }

        public void UpdatePreview()
        {
            AppDispatcher.BeginInvoke(() =>
            {
                try
                {
                    var options = new ImageExporterCreateOptions()
                    {
                        HasBackground = _hasBackground,
                        IsOriginalSize = _isOriginalSize,
                        IsDotKeep = _isDotKeep,
                    };
                    var content = _exporter.CreateView(options);
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
            });
        }

        public ImageSource? CreateImageSource()
        {
            var options = new ImageExporterCreateOptions()
            {
                HasBackground = _hasBackground,
                IsOriginalSize = _isOriginalSize,
                IsDotKeep = _isDotKeep,
            };
            return _exporter.CreateImageSource(options);
        }

        public async Task ExportAsync(string path, bool isOverwrite, CancellationToken token)
        {
            path = System.IO.Path.GetFullPath(path);

            var options = new ImageExporterCreateOptions()
            {
                HasBackground = _hasBackground,
                IsOriginalSize = _isOriginalSize,
                IsDotKeep = _isDotKeep,
            };
            await _exporter.ExportAsync(path, isOverwrite, QualityLevel, options, token);
            ExportFolder = System.IO.Path.GetDirectoryName(path);
        }

        public string CreateFileName(ExportImageFileNameMode fileNameMode, ExportImageFormat format)
        {
            var nameMode = fileNameMode == ExportImageFileNameMode.Default
                ? _mode == ExportImageMode.Original ? ExportImageFileNameMode.Original : ExportImageFileNameMode.BookPageNumber
                : fileNameMode;

            var extension = _mode == ExportImageMode.Original
                ? LoosePath.GetExtension(_source.Pages[0].EntryLastName).ToLowerInvariant()
                : format == ExportImageFormat.Png ? ".png" : ".jpg";

            if (nameMode == ExportImageFileNameMode.Original)
            {
                var filename = LoosePath.ValidFileName(_source.Pages[0].EntryLastName);
                return System.IO.Path.ChangeExtension(filename, extension);
            }
            else
            {
                var bookName = LoosePath.GetFileNameWithoutExtension(_source.BookAddress);

                var indexLabel = _mode != ExportImageMode.Original && _source.Pages.Count > 1
                    ? $"{_source.Pages[0].Index:000}-{_source.Pages[1].Index:000}"
                    : $"{_source.Pages[0].Index:000}";

                return LoosePath.ValidFileName($"{bookName}_{indexLabel}{extension}");
            }
        }

        public bool CanExport()
        {
            return _exporter.CanExport();
        }

        public void ThrowIfCannotExport()
        {
            _exporter.ThrowIfCannotExport();
        }

        public DateTime GetLastWriteTime()
        {
            return _exporter.GetLastWriteTime();
        }

        public long GetLength(string path)
        {
            var options = new ImageExporterCreateOptions()
            {
                HasBackground = _hasBackground,
                IsOriginalSize = _isOriginalSize,
                IsDotKeep = _isDotKeep,
            };
            return _exporter.GetLength(path, QualityLevel, options);
        }
    }
}
