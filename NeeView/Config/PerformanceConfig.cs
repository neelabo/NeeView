using Generator.Equals;
using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class PerformanceConfig : BindableBase
    {
        [DefaultEquality] private int _cacheMemorySize = 200;
        [DefaultEquality] private int _jobWorkerSize = 4;
        [DefaultEquality] private Size _maximumSize = new(4096, 4096);
        [DefaultEquality] private int _preLoadSize = 5;
        [DefaultEquality] private bool _isLimitSourceSize;
        [DefaultEquality] private bool _isLoadingPageVisible = true;
        [DefaultEquality] private int _preExtractSolidSize = 1000;


        /// <summary>
        /// キャッシュメモリサイズ (MB)
        /// </summary>
        [PropertyMember]
        public int CacheMemorySize
        {
            // 64bit,32bit共用のため、設定時、取得時に最大メモリ制限をしている
            get { return Math.Min(_cacheMemorySize, GetMaxCacheMemorySize()); }
            set { SetProperty(ref _cacheMemorySize, Math.Min(value, GetMaxCacheMemorySize())); }
        }

        /// <summary>
        /// 先読みページ数
        /// </summary>
        [PropertyMember]
        public int PreLoadSize
        {
            get { return _preLoadSize; }
            set { SetProperty(ref _preLoadSize, Math.Max(value, 1)); }
        }

        /// <summary>
        /// JobWorker数
        /// </summary>
        [PropertyMember]
        public int JobWorkerSize
        {
            get { return _jobWorkerSize; }
            set { SetProperty(ref _jobWorkerSize, MathUtility.Clamp(value, 1, GetMaxJobWorkerSzie())); }
        }

        // 画像処理の最大サイズ
        // リサイズフィルターで使用される。
        // IsLimitSourceSize フラグがONのときには、読み込みサイズにもこの制限が適用される
        [PropertyMember]
        public Size MaximumSize
        {
            get { return _maximumSize; }
            set { SetProperty(ref _maximumSize, new Size(Math.Max(value.Width, 1024), Math.Max(value.Height, 1024))); }
        }

        // 読み込みデータのサイズ制限適用フラグ
        [PropertyMember]
        public bool IsLimitSourceSize
        {
            get { return _isLimitSourceSize; }
            set { SetProperty(ref _isLimitSourceSize, value); }
        }

        // ページ読み込み中表示
        [PropertyMember]
        public bool IsLoadingPageVisible
        {
            get { return _isLoadingPageVisible; }
            set { SetProperty(ref _isLoadingPageVisible, value); }
        }

        // 事前展開メモリサイズ(MB)
        [PropertyMember]
        public int PreExtractSolidSize
        {
            get { return _preExtractSolidSize; }
            set { SetProperty(ref _preExtractSolidSize, value); }
        }

        #region Obsolete

        [Obsolete("no used"), Alternative(null, 41, ScriptErrorLevel.Warning)] // ver.41
        [JsonIgnore]
        public bool IsPreExtractToMemory
        {
            get { return true; }
            set { }
        }

        #endregion Obsolete

        /// <summary>
        /// 最大キャッシュメモリサイズ計算
        /// </summary>
        public int GetMaxCacheMemorySize()
        {
            int max = (int)(Environment.GetTotalPhysicalMemory() / 1024 / 1024);

            // -2GB or half size
            max = Math.Max(max - 2 * 1024, max / 2);

            // if 32bit, limit 2GB
            if (!Environment.IsX64)
            {
                max = Math.Min(max, 2 * 1024);
            }

            return max;
        }

        public int GetMaxJobWorkerSzie()
        {
            return Math.Max(4, System.Environment.ProcessorCount);
        }
    }

}
