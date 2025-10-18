//#define LOCAL_DEBUG

using NeeLaboratory.Collection;
using NeeLaboratory.Generators;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Threading
{
    [LocalDebug]
    public partial class JobAction : IDisposable
    {
        private Task? _task;
        private bool _disposedValue;
        private readonly Lock _lock = new();
        private readonly UniqueQueue<Func<CancellationToken, Task>> _queue = new();
        private readonly CancellationTokenSource _tokenSource = new();

        public string Prefix { get; set; } = "";


        public void Remove(int id)
        {
            lock (_lock)
            {
                LocalDebug.WriteLine($"{Prefix}: {id}: Remove.");
                _queue.Remove(id);
            }
        }

        // TASKになった瞬間に動作してしまうのでアウト
        public Task? Request(int id, Func<CancellationToken, Task> job)
        {
            if (_disposedValue) return null;

            lock (_lock)
            {
                if (_task?.Status <= TaskStatus.Running)
                {
                    LocalDebug.WriteLine($"{Prefix}: {id}: Queue request.");
                    _queue.Enqueue(id, job);
                }
                else
                {
                    LocalDebug.WriteLine($"{Prefix}: {id}: Run now.");
                    _task = ActionAsync(job, _tokenSource.Token);
                }
            }

            return _task;
        }

        private void ContinuationAction(Task task)
        {
            lock (_lock)
            {
                var unit = _queue.Dequeue();
                if (!_disposedValue && unit is not null)
                {
                    LocalDebug.WriteLine($"{Prefix}: {unit.Id}: Action queue.");
                    _task = ActionAsync(unit.Value, _tokenSource.Token);
                }
                else
                {
                    LocalDebug.WriteLine($"{Prefix}: -: Complete requests.");
                    _task = null;
                }
            }
        }

        private Task ActionAsync(Func<CancellationToken, Task> task, CancellationToken token)
        {
            return task.Invoke(token)
                .ContinueWith(ContinuationAction, token);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tokenSource.Cancel();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
