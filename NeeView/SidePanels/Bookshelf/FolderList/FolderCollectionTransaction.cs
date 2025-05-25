//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// 簡易トランザクション
    /// </summary>
    /// <remarks>
    /// 簡易のため、名前変更を追加削除として扱う。
    /// </remarks>
    [LocalDebug]
    public partial class FolderCollectionTransaction
    {
        private readonly List<QueryPath> _addItems = new();
        private readonly List<QueryPath> _deleteItems = new();


        public void EnqueueCreate(QueryPath path)
        {
            if (_deleteItems.Contains(path))
            {
                LocalDebug.WriteLine($"DeleteItems - {path}");
                _deleteItems.Remove(path);
            }
            else if (!_addItems.Contains(path))
            {
                LocalDebug.WriteLine($"AddItems + {path}");
                _addItems.Add(path);
            }
        }

        public void EnqueueDelete(QueryPath path)
        {
            if (_addItems.Contains(path))
            {
                LocalDebug.WriteLine($"AddItems - {path}");
                _addItems.Remove(path);
            }
            else if (!_deleteItems.Contains(path))
            {
                LocalDebug.WriteLine($"DeleteItems + {path}");
                _deleteItems.Add(path);
            }
        }

        public void EnqueueRename(QueryPath oldPath, QueryPath path)
        {
            LocalDebug.WriteLine($"{oldPath} => {path}");

            // NOTE: トランザクションでは名前変更は削除と追加のセットして扱い、ファイルの同一性を無視する
            EnqueueDelete(oldPath);
            EnqueueCreate(path);
        }

        public void Flush(FolderCollectionEngine _engine)
        {
            foreach (var path in _addItems)
            {
                LocalDebug.WriteLine($"Flush.Add: {path}");
                _engine.EnqueueCreate(path);
            }
            foreach (var path in _deleteItems)
            {
                LocalDebug.WriteLine($"Flush.Delete: {path}");
                _engine.EnqueueDelete(path);
            }
            _addItems.Clear();
            _deleteItems.Clear();
        }

    }
}
