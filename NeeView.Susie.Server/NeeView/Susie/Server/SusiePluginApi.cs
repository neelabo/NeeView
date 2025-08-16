using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Susie.Server
{
    /// <summary>
    /// Susie Plugin API
    /// アンマネージなDLLアクセスを行います。
    /// </summary>
    public class SusiePluginApi : IDisposable, ISusiePluginApi
    {
        // DLLハンドル
        private IntPtr hModule = IntPtr.Zero;

        // APIデリゲートリスト
        private readonly Dictionary<Type, object> _apiDelegateList = new();

        private bool _disposedValue = false;


        private SusiePluginApi()
        {
        }

        public bool IsDisposed => _disposedValue;

        /// <summary>
        /// プラグインをロードし、使用可能状態にする
        /// </summary>
        /// <param name="fileName">spiファイル名</param>
        /// <returns>プラグインインターフェイス</returns>
        public static SusiePluginApi Create(string fileName)
        {
            var lib = new SusiePluginApi();
            lib.Open(fileName);
            if (lib == null) throw new ArgumentException("not support " + fileName);
            return lib;
        }

        /// <summary>
        /// DLLロードし、使用可能状態にする
        /// </summary>
        /// <param name="fileName">spiファイル名</param>
        /// <returns>DLLハンドル</returns>
        private IntPtr Open(string fileName)
        {
            Close();
            this.hModule = NativeMethods.LoadLibrary(fileName);
            return hModule;
        }

        /// <summary>
        /// DLLをアンロードする
        /// </summary>
        private void Close()
        {
            if (hModule != IntPtr.Zero)
            {
                _apiDelegateList.Clear();
                NativeMethods.FreeLibrary(this.hModule);
                hModule = IntPtr.Zero;
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                Close();
                _disposedValue = true;
            }
        }

        ~SusiePluginApi()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// APIの存在確認
        /// </summary>
        /// <param name="name">API名</param>
        /// <returns>trueなら存在する</returns>
        public bool IsExistFunction(string name)
        {
            if (hModule == IntPtr.Zero) throw new InvalidOperationException();

            IntPtr add = NativeMethods.GetProcAddress(this.hModule, name);
            return (add != IntPtr.Zero);
        }


        /// <summary>
        /// API取得
        /// </summary>
        /// <typeparam name="T">APIのデリゲート</typeparam>
        /// <param name="procName">API名</param>
        /// <returns></returns>
        private T GetApiDelegate<T>(string procName)
            where T : notnull
        {
            if (!_apiDelegateList.ContainsKey(typeof(T)))
            {
                IntPtr add = NativeMethods.GetProcAddress(this.hModule, procName);
                if (add == IntPtr.Zero) throw new NotSupportedException("not support " + procName);
                _apiDelegateList.Add(typeof(T), Marshal.GetDelegateForFunctionPointer<T>(add));
            }

            return (T)_apiDelegateList[typeof(T)];
        }



        // callback delegate
        private delegate int ProgressCallback(int nNum, int nDenom, int lData);

        /// <summary>
        /// Dummy Callback
        /// </summary>
        /// <param name="nNum"></param>
        /// <param name="nDenom"></param>
        /// <param name="lData"></param>
        /// <returns></returns>
        private static int ProgressCallbackDummy(int nNum, int nDenom, int lData)
        {
            return 0;
        }


        #region 00IN,00AM 必須 GetPluginInfo
        private delegate int GetPluginInfoDelegate(int infono, StringBuilder buf, int len);

        /// <summary>
        /// Plug-inに関する情報を得る
        /// </summary>
        /// <param name="infono">取得する情報番号</param>
        /// <returns>情報の文字列。情報番号が無効の場合は nullを返す</returns>
        public string? GetPluginInfo(int infono)
        {
            if (hModule == IntPtr.Zero) throw new InvalidOperationException();
            var getPluginInfo = GetApiDelegate<GetPluginInfoDelegate>("GetPluginInfo");

            var strb = new StringBuilder(1024);
            int ret = getPluginInfo(infono, strb, strb.Capacity);
            if (ret <= 0) return null;
            return strb.ToString();
        }
        #endregion


        #region 00IN,00AM 任意 ConfigurationDlg()
        private delegate int ConfigurationDlgDelegate(IntPtr parent, int fnc);


        /// <summary>
        /// Plug-in設定ダイアログの表示 
        /// </summary>
        /// <param name="parent">親ウィンドウのウィンドウハンドル</param>
        /// <param name="func">0:aboutダイアログ / 1:設定ダイアログ</param>
        /// <returns>0なら正常終了、それ以外はエラーコードを返す</returns>
        public int ConfigurationDlg(IntPtr parent, int func)
        {
            if (hModule == IntPtr.Zero) throw new InvalidOperationException();
            var configurationDlg = GetApiDelegate<ConfigurationDlgDelegate>("ConfigurationDlg");

            return configurationDlg(parent, func);
        }
        #endregion

        #region 00IN,00AM 必須 IsSupported()
        private delegate bool IsSupportedFromFileDelegate(string filename, IntPtr dw);
        private delegate bool IsSupportedFromMemoryDelegate(string filename, [In] byte[] dw);

        /// <summary>
        /// サポート判定(ファイル版)
        /// 注意：Susie本体はこの関数(ファイル版)を使用していないため、正常に動作しないプラグインが存在します！
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>サポートしていれば true</returns>
        public bool IsSupported(string filename)
        {
            if (hModule == IntPtr.Zero) throw new InvalidOperationException();
            var isSupported = GetApiDelegate<IsSupportedFromFileDelegate>("IsSupported");

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return isSupported(filename, fs.SafeFileHandle.DangerousGetHandle());
            }
        }

        /// <summary>
        /// サポート判定(メモリ版)
        /// </summary>
        /// <param name="filename">ファイル名(判定用)</param>
        /// <param name="buff">対象データ</param>
        /// <returns>サポートしていれば true</returns>
        public bool IsSupported(string filename, byte[] buff)
        {
            if (hModule == IntPtr.Zero) throw new InvalidOperationException();
            var isSupported = GetApiDelegate<IsSupportedFromMemoryDelegate>("IsSupported");

            return isSupported(filename, buff);
        }
        #endregion


        #region 00AM 必須 GetArchiveInfo()
        private delegate int GetArchiveInfoFromFileDelegate([In] string filename, int offset, uint flag, out IntPtr hInfo);
        private delegate int GetArchiveInfoFromMemoryDelegate([In] byte[] buf, int offset, uint flag, out IntPtr hInfo);

        /// <summary>
        /// アーカイブ情報取得
        /// </summary>
        /// <param name="file">アーカイブファイル名</param>
        /// <returns>アーカイブエントリ情報(RAW)。失敗した場合は null</returns>
        public List<ArchiveFileInfoRaw>? GetArchiveInfo(string file)
        {
            if (hModule == IntPtr.Zero) throw new InvalidOperationException();
            var getArchiveInfo = GetApiDelegate<GetArchiveInfoFromFileDelegate>("GetArchiveInfo");

            IntPtr hInfo = IntPtr.Zero;
            try
            {
                int ret = getArchiveInfo(file, 0, 0, out hInfo);
                if (ret == 0)
                {
                    var list = new List<ArchiveFileInfoRaw>();
                    var structSize = Marshal.SizeOf<ArchiveFileInfoRaw>();
                    IntPtr p = NativeMethods.LocalLock(hInfo);
                    while (true)
                    {
                        ArchiveFileInfoRaw fileInfo = Marshal.PtrToStructure<ArchiveFileInfoRaw>(p);
                        if (string.IsNullOrEmpty(fileInfo.method)) break;
                        list.Add(fileInfo);
                        p += structSize;
                    }

                    return list;
                }
            }
            finally
            {
                NativeMethods.LocalUnlock(hInfo);
                NativeMethods.LocalFree(hInfo);
            }

            return null;
        }
        #endregion


        #region 00AM 必須 GetFile()
        private delegate int GetFileFromFileHandler(string filename, int position, out IntPtr hBuff, uint flag, ProgressCallback lpProgressCallback, int lData);
        private delegate int GetFileFromFileToFileHandler(string filename, int position, string dest, uint flag, ProgressCallback lpProgressCallback, int lData);

        /// <summary>
        /// アーカイブエントリ取得(メモリ版)
        /// </summary>
        /// <param name="file">アーカイブファイル名</param>
        /// <param name="position">アーカイブエントリ位置</param>
        /// <returns>出力されたバッファ。失敗した場合は null</returns>
        public byte[]? GetFile(string file, int position)
        {
            if (hModule == IntPtr.Zero) throw new InvalidOperationException();
            var getFile = GetApiDelegate<GetFileFromFileHandler>("GetFile");

            IntPtr hBuff = IntPtr.Zero;
            try
            {
                int ret = getFile(file, position, out hBuff, 0x0100, ProgressCallbackDummy, 0); // 0x0100 > File To Memory
                if (ret == 0)
                {
                    IntPtr pBuff = NativeMethods.LocalLock(hBuff);
                    var buffSize = (int)NativeMethods.LocalSize(hBuff);
                    if (buffSize == 0) throw new SusieException("Memory error.");
                    byte[] buf = new byte[buffSize];
                    Marshal.Copy(pBuff, buf, (int)0, (int)buffSize);
                    return buf;
                }
                return null;
            }
            finally
            {
                NativeMethods.LocalUnlock(hBuff);
                NativeMethods.LocalFree(hBuff);
            }
        }

        /// <summary>
        /// アーカイブエントリ取得(ファイル版)
        /// </summary>
        /// <param name="file">アーカイブファイル名</param>
        /// <param name="position">アーカイブエントリ位置</param>
        /// <param name="extractFolder">出力フォルダー</param>
        /// <returns>成功した場合は0</returns>
        public int GetFile(string file, int position, string extractFolder)
        {
            if (hModule == IntPtr.Zero) throw new InvalidOperationException();
            var getFile = GetApiDelegate<GetFileFromFileToFileHandler>("GetFile");

            return getFile(file, position, extractFolder, 0x0000, ProgressCallbackDummy, 0); // 0x0000 > File To File
        }

        #endregion


        #region 00IN 必須 GetPicture()
        private delegate int GetPictureFromMemoryDelegate([In] byte[] buf, int len, uint flag, out IntPtr pHBInfo, out IntPtr pHBm, ProgressCallback lpProgressCallback, int lData);
        private delegate int GetPictureFromFileDelegate([In] string filename, int offset, uint flag, out IntPtr pHBInfo, out IntPtr pHBm, ProgressCallback lpProgressCallback, int lData);



        /// <summary>
        /// 画像取得(メモリ版)
        /// </summary>
        /// <param name="buff">入力画像データ</param>
        /// <returns>Bitmap。失敗した場合は null</returns>
        public byte[]? GetPicture(byte[] buff)
        {
            if (hModule == IntPtr.Zero) throw new InvalidOperationException();
            var getPicture = GetApiDelegate<GetPictureFromMemoryDelegate>("GetPicture");

            IntPtr pHBInfo = IntPtr.Zero;
            IntPtr pHBm = IntPtr.Zero;
            try
            {
                int ret = getPicture(buff, buff.Length, 0x01, out pHBInfo, out pHBm, ProgressCallbackDummy, 0);
                if (ret == 0)
                {
                    IntPtr pBInfo = NativeMethods.LocalLock(pHBInfo);
                    int pBInfoSize = (int)NativeMethods.LocalSize(pHBInfo);
                    IntPtr pBm = NativeMethods.LocalLock(pHBm);
                    int pBmSize = (int)NativeMethods.LocalSize(pHBm);
                    return CreateBitmapImage(pBInfo, pBInfoSize, pBm, pBmSize);
                }
                return null;
            }
            finally
            {
                NativeMethods.LocalUnlock(pHBInfo);
                NativeMethods.LocalUnlock(pHBm);
                NativeMethods.LocalFree(pHBInfo);
                NativeMethods.LocalFree(pHBm);
            }
        }


        /// <summary>
        /// 画像取得(ファイル版)
        /// </summary>
        /// <param name="filename">入力ファイル名</param>
        /// <returns>Bitmap。失敗した場合は null</returns>
        public byte[]? GetPicture(string filename)
        {
            if (hModule == IntPtr.Zero) throw new InvalidOperationException();
            var getPicture = GetApiDelegate<GetPictureFromFileDelegate>("GetPicture");

            IntPtr pHBInfo = IntPtr.Zero;
            IntPtr pHBm = IntPtr.Zero;
            try
            {
                int ret = getPicture(filename, 0, 0x00, out pHBInfo, out pHBm, ProgressCallbackDummy, 0);
                if (ret == 0)
                {
                    IntPtr pBInfo = NativeMethods.LocalLock(pHBInfo);
                    int pBInfoSize = (int)NativeMethods.LocalSize(pHBInfo);
                    IntPtr pBm = NativeMethods.LocalLock(pHBm);
                    int pBmSize = (int)NativeMethods.LocalSize(pHBm);
                    return CreateBitmapImage(pBInfo, pBInfoSize, pBm, pBmSize);
                }
                return null;
            }
            finally
            {
                NativeMethods.LocalUnlock(pHBInfo);
                NativeMethods.LocalUnlock(pHBm);
                NativeMethods.LocalFree(pHBInfo);
                NativeMethods.LocalFree(pHBm);
            }
        }


        // Bitmap 作成
        private static byte[] CreateBitmapImage(IntPtr pBInfo, int pBInfoSize, IntPtr pBm, int pBmSize)
        {
            if (pBInfoSize == 0 || pBmSize == 0)
            {
                throw new SusieException("Memory error.");
            }

            var bf = CreateBitmapFileHeader(pBInfoSize, pBmSize);
            byte[] mem = new byte[bf.bfSize];
            GCHandle gch = GCHandle.Alloc(mem, GCHandleType.Pinned);
            try { Marshal.StructureToPtr<BitmapFileHeader>(bf, gch.AddrOfPinnedObject(), false); }
            finally { gch.Free(); }
            Marshal.Copy(pBInfo, mem, Marshal.SizeOf(bf), pBInfoSize);
            Marshal.Copy(pBm, mem, (int)bf.bfOffBits, pBmSize);
            return mem;
        }


        // BitmapFileHeader作成
        private static BitmapFileHeader CreateBitmapFileHeader(int pBInfoSize, int pBmSize)
        {
            var bf = new BitmapFileHeader();
            bf.bfSize += bf.bfOffBits;
            bf.bfType = 0x4d42;
            bf.bfSize = (uint)Marshal.SizeOf(bf) + (uint)pBInfoSize + (uint)pBmSize;
            bf.bfReserved1 = 0;
            bf.bfReserved2 = 0;
            bf.bfOffBits = (uint)Marshal.SizeOf(bf) + (uint)pBInfoSize;
            return bf;
        }
        #endregion
    }


    /// <summary>
    /// アーカイブエントリ情報(Raw)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ArchiveFileInfoRaw
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string method; // 圧縮法の種類
        public uint position; // ファイル上での位置
        public uint compsize; // 圧縮されたサイズ
        public uint filesize; // 元のファイルサイズ
        public uint timestamp; // ファイルの更新日時
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string path; // 相対パス
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string filename; // ファイルネーム
        public uint crc; // CRC 
    }

}
