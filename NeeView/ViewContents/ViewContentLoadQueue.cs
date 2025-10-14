//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ViewContent の Load 要求を管理する
    /// </summary>
    /// <remarks>
    /// Load 中は要求だけ保持しておき、Load 完了後にあらためて実行する。
    /// </remarks>
    [LocalDebug]
    public partial class ViewContentLoadQueue
    {
        private readonly ViewContent _viewContent;
        private Task? _task;
        private bool _request;
        private CancellationToken _requestToken;
        private readonly Lock _lock = new();

        public ViewContentLoadQueue(ViewContent viewContent)
        {
            _viewContent = viewContent;
        }

        private string Prefix => _viewContent.ArchiveEntry.ToString() ?? "";

        public Task? RequestLoadViewSource(CancellationToken token)
        {
            lock (_lock)
            {
                if (_task?.Status <= TaskStatus.Running)
                {
                    LocalDebug.WriteLine($"{Prefix}: Queue request.");
                    _requestToken = token;
                    _request = true;
                }
                else
                {
                    LocalDebug.WriteLine($"{Prefix}: Run request.");
                    _task = LoadViewSourceAsync(token);
                }
            }

            return _task;
        }

        private void ContinuationAction(Task task)
        {
            lock (_lock)
            {
                if (_request)
                {
                    LocalDebug.WriteLine($"{Prefix}: Flush queue.");
                    _request = false;
                    _task = LoadViewSourceAsync(_requestToken);
                }
                else
                {
                    LocalDebug.WriteLine($"{Prefix}: Complete requests.");
                    _task = null;
                }
            }
        }

        private Task? LoadViewSourceAsync(CancellationToken token)
        {
            if (!_viewContent.CanLoadViewSource())
            {
                LocalDebug.WriteLine($"{Prefix}: Cannot Request");
                return null;
            }

            return Task.Run(() => _viewContent.LoadViewSourceAsync(token), token)
                .ContinueWith(ContinuationAction, CancellationToken.None);
        }
    }
}
