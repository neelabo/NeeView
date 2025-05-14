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
    public partial class AddressBreadcrumbProfile : IBreadcrumbProfile
    {
        [GeneratedRegex(@"^[a-zA-Z]:\\?$")]
        private static partial Regex _driveRegex { get; }

        public string GetDisplayName(string s, int index)
        {
            try
            {
                if (index == 1 && _driveRegex.IsMatch(s))
                {
                    var driveInfo = new DriveInfo(s);
                    return GetDriveLabel(driveInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return s;
        }

        public List<BreadcrumbToken> GetChildren(string path, int index, CancellationToken token)
        {
            if (string.IsNullOrEmpty(path))
            {
                return System.IO.Directory.GetLogicalDrives().Select(e => new FileBreadcrumbToken("", e, GetDisplayName(e, index + 1))).ToList<BreadcrumbToken>();
            }

            var collection = new FolderEntryCollection(new QueryPath(path), false, false);
            collection.InitializeItems(FolderOrder.FileName, token);

            return collection.Where(e => !e.IsEmpty()).Select(e => new FileBreadcrumbToken(path, e.Name ?? "None", null)).ToList<BreadcrumbToken>();
        }

        public bool CanHasChild(string path, int index)
        {
            return Directory.Exists(path);
        }

        private static string GetDriveLabel(DriveInfo driveInfo)
        {
            var driveName = driveInfo.Name.TrimEnd('\\');
            var volumeLabel = driveInfo.DriveType.ToDisplayString();
            var driveLabel = $"{volumeLabel} ({driveName})";

            try
            {
                // NOTE: ドライブによってはこのプロパティの取得に時間がかかる
                var IsReady = driveInfo.IsReady;
                if (driveInfo.IsReady)
                {
                    volumeLabel = string.IsNullOrEmpty(driveInfo.VolumeLabel) ? driveInfo.DriveType.ToDisplayString() : driveInfo.VolumeLabel;
                    driveLabel = $"{volumeLabel} ({driveName})";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return driveLabel;
        }

    }
}
