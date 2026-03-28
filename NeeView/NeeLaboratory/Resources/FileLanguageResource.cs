using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NeeLaboratory.Resources
{
    public class FileLanguageResource : LanguageResource
    {
        private string _path;


        public FileLanguageResource(string path)
        {
            _path = path;

            List<CultureInfo> cultures;
            try
            {
                // TODO: ファイルシステムにアクセスしているので async が望ましい
                cultures = Directory.GetFiles(_path, "*" + _ext)
                    .Select(e => GetCultureInfoFromFileName(e))
                    .WhereNotNull()
                    .OrderBy(e => e.NativeName)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                cultures = [];
            }

            SetCultures(cultures);
        }

        /// <summary>
        /// リソースファイル名前からカルチャを得る
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static CultureInfo? GetCultureInfoFromFileName(string fileName)
        {
            try
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                return CultureInfo.GetCultureInfo(name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// カルチャからファイルソースを作成する
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override IFileSource CreateFileSource(CultureInfo culture)
        {
            if (string.IsNullOrEmpty(_path)) throw new InvalidOperationException();

            return new FileSource(Path.Combine(_path, CreateResTextFileName(culture)));
        }
    }
}
