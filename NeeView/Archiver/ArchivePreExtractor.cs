//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 事前展開
    /// </summary>
    [LocalDebug]
    public partial class ArchivePreExtractor : IDisposable
    {
        private readonly Archive _archiver;
        private CancellationTokenSource _cancellationTokenSource = new();
        private ArchivePreExtractState _state;
        private TempDirectory? _extractDirectory;
        private readonly System.Threading.Lock _lock = new();
        private bool _disposedValue;


        public ArchivePreExtractor(Archive archiver)
        {
            _archiver = archiver;
            _state = ArchivePreExtractState.None;
        }


        [Subscribable]
        public event EventHandler<PreExtractStateChangedEventArgs>? StateChanged;

        [Subscribable]
        public event EventHandler<PreExtractExceptionEventArgs>? ExtractCanceled;

        [Subscribable]
        public event EventHandler<PreExtractExceptionEventArgs>? ExtractFailed;

        [Subscribable]
        public event EventHandler? ExtractCompleted;


        public ArchivePreExtractState State => _state;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                _cancellationTokenSource.Cancel();
                _disposedValue = true;
            }
        }

        ~ArchivePreExtractor()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Sleep()
        {
            if (_disposedValue) return;
            if (_state == ArchivePreExtractState.Sleep) return;

            _cancellationTokenSource.Cancel();
            SetState(ArchivePreExtractState.Sleep);
        }

        public void Resume()
        {
            if (_disposedValue) return;
            if (_state != ArchivePreExtractState.Sleep) return;

            LocalWriteLine($"Resume");
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            SetState(ArchivePreExtractState.None);
        }

        private void SetState(ArchivePreExtractState state, CancellationToken sleepToken)
        {
            if (sleepToken.IsCancellationRequested) return;

            SetState(state);
        }

        private void SetState(ArchivePreExtractState state)
        {
            if (_state != state)
            {
                _state = state;
                LocalWriteLine($"State = {state}");
                StateChanged?.Invoke(this, new PreExtractStateChangedEventArgs(state));
            }
        }

        /// <summary>
        /// 可能であれば状態を初期化する
        /// </summary>
        private void ResetState()
        {
            if (State.IsReady())
            {
                SetState(ArchivePreExtractState.None, _cancellationTokenSource.Token);
            }
        }

        // TODO: async? 7z の solid 判定は非同期化する必要あるかも？
        private bool CanPreExtract()
        {
            return _archiver.CanPreExtract();
        }


        /// <summary>
        /// 事前展開メイン
        /// </summary>
        /// <param name="token"></param>
        /// <returns>事前展開が実行され完了すれば true, 実行されなければ false</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="PreExtractException">スリープ状態です</exception>
        private async ValueTask<bool> PreExtractAsync(CancellationToken token)
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);

            Debug.Assert(CanPreExtract());

            var sleepToken = _cancellationTokenSource.Token;

            // NOTE: 実行は同時に１つのみ
            lock (_lock)
            {
                if (sleepToken.IsCancellationRequested) throw new PreExtractException("PreExtractor is asleep");
                if (!State.IsReady()) return false;
                SetState(ArchivePreExtractState.Extracting, sleepToken);
            }

            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, sleepToken);

                var sw = Stopwatch.StartNew();
                LocalWriteLine($"PreExtract ...");
                if (_extractDirectory is null)
                {
                    var directory = Temporary.Current.CreateCountedTempFileName("arc", "");
                    Directory.CreateDirectory(directory);
                    _extractDirectory = new TempDirectory(directory);
                    LocalWriteLine($"PreExtract create directory. {sw.ElapsedMilliseconds}ms");
                }

                // NOTE: 事前展開は常にパスワード要求。コレ大丈夫？
                await _archiver.PreExtractAsync(_extractDirectory.Path, true, linked.Token);
                sw.Stop();
                LocalWriteLine($"PreExtract done. {sw.ElapsedMilliseconds}ms");
                SetState(ArchivePreExtractState.Done, sleepToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                SetState(ArchivePreExtractState.Canceled, sleepToken);
                throw;
            }
            catch
            {
                SetState(ArchivePreExtractState.Failed, sleepToken);
                throw;
            }
        }

        /// <summary>
        /// 事前展開リクエスト
        /// </summary>
        /// <param name="token"></param>
        private void RunPreExtractAsync(CancellationToken token)
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);

            // 実行は同時に１つのみ
            lock (_lock)
            {
                if (State == ArchivePreExtractState.Sleep) throw new PreExtractException("PreExtractor is asleep");
                if (!State.IsReady()) return;
            }

            // 非同期に実行。例外はイベントで通知する
            Task.Run(async () =>
            {
                try
                {
                    var isCompleted = await PreExtractAsync(token);
                    if (isCompleted)
                    {
                        ExtractCompleted?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    ExtractCanceled?.Invoke(this, new PreExtractExceptionEventArgs(ex));
                }
                catch (Exception ex)
                {
                    ExtractFailed?.Invoke(this, new PreExtractExceptionEventArgs(ex));
                }
            });
        }

        /// <summary>
        /// エントリーの事前展開完了を待機
        /// </summary>
        /// <remarks>
        /// 事前展開開始も行う
        /// </remarks>
        /// <param name="entry"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async ValueTask WaitPreExtractAsync(ArchiveEntry entry, CancellationToken token)
        {
            if (!CanPreExtract()) return;
            if (entry.Data is not null) return;

            // キャンセル状態を初期状態に戻す
            ResetState();

            LocalWriteLine($"WaitPreExtract: {entry} ...");

            RunPreExtractAsync(CancellationToken.None);

            // wait for entry.Data changed
            using var disposables = new DisposableCollection();
            var tcs = new TaskCompletionSource();
            disposables.Add(token.Register(() => tcs.TrySetCanceled()));
            disposables.Add(entry.SubscribeDataChanged((s, e) => tcs.TrySetResult()));
            disposables.Add(this.SubscribeExtractCompleted((s, e) => tcs.TrySetResult()));
            disposables.Add(this.SubscribeExtractCanceled((s, e) => tcs.TrySetCanceled()));
            disposables.Add(this.SubscribeExtractFailed((s, e) => tcs.TrySetException(e.Exception)));

            if (entry.Data is null && !State.IsCompleted())
            {
                await tcs.Task;
            }

            if (entry.Data is null)
            {
                
                throw new PreExtractException($"Could not pre extract: {entry}");
            }

            LocalWriteLine($"WaitPreExtract: {entry} done.");

            LocalDebug.WriteLine("TEST");
        }


        private string TraceHeader()
        {
            return "[" + _archiver.EntryName + "]";
        }

        [Conditional("LOCAL_DEBUG")]
        private void LocalWriteLine(string s)
        {
            LocalDebug.WriteLine(TraceHeader() + ": " + s);
        }
    }


    public class PreExtractStateChangedEventArgs(ArchivePreExtractState state) : EventArgs
    {
        public ArchivePreExtractState State { get; } = state;
    }

    public class PreExtractExceptionEventArgs(Exception exception) : EventArgs
    {
        public Exception Exception { get; } = exception;
    }

    public class PreExtractException : Exception
    {
        public PreExtractException(string message) : base(message) { }
        public PreExtractException(string message, Exception innerException) : base(message, innerException) { }
    }
}

