using NeeView.Effects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    static class SidePanelFactory
    {
        public static List<IPanel> CreatePanels(params string[] keys)
        {
            return keys.Select(e => Create(e)).ToList();
        }

        public static IPanel Create(string key)
        {
            return key switch
            {
                nameof(FolderPanel) => new FolderPanel(BookshelfFolderList.Current),
                nameof(HistoryPanel) => new HistoryPanel(HistoryList.Current),
                nameof(FileInformationPanel) => new FileInformationPanel(FileInformation.Current),
                nameof(NavigatePanel) => new NavigatePanel(NavigateModel.Current),
                nameof(ImageEffectPanel) => new ImageEffectPanel(),
                nameof(BookmarkPanel) => new BookmarkPanel(BookmarkFolderList.Current),
                nameof(PageListPanel) => new PageListPanel(PageList.Current),
                nameof(PlaylistPanel) => new PlaylistPanel(PlaylistHub.Current),
                _ => throw new NotSupportedException(),
            };
        }
    }

}
