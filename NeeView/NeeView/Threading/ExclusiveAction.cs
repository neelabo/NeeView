//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Threading
{
    [LocalDebug]
    public partial class ExclusiveAction
    {
        private Task? _task;
        private Func<Task>? _requestTask;
        private CancellationToken _requestToken;
        private readonly Lock _lock = new();


        public ExclusiveAction()
        {
        }


        public string Prefix { get; set; } = "";


        // TASKになった瞬間に動作してしまうのでアウト
        public Task? Request(Func<Task> task, CancellationToken token)
        {
            lock (_lock)
            {
                if (_task?.Status <= TaskStatus.Running)
                {
                    LocalDebug.WriteLine($"{Prefix}: Queue request.");
                    _requestToken = token;
                    _requestTask = task;
                }
                else
                {
                    LocalDebug.WriteLine($"{Prefix}: Run request.");
                    _task = ActionAsync(task, token);
                }
            }

            return _task;
        }

        private void ContinuationAction(Task task)
        {
            lock (_lock)
            {
                if (_requestTask is not null)
                {
                    LocalDebug.WriteLine($"{Prefix}: Flush queue.");
                    _task = ActionAsync(_requestTask, _requestToken);
                    _requestTask = null;
                }
                else
                {
                    LocalDebug.WriteLine($"{Prefix}: Complete requests.");
                    _task = null;
                }
            }
        }

        private Task ActionAsync(Func<Task> task, CancellationToken token)
        {
            return task.Invoke()
                .ContinueWith(ContinuationAction, token);
        }

    }
}
