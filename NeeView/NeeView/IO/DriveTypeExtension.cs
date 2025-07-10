using NeeView.Properties;
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
            [DriveType.Removable] = TextResources.GetString("Word.RemovableDrive"),
            [DriveType.Fixed] = TextResources.GetString("Word.FixedDrive"),
            [DriveType.Network] = TextResources.GetString("Word.NetworkDrive"),
            [DriveType.CDRom] = TextResources.GetString("Word.CDRomDrive"),
            [DriveType.Ram] = TextResources.GetString("Word.RamDrive"),
        };

        public static string ToDisplayString(this DriveType driveType)
        {
            return _driveTypeNames[driveType];
        }
    }
}

