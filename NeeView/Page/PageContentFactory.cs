using NeeView.IO;
using System.IO;
using System.Threading;

namespace NeeView
{
    public class PageContentFactory
    {
        private readonly BookMemoryService? _bookMemoryService;
        private readonly bool _allowAnimatedImage;

        public PageContentFactory(BookMemoryService? bookMemoryService, bool arrowAnimatedImage)
        {
            _bookMemoryService = bookMemoryService;
            _allowAnimatedImage = arrowAnimatedImage;
        }


        public PageContent CreatePageContent(ArchiveEntry entry, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var path = entry.TargetPath;
            if (entry.IsImage())
            {
                if (entry.Archive is MediaArchive)
                {
                    return new MediaPageContent(entry, _bookMemoryService);
                }
                else if (entry.Archive is PdfArchive)
                {
                    return new PdfPageContent(entry, _bookMemoryService);
                }
                else if (PictureProfile.Current.IsSvgSupported(path))
                {
                    return new SvgPageContent(entry, _bookMemoryService);
                }
                else if (PictureProfile.Current.IsMediaSupported(path))
                {
                    return new MediaPageContent(entry, _bookMemoryService);
                }
                else if (_allowAnimatedImage && PictureProfile.Current.IsAnimatedGifSupported(path))
                {
                    return new AnimatedPageContent(entry, _bookMemoryService, AnimatedImageType.Gif);
                }
                else if (_allowAnimatedImage && PictureProfile.Current.IsAnimatedPngSupported(path))
                {
                    return new AnimatedPageContent(entry, _bookMemoryService, AnimatedImageType.Png);
                }
                else if (_allowAnimatedImage && PictureProfile.Current.IsAnimatedWebpSupported(path))
                {
                    return new AnimatedPageContent(entry, _bookMemoryService, AnimatedImageType.Webp);
                }
                else
                {
                    return new BitmapPageContent(entry, _bookMemoryService);
                }
            }
            else if (entry.IsBook())
            {
                return new ArchivePageContent(entry, _bookMemoryService);
                //page.Thumbnail.IsCacheEnabled = true;
            }
            else
            {
                var type = entry.IsDirectory ? ArchiveType.FolderArchive : ArchiveManager.Current.GetSupportedType(path);
                switch (type)
                {
                    case ArchiveType.None:
                        if (Config.Current.Image.Standard.IsAllFileSupported)
                        {
                            return new BitmapPageContent(entry, _bookMemoryService);
                        }
                        else
                        {
                            return new FilePageContent(entry, FilePageIcon.File, null, _bookMemoryService);
                        }
                    case ArchiveType.FolderArchive:
                        return new FilePageContent(entry, FilePageIcon.Folder, null, _bookMemoryService);
                    default:
                        return new FilePageContent(entry, FilePageIcon.Archive, null, _bookMemoryService);
                }
            }
        }
    }




}
