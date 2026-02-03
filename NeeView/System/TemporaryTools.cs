using System;
using System.Globalization;
using System.IO;

namespace NeeView
{
    public static class TemporaryTools
    {
        private static int _count = 0;
        private static readonly System.Threading.Lock _lock = new();

        /// <summary>
        /// カウントされたテンポラリファイルパスを作成する
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="prefix"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string CreateCountedTempFileName(string directoryPath, string prefix, string ext)
        {
            if (!directoryPath.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal)) throw new InvalidOperationException();

            lock (_lock)
            {
                Directory.CreateDirectory(directoryPath);

                string tempFileName;
                do
                {
                    tempFileName = Path.Combine(directoryPath, string.Create(CultureInfo.InvariantCulture, $"{prefix}{_count:D4}{ext}"));
                    _count++;
                }
                while (File.Exists(tempFileName) || Directory.Exists(tempFileName));

                return tempFileName;
            }
        }

        /// <summary>
        /// ファイル名を指定してテンポラリファイルパスを作成する
        /// </summary>
        /// <remarks>
        /// 重複する場合はサブディレクトリのパスを作る。
        /// </remarks>
        /// <param name="directoryPath"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string CreateTempFileName(string directoryPath, string name)
        {
            if (!directoryPath.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal)) throw new InvalidOperationException();

            Directory.CreateDirectory(directoryPath);

            // 不正なファイル名を修正。アーカイブエントリなどでは使用できない文字が使われている可能性があるため。
            var validName = LoosePath.ValidFileName(name);

            string tempFileName = Path.Combine(directoryPath, validName);
            int count = 0;

            while (File.Exists(tempFileName) || Directory.Exists(tempFileName))
            {
                var subDirectory = Path.Combine(directoryPath, $"tmp{count:D4}");
                count++;

                if (File.Exists(subDirectory))
                {
                    continue;
                }
                Directory.CreateDirectory(subDirectory);

                tempFileName = Path.Combine(subDirectory, validName);
            }

            return tempFileName;
        }
    }
}
