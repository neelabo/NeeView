﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    [Serializable]
    public class BookAddressException : Exception
    {
        public BookAddressException(string message) : base(message)
        {
        }

        public BookAddressException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// アーカイブパスに対応したブックアドレス
    /// </summary>
    public class BookAddress
    {
        #region Fields

        private ArchiveEntry _archiveEntry;

        #endregion

        #region Properties

        /// <summary>
        /// 開始ページ名
        /// </summary>
        public string EntryName { get; set; }

        /// <summary>
        /// ブックの場所
        /// </summary>
        public string Place { get; set; }

        /// <summary>
        /// ページを含めたアーカイブパス
        /// </summary>
        public string SystemPath => LoosePath.Combine(Place, EntryName);

        #endregion

        #region Methods

        /// <summary>
        /// BookAddress生成
        /// </summary>
        ///  <param name="path">入力パス</param>
        /// <param name="entryName">開始ページ名</param>
        /// <param name="option"></param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>生成したインスタンス</returns>
        public static async Task<BookAddress> CreateAsync(string path, string entryName, BookLoadOption option, CancellationToken token)
        {
            var address = new BookAddress();
            await address.ConstructAsync(path, entryName, option, token);
            return address;
        }

        /// <summary>
        /// 初期化。
        /// アーカイブ展開等を含むため、非同期処理。
        /// </summary>
        private async Task ConstructAsync(string path, string entryName, BookLoadOption option, CancellationToken token)
        {
            var query = new QueryPath(path);

            if (query.Scheme == QueryScheme.Pagemark)
            {
                this.Place = QueryScheme.Pagemark.ToSchemeString();
                this.EntryName = entryName;
                return;
            }

            if (query.Scheme == QueryScheme.Bookmark)
            {
                var node = BookmarkCollection.Current.FindNode(path);
                switch (node.Value)
                {
                    case Bookmark bookmark:
                        path = bookmark.Place;
                        break;
                    case BookmarkFolder folder:
                        throw new BookAddressException(string.Format(Properties.Resources.NotifyCannotOpenBookmarkFolder, path));
                }
            }

            _archiveEntry = await ArchiveEntryUtility.CreateAsync(path, token);

            if (entryName != null)
            {
                Debug.Assert(!option.HasFlag(BookLoadOption.IsBook));
                this.Place = path;
                this.EntryName = entryName;
            }
            else if (Directory.Exists(path) || ArchiverManager.Current.IsSupported(_archiveEntry.SystemPath))
            {
                Debug.Assert(!option.HasFlag(BookLoadOption.IsPage));
                this.Place = path;
                this.EntryName = null;
            }
            else if (_archiveEntry.Archiver != null) // TODO: この判定いるのか？
            {
                if (_archiveEntry.IsDirectory || _archiveEntry.IsArchive())
                {
                    this.Place = path;
                    this.EntryName = null;
                }
                else
                {
                    this.Place = LoosePath.GetDirectoryName(path);
                    this.EntryName = LoosePath.GetFileName(path);
                }
            }
            else
            {
                if (option.HasFlag(BookLoadOption.IsBook))
                {
                    this.Place = path;
                    this.EntryName = null;
                }
                else
                {
                    this.Place = LoosePath.GetDirectoryName(path);
                    this.EntryName = LoosePath.GetFileName(path);
                }
            }
        }

        #endregion
    }

}
