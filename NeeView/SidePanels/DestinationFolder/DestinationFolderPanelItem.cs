namespace NeeView
{
    /// <summary>
    /// 目标文件夹面板中的一条分类目录；仅手动管理项带数字映射。
    /// </summary>
    /// <param name="Number">手动管理项的显示序号；当前目录子文件夹为 null</param>
    /// <param name="Folder">可接收当前图片的目标文件夹</param>
    public sealed record DestinationFolderPanelItem(int? Number, DestinationFolder Folder)
    {
        /// <summary>
        /// 获取用于按钮显示的名称。
        /// </summary>
        public string Name => Folder.Name;

        /// <summary>
        /// 获取用于提示的完整路径。
        /// </summary>
        public string Path => Folder.Path;
    }
}
