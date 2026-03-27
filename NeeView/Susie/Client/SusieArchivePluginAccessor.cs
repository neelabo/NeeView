using NeeView.Susie;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// 書庫プラグインアクセサ
    /// </summary>
    public class SusieArchivePluginAccessor
    {
        private readonly SusiePluginManager _manager;

        public SusieArchivePluginAccessor(SusiePluginManager manager, SusiePluginInfo plugin)
        {
            _manager = manager;
            Plugin = plugin;
        }

        public SusiePluginInfo Plugin { get; }

        public List<SusieArchiveEntry> GetArchiveEntries(string fileName)
        {
            return _manager.GetArchiveEntries(Plugin.Name, fileName);
        }

        public byte[] ExtractArchiveEntry(string fileName, int position)
        {
            return _manager.ExtractArchiveEntry(Plugin.Name, fileName, position);
        }

        public void ExtractArchiveEntryToFolder(string fileName, int position, string extractFolder)
        {
            _manager.ExtractArchiveEntryToFolder(Plugin.Name, fileName, position, extractFolder);
        }
    }
}
