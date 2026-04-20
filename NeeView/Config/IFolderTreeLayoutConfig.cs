using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// フォルダーツリーレイアウト設定
    /// </summary>
    public interface IFolderTreeLayoutConfig : INotifyPropertyChanged
    {
        bool IsFolderTreeVisible { get; set; }
        FolderTreeLayout FolderTreeLayout { get; set; }
        double FolderTreeAreaWidth { get; set; }
        double FolderTreeAreaHeight { get; set; }
    }
}

