using NeeView.Susie;

namespace NeeView
{
    /// <summary>
    /// 画像プラグインアクセサ
    /// </summary>
    public class SusieImagePluginAccessor
    {
        private readonly SusiePluginManager _manager;

        public SusieImagePluginAccessor(SusiePluginManager manager, SusiePluginInfo? plugin)
        {
            _manager = manager;
            Plugin = plugin;
        }

        public SusiePluginInfo? Plugin { get; }


        public SusieImage? GetPicture(string fileName, byte[]? buff, bool isCheckExtension)
        {
            return _manager.GetImage(Plugin?.Name, fileName, buff, isCheckExtension);
        }
    }
}
