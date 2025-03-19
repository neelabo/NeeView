using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Zone.Identifier をコピーするためのもの。生成はしない。
    /// </summary>
    public class ZoneIdentifier
    {
        private readonly byte[] _bytes;

        private ZoneIdentifier(byte[] bytes)
        {
            _bytes = bytes;
        }

        public static ZoneIdentifier? Read(string path)
        {
            try
            {
                var zoneIdentifierPath = GetZoneIdentifierPath(path);
                if (File.Exists(zoneIdentifierPath))
                {
                    var bytes = File.ReadAllBytes(zoneIdentifierPath);
                    return new ZoneIdentifier(bytes);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<ZoneIdentifier?> ReadAsync(string path, CancellationToken token)
        {
            try
            {
                var zoneIdentifierPath = GetZoneIdentifierPath(path);
                if (File.Exists(zoneIdentifierPath))
                {
                    var bytes = await File.ReadAllBytesAsync(zoneIdentifierPath, token);
                    return new ZoneIdentifier(bytes);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        public void Write(string path)
        {
            if (_bytes is null) return;

            try
            {
                var zoneIdentifierPath = GetZoneIdentifierPath(path);
                File.WriteAllBytes(zoneIdentifierPath, _bytes);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task WriteAsync(string path, CancellationToken token)
        {
            if (_bytes is null) return;

            try
            {
                var zoneIdentifierPath = GetZoneIdentifierPath(path);
                await File.WriteAllBytesAsync(zoneIdentifierPath, _bytes, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static string GetZoneIdentifierPath(string path)
        {
            return path + ":Zone.Identifier";
        }
    }
}
