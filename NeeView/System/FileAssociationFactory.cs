//#define LOCAL_DEBUG

using Microsoft.Win32;
using NeeLaboratory.Generators;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NeeView
{
    [LocalDebug]
    public static partial class FileAssociationFactory
    {
        [GeneratedRegex(@"^(.+),(-?\d+)$")]
        private static partial Regex _defaultIconRegex { get; }

        private static Dictionary<string, string>? _map;


        private static string? GetDescription(string ext)
        {
            if (_map is null)
            {
                _map = new Dictionary<string, string>()
                {
                    [".nvpls"] = TextResources.GetString("FileType.Playlist"),
                    [".nvzip"] = TextResources.GetString("FileType.ExportData"),
                };
            }

            if (_map.TryGetValue(ext, out var description))
            {
                return description;
            }

            return null;
        }

        public static FileAssociation Create(string ext, FileAssociationCategory category)
        {
            var description = GetDescription(ext);
            return new FileAssociation(ext, category, description);
        }

        public static FileAssociation? CreateFromRegistry(string progId)
        {
            var ext = progId[FileAssociation.ProgIdPrefix.Length..];
            if (string.IsNullOrEmpty(ext) || ext[0] != '.') return null;

            using var prog = Registry.CurrentUser.CreateSubKey(@$"Software\Classes\{progId}", false);
            if (prog is null) return null;

            var s = prog.GetValue("Category") as string;
            if (Enum.TryParse<FileAssociationCategory>(s, out var category) != true) return null;

            // アイコン情報取得
            FileAssociationIcon? icon = null;
            using var defaultIcon = prog.OpenSubKey("DefaultIcon");
            if (defaultIcon != null)
            {
                var iconSource = defaultIcon.GetValue("") as string;
                if (!string.IsNullOrEmpty(iconSource))
                {
                    var match = _defaultIconRegex.Match(iconSource);
                    if (match.Success)
                    {
                        string iconPath = match.Groups[1].Value;
                        int iconIndex = int.Parse(match.Groups[2].Value);
                        icon = new FileAssociationIcon(iconPath, iconIndex);
                    }
                    else
                    {
                        icon = new FileAssociationIcon(iconSource, 0);
                    }
                }
                LocalDebug.WriteLine($"Icon={icon}");
            }

            var description = GetDescription(ext);
            return new FileAssociation(ext, category, description, icon);
        }

    }
}
