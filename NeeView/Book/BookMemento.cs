using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class BookMemento : IBookSetting
    {
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
        public double BaseScale { get; set; } = 1.0;

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
    }
}

