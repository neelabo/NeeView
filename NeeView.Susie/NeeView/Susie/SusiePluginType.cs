namespace NeeView.Susie
{
    /// <summary>
    /// Susieプラグインの種類
    /// </summary>
    public enum SusiePluginType
    {
        None,
        Image,
        Archive,
    }


    public static class SusiePluginTypeExtensions
    {
        /// <summary>
        /// APIバージョンからプラグインの種類を取得します
        /// </summary>
        /// <param name="apiVersion"></param>
        /// <returns></returns>
        public static SusiePluginType FromApiVersion(string? apiVersion)
        {
            return apiVersion switch
            {
                "00IN" => SusiePluginType.Image,
                "00AM" => SusiePluginType.Archive,
                _ => SusiePluginType.None,
            };
        }
    }
}
