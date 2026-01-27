using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NeeView.Susie.Server
{
    class Program
    {
        #region Native

        internal static class NativeMethods
        {
            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool SetDllDirectory(string lpPathName);
        }

        #endregion

        static void Main(string[] args)
        {
            Trace.WriteLine("");
            Trace.WriteLine($"---------------- {DateTime.Now}");

            if (args.Length != 1 || args[0] != SusiePluginRemote.BootKeyword)
            {
                Trace.WriteLine("BootKeyword does not match.");
                return;
            }

            // INFO: DLL 検索パスから現在の作業ディレクトリ (CWD) を削除
            NativeMethods.SetDllDirectory("");

            new SusiePluginRemoteServer().Run();

            Trace.WriteLine($"Shutdown.");
        }
    }
}
