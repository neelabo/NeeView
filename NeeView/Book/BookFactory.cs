using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public static class BookFactory
    {
        public static async ValueTask<Book> CreateAsync(object? sender, BookAddress address, BookCreateSetting setting, BookMemento memento, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var bookSource = await BookSourceFactory.CreateAsync(address, setting, token);

            // NOTE: メディアブック移動で前のブックに戻ったときにフレーム終端から再生させるためのフラグ設定
            // NOTE: 不要な機能と思われるので無効にしておく
#if false
            if (bookSource.IsMedia)
            {
                foreach (var mediaContent in bookSource.Pages.Select(e => e.ContentAccessor).OfType<MediaContent>())
                {
                    mediaContent.IsLastStart = setting.StartPage.StartPageType == BookStartPageType.LastPage;
                }
            }
#endif

            var book = new Book(address, bookSource, memento, setting.LoadOption, setting.IsNew, setting.ArchiveHint);

            // HACK: Start() で行いたい
            book.SetStartPage(sender, setting.StartPage);

            return book;
        }
    }

}
