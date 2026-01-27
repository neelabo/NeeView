// デザイン用データ

using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

#pragma warning disable CA1822

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class Thumbnail_Design
    {
        public BitmapImage BitmapSource => new(new Uri(@"E:\Work\test.png"));
        public bool IsUniqueImage => true;
    }

    /// <summary>
    /// 
    /// </summary>
    public class FolderItem_Design
    {
        public string Name => "A way to continue to a sunny afternoon afternoon market. The wagon trains a calf goby calf.";

        public bool IsDirectory => true;
        public bool IsShortcut => false;
        public bool IsOverlayChecked => true;
        public bool IsOverlayStar => true;
        public bool IsDisable => false;
        public bool IsPlaylist => true;

        public object ArchivePage => new
        {
            LastWriteTime = DateTime.Now,
            Length = 12345678L,
            Thumbnail = new Thumbnail_Design(),
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public class FolderListDataContext_Design
    {
        public object Model => new
        {
            PanelListItemStyle = PanelListItemStyle.Banner,
        };

        public object FolderCollection => new
        {
            Items = new List<FolderItem_Design>()
            {
                new FolderItem_Design(),
                new FolderItem_Design(),
            },
        };
    }
}
