﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// アーカイブコンテンツ
    /// 対象のサムネイルを作成
    /// </summary>
    public class ArchiveContent : BitmapContent
    {
        private string _path;

        /// <summary>
        /// コンテンツ有効フラグは常にfalse
        /// </summary>
        public override bool IsLoaded => false;

        /// <summary>
        /// コンスラクタ
        /// </summary>
        /// <param name="entry">対象アーカイブもしくはファイルのエントリ</param>
        public ArchiveContent(ArchiveEntry entry) : base(entry)
        {
            _path = entry?.SystemPath;

            PageMessage = new PageMessage()
            {
                Icon = FilePageIcon.Alart,
                Message = "For thumbnail creation only",
            };

            // エントリが有効でない場合の処理
            if (!entry.IsValid && !entry.IsArchivePath)
            {
                Thumbnail.Initialize(null);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        public ArchiveContent(string path) : base(null)
        {
            _path = path;

            PageMessage = new PageMessage()
            {
                Icon = FilePageIcon.Alart,
                Message = "For thumbnail creation only",
            };
        }


        /// <summary>
        /// Entryの初期化
        /// </summary>
        public override async Task InitializeEntryAsync(CancellationToken token)
        {
            if (Entry == null)
            {
                Entry = await ArchiveEntryUtility.CreateAsync(_path, token);
            }
        }


        /// <summary>
        /// コンテンツロードは非サポート
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override Task LoadAsync(CancellationToken token)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// サムネイル初期化
        /// ページ指定があるため特殊
        /// </summary>
        public override void InitializeThumbnail()
        {
            Thumbnail.Initialize(Entry, null);
        }

        /// <summary>
        /// サムネイルロード
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task LoadThumbnailAsync(CancellationToken token)
        {
            if (Thumbnail.IsValid) return;

            if (!Entry.IsValid && !Entry.IsArchivePath)
            {
                Thumbnail.Initialize(null);
                return;
            }

            try
            {
                var picture = await LoadPictureAsync(token);
                if (picture == null)
                {
                    Thumbnail.Initialize(null);
                }
                else if (picture.Type == ThumbnailType.Unique)
                {
                    Thumbnail.Initialize(picture.Picture?.CreateThumbnail());
                }
                else
                {
                    Thumbnail.Initialize(picture.Type);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                // 例外無効
                Debug.WriteLine($"LoadThumbnail: {e.Message}");
                Thumbnail.Initialize(null);
            }
        }

        /// <summary>
        /// 画像、もしくはサムネイルタイプを指定するもの
        /// </summary>
        public class ThumbnailPicture
        {
            public ThumbnailType Type { get; set; }
            public Picture Picture { get; set; }

            public ThumbnailPicture(ThumbnailType type)
            {
                Type = type;
            }

            public ThumbnailPicture(Picture picture)
            {
                Type = ThumbnailType.Unique;
                Picture = picture;
            }
        }

        /// <summary>
        /// エントリに対応するサムネイル画像生成
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<ThumbnailPicture> LoadPictureAsync(CancellationToken token)
        {
            if (this.Entry.Archiver != null && this.Entry.Archiver is MediaArchiver)
            {
                return new ThumbnailPicture(ThumbnailType.Media);
            }
            if (this.Entry.IsArchivePath)
            {
                var entry = await ArchiveEntryUtility.CreateAsync(this.Entry.SystemPath, token);
                if (entry.IsBook())
                {
                    return await LoadArchivePictureAsync(entry, token);
                }
                else
                {
                    return new ThumbnailPicture(await LoadPictureAsync(entry, PictureCreateOptions.CreateThumbnail | PictureCreateOptions.IgnoreImageCache, token));
                }
            }
            else
            {
                return await LoadArchivePictureAsync(this.Entry, token);
            }
        }


        /// <summary>
        /// アーカイブサムネイル読込
        /// 名前順で先頭のページ
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="entryName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<ThumbnailPicture> LoadArchivePictureAsync(ArchiveEntry entry, CancellationToken token)
        {
            // ブックサムネイル検索範囲
            const int searchRange = 2;

            if (System.IO.Directory.Exists(entry.SystemPath) || entry.IsBook())
            {
                if (ArchiverManager.Current.GetSupportedType(entry.SystemPath) == ArchiverType.MediaArchiver)
                {
                    return new ThumbnailPicture(ThumbnailType.Media);
                }

                var select = await ArchiveEntryUtility.CreateFirstImageArchiveEntryAsync(entry, searchRange, token);
                if (select != null)
                {
                    return new ThumbnailPicture(await LoadPictureAsync(select, PictureCreateOptions.CreateThumbnail | PictureCreateOptions.IgnoreImageCache, token));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new ThumbnailPicture(await LoadPictureAsync(entry, PictureCreateOptions.CreateThumbnail | PictureCreateOptions.IgnoreImageCache, token));
            }
        }
    }

}
