using System;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// ロードオプションフラグ
    /// </summary>
    [Flags]
    public enum BookLoadOption
    {
        None = 0,

        /// <summary>
        /// 再帰
        /// </summary>
        Recursive = 0x0001,

        /// <summary>
        /// 再帰しない
        /// </summary>
        NotRecursive = 0x0002,

        /// <summary>
        /// 初期ページを先頭ページにする
        /// </summary>
        FirstPage = 0x0004,

        /// <summary>
        /// 初期ページを最終ページにする
        /// </summary>
        LastPage = 0x0008,

        /// <summary>
        /// 再読み込みフラグ(BookHubで使用)
        /// </summary>
        ReLoad = 0x0010,

        /// <summary>
        /// 履歴の順番を変更しない
        /// </summary>
        KeepHistoryOrder = 0x0020,

        /// <summary>
        /// 可能ならば履歴リストで選択する
        /// </summary>
        SelectHistoryMaybe = 0x0040,

        /// <summary>
        /// 同じフォルダーならば読み込まない
        /// </summary>
        SkipSamePlace = 0x0080,

        /// <summary>
        /// 履歴情報から全て復元
        /// </summary>
        Resume = 0x0100,

        /// <summary>
        /// 再帰、ただし履歴が優先
        /// </summary>
        DefaultRecursive = 0x0200,

        /// <summary>
        /// このアドレスはブックです
        /// </summary>
        IsBook = 0x0400,

        /// <summary>
        /// このアドレスはページです
        /// </summary>
        IsPage = 0x0800,

        /// <summary>
        /// アーカイブキャッシュ無効
        /// </summary>
        IgnoreCache = 0x1000,

        /// <summary>
        /// 削除不可
        /// </summary>
        Undeletable = 0x2000,

        /// <summary>
        /// 最終ページならばリセット
        /// </summary>
        ResetLastPage = 0x4000,

        /// <summary>
        /// 名前変更による再読み込み
        /// </summary>
        Rename = 0x8000,

        /// <summary>
        /// ブック読み込み後にフォーカスを移動する
        /// </summary>
        FocusOnLoaded = 0x0001_0000,
    };


    public enum BookStartPageType
    {
        Custom,
        FirstPage,
        LastPage,
    }

    public class BookStartPage
    {
        public BookStartPage(BookStartPageType startPageType)
        {
            Debug.Assert(startPageType != BookStartPageType.Custom);
            StartPageType = startPageType;
        }

        public BookStartPage(string pageName, bool isResetLastPage)
        {
            Debug.Assert(!string.IsNullOrEmpty(pageName));
            StartPageType = BookStartPageType.Custom;
            PageName = pageName;
            IsResetLastPage = isResetLastPage;
        }

        public BookStartPageType StartPageType { get; private set; }
        public string? PageName { get; private set; }
        public bool IsResetLastPage { get; private set; }
    }

    public static class BookLoadOptionHelper
    {
        /// <summary>
        /// 設定を加味した再帰フラグを取得
        /// </summary>
        /// <param name="isRecursive">デフォルト値</param>
        /// <param name="setting">設定</param>
        public static bool CreateIsRecursiveFolder(bool isRecursive, BookLoadOption options)
        {
            if (options.HasFlag(BookLoadOption.NotRecursive))
            {
                return false;
            }
            else if (options.HasFlag(BookLoadOption.Recursive))
            {
                return true;
            }
            else
            {
                return isRecursive;
            }
        }

        public static BookStartPage CreateBookStartPage(string? entry, BookLoadOption options)
        {
            if ((options & BookLoadOption.FirstPage) == BookLoadOption.FirstPage)
            {
                return new BookStartPage(BookStartPageType.FirstPage);
            }
            else if ((options & BookLoadOption.LastPage) == BookLoadOption.LastPage)
            {
                return new BookStartPage(BookStartPageType.LastPage);
            }
            else if (!string.IsNullOrEmpty(entry))
            {
                return new BookStartPage(entry, options.HasFlag(BookLoadOption.ResetLastPage));
            }
            else
            {
                return new BookStartPage(BookStartPageType.FirstPage);
            }
        }
    }

}
