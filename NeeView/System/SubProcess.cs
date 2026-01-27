using NeeLaboratory.Diagnostics;
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;

namespace NeeView
{
    public partial class SubProcess : IDisposable
    {
        private static readonly ProcessJobObject _processJobObject;

        static SubProcess()
        {
            _processJobObject = new ProcessJobObject();
        }

        private readonly string _filename;
        private readonly string _args;
        private Process? _process;


        public SubProcess(string path, string args)
        {
            _filename = path;
            _args = args;
        }


        [Subscribable]
        public event EventHandler? Exited;


        public Process? Process => _process;

        public bool IsActive => _process != null && !_process.HasExited;


        public void Start()
        {
            if (_disposedValue) return;
            if (IsActive) return;

            var psInfo = new ProcessStartInfo();
            psInfo.FileName = _filename;
            psInfo.Arguments = _args;
            psInfo.CreateNoWindow = true;
            psInfo.UseShellExecute = false;

            _process = Process.Start(psInfo) ?? throw new InvalidOperationException($"Cannot start process: {_filename}");
            _processJobObject.AddProcess(_process.Handle);
            _process.Exited += (s, e) => Exited?.Invoke(s, e);
            _process.EnableRaisingEvents = true;
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                Exited = null;

                if (_process != null && !_process.HasExited)
                {
                    _process.EnableRaisingEvents = false;
                    _process.Kill();
                }

                _disposedValue = true;
            }
        }

        ~SubProcess()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
