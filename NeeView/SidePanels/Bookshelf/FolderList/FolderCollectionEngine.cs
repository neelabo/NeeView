using NeeLaboratory.Threading.Jobs;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Jobs = NeeLaboratory.Threading.Jobs;

namespace NeeView
{

    public class FolderCollectionEngine : IDisposable
    {
        private readonly FolderCollection _folderCollection;
        private readonly DelaySingleJobEngine _engine;


        public FolderCollectionEngine(FolderCollection folderCollection)
        {
            _folderCollection = folderCollection;

            _engine = new DelaySingleJobEngine(nameof(FolderCollectionEngine));
            _engine.JobError += JobEngine_Error;
            _engine.StartEngine();
        }



        /// <summary>
        /// JobEngineで例外発生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JobEngine_Error(object? sender, Jobs.JobErrorEventArgs e)
        {
            Debug.WriteLine($"FolderCollection JOB Exception!: {e.Job}: {e.GetException().Message}");
            e.Handled = true;
        }

        /// <summary>
        /// 項目追加
        /// </summary>
        /// <param name="path"></param>
        public void RequestCreate(QueryPath path)
        {
            if (_disposedValue) return;

            _engine.Enqueue(new CreateJob(this, path, false));
        }

        /// <summary>
        /// 項目削除
        /// </summary>
        /// <param name="path"></param>
        public void RequestDelete(QueryPath path)
        {
            if (_disposedValue) return;

            _engine.Enqueue(new DeleteJob(this, path, false));
        }

        /// <summary>
        /// 項目名変更
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="path"></param>
        public void RequestRename(QueryPath oldPath, QueryPath path)
        {
            if (_disposedValue) return;

            if (oldPath == path || path == null)
            {
                return;
            }

            _engine.Enqueue(new RenameJob(this, oldPath, path, false));
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _engine.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion



        public abstract class FolderCollectionJob : JobBase
        {
        }


        public class CreateJob : FolderCollectionJob
        {
            private readonly FolderCollectionEngine _target;
            private readonly QueryPath _path;

            public CreateJob(FolderCollectionEngine target, QueryPath path, bool verify)
            {
                _target = target;
                _path = path;
            }

            protected override async Task ExecuteAsync(CancellationToken token)
            {
                ////Debug.WriteLine($"Create: {_path}");
                _target._folderCollection.AddItem(_path); // TODO: ファイルシステム以外のFolderCollectionでは不正な操作になる
                await Task.CompletedTask;
            }
        }

        public class DeleteJob : FolderCollectionJob
        {
            private readonly FolderCollectionEngine _target;
            private readonly QueryPath _path;

            public DeleteJob(FolderCollectionEngine target, QueryPath path, bool verify)
            {
                _target = target;
                _path = path;
            }

            protected override async Task ExecuteAsync(CancellationToken token)
            {
                ////Debug.WriteLine($"Delete: {_path}");
                _target._folderCollection.DeleteItem(_path);
                await Task.CompletedTask;
            }
        }

        public class RenameJob : FolderCollectionJob
        {
            private readonly FolderCollectionEngine _target;
            private readonly QueryPath _oldPath;
            private readonly QueryPath _path;

            public RenameJob(FolderCollectionEngine target, QueryPath oldPath, QueryPath path, bool verify)
            {
                _target = target;
                _oldPath = oldPath;
                _path = path;
            }

            protected override async Task ExecuteAsync(CancellationToken token)
            {
                ////Debug.WriteLine($"Rename: {_oldPath} => {_path}");
                _target._folderCollection.RenameItem(_oldPath, _path);
                await Task.CompletedTask;
            }
        }

    }
}
