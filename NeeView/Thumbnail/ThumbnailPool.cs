//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// サムネイル寿命管理.
    /// サムネイルを使用するタイミングで随時追加。
    /// 容量を越えたら古いものからクリア処理を行う。
    /// </summary>
    [LocalDebug]
    public partial class ThumbnailPool
    {
        /// <summary>
        /// 管理ユニット
        /// </summary>
        public class ThumbnailUnit
        {
            /// <summary>
            /// サムネイル
            /// </summary>
            private readonly Thumbnail _thumbnail;

            /// <summary>
            /// 寿命シリアル番号
            /// </summary>
            private int _lifeSerial;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="thumbnail"></param>
            public ThumbnailUnit(Thumbnail thumbnail)
            {
                _thumbnail = thumbnail;
                _lifeSerial = thumbnail.LifeSerial;
            }

            /// <summary>
            /// 有効判定.
            /// 寿命シリアル番号が一致すれば有効
            /// </summary>
            public bool IsValid => _lifeSerial == _thumbnail.LifeSerial;

            /// <summary>
            /// ターゲットサムネイル
            /// </summary>
            public Thumbnail Thumbnail => _thumbnail;

            /// <summary>
            /// 最新情報に更新
            /// </summary>
            public void Touch()
            {
                _lifeSerial = _thumbnail.LifeSerial;
            }

            /// <summary>
            /// サムネイルクリア
            /// </summary>
            public void Clear()
            {
                //Debug.WriteLine($"TC: {_lifeSerial}");
                _thumbnail.Clear();
            }
        }


        /// <summary>
        /// サムネイルユニット群
        /// </summary>
        private List<ThumbnailUnit> _collection = new();

        /// <summary>
        /// 寿命シリアル番号生成用
        /// </summary>
        private int _serial;

        /// <summary>
        /// 排他ロックオブジェクト
        /// </summary>
        private readonly System.Threading.Lock _lock = new();


        /// <summary>
        /// サムネイル保証数
        /// </summary>
        public virtual int Limit { get; } = 1000;



        /// <summary>
        /// 廃棄処理 part1 許容値
        /// </summary>
        private int GetTolerance1() => (Limit * 150 / 100); // 150%

        /// <summary>
        /// 廃棄処理 part2 許容値
        /// </summary>
        private int GetTolerance2() => (Limit * 120 / 100); // 120%

        /// <summary>
        /// 管理にサムネイル登録
        /// 「使用する」タイミングで随時追加
        /// </summary>
        /// <param name="thumbnail"></param>
        public void Add(Thumbnail thumbnail)
        {
            lock (_lock)
            {
                _serial = (_serial + 1) & 0x7fffffff;
                thumbnail.LifeSerial = _serial;

                var last = _collection.LastOrDefault();
                if (last?.Thumbnail == thumbnail)
                {
                    last.Touch();
                    LocalDebug.WriteLine($"Touch: {thumbnail}, count={_collection.Count}");
                }
                else
                {
                    _collection.Add(new ThumbnailUnit(thumbnail));
                    LocalDebug.WriteLine($"Add: {thumbnail}, count={_collection.Count}");
                    Cleanup();
                }
            }
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        /// <returns></returns>
        private bool Cleanup()
        {
            // 1st path.
            if (_collection.Count < GetTolerance1()) return false;

            LocalDebug.WriteLine($"Cleanup... {_collection.Count}");

            _collection.RemoveAll(e => !e.IsValid);

            LocalDebug.WriteLine($"Cleanup: Level.1: {_collection.Count}: No.{_serial}");

            // 2nd path.
            if (_collection.Count < GetTolerance2()) return false;

            int erase = _collection.Count - Limit;

            for (int i=0; i<erase; i++)
            {
                _collection[i].Clear();
            }

            _collection.RemoveRange(0, erase);

            LocalDebug.WriteLine($"Cleanup: Level.2: {_collection.Count}");

            return true;
        }

    }
}
