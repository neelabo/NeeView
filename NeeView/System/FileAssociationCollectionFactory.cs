using Microsoft.Win32;
using System;
using System.Linq;

namespace NeeView
{
    public static class FileAssociationCollectionFactory
    {
        public static FileAssociationCollection Create(FileAssociationCollectionCreateOptions options = FileAssociationCollectionCreateOptions.None)
        {
            var collection = new FileAssociationCollection();

            // レジストリから収集
            using var root = OpenClassesKey(false);
            if (root is not null)
            {
                var progIds = root.GetSubKeyNames().Where(e => e.StartsWith(FileAssociation.ProgIdPrefix)).ToList();
                foreach (var progId in progIds)
                {
                    var association = FileAssociationFactory.CreateFromRegistry(progId);
                    if (association is not null)
                    {
                        collection.Add(association);
                    }
                }
            }

            if (!options.HasFlag(FileAssociationCollectionCreateOptions.OnlyRegistered))
            {
                // システム
                collection.TryAdd(".nvpls", FileAssociationCategory.ForNeeView);
                collection.TryAdd(".nvzip", FileAssociationCategory.ForNeeView);

                // 画像拡張子
                foreach (var ext in PictureProfile.Current.GetFileTypes(false))
                {
                    collection.TryAdd(ext.ToLower(), FileAssociationCategory.Image);
                }

                // アーカイブ拡張子
                foreach (var ext in ArchiveManager.Current.GetFileTypes(false))
                {
                    collection.TryAdd(ext.ToLower(), FileAssociationCategory.Archive);
                }

                // 動画拡張子
                foreach (var ext in Config.Current.Archive.Media.SupportFileTypes.Items)
                {
                    collection.TryAdd(ext.ToLower(), FileAssociationCategory.Media);
                }
            }

            collection.Sort((x, y) => string.Compare(x.Extension, y.Extension));

            return collection;
        }


        private static RegistryKey? OpenClassesKey(bool writable)
        {
            return Registry.CurrentUser.OpenSubKey(@"Software\Classes", writable);
        }
    }

    [Flags]
    public enum FileAssociationCollectionCreateOptions
    {
        None = 0,
        OnlyRegistered = (1 << 0),
    }
}