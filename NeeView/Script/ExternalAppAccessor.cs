using NeeLaboratory.Linq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public record class ExternalAppAccessor
    {
        private readonly ExternalApp _externalApp;

        public ExternalAppAccessor(ExternalApp externalApp)
        {
            _externalApp = externalApp;
        }

        internal ExternalApp Source => _externalApp;


        [WordNodeMember]
        public string Name
        {
            get { return _externalApp.DisplayName; }
            set { AppDispatcher.Invoke(() => _externalApp.Name = value); }
        }

        [WordNodeMember]
        public string Command
        {
            get { return _externalApp.Command ?? ""; }
            set { AppDispatcher.Invoke(() => _externalApp.Command = value); }
        }

        [WordNodeMember]
        public string Parameter
        {
            get { return _externalApp.Parameter; }
            set { AppDispatcher.Invoke(() => _externalApp.Parameter = value ?? ""); }
        }

        [WordNodeMember(DocumentType = typeof(ArchivePolicy))]
        public string ArchivePolicy
        {
            get { return _externalApp.ArchivePolicy.ToString(); }
            set { AppDispatcher.Invoke(() => _externalApp.ArchivePolicy = value.ToEnum<ArchivePolicy>()); }
        }

        [WordNodeMember]
        public string WorkingDirectory
        {
            get { return _externalApp.WorkingDirectory ?? ""; }
            set { AppDispatcher.Invoke(() => _externalApp.WorkingDirectory = value); }
        }

        [WordNodeMember]
        public void Execute(PageAccessor page)
        {
            Execute([page]);
        }

        [WordNodeMember]
        public void Execute(PageAccessor[] pages)
        {
            _externalApp.ExecuteAsync(pages.Select(e => e.Source).ToList(), CancellationToken.None).Wait();
        }

        [WordNodeMember]
        public void Execute(string path)
        {
            Execute([path]);
        }

        [WordNodeMember]
        public void Execute(string[] paths)
        {
            ExecuteAsync(paths, CancellationToken.None).Wait();
        }

        private async Task ExecuteAsync(string[] paths, CancellationToken token)
        {
            var pages = paths.Select(e => GetPage(e)).ToList();
            if (pages.All(e => e is not null))
            {
                await _externalApp.ExecuteAsync(pages.WhereNotNull(), token);
            }
            else
            {
                _externalApp.Execute(paths);
            }
        }

        /// <summary>
        /// パスからページに変換
        /// </summary>
        /// <param name="path"></param>
        /// <returns>現在ブックに対応するページ。存在しない場合は null</returns>
        private static Page? GetPage(string path)
        {
            return BookHub.Current.GetCurrentBook()?.Pages.GetPageWithEntryFullName(path);
        }
    }


}
