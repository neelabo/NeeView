using System;
using System.IO;
using System.Globalization;

namespace NeeView
{
    public static class TemporaryTools
    {
        private static int _count = 0;
        private static readonly System.Threading.Lock _lock = new();

        public static string CreateCountedTempFileName(string directoryPath, string prefix, string ext)
        {
            lock (_lock)
            {
                _count = (_count + 1) % 10000;
                return CreateTempFileName(directoryPath, string.Format(CultureInfo.InvariantCulture, "{0}{1:0000}{2}", prefix, _count, ext));
            }
        }

        public static string CreateTempFileName(string directoryPath, string name)
        {
            if (!directoryPath.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal)) throw new InvalidOperationException();

            // 専用フォルダー作成
            Directory.CreateDirectory(directoryPath);

            // 名前の修正
            var validName = LoosePath.ValidFileName(name);

            // ファイル名作成
            string tempFileName = Path.Combine(directoryPath, validName);
            int count = 1;
            while (File.Exists(tempFileName) || Directory.Exists(tempFileName))
            {
                tempFileName = Path.Combine(directoryPath, Path.GetFileNameWithoutExtension(validName) + $"-{count++}" + Path.GetExtension(validName));
            }

            return tempFileName;
        }
    }
}
