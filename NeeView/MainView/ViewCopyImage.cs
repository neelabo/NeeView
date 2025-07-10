using NeeView.Properties;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class ViewCopyImage : IViewCopyImage
    {
        private readonly PageFrameBoxPresenter _presenter;

        public ViewCopyImage(PageFrameBoxPresenter presenter)
        {
            _presenter = presenter;
        }


        public bool CanCopyImageToClipboard()
        {
            return GetSelectedImageSource() is BitmapSource;
        }

        // TODO: Bitmap でない ImageSource はレンダリングして Bitmap にする
        public void CopyImageToClipboard()
        {
            try
            {
                var imageSource = GetSelectedImageSource();
                if (imageSource is BitmapSource bitmapSource)
                {
                    ClipboardUtility.CopyImage(bitmapSource);
                }
            }
            catch (Exception e)
            {
                new MessageDialog($"{TextResources.GetString("Word.Cause")}: {e.Message}", TextResources.GetString("CopyImageErrorDialog.Title")).ShowDialog();
            }
        }

        private ImageSource? GetSelectedImageSource()
        {
            var pageFrameContent = _presenter.GetSelectedPageFrameContent();
            if (pageFrameContent == null) return null;

            var viewContent = pageFrameContent.ViewContents.FirstOrDefault();
            if (viewContent == null) return null;

            var imageSource = (viewContent as IHasImageSource)?.ImageSource;
            if (imageSource == null) return null;

            return imageSource;
        }
    }
}
