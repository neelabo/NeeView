using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public class BitmapMetadataDatabase : IReadOnlyDictionary<BitmapMetadataKey, object?>
    {
        private static readonly Dictionary<BitmapMetadataKey, object?> _emptyMap = Enum.GetValues(typeof(BitmapMetadataKey)).Cast<BitmapMetadataKey>().ToDictionary(e => e, e => (object?)null);
        public static BitmapMetadataDatabase Default { get; } = new BitmapMetadataDatabase();


        private readonly Dictionary<BitmapMetadataKey, object?> _map;
        private readonly Dictionary<string, object?> _extraMap;
        private Dictionary<string, object?>? _lowerExtraMap;

        public BitmapMetadataDatabase()
        {
            _map = _emptyMap;
            _extraMap = new();
            this.Format = "(Undefined)";
        }

        public BitmapMetadataDatabase(BitmapMetadata? meta)
        {
            var accessor = (meta != null) ? BitmapMetadataAccessorFactory.Create(meta) : null;
            if (accessor is null)
            {
                _map = _emptyMap;
                _extraMap = new();

                this.Format = "(Undefined)";
            }
            else
            {
                _map = CreateMap(accessor);
                _extraMap = CreateExtraMap(accessor);
                this.Format = accessor.GetFormat();
            }
            this.IsOrientationEnabled = true;
        }

        public BitmapMetadataDatabase(Stream stream)
        {
            var accessor = new MetadataExtractorAccessor(stream);
            _map = CreateMap(accessor);
            _extraMap = CreateExtraMap(accessor);
            this.Format = accessor.GetFormat();

            this.IsOrientationEnabled = IsOriantationEnabled(this.Format);
        }

        // ピクセルデータの回転補正を行うかどうか
        // コーデックによって Orientation 適用済みだったりする
        // TODO: 確認が取れ次第対応する
        private bool IsOriantationEnabled(string? format)
        {
            switch (format?.ToLowerInvariant())
            {
                case "avif":
                case "heif":
                   return false;
                default:
                    return true;
            }
        }


        public bool IsValid => _map != _emptyMap;

        public string Format { get; private set; }

        public bool IsOrientationEnabled { get; private set; }


        private static Dictionary<BitmapMetadataKey, object?> CreateMap(BitmapMetadataAccessor accessor)
        {
            var map = new Dictionary<BitmapMetadataKey, object?>();

            foreach (BitmapMetadataKey key in Enum.GetValues(typeof(BitmapMetadataKey)))
            {
                try
                {
                    map[key] = accessor.GetValue(key);
                }
#if DEBUG
                catch (Exception ex)
                {
                    map[key] = $"⚠ {ex.Message}";
                }
#else
                catch
                {
                    map[key] = null;
                }
#endif
            }

            return map;
        }

        private static Dictionary<string, object?> CreateExtraMap(BitmapMetadataAccessor accessor)
        {
            return accessor.GetExtraValues();
        }


        public object? ElementAt(BitmapMetadataKey key) => _map[key];

        /// <summary>
        /// 拡張メタマップ
        /// </summary>
        public Dictionary<string, object?> ExtraMap => _extraMap;

        /// <summary>
        /// キーを小文字にした拡張メタマップ
        /// </summary>
        public Dictionary<string, object?> LowerExtraMap
        {
            get
            {
                if (_lowerExtraMap is null)
                {
                    _lowerExtraMap = _extraMap.ToDictionary(e => e.Key.Replace(" ", "", StringComparison.Ordinal).ToLowerInvariant(), e => e.Value);
                }
                return _lowerExtraMap;
            }
        }

        /// <summary>
        /// プロパティ名から値を取得する
        /// </summary>
        /// <remarks>
        /// 標準メタと拡張メタからアクセス名を指定して値を取得する。
        /// </remarks>
        /// <param name="name">アクセス名。小文字空白なしのプロパティ名</param>
        /// <param name="value">プロパティ値</param>
        /// <returns></returns>
        public bool TryGetValue(string name, out object? value)
        {
            if (BitmapMetadataKeyExtensions.TryParse(name, out BitmapMetadataKey key))
            {
                value = _map[key];
                return true;
            }

            return LowerExtraMap.TryGetValue(name, out value);
        }


        // NOTE: この辞書インターフェイスは ExtraMap に非対応
        #region IReadOnlyDictionary

        public object? this[BitmapMetadataKey key] => ((IReadOnlyDictionary<BitmapMetadataKey, object?>)_map)[key];

        public IEnumerable<BitmapMetadataKey> Keys => ((IReadOnlyDictionary<BitmapMetadataKey, object?>)_map).Keys;

        public IEnumerable<object?> Values => ((IReadOnlyDictionary<BitmapMetadataKey, object?>)_map).Values;

        public int Count => ((IReadOnlyCollection<KeyValuePair<BitmapMetadataKey, object?>>)_map).Count;

        public bool ContainsKey(BitmapMetadataKey key)
        {
            return ((IReadOnlyDictionary<BitmapMetadataKey, object?>)_map).ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<BitmapMetadataKey, object?>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<BitmapMetadataKey, object?>>)_map).GetEnumerator();
        }

        public bool TryGetValue(BitmapMetadataKey key, out object? value)
        {
            return ((IReadOnlyDictionary<BitmapMetadataKey, object?>)_map).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_map).GetEnumerator();
        }

        #endregion IReadOnlyDictionary
    }

}
