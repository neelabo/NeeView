#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeLaboratory.Text;
using NeeView.Properties;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace NeeView
{
    /// <summary>
    /// アプリの環境
    /// </summary>
    [LocalDebug]
    public static partial class Environment
    {
        private const string ReleaseDevTag = "Dev";
        private const string ReleaseAlphaTag = "Alpha";
        private const string ReleaseBetaTag = "Beta";
        private const string ReleaseStableTag = "Stable";

        [GeneratedRegex(@"-(?<tag>[a-z]+)(?<ver>\.\d+)+")]
        private static partial Regex _releaseTypeRegex { get; }

        private static string? _localApplicationDataPath;
        private static string? _userDataPath;
        private static string? _defaultTempPath;
        private static string? _packageType;
        private static string? _revision;
        private static bool? _isUseLocalApplicationDataFolder;
        private static bool? _selfContained;
        private static string? _pdfRenderer;
        private static bool? _watermark;
        private static string? _logFile;
        private static Encoding? _encoding;
        private static string? _neeviewProfile;
        private static FormatVersion? _checkVersion;

        // TODO: static でなくてよい
        static Environment()
        {
            // エンコーディングプロバイダの登録
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ProcessId = System.Environment.ProcessId;

            AssemblyLocation = System.Environment.ProcessPath ?? throw new InvalidOperationException("Cannot get this AssemblyLocation");
            AssemblyFolder = Path.GetDirectoryName(AssemblyLocation) ?? throw new InvalidOperationException("Cannot get this AssemblyFolder");

            var assembly = Assembly.GetExecutingAssembly();
            ValidateProductInfo(assembly);
        }


        public static event EventHandler? LocalApplicationDataRemoved;


        /// <summary>
        /// プロセスID
        /// </summary>
        public static int ProcessId { get; private set; }

        /// <summary>
        /// マルチ起動での2番目以降のプロセス
        /// </summary>
        public static bool IsSecondProcess { get; set; }

        /// <summary>
        /// アセンブリの場所
        /// </summary>
        public static string AssemblyLocation { get; private set; }

        /// <summary>
        /// アセンブリの場所
        /// </summary>
        public static string AssemblyFolder { get; private set; }

        /// <summary>
        /// 会社名
        /// </summary>
        public static string CompanyName { get; private set; }

        /// <summary>
        /// ソリューション名
        /// </summary>
        public static string SolutionName => "NeeView";

        /// <summary>
        /// タイトル名
        /// </summary>
        public static string AssemblyTitle { get; private set; }

        /// <summary>
        /// プロダクト名
        /// </summary>
        public static string AssemblyProduct { get; private set; }

        /// <summary>
        /// プロダクトバージョン
        /// </summary>
        public static Version AssemblyVersion { get; private set; }

        /// <summary>
        /// アプリ名
        /// </summary>
        public static string ApplicationName => AssemblyTitle;

        /// <summary>
        /// プロダクトバージョン (Major.Minor.Build)
        /// </summary>
        public static string ProductVersion { get; private set; }

        /// <summary>
        /// アプリバージョン (Major.Minor)
        /// </summary>
        /// <remarks>
        /// 表示には通常こちらを使用します。
        /// </remarks>
        public static string ApplicationVersion { get; private set; }

        /// <summary>
        /// 表示用バージョン
        /// </summary>
        public static string DisplayVersion
        {
            get
            {
                return ReleaseType switch
                {
                    ReleaseDevTag
                        => ProductVersion + "-Dev",
                    ReleaseStableTag
                        => ApplicationVersion,
                    _
                        => ApplicationVersion + $"-{ReleaseType}{ReleaseNumber} / Rev {Revision}"
                };
            }
        }

        /// <summary>
        /// 表示用バージョン (ショート)
        /// </summary>
        public static string DisplayVersionShort
        {
            get
            {
                return ReleaseType switch
                {
                    ReleaseDevTag
                        => ProductVersion + "-Dev",
                    ReleaseStableTag
                        => ApplicationVersion,
                    _
                        => ApplicationVersion + $"-{ReleaseType}{ReleaseNumber}"
                };
            }
        }

        public static string UserAgent
        {
            get
            {
                return SolutionName + "/" + ProductVersion + $" ({OSVersion}) {DisplayVersionShort} (Rev {Revision}{(SelfContained ? "" : "; fd")})";
            }
        }

        public static string OSVersion => $"{System.Environment.OSVersion}; {(System.Environment.Is64BitOperatingSystem ? "x64" : "x86")}";

        public static string VersionNote =>
                        $"Version: {ApplicationName} {DisplayVersion}\r\n" +
                        $"Package: {PackageType}\r\n" +
                        $"OS: {OSVersion}";

        /// <summary>
        /// 環境変数 NEEVIEW_PROFILE 取得
        /// </summary>
        private static string NeeViewProfile
        {
            get
            {
                if (_neeviewProfile is null)
                {
                    // 環境変数 NEEVIEW_PROFILE から取得
                    _neeviewProfile = GetEnvironmentValue("NEEVIEW_PROFILE").Trim();
                    if (!string.IsNullOrEmpty(_neeviewProfile))
                    {
                        if (!Path.IsPathRooted(_neeviewProfile))
                        {
                            // Error: 環境変数 NEEVIEW_PROFILE は絶対パスではありません
                            throw new IOException("NEEVIEW_PROFILE: Not an absolute path");
                        }
                        if (!Directory.Exists(_neeviewProfile))
                        {
                            // Error: 環境変数 NEEVIEW_PROFILE が示すディレクトリが存在しません: (path)
                            throw new DirectoryNotFoundException($"NEEVIEW_PROFILE: Directory not found: {_neeviewProfile}");
                        }
                    }
                }
                return _neeviewProfile;
            }
        }

        /// <summary>
        /// アプリケーションデータフォルダー
        /// </summary>
        public static string LocalApplicationDataPath
        {
            get
            {
                if (_localApplicationDataPath == null)
                {
                    // 環境変数 NEEVIEW_PROFILE から取得
                    if (!string.IsNullOrEmpty(NeeViewProfile))
                    {
                        _localApplicationDataPath = NeeViewProfile;
                    }
                    // configファイルの設定で LocalApplicationData を使用するかを判定。インストール版用
                    else if (IsUseLocalApplicationDataFolder)
                    {
                        _localApplicationDataPath = GetLocalAppDataPath();
                        CreateFolder(_localApplicationDataPath);
                    }
                    // 既定ではアプリの場所の Profile フォルダーに作る
                    else
                    {
                        _localApplicationDataPath = Path.Combine(AssemblyFolder, "Profile");
                        CreateFolder(_localApplicationDataPath);
                    }
                    LocalDebug.WriteLine($"LocalApplicationDataPath: {_localApplicationDataPath}");
                }

                return _localApplicationDataPath;
            }
        }


        /// <summary>
        /// ユーザーデータフォルダー
        /// ユーザーが直接編集する可能性のあるデータ(スクリプトとか)の場所を区別するため LocalApplicationDataPath とは別定義
        /// </summary>
        public static string UserDataPath
        {
            get
            {
                if (_userDataPath == null)
                {
                    // 環境変数 NEEVIEW_PROFILE から取得
                    if (!string.IsNullOrEmpty(NeeViewProfile))
                    {
                        _userDataPath = NeeViewProfile;
                    }
                    // インストール版では MyDocument を使用
                    else if (IsUseLocalApplicationDataFolder)
                    {
                        _userDataPath = GetMyDocumentPath();
                        if (string.IsNullOrEmpty(_userDataPath))
                        {
                            _userDataPath = LocalApplicationDataPath;
                        }
                    }
                    // 既定では LocalApplicationDataPath
                    else
                    {
                        _userDataPath = LocalApplicationDataPath;
                    }
                    LocalDebug.WriteLine($"UserDataPath: {_userDataPath}");
                }
                return _userDataPath;
            }
        }


        /// <summary>
        /// 既定のテンポラリフォルダー
        /// </summary>
        public static string DefaultTempPath
        {
            get
            {
                if (_defaultTempPath == null)
                {
                    if (AppSettings.Current.TemporaryFilesInProfileFolder)
                    {
                        _defaultTempPath = Path.Combine(Environment.LocalApplicationDataPath, "Temp");
                    }
                    else
                    {
                        _defaultTempPath = System.IO.Path.GetTempPath().TrimEnd('\\');
                    }
                    LocalDebug.WriteLine($"DefaultTempPath: {_defaultTempPath}");
                }
                return _defaultTempPath;
            }
        }


        /// <summary>
        /// ライブラリーパス
        /// </summary>
        public static string LibrariesPath => AssemblyFolder;

        /// <summary>
        /// ライブラリーパス(Platform別)
        /// </summary>
        public static string LibrariesPlatformPath
        {
            get { return Path.Combine(LibrariesPath, "Libraries", PlatformName); }
        }

        /// <summary>
        /// x86/x64判定
        /// </summary>
        public static bool IsX64
        {
            get { return System.Environment.Is64BitProcess; }
        }

        public static string PlatformName
        {
            get { return IsX64 ? "x64" : "x86"; }
        }

        // データ保存にアプリケーションデータフォルダーを使用するか
        public static bool IsUseLocalApplicationDataFolder
        {
            get
            {
                if (_isUseLocalApplicationDataFolder == null)
                {
                    _isUseLocalApplicationDataFolder = AppSettings.Current.UseLocalApplicationData;
                }
                return (bool)_isUseLocalApplicationDataFolder;
            }
        }

        // 自己完結型？
        public static bool SelfContained
        {
            get
            {
                if (_selfContained == null)
                {
                    _selfContained = AppSettings.Current.SelfContained;
                }
                return (bool)_selfContained;
            }
        }

        // パッケージの種類(拡張子)
        public static string PackageType
        {
            get
            {
                if (_packageType == null)
                {
                    _packageType = AppSettings.Current.PackageType;
                }
                return _packageType;
            }
        }


        // Dev, Alpha, Beta, "" のいずれか
        public static string ReleaseType { get; private set; } = "";
        public static string ReleaseNumber { get; private set; } = "";

        public static bool IsDevPackage => PackageType == "Dev";
        public static bool IsZipPackage => PackageType == "Zip";
        public static bool IsMsiPackage => PackageType == "Msi";
        public static bool IsAppxPackage => PackageType == "Appx";

        public static bool IsZipLikePackage => IsZipPackage || IsDevPackage;

        public static bool IsDevRelease => ReleaseType == ReleaseDevTag;
        public static bool IsAlphaRelease => ReleaseType == ReleaseAlphaTag;
        public static bool IsBetaRelease => ReleaseType == ReleaseBetaTag;
        public static bool IsStableRelease => ReleaseType == ReleaseStableTag;

#if DEBUG
        public static readonly string ConfigType = "Debug";
#else
        public static readonly string ConfigType = "Release";
#endif

        // リビジョン番号 (GitのコミットID)
        public static string Revision
        {
            get
            {
                if (_revision == null)
                {
                    _revision = AppSettings.Current.Revision;
                }
                return _revision;
            }
        }

        /// <summary>
        /// システムのエンコーディング
        /// </summary>
        public static Encoding Encoding
        {
            get
            {
                if (_encoding is null)
                {
                    _encoding = Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
                }
                return _encoding;
            }
        }


        public static string PdfRenderer
        {
            get
            {
                if (_pdfRenderer is null)
                {
                    _pdfRenderer = AppSettings.Current.PdfRenderer ?? "Pdfium";
                }
                return _pdfRenderer;
            }
        }

        public static bool Watermark
        {
            get
            {
                if (_watermark is null)
                {
                    _watermark = AppSettings.Current.Watermark;
                }
                return (bool)_watermark;
            }
        }

        // [開発用] バージョンチェック用のバージョンを指定
        public static FormatVersion CheckVersion
        {
            get
            {
                if (_checkVersion is null)
                {
                    var version = AppSettings.Current.CheckVersion;
                    _checkVersion = version is null ? new FormatVersion(Environment.SolutionName) with { BuildVersion = 0 } : new FormatVersion(Environment.SolutionName, version);
                }
                return _checkVersion;
            }
        }

        // [開発用] 出力用ログファイル名
        public static string LogFile
        {
            get
            {
                if (_logFile == null)
                {
                    var logFile = AppSettings.Current.LogFile;
                    if (string.IsNullOrEmpty(logFile))
                    {
                        _logFile = "";
                    }
                    else if (Path.IsPathRooted(logFile))
                    {
                        _logFile = logFile;
                    }
                    else
                    {
                        _logFile = Path.Combine(LocalApplicationDataPath, logFile);
                    }
                }
                return _logFile;
            }
        }


        /// <summary>
        /// 環境変数取得
        /// </summary>
        /// <param name="variable">変数名</param>
        /// <returns>値。取得できないときは空文字列</returns>
        public static string GetEnvironmentValue(string variable)
        {
            try
            {
                return System.Environment.GetEnvironmentVariable(variable) ?? "";
            }
            catch (SecurityException)
            {
                return "";
            }
        }

        // PCメモリサイズ
        public static ulong GetTotalPhysicalMemory()
        {
            var info = new Microsoft.VisualBasic.Devices.ComputerInfo();
            return info.TotalPhysicalMemory;
        }

        /// <summary>
        /// アセンブリ情報収集
        /// </summary>
        /// <param name="asm"></param>
        [MemberNotNull(nameof(CompanyName), nameof(AssemblyTitle), nameof(AssemblyProduct), nameof(AssemblyVersion), nameof(ProductVersion), nameof(ApplicationVersion))]
        private static void ValidateProductInfo(Assembly asm)
        {
            // 会社名
            AssemblyCompanyAttribute? companyAttribute = Attribute.GetCustomAttribute(asm, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
            CompanyName = companyAttribute?.Company ?? throw new InvalidOperationException("Cannot get AssemblyCompany");

            // タイトル
            AssemblyTitleAttribute? titleAttribute = Attribute.GetCustomAttribute(asm, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
            AssemblyTitle = titleAttribute?.Title ?? throw new InvalidOperationException("Cannot get AssemblyTitle");

            // プロダクト
            AssemblyProductAttribute? productAttribute = Attribute.GetCustomAttribute(asm, typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
            AssemblyProduct = productAttribute?.Product ?? throw new InvalidOperationException("Cannot get AssemblyProduct");

            // バージョンの取得
            AssemblyVersion = asm.GetName().Version ?? throw new InvalidOperationException("Cannot get AssemblyVersion");
            ProductVersion = $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build}";
            ApplicationVersion = $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}";

            // リリースタイプの取得
            (ReleaseType, ReleaseNumber) = GetReleaseType(asm);
        }

        /// <summary>
        /// リリースタイプの取得
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        private static (string type, string number) GetReleaseType(Assembly asm)
        {
            // Dev版
            if (IsDevPackage)
            {
                return (ReleaseDevTag, "");
            }

            // Alpha,Beta版はファイルのバージョン情報から取得
            var informationalVersionAttribute = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var informationalVersion = informationalVersionAttribute?.InformationalVersion;
            if (informationalVersion is not null)
            {
                var match = _releaseTypeRegex.Match(informationalVersion);
                if (match.Success)
                {
                    return (match.Groups["tag"].Value.ToTitleCase(), match.Groups["ver"].Value);
                }
            }

            // それ以外は安定版とみなす
            return (ReleaseStableTag, "");
        }

        /// <summary>
        /// マイドキュメントのアプリ専用フォルダー
        /// </summary>
        /// <returns>マイドキュメントのパス。取得できないときは空文字</returns>
        public static string GetMyDocumentPath()
        {
            var myDocuments = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            return string.IsNullOrEmpty(myDocuments) ? "" : System.IO.Path.Combine(myDocuments, CompanyName, SolutionName);
        }

        /// <summary>
        /// データフォルダーを取得する
        /// </summary>
        /// <param name="name">フォルダー名</param>
        /// <returns>フルパス。取得できない場合はEmptyを返す</returns>
        public static string GetUserDataPath(string name)
        {
            if (string.IsNullOrEmpty(UserDataPath))
            {
                return "";
            }
            else
            {
                return Path.Combine(UserDataPath, name);
            }
        }

        /// <summary>
        /// フォルダー生成
        /// </summary>
        private static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// AppDataのアプリ用ローカルフォルダーのパスを取得
        /// </summary>
        public static string GetLocalAppDataPath()
        {
            if (IsAppxPackage)
            {
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), CompanyName + "-" + SolutionName);
            }
            else
            {
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), CompanyName, SolutionName);
            }
        }

        /// <summary>
        /// AppDataのカンパニーフォルダーのパスを取得
        /// </summary>
        /// <remarks>
        /// Appxではカンパニーフォルダーは存在しないのでnullになる
        /// </remarks>
        private static string? GetLocalAppDataCompanyPath()
        {
            if (IsAppxPackage)
            {
                return null;
            }
            else
            {
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), CompanyName);
            }
        }

        // 全ユーザデータ削除
        public static void RemoveApplicationData(Window? owner)
        {
            var dialog = new MessageDialog(TextResources.GetString("DeleteApplicationDataDialog.Message"), TextResources.GetString("DeleteApplicationDataDialog.Title"));
            dialog.Commands.Add(UICommands.Delete);
            dialog.Commands.Add(UICommands.Cancel);
            var result = dialog.ShowDialog(owner);

            if (result.Command == UICommands.Delete)
            {
                // キャッシュDBを閉じる
                ThumbnailCache.Current.Close();

                try
                {
                    RemoveApplicationDataCore();
                    ClearRegistry();
                    new MessageDialog(TextResources.GetString("DeleteApplicationDataCompleteDialog.Message"), TextResources.GetString("DeleteApplicationDataCompleteDialog.Title")).ShowDialog(owner);
                    LocalApplicationDataRemoved?.Invoke(null, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    new MessageDialog(ex.Message, TextResources.GetString("DeleteApplicationDataErrorDialog.Title")).ShowDialog(owner);
                }
            }
        }

        // 全ユーザデータ削除
        private static bool RemoveApplicationDataCore()
        {
            // LocalApplicationDataフォルダーを使用している場合のみ
            if (!IsUseLocalApplicationDataFolder)
            {
                throw new ApplicationException(TextResources.GetString("CannotDeleteDataException.Message"));
            }

            LocalDebug.WriteLine("RemoveAllApplicationData ...");

            var productFolder = GetLocalAppDataPath();
            Directory.Delete(productFolder, true);
            System.Threading.Thread.Sleep(500);

            var companyFolder = GetLocalAppDataCompanyPath();
            if (companyFolder != null)
            {
                if (Directory.GetFileSystemEntries(companyFolder).Length == 0)
                {
                    Directory.Delete(companyFolder);
                }
            }

            LocalDebug.WriteLine("RemoveAllApplicationData done.");
            return true;
        }

        /// <summary>
        /// APPXデータフォルダー移動 (ver.38)
        /// </summary>
        /// <remarks>
        /// これまでの NeeLaboratory\NeeView.a では NeeLaboratory フォルダーがインストール前に存在していると NeeView.a がアンインストールでも消えないため、
        /// ver.38からの専用のフォルダー NeeLaboratory-NeeView に移動させる。
        /// アプリ専用の仮想フォルダとして生成されるため、アンインストールで自動削除される
        /// </remarks>
        public static void CorrectLocalAppDataFolder()
        {
            // this function is support Appx only.
            if (!IsAppxPackage) return;
            if (!IsUseLocalApplicationDataFolder) return;

            try
            {
                string oldPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), CompanyName, SolutionName) + ".a";
                string newPath = GetLocalAppDataPath();

                // if already exist new path, exit.
                if (Directory.Exists(newPath)) return;

                // if old path not exist, exit
                var directory = new DirectoryInfo(oldPath);
                if (!directory.Exists) return;

                // move ... OK?
                directory.MoveTo(newPath);
            }
            catch (Exception ex)
            {
                LocalDebug.WriteLine(nameof(CorrectLocalAppDataFolder) + " failed: " + ex.Message);
            }
        }


        // レジストリ解除
        public static void ClearRegistry()
        {
            if (IsAppxPackage) return;

            ExplorerContextMenu.Current.IsEnabled = false;
            FileAssociationTools.UnassociateAll();
            FileAssociationTools.RefreshShellIcons();
        }
    }
}
