using NeeLaboratory.Linq;
using NeeView.Effects;
using NeeView.PageFrames;
using NeeView.Properties;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class PrintController
    {
        private readonly MainViewComponent _viewComponent;
        private readonly PageFrameBoxPresenter _presenter;
        private readonly MainView _mainView;

        public PrintController(MainViewComponent viewComponent, MainView mainView, PageFrameBoxPresenter presenter)
        {
            _viewComponent = viewComponent;
            _mainView = mainView;
            _presenter = presenter;
        }

        public bool CanPrint()
        {
            return _presenter.GetSelectedPageFrameContent() is not null;
        }

        public void Print()
        {
            var pageFrameContent = _presenter.GetSelectedPageFrameContent();
            if (pageFrameContent is null) return;

            var frameworkElement = _presenter.GetSelectedPageFrameContent()?.ViewElement;
            if (frameworkElement is null) return;

            var transform = _presenter.GetSelectedPageFrameContent()?.ViewTransform;
            if (transform is null) return;

            try
            {
                Print(Window.GetWindow(_mainView), pageFrameContent, frameworkElement, transform, _mainView.View.ActualWidth, _mainView.View.ActualHeight);
            }
            catch(Exception ex)
            {
                new MessageDialog($"{TextResources.GetString("Word.Cause")}: {ex.Message}", TextResources.GetString("PrintErrorDialog.Title")).ShowDialog();
            }
        }


        private record class MediaStorage(IMediaPlayer Player, bool IsOldEnabled);

        private void Print(Window owner, PageFrameContent content, FrameworkElement element, Transform transform, double width, double height)
        {
            if (!CanPrint()) return;

            var contents = content.ViewContents;
            var mainContent = content.ViewContents.FirstOrDefault();

            // アニメーション停止
            var medias = contents.Select(e => (e as IHasViewContentMediaPlayer)?.Player).WhereNotNull().Select(e => new MediaStorage(e, e.IsEnabled)).ToList();
            foreach(var media in medias)
            {
                media.Player.IsEnabled = false;
            }

            // 読み込み停止
            BookHub.Current.IsEnabled = false;

            // スライドショー停止
            SlideShow.Current.PauseSlideShow();

            try
            {
                var background = _viewComponent.Background;
                var bg1 = background.Bg1Brush;
                var bg2 = background.Bg2Brush;

                var context = new PrintContext(
                    pageFrameContent: content,
                    mainContent: mainContent,
                    contents: contents,
                    view: element,
                    viewTransform: transform,
                    viewWidth: width,
                    viewHeight: height,
                    viewEffect: ImageEffect.Current.Effect,
                    background: bg1,
                    backgroundFront: bg2
                );

                var dialog = new PrintWindow(context);
                dialog.Owner = owner;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.ShowDialog();
            }
            finally
            {
                // スケールモード復元
                foreach (var viewContent in contents.OfType<IHasScalingMode>())
                {
                    viewContent.ScalingMode = null;
                }

                // アニメーション復元
                foreach (var media in medias)
                {
                    media.Player.IsEnabled = media.IsOldEnabled;
                }

                // 読み込み再会
                BookHub.Current.IsEnabled = true;

                // スライドショー再開
                SlideShow.Current.ResumeSlideShow();
            }
        }
    }

}
