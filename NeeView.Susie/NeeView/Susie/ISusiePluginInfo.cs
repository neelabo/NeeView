using NeeLaboratory.Collections.Specialized;

namespace NeeView.Susie
{
    public interface ISusiePluginInfo
    {
        string? ApiVersion { get; set; }
        FileExtensionCollection? DefaultExtensions { get; set; }
        bool HasConfigurationDlg { get; set; }
        bool IsCacheEnabled { get; set; }
        bool IsEnabled { get; set; }
        bool IsPreExtract { get; set; }
        string Name { get; set; }
        string? PluginVersion { get; set; }
        FileExtensionCollection? UserExtensions { get; set; }
    }
}