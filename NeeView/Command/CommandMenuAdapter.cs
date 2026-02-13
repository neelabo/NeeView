using System.Windows.Controls;

namespace NeeView
{
    public class CommandMenuAdapter
    {
        private readonly ContextMenu _contextMenu;

        public CommandMenuAdapter(ContextMenu contextMenu)
        {
            _contextMenu = contextMenu;
        }


        public void OpenExternalAppMenu(ExternalAppMenuFactory menuFactory)
        {
            menuFactory.UpdateFolderMenu(_contextMenu.Items);
            _contextMenu.IsOpen = true;
        }

        public void OpenDestinationFolderMenu(DestinationFolderMenuFactory menuFactory)
        {
            menuFactory.UpdateFolderMenu(_contextMenu.Items);
            _contextMenu.IsOpen = true;
        }

        public void OpenSelectArchiverMenu()
        {
            BookCommandTools.UpdateSelectArchiverMenu(_contextMenu.Items);
            _contextMenu.IsOpen = true;
        }

        public void OpenRecentBookMenu()
        {
            RecentBookTools.UpdateRecentBookMenu(_contextMenu.Items);
            _contextMenu.IsOpen = true;
        }

        public void Close()
        {
            _contextMenu.IsOpen = false;
        }

    }
}
