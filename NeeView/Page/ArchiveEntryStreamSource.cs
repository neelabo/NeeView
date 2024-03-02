﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntry を StreamSourceにする
    /// </summary>
    public class ArchiveEntryStreamSource : IStreamSource, IHasCache
    {
        private ArraySegment<byte> _cache;

        public ArchiveEntryStreamSource(ArchiveEntry archiveEntry)
        {
            ArchiveEntry = archiveEntry;
        }

        public ArchiveEntry ArchiveEntry { get; }

        public long Length => ArchiveEntry.Length;

        public long CacheSize => _cache.Count;

        public async Task<Stream> OpenStreamAsync(CancellationToken token)
        {
            await CreateCacheAsync(token);

            if (_cache.Array is not null)
            {
                return new MemoryStream(_cache.Array, _cache.Offset, _cache.Count, false);
            }
            else
            {
                return await ArchiveEntry.OpenEntryAsync(token);
            }
        }

        public async Task CreateCacheAsync(CancellationToken token)
        {
            // 展開処理の重複を避けるため、ファイルシステムエントリ以外はキャッシュを作る
            if (_cache.Array is not null || ArchiveEntry.HasCache || ArchiveEntry.IsFileSystem) return;

            using var stream = await ArchiveEntry.OpenEntryAsync(token);

            // メモリストリームであればバッファを直接取得
            if (stream is MemoryStream memoryStream)
            {
                if (memoryStream.TryGetBuffer(out _cache))
                {
                    return;
                }
                else
                {
                    Debug.Assert(false, "Unable to obtain stream buffer.");
                }
            }

            // バッファが直接取得できなかったときはストリームから生成する
            var array = stream.ToArray(0, (int)ArchiveEntry.Length);
            _cache = new ArraySegment<byte>(array);
            Debug.Assert((int)ArchiveEntry.Length == _cache.Count);
        }

        public void ClearCache()
        {
            _cache = default;
        }

        public long GetMemorySize()
        {
            return _cache.Count;
        }
    }


    public interface IHasCache
    {
        long CacheSize { get; }

        void ClearCache();
    }

}
