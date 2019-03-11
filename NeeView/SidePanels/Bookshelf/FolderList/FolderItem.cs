﻿using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO;
using NeeView.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    [Flags]
    public enum FolderItemAttribute
    {
        None = 0,
        Directory = (1 << 0),
        Drive = (1 << 1),
        Empty = (1 << 2),
        Shortcut = (1 << 3),
        ArchiveEntry = (1 << 4),
        Bookmark = (1 << 5),
        QuickAccess = (1 << 6),
        System = (1 << 7),
        ReadOnly = (1 << 8),
    }

    /// <summary>
    /// FolderItemAttribute メソッド拡張
    /// </summary>
    public static class FolderItemAttributeExtensions
    {
        /// <summary>
        /// いずれかのフラグのONをチェック
        /// </summary>
        public static bool AnyFlag(this FolderItemAttribute self, FolderItemAttribute value)
        {
            return (self & value) != 0;
        }
    }



    /// <summary>
    /// アイコンオーバーレイ
    /// </summary>
    public enum FolderItemIconOverlay
    {
        Uninitialized,
        None,
        Checked,
        Star,
        Disable,
    }

    /// <summary>
    /// FolderItemの種類。ソート用
    /// </summary>
    public enum FolderItemType
    {
        Empty,
        Directory,
        DirectoryShortcut,
        File,
        FileShortcut,
        ArchiveEntry,
    }

    /// <summary>
    /// フォルダー情報
    /// フォルダーリストの１項目の情報 
    /// </summary>
    public abstract class FolderItem : BindableBase, IHasPage, IHasName
    {
        private bool _isOverlayEnabled;

        public FolderItem(bool isOverlayEnabled)
        {
            _isOverlayEnabled = isOverlayEnabled;
        }

        #region Properties

        /// <summary>
        /// このFolderItemと関係のある情報
        /// </summary>
        public object Source { get; set; }

        // 属性
        public FolderItemAttribute Attributes { get; set; }

        public bool IsDirectory => (Attributes & FolderItemAttribute.Directory) == FolderItemAttribute.Directory;
        public bool IsShortcut => (Attributes & FolderItemAttribute.Shortcut) == FolderItemAttribute.Shortcut;

        // 種類。ソート用
        public FolderItemType Type { get; set; }

        // このアイテムが存在しているディレクトリ
        private QueryPath _place;
        public QueryPath Place
        {
            get { return _place; }
            set
            {
                if (SetProperty(ref _place, value))
                {
                    RaisePropertyChanged(nameof(Path));
                }
            }
        }

        // アイテム名
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    RaisePropertyChanged(nameof(DispName));
                    RaisePropertyChanged(nameof(Path));
                    RaisePropertyChanged(nameof(Detail));
                }
            }
        }

        // 表示名
        private string _dispName;
        public string DispName
        {
            get { return _dispName ?? (IsShortcut ? System.IO.Path.GetFileNameWithoutExtension(_name) : _name); }
            set { SetProperty(ref _dispName, value); }
        }

        // パス
        public QueryPath Path => _place.ReplacePath(LoosePath.Combine(_place.Path, _name));

        // 実体へのパス。nullの場合は Path と同じ
        private QueryPath _targetPath;
        public QueryPath TargetPath
        {
            get { return _targetPath ?? Path; }
            set { if (_targetPath != value) { _targetPath = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 実体パスはディレクトリ？
        /// </summary>
        public bool IsDirectoryTarget { get; set; }

        // アーカイブエントリ
        private ArchiveEntry _archiveEntry;
        public ArchiveEntry ArchiveEntry
        {
            get { return _archiveEntry; }
            set { if (_archiveEntry != value) { _archiveEntry = value; RaisePropertyChanged(); } }
        }

        // 最終更新日
        public DateTime LastWriteTime { get; set; }

        // 登録日時
        public DateTime EntryTime { get; set; }

        // ファイルサイズ
        public long Length { get; set; }

        /// <summary>
        /// 編集可能
        /// </summary>
        public bool IsEditable => (this.Attributes & (FolderItemAttribute.Empty | FolderItemAttribute.Drive | FolderItemAttribute.ArchiveEntry | FolderItemAttribute.ReadOnly | FolderItemAttribute.System)) == 0;

        /// <summary>
        /// アクセス可能？(ドライブの準備ができているか)
        /// </summary>
        private bool _isReady;
        public bool IsReady
        {
            get { return _isReady; }
            set
            {
                if (SetProperty(ref _isReady, value))
                {
                    UpdateOverlay();
                    RaisePropertyChanged(nameof(IconOverlay));
                }
            }
        }


        /// <summary>
        /// フォルダーリストのコンテキストメニュー用
        /// </summary>
        private bool _isRecursived;
        public bool IsRecursived
        {
            get { return _isRecursived; }
            set { if (_isRecursived != value) { _isRecursived = value; RaisePropertyChanged(); } }
        }


        // アイコンオーバーレイの種類を返す
        private FolderItemIconOverlay _iconOverlay = FolderItemIconOverlay.Uninitialized;
        public FolderItemIconOverlay IconOverlay
        {
            get
            {
                if (_iconOverlay == FolderItemIconOverlay.Uninitialized)
                {
                    UpdateOverlay();
                }
                return _iconOverlay;
            }
        }

        public virtual string Detail => Name;

        public abstract IThumbnail Thumbnail { get; }

        #endregion Properties

        #region Methods

        public virtual Page GetPage() => null;

        public bool IsDrive() => (Attributes & FolderItemAttribute.Drive) == FolderItemAttribute.Drive;
        public bool IsEmpty() => (Attributes & FolderItemAttribute.Empty) == FolderItemAttribute.Empty;
        public bool IsDisable() => IsDirectory && !IsReady;
        public bool IsBookmark() => (Attributes & FolderItemAttribute.Bookmark) == FolderItemAttribute.Bookmark;
        public bool IsFileSystem() => (Attributes & (FolderItemAttribute.System | FolderItemAttribute.Bookmark | FolderItemAttribute.QuickAccess | FolderItemAttribute.Empty | FolderItemAttribute.None)) == 0;

        /// <summary>
        /// IsRecursived 更新
        /// </summary>
        public void UpdateIsRecursived(bool isDefaultRecursive)
        {
            var option = isDefaultRecursive ? BookLoadOption.DefaultRecursive : BookLoadOption.None;
            var memento = BookHub.Current.GetLastestBookMemento(this.TargetPath.SimplePath, option);
            this.IsRecursived = memento.IsRecursiveFolder;
        }

        // エクスプローラーへのドラッグオブジェクト
        public DataObject GetFileDragData()
        {
            return new DataObject(DataFormats.FileDrop, new string[] { this.Path.SimplePath });
        }

        // パスの存在チェック
        public bool IsExist()
        {
            return IsDirectory ? Directory.Exists(Path.SimplePath) : File.Exists(Path.SimplePath);
        }

        private void UpdateOverlay()
        {
            if (_isOverlayEnabled)
            {
                if (IsDisable())
                    _iconOverlay = FolderItemIconOverlay.Disable;
                else if (BookshelfFolderList.Current.IsVisibleBookmarkMark && BookmarkCollection.Current.Contains(TargetPath.SimplePath))
                    _iconOverlay = FolderItemIconOverlay.Star;
                else if (BookshelfFolderList.Current.IsVisibleHistoryMark && BookHistoryCollection.Current.Contains(TargetPath.SimplePath))
                    _iconOverlay = FolderItemIconOverlay.Checked;
                else
                    _iconOverlay = FolderItemIconOverlay.None;
            }
            else
            {
                _iconOverlay = FolderItemIconOverlay.None;
            }
        }

        // アイコンオーバーレイの変更を通知
        public void NotifyIconOverlayChanged()
        {
            UpdateOverlay();
            RaisePropertyChanged(nameof(IconOverlay));
        }

        /// <summary>
        /// フォルダーとして展開可能？
        /// </summary>
        public bool CanOpenFolder()
        {
            if (IsDirectory)
            {
                return true;
            }

            var archiveType = ArchiverManager.Current.GetSupportedType(TargetPath.SimplePath, false);
            if (IsFileSystem() && BookHub.Current.ArchiveRecursiveMode != ArchiveEntryCollectionMode.IncludeSubArchives && archiveType.IsRecursiveSupported())
            {
                return true;
            }

            return false;
        }

        //
        public virtual string GetNote(FolderOrder order)
        {
            return null;
        }

        //
        public override string ToString()
        {
            return $"FolderItem: {Path}";
        }

        #endregion
    }

    /// <summary>
    /// 標準 FolderItem
    /// </summary>
    public class FileFolderItem : FolderItem
    {
        private Page _archivePage;


        public FileFolderItem(bool isOverlayEnabled) : base(isOverlayEnabled)
        {
        }


        /// <summary>
        /// サムネイルロード完了時のイベント (開発用)
        /// </summary>
        public event EventHandler ThumbnailLoaded;


        /// <summary>
        /// サムネイル.
        /// アクセスすることで自動でサムネイル読み込み処理が開始される
        /// </summary>
        public override IThumbnail Thumbnail => GetArchivePage()?.Thumbnail;


        public override Page GetPage()
        {
            return GetArchivePage();
        }

        private Page GetArchivePage()
        {
            if (_archivePage == null)
            {
                _archivePage = new ArchivePage(TargetPath.SimplePath);
                _archivePage.Thumbnail.IsCacheEnabled = true;
                _archivePage.Thumbnail.Touched += Thumbnail_Touched;
            }
            return _archivePage;
        }

        private void Thumbnail_Touched(object sender, EventArgs e)
        {
            var thumbnail = (Thumbnail)sender;
            BookThumbnailPool.Current.Add(thumbnail);
            ThumbnailLoaded?.Invoke(sender, e);
        }

        public override string GetNote(FolderOrder order)
        {
            if (!IsFileSystem() && IsDirectory) return null;

            string GetLastWriteTimeString() => $"{LastWriteTime:yyyy/MM/dd HH:mm:ss}   ";

            switch (order)
            {
                default:
                    return GetLastWriteTimeString() + FileSizeToStringConverter.ByteToDispString(Length);

                case FolderOrder.FileType:
                case FolderOrder.FileTypeDescending:
                    return GetLastWriteTimeString() + (IsDirectoryTarget ? Properties.Resources.WordFolder : LoosePath.GetExtension(Name));

                case FolderOrder.Path:
                case FolderOrder.PathDescending:
                    return LoosePath.GetFileName(LoosePath.GetDirectoryName(TargetPath.SimplePath));
            }
        }
    }

    /// <summary>
    /// Drive FolderItem
    /// </summary>
    public class DriveFolderItem : FolderItem
    {
        private IThumbnail _thumbnail;

        public DriveFolderItem(DriveInfo driveInfo, bool isOverlayEnabled) : base(isOverlayEnabled)
        {
            _thumbnail = new DriveThumbnail(driveInfo.Name);
            ////_thumbnail = new ResourceThumbnail("ic_drive", MainWindow.Current);
        }

        public override IThumbnail Thumbnail => _thumbnail;
    }

    /// <summary>
    /// 固定表示用FolderItem.
    /// </summary>
    public class ConstFolderItem : FolderItem
    {
        private IThumbnail _thumbnail;

        public ConstFolderItem(IThumbnail thumbnail, bool isOverlayEnabled) : base(isOverlayEnabled)
        {
            _thumbnail = thumbnail;
        }

        public override IThumbnail Thumbnail => _thumbnail;
    }
}
