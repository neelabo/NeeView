#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System.Collections.Generic;

namespace NeeView
{
    [LocalDebug]
    public static partial class UncPathTools
    {
        private static readonly Dictionary<string, string> _cache = new();

        public static bool IsUnc(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.StartsWith(@"\\");
        }

        public static string? GetUncPart(string path)
        {
            if (!IsUnc(path)) return null;

            string[] parts = path.TrimStart('\\').Split('\\');
            if (parts.Length < 2) return null;
            string uncRoot = @$"\\{parts[0]}\{parts[1]}";
            return uncRoot;
        }

        public static string ConvertPathToLowercase(string path)
        {
            var uncPart = GetUncPart(path);
            if (uncPart is null) return path;

            return uncPart.ToLowerInvariant() + path.Substring(uncPart.Length);
        }

        public static string ConvertPathToNormalized(string path)
        {
            var uncPart = GetUncPart(path);
            if (uncPart is null) return path;

            var key = uncPart.ToLowerInvariant();
            if (_cache.TryGetValue(key, out var normalizedPart))
            {
                return normalizedPart + path.Substring(normalizedPart.Length);
            }
            else
            {
                CacheUncPart(path);
                return path;
            }
        }

        private static void CacheUncPart(string path)
        {
            var uncPart = GetUncPart(path);
            if (uncPart is null) return;

            var key = uncPart.ToLowerInvariant();
            if (_cache.ContainsKey(key)) return;

            _cache.Add(key, uncPart);
            LocalDebug.WriteLine($"Cache: [{key}] => {uncPart}");
        }
    }

}

