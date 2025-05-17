using System;
using System.Threading.Tasks;
using System.Threading;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System.Windows.Media;
using System.Windows;

namespace NeeView
{
    public class SvgPageContent : PageContent
    {
        private readonly System.Threading.Lock _lock = new();


        public SvgPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService) : base(archiveEntry, bookMemoryService)
        {
        }

        protected override async ValueTask<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            var drawing = await LoadDrawingImageAsync(token);
            var pictureInfo = CreatePictureInfo(drawing, token);
            return await Task.FromResult(pictureInfo);
        }

        protected override async ValueTask<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            await Task.CompletedTask;

            try
            {
                token.ThrowIfCancellationRequested();
                var drawing = await LoadDrawingImageAsync(token);
                var pictureInfo = CreatePictureInfo(drawing, token);
                return PageSource.Create(new SvgPageData(drawing), pictureInfo);
            }
            catch (OperationCanceledException)
            {
                return PageSource.CreateEmpty();
            }
            catch (Exception ex)
            {
                return PageSource.CreateError(ex.Message);
            }
        }

        private async Task<DrawingGroup> LoadDrawingImageAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using (var stream = await ArchiveEntry.OpenEntryAsync(Decrypt, token))
            {
                var settings = new WpfDrawingSettings();
                settings.IncludeRuntime = false;
                settings.TextAsGeometry = true;

                DrawingGroup drawing;
                lock (_lock)
                {
                    var reader = new FileSvgReader(settings);
                    drawing = reader.Read(stream);
                }
                drawing.Freeze();

                return drawing;
            }
        }

        private PictureInfo CreatePictureInfo(DrawingGroup drawing, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var pictureInfo = new PictureInfo();
            var size = drawing.Bounds.IsEmpty ? new Size() : new Size(drawing.Bounds.Width, drawing.Bounds.Height);
            pictureInfo.OriginalSize = size;
            pictureInfo.Size = size;
            pictureInfo.BitsPerPixel = 32;
            pictureInfo.Decoder = "SharpVectors";
            return pictureInfo;
        }
    }
}
