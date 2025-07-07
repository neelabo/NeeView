using System.Linq;

namespace NeeView
{
    [WordNodeMember]
    public class SusiePluginCollectionAccessor
    {
        private readonly SusiePluginManager _pluginManager;

        public SusiePluginCollectionAccessor()
        {
            _pluginManager = SusiePluginManager.Current;
        }

        [WordNodeMember]
        public SusiePluginAccessor[] INPlugins => _pluginManager.INPlugins.Select(e => new SusiePluginAccessor(e)).ToArray();

        [WordNodeMember]
        public SusiePluginAccessor[] AMPlugins => _pluginManager.AMPlugins.Select(e => new SusiePluginAccessor(e)).ToArray();

    }
}
