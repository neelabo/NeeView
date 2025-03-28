using System;
using System.Collections.Generic;

namespace NeeView.Susie.Server
{
    public class SusiePluginApiAdapter : IDisposable, ISusiePluginApi
    {
        private readonly Locker.Key _key;
        private readonly SusiePluginApi _api;
        private bool _disposedValue;

        public SusiePluginApiAdapter(Locker.Key key, SusiePluginApi api)
        {
            _key = key;
            _api = api;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _key.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public int ConfigurationDlg(nint parent, int func)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiAdapter));
            return _api.ConfigurationDlg(parent, func);
        }

        public List<ArchiveFileInfoRaw>? GetArchiveInfo(string file)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiAdapter));
            return _api.GetArchiveInfo(file);
        }

        public byte[]? GetFile(string file, int position)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiAdapter));
            return _api.GetFile(file, position);
        }

        public int GetFile(string file, int position, string extractFolder)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiAdapter));
            return _api.GetFile(file, position, extractFolder);
        }

        public byte[]? GetPicture(byte[] buff)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiAdapter));
            return _api.GetPicture(buff);
        }

        public byte[]? GetPicture(string filename)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiAdapter));
            return _api.GetPicture(filename);
        }

        public string? GetPluginInfo(int infono)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiAdapter));
            return _api.GetPluginInfo(infono);
        }

        public bool IsExistFunction(string name)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiAdapter));
            return _api.IsExistFunction(name);
        }

        public bool IsSupported(string filename)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiAdapter));
            return _api.IsSupported(filename);
        }

        public bool IsSupported(string filename, byte[] buff)
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiAdapter));
            return _api.IsSupported(filename, buff);
        }
    }

}
