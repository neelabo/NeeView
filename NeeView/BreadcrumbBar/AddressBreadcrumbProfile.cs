using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


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
            var tokens = query.Tokens;
            if (tokens.Length <= index)
            {
                return "";
            }
            var s = tokens[index];
            if (index == 1)
            {
                return FileIO.GetDriveDisplayName(s) ?? s;
            }
            return s;
        }

        public async ValueTask<List<BreadcrumbToken>> GetChildrenAsync(QueryPath query, CancellationToken token)
        {
            if (string.IsNullOrEmpty(query.Path))
            {
                return GetDriveChildren();
            }

            IEnumerable<FolderItem> items = [];
            if (query.Scheme == QueryScheme.File)
            {
                if (Directory.Exists(query.SimplePath))
                {
                    var collection = new FolderEntryCollection(query, false, false);
                    collection.InitializeItems(FolderOrder.FileName, token);
                    items = collection.Where(e => !e.IsEmpty());
                }
                else if (File.Exists(query.SimplePath) || FileIO.IsArchivePath(query.SimplePath))
                {
                    var collection = new FolderArchiveCollection(query, Config.Current.System.ArchiveRecursiveMode, false, false);
                    await collection.InitializeItemsAsync(FolderOrder.FileName, token);
                    items = collection.Where(e => !e.IsEmpty());
                }
            }

            if (!IncludeFile)
            {
                items = items.Where(e => e.IsDirectoryMaybe() || e.Attributes.HasFlag(FolderItemAttribute.ArchiveEntry));
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
