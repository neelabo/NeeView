using System.Collections.Generic;
using System.IO;

namespace NeeView.IO
{
    public static class DriveTypeExtension
    {
        private static readonly Dictionary<DriveType, string> _driveTypeNames = new()
        {
            [DriveType.Unknown] = "",
            [DriveType.NoRootDirectory] = "",
            [DriveType.Removable] = Properties.TextResources.GetString("Word.RemovableDrive"),
            [DriveType.Fixed] = Properties.TextResources.GetString("Word.FixedDrive"),
            [DriveType.Network] = Properties.TextResources.GetString("Word.NetworkDrive"),
            [DriveType.CDRom] = Properties.TextResources.GetString("Word.CDRomDrive"),
            [DriveType.Ram] = Properties.TextResources.GetString("Word.RamDrive"),
        };

        public static string ToDisplayString(this DriveType driveType)
        {
            return _driveTypeNames[driveType];
        }
    }
}

