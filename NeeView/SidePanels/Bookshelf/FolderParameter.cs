using NeeLaboratory.ComponentModel;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// フォルダーの並び順とかの保存される情報
    /// </summary>
    public class FolderParameter : BindableBase
    {
        // TODO: Path to QueryPath

        private FolderOrder _folderOrder;
        private bool _isFolderRecursive;
        private int _seed;

        public FolderParameter(string path)
        {
            Path = path;
            Load();
        }


        /// <summary>
        /// 場所
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// ソート順
        /// </summary>
        public FolderOrder FolderOrder
        {
            get { return _folderOrder; }
            set
            {
                if (_folderOrder != value || value == FolderOrder.Random)
                {
                    _folderOrder = value;
                    _seed = GenerateRandomSeed(_folderOrder);
                    Save();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// シャッフル用ランダムシード
        /// </summary>
        public int Seed => _seed;

        /// <summary>
        /// この場所にあるフォルダーはサブフォルダーを読み込む
        /// </summary>
        public bool IsFolderRecursive
        {
            get { return _isFolderRecursive; }
            set
            {
                if (_isFolderRecursive != value)
                {
                    _isFolderRecursive = value;
                    Save();
                    RaisePropertyChanged();
                }

            }
        }

        public void Save()
        {
            FolderConfigCollection.Current.SetFolderParameter(Path, CreateMemento());
        }

        private void Load()
        {
            var memento = FolderConfigCollection.Current.GetFolderParameter(new QueryPath(Path));
            Restore(memento);

            // NOTE: ver44 前はシード値が保存されていないので、Restore()でシード値が補正された場合は保存しなおす。
            if (_seed != memento.Seed)
            {
                Save();
            }
        }

        /// <summary>
        /// ランダムシード値生成
        /// </summary>
        /// <param name="folderOrder">ソートモード</param>
        /// <param name="seed">希望シード値。0で新しく生成</param>
        /// <returns>シード値</returns>
        private int GenerateRandomSeed(FolderOrder folderOrder, int seed = 0)
        {
            if (folderOrder != FolderOrder.Random)
            {
                return 0;
            }
            if (seed == 0)
            {
                return Random.Shared.Next(1, int.MaxValue);
            }
            return seed;
        }

        public static FolderOrder GetDefaultFolderOrder(string path)
        {
            if (QueryScheme.Bookmark.IsMatch(path))
            {
                return Config.Current.Bookmark.BookmarkFolderOrder;
            }
            else if (PlaylistArchive.IsSupportExtension(path))
            {
                return Config.Current.Bookshelf.PlaylistFolderOrder;
            }
            else
            {
                return Config.Current.Bookshelf.DefaultFolderOrder;
            }
        }

        #region Memento
        [Memento]
        public record class Memento
        {
            public Memento()
            {
            }

            public Memento(FolderOrder folderOrder, bool isFolderRecursive, int seed)
            {
                FolderOrder = folderOrder;
                IsFolderRecursive = isFolderRecursive;
                Seed = seed;
            }

            public FolderOrder FolderOrder { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool IsFolderRecursive { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public int Seed { get; set; }


            public bool IsDefault(string path)
            {
                return this.Equals(GetDefault(path));
            }

            public static Memento GetDefault(string path)
            {
                return new Memento()
                {
                    FolderOrder = GetDefaultFolderOrder(path)
                };
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.FolderOrder = this.FolderOrder;
            memento.IsFolderRecursive = this.IsFolderRecursive;
            memento.Seed = this.Seed;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            // プロパティで設定するとSave()されてしまうのを回避
            _folderOrder = memento.FolderOrder;
            _isFolderRecursive = memento.IsFolderRecursive;
            _seed = GenerateRandomSeed(memento.FolderOrder, memento.Seed);
            RaisePropertyChanged(null);
        }

#endregion
    }
}
