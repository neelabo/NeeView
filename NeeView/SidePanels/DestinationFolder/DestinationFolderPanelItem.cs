namespace NeeView
{
    /// <summary>
    /// Destination-folder panel item; only managed entries have a shortcut number.
    /// </summary>
    /// <param name="Number">Managed display number; null for a current-folder subfolder.</param>
    /// <param name="Folder">Destination folder available for classification.</param>
    public sealed record DestinationFolderPanelItem(int? Number, DestinationFolder Folder)
    {
        /// <summary>
        /// Get the display name.
        /// </summary>
        public string Name => Folder.Name;

        /// <summary>
        /// Get the full path used by the tooltip.
        /// </summary>
        public string Path => Folder.Path;
    }
}
