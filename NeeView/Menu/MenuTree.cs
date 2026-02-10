using NeeView.Collections.Generic;
using NeeView.Properties;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace NeeView
{
    public class MenuTree : TreeCollection<MenuElement>
    {
        public MenuTree(TreeListNode<MenuElement> root) : base(root)
        {
        }


        public ContextMenu? CreateContextMenu()
        {
            return MenuTreeTools.CreateContextMenu(Root);
        }

        public List<object> CreateContextMenuItems()
        {
            return MenuTreeTools.CreateContextMenuItems(Root);
        }

        public Menu? CreateMenu(bool isDefault)
        {
            return MenuTreeTools.CreateMenu(Root, isDefault);
        }

        public List<object> CreateMenuItems(bool isDefault)
        {
            return MenuTreeTools.CreateMenuItems(Root, isDefault);
        }

        public bool IsEqual(MenuTree other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return MenuTreeTools.IsEqual(Root, other.Root);
        }

        public void Validate()
        {
            MenuTreeTools.Validate(Root);
        }

        public void RaisePropertyChangedAll()
        {
            MenuTreeTools.RaisePropertyChangedAll(Root);
        }

        public MenuTree Clone()
        {
            return new MenuTree(Root.Clone());
        }

        public void Reset()
        {
            Root.Clear();

            var children = CreateDefault().Root.ToList();
            foreach (var node in children)
            {
                node.RemoveSelf();
                Root.Add(node);
            }
        }


        public static MenuTree CreateDefault()
        {
            var root = new TreeListNode<MenuElement>(new GroupMenuElement())
            {
                new TreeListNode<MenuElement>(new GroupMenuElement() { Name = TextResources.GetString("MenuTree.File") })
                {
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "LoadAs" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "Unload" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "LoadRecentBook" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "TogglePlaylistItem" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "OpenExplorer" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "OpenExternalAppAs" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "CutFile" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "CopyFile" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "Paste" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "CopyToFolderAs" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "MoveToFolderAs" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ExportImageAs" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "Print" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "DeleteFile" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "CloseApplication" }),
                },
                new TreeListNode<MenuElement>(new GroupMenuElement() { Name=TextResources.GetString("MenuTree.View") })
                {
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisibleBookshelf" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisiblePageList" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisibleBookmarkList" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisiblePlaylist" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisibleHistoryList" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisibleFileInfo" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisibleNavigator" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisibleEffectInfo" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisibleSideBar" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleHideLeftPanel" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleHideRightPanel" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisibleAddressBar" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleHideMenu" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisibleThumbnailList" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleHideThumbnailList" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleVisiblePageSlider" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleHidePageSlider" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleMainViewFloating" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleTopmost" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleFullScreen" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleSlideShow" }),
                },
                new TreeListNode<MenuElement>(new GroupMenuElement() { Name = TextResources.GetString("MenuTree.Image") })
                {
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetStretchModeNone" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetStretchModeUniform" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetStretchModeUniformToFill" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetStretchModeUniformToSize" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetStretchModeUniformToVertical" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetStretchModeUniformToHorizontal" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleStretchAllowScaleUp" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleStretchAllowScaleDown" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleNearestNeighbor" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetBackgroundBlack" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetBackgroundWhite" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetBackgroundAuto" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetBackgroundCheck" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetBackgroundCheckDark" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetBackgroundCustom" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleHoverScroll" }),
                },
                new TreeListNode<MenuElement>(new GroupMenuElement() { Name = TextResources.GetString("MenuTree.Jump") })
                {
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "PrevPage" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "NextPage" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "PrevOnePage" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "NextOnePage" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "PrevSizePage" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "NextSizePage" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "FirstPage" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "LastPage" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "PrevPlaylistItemInBook"}),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "NextPlaylistItemInBook" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "PrevBook" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "NextBook" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "PrevPlaylistItem"}),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "NextPlaylistItem" }),
                },
                new TreeListNode<MenuElement>(new GroupMenuElement() { Name = TextResources.GetString("MenuTree.Page") })
                {
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetPageModeOne" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetPageModeTwo" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleIsPanorama" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetPageOrientationHorizontal" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetPageOrientationVertical" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetBookReadOrderRight" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetBookReadOrderLeft" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleIsAutoRotateLeft" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleIsAutoRotateRight" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleIsAutoRotateForcedLeft" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleIsAutoRotateForcedRight" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleIsSupportedDividePage" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleIsSupportedWidePage" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleIsSupportedSingleFirstPage" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleIsSupportedSingleLastPage" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleIsRecursiveFolder" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetSortModeFileName" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetSortModeFileNameDescending" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetSortModeTimeStamp" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetSortModeTimeStampDescending" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetSortModeSize" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetSortModeSizeDescending" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetSortModeEntry" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetSortModeEntryDescending" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetSortModeRandom" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SetDefaultPageSetting" }),
                },
                new TreeListNode<MenuElement>(new GroupMenuElement() { Name = TextResources.GetString("MenuTree.Book") })
                {
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ToggleBookmark" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "SelectArchiver" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    //new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "OpenBookExplorer" }),
                    //new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "OpenBookExternalAppAs" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "CutBook" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "CopyBook" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "CopyBookToFolderAs" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "MoveBookToFolderAs" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "DeleteBook" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "RenameBook" }),
                },
                new TreeListNode<MenuElement>(new GroupMenuElement() { Name = TextResources.GetString("MenuTree.Option") })
                {
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "OpenOptionsWindow" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ReloadSetting" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ExportBackup"}),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "ImportBackup"}),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "OpenSettingFilesFolder" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "OpenScriptsFolder" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "OpenConsole" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "TogglePermitFile"}),
                },
                new TreeListNode<MenuElement>(new GroupMenuElement() { Name = TextResources.GetString("MenuTree.Help") })
                {
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "HelpMainMenu" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "HelpCommandList" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "HelpScript" }),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "HelpSearchOption" }),
                    new TreeListNode<MenuElement>(new SeparatorMenuElement()),
                    new TreeListNode<MenuElement>(new CommandMenuElement() { CommandName = "OpenVersionWindow" }),
                }
            };

            var tree = new MenuTree(root);

            // Appxは設定ファイル閲覧が無意味
            if (Environment.IsAppxPackage)
            {
                tree.RemoveCommand("OpenSettingFilesFolder");
            }

            CheckCommandEntry(tree);

            return tree;
        }


        private void RemoveCommand(string commandName)
        {
            var removes = this.Root.WalkChildren().Where(e => e.Value.MenuElementType == MenuElementType.Command && e.Value.CommandName == commandName).ToList();
            foreach (var node in removes)
            {
                node.RemoveSelf();
            }
        }

        [Conditional("DEBUG")]
        private static void CheckCommandEntry(MenuTree tree)
        {
            foreach (var node in tree.Root)
            {
                if (node.Value.MenuElementType == MenuElementType.Command)
                {
                    Debug.Assert(node.Value.CommandName is not null && CommandTable.Current.ContainsKey(node.Value.CommandName));
                }
            }
        }
    }
}
