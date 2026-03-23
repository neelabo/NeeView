using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class BookMemento : IBookSetting
    {
        private const string IsSupportedDividePageLabel = "IsDivide";
        private const string IsSupportedSingleFirstPageLabel = "IsSingleFirst";
        private const string IsSupportedSingleLastPageLabel = "IsSingleLast";
        private const string IsSupportedWidePageLabel = "IsWide";
        private const string IsRecursiveFolderLabel = "IsRecursive";
        private const string SortModeLabel = "Sort";
        private const string AutoRotateLabel = "Rot";
        private const string BaseScaleLabel = "Base";
        private const string SortSeedLabel = "Seed";

        // フォルダーの場所
        public string Path { get; set; } = "";

        // 名前
        public string Name => BookTools.PathToBookName(Path);

        // 現在ページ
        public string Page { get; set; } = "";

        // 1ページ表示 or 2ページ表示
        public PageMode PageMode { get; set; }

        // 右開き or 左開き
        public PageReadOrder BookReadOrder { get; set; }

        // 横長ページ分割 (1ページモード)
        public bool IsSupportedDividePage { get; set; }

        // 最初のページを単独表示 
        public bool IsSupportedSingleFirstPage { get; set; }

        // 最後のページを単独表示
        public bool IsSupportedSingleLastPage { get; set; }

        // 横長ページを2ページ分とみなす(2ページモード)
        public bool IsSupportedWidePage { get; set; } = true;

        // フォルダーの再帰
        public bool IsRecursiveFolder { get; set; }

        // ページ並び順
        public PageSortMode SortMode { get; set; }

        // ページ並び順用シード。PageSortMode.Random のときに使用する
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int SortSeed { get; set; }

        // 自動回転
        public AutoRotateType AutoRotate { get; set; }

        // 基底スケール
        public double BaseScale { get; set { field = AppMath.Round(value); } } = 1.0;

        /// <summary>
        /// 複製
        /// </summary>
        public BookMemento Clone()
        {
            return (BookMemento)this.MemberwiseClone();
        }

        // 保存用バリデート
        // この memento は履歴とデフォルト設定の２つに使われるが、デフォルト設定には本の場所やページ等は不要
        public void ValidateForDefault()
        {
            Path = "";
            Page = "";
        }

        // バリデートされたクローン
        public BookMemento ValidatedClone()
        {
            var clone = this.Clone();
            clone.ValidateForDefault();
            return clone;
        }

        // 値の等価判定
        public bool IsEquals(BookMemento? other)
        {
            return other is not null &&
                   ((IBookSetting)this).IsEquals(other) &&
                   Path == other.Path;
        }

        // 設定のみの比較
        public bool IsSettingEquals(BookMemento? other)
        {
            return ((IBookSetting)this).IsSettingEquals(other);
        }



        public string ToPropertiesString()
        {
            var list = new List<string>(16);

            if (PageMode != default)
            {
                list.Add(PageMode.ToString());
            }

            if (BookReadOrder != default)
            {
                list.Add(BookReadOrder.ToString());
            }

            if (IsSupportedDividePage)
            {
                list.Add(IsSupportedDividePageLabel);
            }

            if (IsSupportedSingleFirstPage)
            {
                list.Add(IsSupportedSingleFirstPageLabel);
            }

            if (IsSupportedSingleLastPage)
            {
                list.Add(IsSupportedSingleLastPageLabel);
            }

            if (IsSupportedWidePage)
            {
                list.Add(IsSupportedWidePageLabel);
            }

            if (IsRecursiveFolder)
            {
                list.Add(IsRecursiveFolderLabel);
            }

            if (SortMode != default)
            {
                list.Add($"{SortModeLabel}={SortMode}");
            }

            if (AutoRotate != default)
            {
                list.Add($"{AutoRotateLabel}={AutoRotate}");
            }

            if (BaseScale !=  1.0)
            {
                list.Add(string.Create(CultureInfo.InvariantCulture, $"{BaseScaleLabel}={BaseScale}"));
            }

            if (SortSeed != 0)
            {
                list.Add(string.Create(CultureInfo.InvariantCulture, $"{SortSeedLabel}={SortSeed}"));
            }

            return string.Join(' ', list);
        }

        public static BookMemento? ParseWithProperties(string path, string? page, string? props)
        {
            if (page is null)
            {
                return null;
            }

            var memento = new BookMemento();
            memento.Path = path;
            memento.Page = page;

            if (string.IsNullOrWhiteSpace(props))
            {
                return memento;
            }

            var span = props.AsSpan();
            foreach (var range in span.SplitAny(" ,"))
            {
                var token = span[range];
                var word = token.Split('=');
                var key = word.MoveNext() ? token[word.Current].ToString() : null;
                var value = word.MoveNext() ? token[word.Current].ToString() : null;

                switch (key)
                {
                    case nameof(PageMode.SinglePage):
                        memento.PageMode = PageMode.SinglePage;
                        break;

                    case nameof(PageMode.WidePage):
                        memento.PageMode = PageMode.WidePage;
                        break;

                    case nameof(PageReadOrder.RightToLeft):
                        memento.BookReadOrder = PageReadOrder.RightToLeft;
                        break;

                    case nameof(PageReadOrder.LeftToRight):
                        memento.BookReadOrder = PageReadOrder.LeftToRight;
                        break;

                    case SortModeLabel:
                        if (value is null) throw new FormatException();
                        memento.SortMode = value.ToEnumOrDefault<PageSortMode>();
                        break;

                    case AutoRotateLabel:
                        if (value is null) throw new FormatException();
                        memento.AutoRotate = value.ToEnumOrDefault<AutoRotateType>();
                        break;

                    case BaseScaleLabel:
                        if (value is null) throw new FormatException();
                        memento.BaseScale = double.Parse(value, CultureInfo.InvariantCulture);
                        break;

                    case IsSupportedDividePageLabel:
                        memento.IsSupportedDividePage = true;
                        break;

                    case IsSupportedSingleFirstPageLabel:
                        memento.IsSupportedSingleFirstPage = true;
                        break;

                    case IsSupportedSingleLastPageLabel:
                        memento.IsSupportedSingleLastPage = true;
                        break;

                    case IsSupportedWidePageLabel:
                        memento.IsSupportedWidePage = true;
                        break;

                    case IsRecursiveFolderLabel:
                        memento.IsRecursiveFolder = true;
                        break;

                    case SortSeedLabel:
                        if (value is null) throw new FormatException();
                        memento.SortSeed = int.Parse(value, CultureInfo.InvariantCulture);
                        break;

                    default:
                        Debug.Assert(false, $"Not supprt{nameof(BookMemento)} property: {key}");
                        break;
                }
            }
            return memento;
        }
    }

}

