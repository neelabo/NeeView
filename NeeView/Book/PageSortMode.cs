using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // ページ整列
    public enum PageSortMode
    {
        [AliasName("SortOrder.FileName")]
        FileName,

        [AliasName("SortOrder.FileNameDescending")]
        FileNameDescending,

        [AliasName("SortOrder.TimeStamp")]
        TimeStamp,

        [AliasName("SortOrder.TimeStampDescending")]
        TimeStampDescending,

        [AliasName("SortOrder.Size")]
        Size,

        [AliasName("SortOrder.SizeDescending")]
        SizeDescending,

        [AliasName("SortOrder.Entry")]
        Entry,

        [AliasName("SortOrder.EntryDescending")]
        EntryDescending,

        [AliasName("SortOrder.Random")]
        Random,
    }

    public static class PageSortModeExtension
    {
        public static PageSortMode GetToggle(this PageSortMode mode)
        {
            return (PageSortMode)(((int)mode + 1) % Enum.GetNames(typeof(PageSortMode)).Length);
        }

        public static bool IsDescending(this PageSortMode mode)
        {
            return mode switch
            {
                PageSortMode.FileNameDescending or PageSortMode.TimeStampDescending or PageSortMode.SizeDescending or PageSortMode.EntryDescending => true,
                _ => false,
            };
        }

        public static bool IsFileNameCategory(this PageSortMode mode)
        {
            return mode switch
            {
                PageSortMode.FileName or PageSortMode.FileNameDescending => true,
                _ => false,
            };
        }

        public static bool IsEntryCategory(this PageSortMode mode)
        {
            return mode switch
            {
                PageSortMode.Entry or PageSortMode.EntryDescending => true,
                _ => false,
            };
        }
    }

    public enum PageSortModeClass
    {
        None,
        Normal,
        WithEntry,
        Full,
    }

    public static class PageSortModeClassExtension
    {
        private static readonly Dictionary<PageSortMode, string> _mapNone;
        private static readonly Dictionary<PageSortMode, string> _mapNormal;
        private static readonly Dictionary<PageSortMode, string> _mapWithEntry;
        private static readonly Dictionary<PageSortMode, string> _mapFull;

        static PageSortModeClassExtension()
        {
            _mapFull = AliasNameExtensions.GetAliasNameDictionary<PageSortMode>();

            _mapWithEntry = _mapFull;

            _mapNormal = _mapWithEntry
                .Where(e => !e.Key.IsEntryCategory())
                .ToDictionary(e => e.Key, e => e.Value);

            _mapNone = _mapNormal
                .Where(e => e.Key == PageSortMode.FileName)
                .ToDictionary(e => e.Key, e => e.Value);
        }

        public static bool Contains(this PageSortModeClass self, PageSortMode mode)
        {
            return self.GetPageSortModeMap().ContainsKey(mode);
        }

        public static Dictionary<PageSortMode, string> GetPageSortModeMap(this PageSortModeClass self)
        {
            return self switch
            {
                PageSortModeClass.Full => _mapFull,
                PageSortModeClass.WithEntry => _mapWithEntry,
                PageSortModeClass.Normal => _mapNormal,
                _ => _mapNone,
            };
        }

        public static PageSortMode ValidatePageSortMode(this PageSortModeClass self, PageSortMode mode)
        {
            var map = self.GetPageSortModeMap();
            if (map.ContainsKey(mode))
            {
                return mode;
            }
            else
            {
                return mode.IsDescending() ? PageSortMode.FileNameDescending : PageSortMode.FileName;
            }
        }

        public static PageSortMode GetTogglePageSortMode(this PageSortModeClass self, PageSortMode mode)
        {
            while (true)
            {
                mode = mode.GetToggle();
                if (self.Contains(mode))
                {
                    return mode;
                }
            }
        }
    }

}
