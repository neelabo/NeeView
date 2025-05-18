using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using NeeLaboratory.Threading.Jobs;

namespace NeeView
{
    /// <summary>
    /// BookHubコマンド引数基底
    /// </summary>
    public class BookHubCommandArgs
    {
    }

    /// <summary>
    /// BookHubコマンド基底
    /// </summary>
    public abstract class BookHubCommand : JobBase
    {
        protected BookHub _bookHub { get; private set; }


        public BookHubCommand(BookHub bookHub)
        {
            _bookHub = bookHub;
        }

        public bool CanBeCanceled { get; set; } = true;
    }

    /// <summary>
    /// CommandLoad 引数
    /// </summary>
    public class BookHubCommandLoadArgs : BookHubCommandArgs
    {
        public BookHubCommandLoadArgs(string path, string sourcePath)
        {
            Debug.Assert(path is not null);
            Debug.Assert(sourcePath is not null);

            Path = path;
            SourcePath = sourcePath;
        }

        public object? Sender { get; set; }
        public string Path { get; set; }
        public string SourcePath { get; set; }
        public string? StartEntry { get; set; }
        public BookLoadOption Option { get; set; }
        public bool IsRefreshFolderList { get; set; }
        public ArchiveHint ArchiveHint { get; set; } = ArchiveHint.None;
    }

    /// <summary>
    /// CommandLoad
    /// </summary>
    public class BookHubCommandLoad : BookHubCommand
    {
        private readonly BookHubCommandLoadArgs _args;

        public string Path => _args.Path;

        public BookHubCommandLoad(BookHub bookHub, BookHubCommandLoadArgs args) : base(bookHub)
        {
            _args = args;
        }

        protected override async ValueTask ExecuteAsync(CancellationToken token)
        {
            await _bookHub.LoadAsync(_args, token);
        }
    }


    /// <summary>
    /// CommandUnload引数
    /// </summary>
    public class BookHubCommandUnloadArgs : BookHubCommandArgs
    {
        public object? Sender { get; set; }
        public bool IsClearViewContent { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// CommandUnload
    /// </summary>
    public class BookHubCommandUnload : BookHubCommand
    {
        private readonly BookHubCommandUnloadArgs _args;

        public BookHubCommandUnload(BookHub bookHub, BookHubCommandUnloadArgs args) : base(bookHub)
        {
            _args = args;

            // キャンセル不可
            this.CanBeCanceled = false;
        }

        protected override async ValueTask ExecuteAsync(CancellationToken token)
        {
            _bookHub.Unload(_args);

            // ブックを閉じたときの移動履歴を表示するために null を履歴に登録
            BookHubHistory.Current.Add(_args.Sender, null);

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// BookHub用コマンドエンジン
    /// </summary>
    public class BookHubCommandEngine : SingleJobEngine
    {
        public BookHubCommandEngine() : base(nameof(BookHubCommandEngine))
        {
        }

        public BookHubCommandEngine(string name) : base(name)
        {
        }


        protected override Queue<IJob> Enqueue(IJob job, Queue<IJob> queue)
        {
            if (job is not BookHubCommand) throw new ArgumentException("job must be BookHubCommand");
            if (queue is null) throw new ArgumentNullException(nameof(queue));

            // 全コマンドキャンセル
            // ※ Unloadはキャンセルできないので残る
            foreach (var e in AllJobs().OfType<BookHubCommand>().Where(e => e.CanBeCanceled))
            {
                e.Cancel();
            }

            return base.Enqueue(job, queue);
        }
    }
}
