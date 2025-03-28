using System.Collections.Generic;

namespace NeeView.Susie.Server
{
    public interface ISusiePluginApi
    {
        int ConfigurationDlg(nint parent, int func);
        List<ArchiveFileInfoRaw>? GetArchiveInfo(string file);
        byte[]? GetFile(string file, int position);
        int GetFile(string file, int position, string extractFolder);
        byte[]? GetPicture(byte[] buff);
        byte[]? GetPicture(string filename);
        string? GetPluginInfo(int infono);
        bool IsExistFunction(string name);
        bool IsSupported(string filename);
        bool IsSupported(string filename, byte[] buff);
    }
}