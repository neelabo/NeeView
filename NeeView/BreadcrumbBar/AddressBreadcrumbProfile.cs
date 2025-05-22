using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;


namespace NeeView
{
    public class AddressBreadcrumbProfile : IBreadcrumbProfile
    {
        /// <summary>
        /// 子供にブックとなるファイルを含める
        /// </summary>
        public bool IncludeFile { get; set; } = true;


        public string GetDisplayName(QueryPath query, int index)
        {
            if (index == 0 && query.Scheme.CanOmit())
            {
                return "";
            }
            var s = query.Tokens[index];
            if (index == 1)
            {
                return FileIO.GetDriveDisplayName(s) ?? s;
            }
            return s;
        }

        public List<BreadcrumbToken> GetChildren(QueryPath query, CancellationToken token)
        {
            if (string.IsNullOrEmpty(query.Path))
            {
                return GetDriveChildren();
            }

            var collection = new FolderEntryCollection(query, false, false);
            collection.InitializeItems(FolderOrder.FileName, token);

            var items = collection.Where(e => !e.IsEmpty());

            if (!IncludeFile)
            {
                items = items.Where(e => e.IsDirectoryMaybe());
            }

            return items.Select(e => new FileBreadcrumbToken(query, e.Name ?? "None", null)).ToList<BreadcrumbToken>();
        }

        public bool CanHasChild(QueryPath query)
        {
            return Directory.Exists(query.SimplePath);
        }

        public List<BreadcrumbToken> GetDriveChildren()
        {
            return System.IO.Directory.GetLogicalDrives().Select(e => new FileBreadcrumbToken(QueryPath.None, e, FileIO.GetDriveDisplayName(e))).ToList<BreadcrumbToken>();
        }
    }
}
