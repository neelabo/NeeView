using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public class AppProcess
    {
        private string[]? _args;

        public void Order(string[] args)
        {
            _args = args;
        }

        public void Cancel()
        {
            _args = null;
        }

        public void StartProcessIfOrdered()
        {
            if (_args is null) return;

            var args = _args.Prepend($"--wait={Process.GetCurrentProcess().Id}");
            StartProcess(args);
            _args = null;
        }

        public static void StartProcess(IEnumerable<string> args)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.FileName = Environment.AssemblyLocation;
            foreach (var arg in args)
            {
                startInfo.ArgumentList.Add(arg);
            }
            Process.Start(startInfo);
        }
    }
}
