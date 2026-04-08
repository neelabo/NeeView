using Generator.Equals;
using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Text;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class BookConfig : BindableBase
    {
        public static StringCollection DefaultExcludeRegexes { get; } = new StringCollection("^__MACOSX$;^\\.DS_Store$");

        [DefaultEquality] private double _wideRatio = 1.0;
        [DefaultEquality] private StringCollection _excludeRegexes = (StringCollection)DefaultExcludeRegexes.Clone();
        [DefaultEquality] private PageEndAction _pageEndAction;
        [DefaultEquality] private bool _resetNextBookPage = true;
        [DefaultEquality] private bool _isPrioritizeBookMove = false;
        [DefaultEquality] private bool _isPrioritizePageMove = true;
        [DefaultEquality] private bool _isReadyToPageMove;
        [DefaultEquality] private bool _isNotifyPageLoop;
        [DefaultEquality] private bool _isConfirmRecursive;
        [DefaultEquality] private double _contentSpace = -1.0;
        [DefaultEquality] private double _frameSpace = -1.0;
        [DefaultEquality] private string? _terminalSound;
        [DefaultEquality] private bool _isAutoRecursive = false;
        [DefaultEquality] private FolderSortOrder _folderSortOrder = FolderSortOrder.First;
        [DefaultEquality] private bool _resetPageWhenRandomSort;
        [DefaultEquality] private bool _isInsertDummyPage;
        [DefaultEquality] private bool _isInsertDummyFirstPage = false;
        [DefaultEquality] private bool _isInsertDummyLastPage = true;
        [DefaultEquality] private Color _dummyPageColor = Colors.White;
        [DefaultEquality] private bool _isPanorama;
        [DefaultEquality] private PageFrameOrientation _orientation = PageFrameOrientation.Horizontal;
        [DefaultEquality] private double _dividePageRate = 0.5;
        [DefaultEquality] private bool _isStaticWidePage;
        [DefaultEquality] private WidePageStretch _widePageStretch = WidePageStretch.UniformHeight;
        [DefaultEquality] private WidePageVerticalAlignment _widePageVerticalAlignment = WidePageVerticalAlignment.Center;
        [DefaultEquality] private Color _loadingPageColor = Color.FromRgb(0xE0, 0xE0, 0xE0);
        [DefaultEquality] private string _bookThumbnailFileName = "folder.jpg";
        [DefaultEquality] private int _bookThumbnailDepth = 2;
        [DefaultEquality] private ExportImageParameter _exportImageParameter = new();
        [DefaultEquality] private ExportBookParameter _exportBookParameter = new();

        [IgnoreEquality]
        private readonly RegexCollectionCache _excludeRegexCache = new();

        /// <summary>
        /// 横長画像判定用比率
        /// </summary>
        [PropertyMember(HasDecimalPoint = true)]
        public double WideRatio
        {
            get { return _wideRatio; }
            set { SetProperty(ref _wideRatio, AppMath.Round(value)); }
        }

        /// <summary>
        /// ページ除外パターン (正規表現)
        /// </summary>
        [PropertyMember]
        public StringCollection ExcludeRegexes
        {
            get { return _excludeRegexes; }
            set { SetProperty(ref _excludeRegexes, value); }
        }

        /// <summary>
        /// フレームの接続をパノラマにする
        /// </summary>
        [PropertyMember]
        public bool IsPanorama
        {
            get { return _isPanorama; }
            set { SetProperty(ref _isPanorama, value); }
        }

        /// <summary>
        /// フレームの並び方向
        /// </summary>
        [PropertyMember]
        public PageFrameOrientation Orientation
        {
            get { return _orientation; }
            set { SetProperty(ref _orientation, value); }
        }

        // 2ページコンテンツの隙間
        [DefaultValue(-1.0)]
        [PropertyRange(-32, 32, TickFrequency = 1)]
        public double ContentsSpace
        {
            get { return _contentSpace; }
            set { SetProperty(ref _contentSpace, AppMath.Round(value)); }
        }

        // フレームの間隔
        [DefaultValue(-1.0)]
        [PropertyRange(-32, 32, TickFrequency = 1)]
        public double FrameSpace
        {
            get { return _frameSpace; }
            set { SetProperty(ref _frameSpace, AppMath.Round(value)); }
        }

        /// <summary>
        /// ブック移動優先設定
        /// </summary>
        [PropertyMember]
        public bool IsPrioritizeBookMove
        {
            get { return _isPrioritizeBookMove; }
            set { SetProperty(ref _isPrioritizeBookMove, value); }
        }

        /// <summary>
        /// ページ移動優先設定
        /// </summary>
        [PropertyMember]
        public bool IsPrioritizePageMove
        {
            get { return _isPrioritizePageMove; }
            set { SetProperty(ref _isPrioritizePageMove, value); }
        }

        /// <summary>
        /// 表示準備ができてからページを移動する
        /// </summary>
        [PropertyMember]
        public bool IsReadyToPageMove
        {
            get { return _isReadyToPageMove; }
            set { SetProperty(ref _isReadyToPageMove, value); }
        }

        // ページ終端でのアクション
        [PropertyMember]
        public PageEndAction PageEndAction
        {
            get { return _pageEndAction; }
            set { SetProperty(ref _pageEndAction, value); }
        }

        [PropertyMember]
        public bool ResetNextBookPage
        {
            get { return _resetNextBookPage; }
            set { SetProperty(ref _resetNextBookPage, value); }
        }

        [PropertyMember]
        public bool IsNotifyPageLoop
        {
            get { return _isNotifyPageLoop; }
            set { SetProperty(ref _isNotifyPageLoop, value); }
        }

        [PropertyPath(Filter = "Wave|*.wav")]
        public string? TerminalSound
        {
            get { return _terminalSound; }
            set { SetProperty(ref _terminalSound, string.IsNullOrWhiteSpace(value) ? null : value); }
        }

        // 再帰を確認する
        [PropertyMember]
        public bool IsConfirmRecursive
        {
            get { return _isConfirmRecursive; }
            set { SetProperty(ref _isConfirmRecursive, value); }
        }

        // 自動再帰
        [PropertyMember]
        public bool IsAutoRecursive
        {
            get { return _isAutoRecursive; }
            set { SetProperty(ref _isAutoRecursive, value); }
        }

        // フォルダーの並べ替え順序
        [PropertyMember]
        public FolderSortOrder FolderSortOrder
        {
            get { return _folderSortOrder; }
            set { SetProperty(ref _folderSortOrder, value); }
        }

        // ランダムソートでページをリセット
        [PropertyMember]
        public bool ResetPageWhenRandomSort
        {
            get { return _resetPageWhenRandomSort; }
            set { SetProperty(ref _resetPageWhenRandomSort, value); }
        }

        // ダミーページの挿入
        [PropertyMember]
        public bool IsInsertDummyPage
        {
            get { return _isInsertDummyPage; }
            set { SetProperty(ref _isInsertDummyPage, value); }
        }

        [PropertyMember]
        public bool IsInsertDummyFirstPage
        {
            get { return _isInsertDummyFirstPage; }
            set { SetProperty(ref _isInsertDummyFirstPage, value); }
        }

        [PropertyMember]
        public bool IsInsertDummyLastPage
        {
            get { return _isInsertDummyLastPage; }
            set { SetProperty(ref _isInsertDummyLastPage, value); }
        }

        // ダミーページ色
        [PropertyMember]
        public Color DummyPageColor
        {
            get { return _dummyPageColor; }
            set { SetProperty(ref _dummyPageColor, value); }
        }

        [PropertyPercent(0.25, 0.75, TickFrequency = 0.01, IsEditable = true)]
        public double DividePageRate
        {
            get { return _dividePageRate; }
            set { SetProperty(ref _dividePageRate, AppMath.Round(Math.Clamp(MathUtility.SnapValue(value, 0.5, 0.0001), 0.1, 1.0))); }
        }

        // ２ページモードでの静的なインデックス
        [PropertyMember]
        public bool IsStaticWidePage
        {
            get { return _isStaticWidePage; }
            set { SetProperty(ref _isStaticWidePage, value); }
        }

        // 2ページモードでの拡大方法
        [PropertyMember]
        public WidePageStretch WidePageStretch
        {
            get { return _widePageStretch; }
            set { SetProperty(ref _widePageStretch, value); }
        }

        // 2ページモードでの縦方向の配置方法
        [PropertyMember]
        public WidePageVerticalAlignment WidePageVerticalAlignment
        {
            get { return _widePageVerticalAlignment; }
            set { SetProperty(ref _widePageVerticalAlignment, value); }
        }

        // 読込中ページカラー
        [PropertyMember]
        public Color LoadingPageColor
        {
            get { return _loadingPageColor; }
            set { SetProperty(ref _loadingPageColor, value); }
        }

        /// <summary>
        /// ブックサムネイル生成に優先されるファイル名
        /// </summary>
        [PropertyMember]
        public string BookThumbnailFileName
        {
            get { return _bookThumbnailFileName; }
            set { SetProperty(ref _bookThumbnailFileName, value.Trim()); }
        }

        /// <summary>
        /// ブックサムネイル生成時の検索フォルダの最大深度
        /// </summary>
        [PropertyRange(1, 4)]
        public int BookThumbnailDepth
        {
            get { return _bookThumbnailDepth; }
            set { SetProperty(ref _bookThumbnailDepth, Math.Max(value, 1)); }
        }

        #region Hidden Parameters

        /// <summary>
        /// ExportImageAsCommand 用設定
        /// </summary>
        public ExportImageParameter ExportImageParameter
        {
            get { return _exportImageParameter; }
            set { SetProperty(ref _exportImageParameter, value); }
        }

        /// <summary>
        /// ExportBookAsCommand 用設定
        /// </summary>
        public ExportBookParameter ExportBookParameter
        {
            get { return _exportBookParameter; }
            set { SetProperty(ref _exportBookParameter, value); }
        }

        #endregion

        #region Obsolete

        /// <summary>
        /// ページ移動命令重複許可
        /// </summary>
        [Obsolete("no used"), Alternative(null, 40, ScriptErrorLevel.Warning)] // ver.40
        [JsonIgnore]
        public bool IsMultiplePageMove
        {
            get { return true; }
            set { }
        }

        // ブックページ画像サイズ
        [Obsolete("no used"), Alternative(null, 40, ScriptErrorLevel.Warning)] // ver.40
        [JsonIgnore]
        public double BookPageSize
        {
            get { return 300.0; }
            set { }
        }

        // ファイル並び順、ファイル優先
        [Obsolete("no used"), Alternative(nameof(FolderSortOrder), 44, ScriptErrorLevel.Warning)] // ver.44
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsSortFileFirst
        {
            get { return false; }
            set { FolderSortOrder = value ? FolderSortOrder.Last : FolderSortOrder.First; }
        }

        /// <summary>
        /// 除外フォルダー
        /// </summary>
        [Obsolete("no used"), Alternative(nameof(ExcludeRegexes), 45, ScriptErrorLevel.Warning)] // ver.45
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public StringCollection? Excludes
        {
            get { return null; }
            set
            {
                if (value is null) return;
                ExcludeRegexes = new StringCollection(value.Items.Select(e => $"^{Regex.Escape(e)}$"));
            }
        }

        #endregion


        /// <summary>
        /// 除外パターンを正規表現で取得する
        /// </summary>
        /// <returns></returns>
        public List<Regex> GetExcludeRegexes()
        {
            return _excludeRegexCache.GetRegexs(ExcludeRegexes);
        }
    }

}
