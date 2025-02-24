using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace NeeView
{
    public class Temporary
    {
        // NOTE: SetDirectory必須
        static Temporary() => Current = new Temporary();
        public static Temporary Current { get; }


        // テンポラリファイル名用のカウンタ
        private int _count = 0;

        // 排他制御用オブジェクト
        private readonly System.Threading.Lock _lock = new();


        private Temporary()
        {
            SetDirectory(TempRootPathDefault, false);
        }


        // テンポラリーフォルダー作成場所(既定)
        public static string TempRootPathDefault => System.IO.Path.GetTempPath().TrimEnd('\\');

        // テンポラリーフォルダー作成場所
        public string TempRootPath { get; private set; }

        // アプリのテンポラリフォルダー(BaseName)
        public string TempDirectoryBaseName { get; private set; }

        // アプリのテンポラリフォルダー
        public string TempDirectory { get; private set; }

        // アプリのダウンロードテンポラリフォルダー
        public string TempDownloadDirectory { get; private set; }

        // アプリのシステムテンポラリフォルダー
        public string TempSystemDirectory { get; private set; }

        // アプリのキャッシュテンポラリフォルダー
        public string TempCacheDirectory { get; private set; }


        /// <summary>
        /// テンポラリフォルダーの場所を指定
        /// </summary>
        /// <param name="path">場所。nullの場合はシステム既定</param>
        [MemberNotNull(nameof(TempRootPath), nameof(TempDirectoryBaseName), nameof(TempDirectory), nameof(TempDownloadDirectory), nameof(TempSystemDirectory), nameof(TempCacheDirectory))]
        public string SetDirectory(string path, bool validate)
        {
            // NOTE: シングルパッケージにすると次の方法ではアセンブリ情報が取得できない
            //var assembly = Assembly.GetExecutingAssembly();
            ////AssemblyCompanyの取得
            //var asmCmp = (AssemblyCompanyAttribute?)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute));
            ////AssemblyProductの取得
            //var asmPrd = (AssemblyProductAttribute?)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));

            var currentProcess = Process.GetCurrentProcess();
            //ProcessIDの取得
            var processId = currentProcess.Id;
            //Process名の取得
            var processName = currentProcess.ProcessName;

            TempRootPath = path ?? TempRootPathDefault;
            if (validate && TempRootPath != TempRootPathDefault)
            {
                if (!Directory.Exists(TempRootPath))
                {
                    ToastService.Current.Show(new Toast(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("Notice.TemporaryErrorDirectoryNotFound"), TempRootPath), Properties.TextResources.GetString("Notice.TemporaryErrorTitle"), ToastIcon.Error));
                    TempRootPath = TempRootPathDefault;
                }
            }

            TempDirectoryBaseName = processName + ".Temp"; //  asmPrd.Product;
            TempDirectory = Path.Combine(TempRootPath, TempDirectoryBaseName) + processId.ToString(CultureInfo.InvariantCulture);
            TempDownloadDirectory = Path.Combine(TempDirectory, "Temporary");
            TempSystemDirectory = Path.Combine(TempDirectory, "System");
            TempCacheDirectory = Path.Combine(TempDirectory, "Cache");

            Debug.WriteLine($"Temporary directory: {TempDirectory}");

            return TempRootPath;
        }

        /// <summary>
        /// テンポラリファイル名をカウンタ付きで生成
        /// マルチスレッド用
        /// </summary>
        /// <param name="prefix">ファイル名前置詞</param>
        /// <param name="ext">ファイル拡張子。例：".txt"</param>
        /// <returns>ユニークなテンポラリファイル名</returns>
        public string CreateCountedTempFileName(string prefix, string ext)
        {
            Debug.Assert(TempDirectory != null, "Need SetDirectory()");

            lock (_lock)
            {
                _count = (_count + 1) % 10000;
                return CreateTempFileName(string.Format(CultureInfo.InvariantCulture, "{0}{1:0000}{2}", prefix, _count, ext));
            }
        }

        /// <summary>
        /// テンポラリファイル名を生成
        /// 既に存在するときはファイル名に数値を付けて重複を避けます
        /// </summary>
        /// <param name="name">希望するファイル名</param>
        /// <returns>テンポラリファイル名</returns>
        public string CreateTempFileName(string name)
        {
            Debug.Assert(TempDirectory != null, "Need SetDirectory()");

            // 専用フォルダー作成
            Directory.CreateDirectory(TempCacheDirectory);

            // 名前の修正
            var validName = LoosePath.ValidFileName(name);

            // ファイル名作成
            string tempFileName = Path.Combine(TempCacheDirectory, validName);
            int count = 1;
            while (File.Exists(tempFileName) || Directory.Exists(tempFileName))
            {
                tempFileName = Path.Combine(TempCacheDirectory, Path.GetFileNameWithoutExtension(validName) + $"-{count++}" + Path.GetExtension(validName));
            }

            return tempFileName;
        }


        /// <summary>
        /// アプリのテンポラリフォルダーを削除
        /// アプリ終了時に呼ばれます
        /// </summary>
        public void RemoveTempFolder()
        {
            if (TempDirectory == null) return;

            try
            {
                var name = Process.GetCurrentProcess().ProcessName;
                var processes = Process.GetProcessesByName(name);

                // 最後のプロセスであればすべてのテンポラリを削除する
                if (processes.Length <= 1)
                {
                    var parent = Path.GetDirectoryName(TempDirectory);
                    if (parent != null)
                    {
                        foreach (var path in Directory.GetDirectories(parent, TempDirectoryBaseName + "*"))
                        {
                            Directory.Delete(path, true);
                        }
                    }
                }
                // 自プロセスのテンポラリのみ削除する
                else
                {
                    Directory.Delete(TempDirectory, true);
                }
            }
            catch
            {
                // 例外スルー
            }
        }
    }
}
