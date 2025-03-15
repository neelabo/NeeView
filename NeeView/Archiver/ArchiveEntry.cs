using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイブエントリ
    /// </summary>
    public partial class ArchiveEntry : IRenameable, IDisposable
    {
        /// <summary>
        /// Emptyインスタンス
        /// </summary>
        public static ArchiveEntry Empty { get; } = new ArchiveEntry(StaticFolderArchive.Default) { IsEmpty = true };

        private string _rawEntryName = "";
        private FileProxy? _fileProxy;
        public object? _data;
        private PreExtractMemory.Key? _preExtractMemoryKey;
        private bool _disposedValue;
        private readonly System.Threading.Lock _lock = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="archiver">所属アーカイバ</param>
        public ArchiveEntry(Archive archiver)
        {
            Archive = archiver;
        }


        [Subscribable]
        public event EventHandler? DataChanged;

        /// <summary>
        /// 属性
        /// </summary>
        public ArchiveEntryAttributes Attributes { get; set; }

        /// <summary>
        ///  Emptyインスタンス？
        /// </summary>
        public bool IsEmpty { get; init; }

        /// <summary>
        /// 仮のエントリー
        /// </summary>
        public bool IsTemporary { get; init; }

        /// <summary>
        /// 所属アーカイバ
        /// </summary>
        public Archive Archive { get; private set; }

        /// <summary>
        /// アーカイブ内登録番号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// エントリ情報
        /// アーカイバーで識別子として使用される
        /// </summary>
        public object? Instance { get; set; }

        /// <summary>
        /// エントリデータ。
        /// 先読みデータ。テンポラリファイル名もしくはバイナリデータ
        /// </summary>
        public object? Data => _data;

        /// <summary>
        /// エントリデータ存在？
        /// </summary>
        public bool HasCache => Data is not null;

        /// <summary>
        /// パスが有効であるか
        /// 無効である場合はアーカイブパスである可能性あり
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// アーカイブパスであるか
        /// </summary>
        public bool IsArchivePath { get; private set; }

        // 例：
        // a.zip 
        // +- b.zip
        //      +- c\001.jpg <- this!

        /// <summary>
        /// エントリ名(重複有)
        /// </summary>
        /// c\001.jpg
        public string RawEntryName
        {
            get { return _rawEntryName; }
            set
            {
                if (_rawEntryName != value)
                {
                    _rawEntryName = value;
                    this.EntryName = NormalizeEntryName(_rawEntryName);
                }
            }
        }

        /// <summary>
        /// エントリ名(重複有、正規化済)
        /// </summary>
        /// c/001.jpg => c\001.jpg
        public string EntryName { get; private set; } = "";

        /// <summary>
        /// エントリ名のファイル名
        /// </summary>
        /// 001.jpg
        public string EntryLastName => LoosePath.GetFileName(EntryName);

        /// <summary>
        /// エントリのフルネーム
        /// </summary>
        public virtual string EntryFullName => LoosePath.Combine(Archive.SystemPath, EntryName);

        /// <summary>
        /// ルートアーカイバー
        /// </summary>
        /// a.zip
        // TODO: ArchiveEntry.RootArchive is not null
        public Archive RootArchive => Archive.RootArchive;

        /// <summary>
        /// 所属名
        /// </summary>
        public string RootArchiveName => RootArchive.EntryName;

        /// <summary>
        /// 対象ファイルのパス<br/>
        /// ファイルの場所を開くときに使用する
        /// </summary>
        public virtual string PlacePath => Archive.GetPlace();

        /// <summary>
        /// システムパス<br/>
        /// プレイリストではターゲットパスになる
        /// </summary>
        public virtual string SystemPath => EntryFullName;

        /// <summary>
        /// 実体パス
        /// </summary>
        /// <remarks>
        /// 存在するアクセス可能パス。アーカイブパス等では null
        /// </remarks>
        public virtual string? EntityPath => null;

        /// <summary>
        /// リンクを解決したシステムパス
        /// </summary>
        public string TargetPath => EntityPath ?? SystemPath;

        /// <summary>
        /// 識別名
        /// アーカイブ内では重複名があるので登録番号を含めたユニークな名前にする
        /// </summary>
        public virtual string Ident => LoosePath.Combine(Archive.Ident, $"{Id}.{EntryName}");

        /// <summary>
        /// ファイルサイズ。
        /// -1 はディレクトリ
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// ディレクトリ？
        /// </summary>
        public bool IsDirectory => Length == -1;

        /// <summary>
        /// ファイル作成日
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// ファイル更新日
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// ファイルシステム所属判定
        /// </summary>
        /// <remarks>
        /// File.Move() できるかどうかの基準
        /// </remarks>
        public virtual bool IsFileSystem => false;

        /// <summary>
        /// 削除済フラグ
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// ショートカット判定
        /// </summary>
        /// <remarks>
        /// プレイリスト自体はショートカット扱いとするが、プレイリスト項目はショートカットとみなさない。
        /// </remarks>
        public virtual bool IsShortcut => PlaylistArchive.IsSupportExtension(TargetPath);

        /// <summary>
        /// 実エントリ
        /// </summary>
        /// <remarks>
        /// 内部エントリがあればそれを優先して返す
        /// </remarks>
        public virtual ArchiveEntry TargetArchiveEntry => this;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _preExtractMemoryKey?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// エントリ名の正規化
        /// </summary>
        public static string NormalizeEntryName(string rawEntryName)
        {
            return LoosePath.TrimEnd(LoosePath.NormalizeSeparator(rawEntryName));
        }

        /// <summary>
        /// エントリデータ設定
        /// </summary>
        /// <param name="value"></param>
        public void SetData(object value)
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                if (_data == value) return;
                ResetData();
                if (value is null) return;
                _data = value;
                if (value is byte[] rawData)
                {
                    _preExtractMemoryKey = PreExtractMemory.Current.Open(rawData.Length);
                }
            }
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// エントリーデータリセット
        /// </summary>
        public void ResetData()
        {
            lock (_lock)
            {
                if (_data is null) return;
                _preExtractMemoryKey?.Dispose();
                _preExtractMemoryKey = null;
                _data = null;
            }
        }

        /// <summary>
        /// ストリームを開く
        /// </summary>
        /// <returns>Stream</returns>
        public async Task<Stream> OpenEntryAsync(CancellationToken token)
        {
            return await Archive.OpenStreamAsync(this, token);
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="exportFileName">出力ファイル名</param>
        /// <param name="isOverwrite">上書き許可フラグ</param>
        public async Task ExtractToFileAsync(string exportFileName, bool isOverwrite, CancellationToken token)
        {
            if (exportFileName is null) throw new ArgumentNullException(nameof(exportFileName));

            await Archive.ExtractToFileAsync(this, exportFileName, isOverwrite, token);
        }


        /// <summary>
        /// テンポラリにアーカイブエントリを解凍する
        /// このテンポラリは自動的に削除される
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="isKeepFileName">エントリー名をファイル名にする</param>
        public async Task<FileProxy> GetFileProxyAsync(bool isKeepFileName, CancellationToken token)
        {
            _fileProxy = _fileProxy ?? await CreateFileProxyAsync(new TempArchiveEntryNamePolicy(isKeepFileName, "entry"), false, token);
            return _fileProxy;
        }

        /// <summary>
        /// テンポラリアーカイブエントリを得る。
        /// 場合によっては存在するファイルをそのまま返す。
        /// </summary>
        /// <param name="fileNamePolicy">ファイル名ポリシ―</param>
        /// <param name="isOverwrite">上書き許可</param>
        /// <returns>ファイル</returns>
        public async Task<FileProxy> CreateFileProxyAsync(TempArchiveEntryNamePolicy fileNamePolicy, bool isOverwrite, CancellationToken token)
        {
            var entityPath = EntityPath;
            if (entityPath is not null)
            {
                return new FileProxy(entityPath);
            }

            await WaitPreExtractAsync(token);

            var tempFileName = fileNamePolicy.Create(this);

            if (this.Data is string fileName)
            {
                if (fileNamePolicy.IsKeepFileName && fileName != tempFileName)
                {
                    await FileIO.CopyFileAsync(fileName, tempFileName, isOverwrite, false, token);
                    return new TempFile(fileName);
                }
                else
                {
                    return new FileProxy(fileName);
                }
            }
            else if (this.Data is byte[] rawData)
            {
                FileIO.CheckOverwrite(tempFileName, isOverwrite);
                await File.WriteAllBytesAsync(tempFileName, rawData, token);
                return new TempFile(tempFileName);
            }
            else
            {
                await ExtractToFileAsync(tempFileName, isOverwrite, token);
                return new TempFile(tempFileName);
            }
        }

        /// <summary>
        /// エントリ単位で事前展開完了を待機する
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitPreExtractAsync(CancellationToken token)
        {
            await Archive.WaitPreExtractAsync(this, token);
        }


        /// <summary>
        /// アーカイブ判定
        /// </summary>
        /// <param name="isAllowFileSystem"></param>
        /// <param name="isAllowMedia"></param>
        /// <returns></returns>
        public bool IsArchiveSupported(bool isAllowFileSystem = true, bool isAllowMedia = true)
        {
            return ArchiveManager.Current.IsSupported(TargetPath, isAllowFileSystem, isAllowMedia);
        }

        /// <summary>
        /// アーカイブタイプ取得
        /// </summary>
        /// <returns></returns>
        public ArchiveType GetArchiveSupportedType()
        {
            return ArchiveManager.Current.GetSupportedType(TargetPath);
        }

        /// <summary>
        /// このエントリがブックであるかを判定。
        /// アーカイブのほかメディアを含める
        /// </summary>
        public bool IsBook()
        {
            if (this.IsDirectory)
            {
                return true;
            }

            return IsArchiveSupported(false, true);
        }

        /// <summary>
        /// このエントリがアーカイブであるかを判定。
        /// メディアは除外する
        /// </summary>
        public virtual bool IsArchive()
        {
            if (this.IsDirectory)
            {
                return this.IsFileSystem; // アーカイブディレクトリは除外
            }

            return IsArchiveSupported(false, false);
        }

        /// <summary>
        /// アーカイブ中のディレクトリ？
        /// </summary>
        public bool IsArchiveDirectory()
        {
            return !IsFileSystem && IsDirectory;
        }

        /// <summary>
        /// メディア？
        /// </summary>
        public bool IsMedia()
        {
            return !this.IsDirectory && GetArchiveSupportedType() == ArchiveType.MediaArchive;
        }

        /// <summary>
        /// このエントリが画像であるか拡張子から判定。
        /// MediaArchive は無条件で画像と認識
        /// </summary>
        public bool IsImage(bool includeMedia = true)
        {
            return !this.IsDirectory && ((this.Archive is MediaArchive) || PictureProfile.Current.IsSupported(TargetPath, includeMedia));
        }

        /// <summary>
        /// exists?
        /// </summary>
        public bool Exists()
        {
            if (IsDeleted) return false;
            return Archive.Exists(this);
        }

        /// <summary>
        /// can delete?
        /// </summary>
        public bool CanDelete(bool strict)
        {
            return Archive.CanDelete(this, strict);
        }

        /// <summary>
        /// delete
        /// </summary>
        public async Task<DeleteResult> DeleteAsync()
        {
            return await Archive.DeleteAsync(this);
        }

        /// <summary>
        /// 複数エントリをまとめて削除
        /// </summary>
        public static async Task<bool> DeleteEntriesAsync(IEnumerable<ArchiveEntry> entries)
        {
            if (!entries.Any()) return false;

            foreach (var group in entries.GroupBy(e => e.Archive))
            {
                var archiver = group.Key;
                var result = await archiver.DeleteAsync(group.ToList());
                if (result < 0) return false;
            }

            return true;
        }

        /// <summary>
        /// to string
        /// </summary>
        public override string? ToString()
        {
            return string.IsNullOrEmpty(EntryName) ? base.ToString() : $"{Id}:{EntryName}";
        }


        public string GetRenameText()
        {
            return EntryLastName;
        }

        public bool CanRename()
        {
            return Archive.CanRename(this);
        }

        public async Task<bool> RenameAsync(string name)
        {
            return await Archive.RenameAsync(this, name);
        }

        /// <summary>
        /// エントリの実体化は可能か？
        /// </summary>
        public bool CanRealize()
        {
            return Archive.CanRealize(this);
        }

        /// <summary>
        /// エントリを実体ファイルにする
        /// </summary>
        /// <param name="token"></param>
        /// <returns>実体ファイルのパス</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public async Task<string?> RealizeAsync(CancellationToken token)
        {
            return await RealizeAsync(ArchivePolicy.SendExtractFile, token);
        }

        /// <summary>
        /// エントリを実体ファイルにする
        /// </summary>
        /// <param name="archivePolicy">アーカイブファイルのときの方針</param>
        /// <param name="token"></param>
        /// <returns>実体ファイルのパス。取得できなかったときは null</returns>
        /// <exception cref="NotSupportedException">サポートされていない ArchivePolicy</exception>
        public virtual async Task<string?> RealizeAsync(ArchivePolicy archivePolicy, CancellationToken token)
        {
            // file
            if (IsFileSystem)
            {
                // NOTE: ショートカットもそのまま渡すよ
                return SystemPath;
            }

            // in archive
            switch (archivePolicy)
            {
                case ArchivePolicy.None:
                    return null;

                case ArchivePolicy.SendArchiveFile:
                    return PlacePath;

                case ArchivePolicy.SendExtractFile:
                    if (CanRealize())
                    {
                        var proxy = await GetFileProxyAsync(true, token);
                        return proxy.Path;
                    }
                    else
                    {
                        // TODO: ArchiveDirectory 対応
                        return null;
                    }

                case ArchivePolicy.SendArchivePath:
                    return SystemPath;

                default:
                    throw new NotSupportedException($"Unsupported archive policy: {archivePolicy}");
            }
        }

        /// <summary>
        /// シンボリックリンクを解決したファイル情報を返す
        /// </summary>
        /// <remarks>
        /// ファイルシステム限定
        /// </remarks>
        /// <returns></returns>
        public virtual FileSystemInfo? ResolveLinkTarget()
        {
            return null;
        }

    }


    public enum ArchiveEntryAttributes
    {
        None = 0,
        Duplicate = (1 << 0),
    }
}

