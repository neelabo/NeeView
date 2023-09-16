﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    // ファイルページ用コンテンツのアイコン
    public enum FilePageIcon
    {
        File,
        Archive,
        Folder,
        Alert,
    }

    /// <summary>
    /// ファイルページ用コンテンツ
    /// FilePageControl のパラメータとして使用される
    /// </summary>
    public class FilePageContent : PageContent
    {
        private FilePageData _source;

        public FilePageContent(ArchiveEntry archiveEntry, FilePageIcon icon, string? message, BookMemoryService? bookMemoryService) : base(archiveEntry, bookMemoryService)
        {
            _source = new FilePageData(archiveEntry, icon, message);
        }

        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            await Task.CompletedTask;
            return new PageSource(_source, null, new PictureInfo(DefaultSize));
        }
    }

}
