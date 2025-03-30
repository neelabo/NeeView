using System;
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

        public void OpenExternalAppMenu(ICommandParameterFactory<ExternalApp> parameterFactory)
        {
            MainViewExternalAppTools.UpdateExternalAppMenu(_contextMenu.Items, parameterFactory);
            _contextMenu.IsOpen = true;
        }

        public void OpenCopyToFolderMenu(ICommandParameterFactory<DestinationFolder> parameterFactory)
        {
            MainViewCopyToFolderTools.UpdateCopyToFolderMenu(_contextMenu.Items, parameterFactory);
            _contextMenu.IsOpen = true;
        }

        public void OpenMoveToFolderMenu(ICommandParameterFactory<DestinationFolder> parameterFactory)
        {
            MainViewMoveToFolderTools.UpdateMoveToFolderMenu(_contextMenu.Items, parameterFactory);
            _contextMenu.IsOpen = true;
        }

        public void OpenSelectArchiverMenu()
        {
            BookCommandTools.UpdateSelectArchiverMenu(_contextMenu.Items);
            _contextMenu.IsOpen = true;
        }

        public void Close()
        {
            _contextMenu.IsOpen = false;
        }

    }
}
