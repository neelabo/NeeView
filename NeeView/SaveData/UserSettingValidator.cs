using NeeView.Runtime.LayoutPanel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace NeeView
{
    public static class UserSettingValidator
    {
#pragma warning disable CS0612, CS0618 // 型またはメンバーが旧型式です

        /// <summary>
        /// 互換性処理
        /// </summary>
        public static UserSetting Validate(this UserSetting self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("UserSetting.Format must not be null.");
            if (self.Config is null) throw new FormatException("UserSetting.Config must not be null.");
            if (self.Commands is null) throw new FormatException("UserSetting.Commands must not be null.");

            // 読み込まれた設定フラグを立てる
            self.Config.System.IsLoadedSettings = true;

            // 画像拡張子初期化
            if (self.Config.Image.Standard.SupportFileTypes is null)
            {
                self.Config.Image.Standard.SupportFileTypes = PictureFileExtensionTools.CreateDefaultSupportedFileTypes(self.Config.Image.Standard.UseWicInformation);
            }

#if !DEBUG
            // 現在のバージョンであればチェック不要
            if (self.Format.Equals(new FormatVersion(Environment.SolutionName)))
            {
                return self;
            }
#endif

            // ver.38
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 38, 0, 0)) < 0)
            {
                // NOTE: Config.Panels.PanelDocks は継承しません。

                self.Commands?.ValidateRename(CommandNameValidator.RenameMap_38_0_0);
                self.ContextMenu?.ValidateRename(CommandNameValidator.RenameMap_38_0_0);
            }

            // ver.39
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 39, 0, 0)) < 0)
            {
                if (self.Config.Panels.FontName_Legacy != default)
                {
                    self.Config.Fonts.FontName = self.Config.Panels.FontName_Legacy;
                }
                if (self.Config.Panels.FontSize_Legacy != default)
                {
                    self.Config.Fonts.PanelFontScale = self.Config.Panels.FontSize_Legacy / SystemVisualParameters.Current.MessageFontSize;
                }
                if (self.Config.Panels.FolderTreeFontSize_Legacy != default)
                {
                    self.Config.Fonts.FolderTreeFontScale = self.Config.Panels.FolderTreeFontSize_Legacy / SystemVisualParameters.Current.MessageFontSize;
                }

                self.Config.Panels.Layout?.ValidateRename("PagemarkPanel", nameof(PlaylistPanel));

                switch (self.Config.System.Language)
                {
                    case "English":
                        self.Config.System.Language = "en";
                        break;
                    case "Japanese":
                        self.Config.System.Language = "ja";
                        break;
                }

                if (self.Config.BookSetting.SortMode == PageSortMode.FileName)
                {
                    self.Config.BookSetting.SortMode = PageSortMode.Entry;
                }
                if (self.Config.BookSetting.SortMode == PageSortMode.FileNameDescending)
                {
                    self.Config.BookSetting.SortMode = PageSortMode.EntryDescending;
                }

                self.Commands?.ValidateRename(CommandNameValidator.RenameMap_39_0_0);
                self.ContextMenu?.ValidateRename(CommandNameValidator.RenameMap_39_0_0);
            }

            // ver.40
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 40, 0, 0)) < 0)
            {
                switch (self.Config.System.Language)
                {
                    case "zh-TW":
                        self.Config.System.Language = "zh-Hant";
                        break;
                    case "zh-CN":
                        self.Config.System.Language = "zh-Hans";
                        break;
                }

                // 水平ホイールのコマンド入れ替え初期化
                self.Config.Command.IsReversePageMoveHorizontalWheel = self.Config.Command.IsReversePageMoveWheel;

                // コマンド関係
                if (self.Commands != null)
                {
                    // メインビューウィンドウ切り替えコマンドにショートカットキーF12を設定
                    var gestureF12 = new KeyGestureSource(Key.F12);
                    if (self.Commands.TryGetValue("ToggleMainViewFloating", out var toggleMainViewFloating) && toggleMainViewFloating.ShortCutKey.IsEmpty && !self.Commands.Any(e => e.Value.ShortCutKey.Gestures.Contains(gestureF12)))
                    {
                        toggleMainViewFloating.ShortCutKey = new ShortcutKey(new List<InputGestureSource>() { gestureF12 });
                    }
                }

                // Drag actions
                if (self.DragActions != null)
                {
                    if (self.DragActions.TryGetValue("MoveScale", out var dragAction) && dragAction.Parameter is SensitiveDragActionParameter sensitiveParameter)
                    {
                        dragAction.Parameter = new MoveScaleDragActionParameter() { Sensitivity = sensitiveParameter.Sensitivity };
                    }
                }
            }

            // ver.40.3
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 40, 3, 0)) < 0)
            {
                // ダミーページ挿入：40.0以降は挙動が同じになるような設定にする
                if (self.Config.Book.IsInsertDummyPage && self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 40, 0, 0)) >= 0)
                {
                    self.Config.Book.IsInsertDummyFirstPage = true;
                    self.Config.Book.IsInsertDummyLastPage = true;
                }
            }

            // ver.40.5
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 40, 5, 0)) < 0)
            {
                if (self.Commands != null)
                {
                    // ver 40.0 前のデータならば TogglePageModeReverse の設定リセット
                    if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 40, 0, 0)) < 0)
                    {
                        var togglePageModeReverseCommand = new TogglePageModeReverseCommand();
                        var togglePageModeReverse = togglePageModeReverseCommand.CreateMemento();
                        self.Commands[togglePageModeReverseCommand.Name] = togglePageModeReverseCommand.CreateMemento();
                    }
                    // ver 40.0 以降のデータならば TogglePageMode/Reverse のパラメータ設定がないのであればループフラグを無効化
                    else
                    {
                        if (self.Commands.TryGetValue("TogglePageMode", out var togglePageMode))
                        {
                            if (togglePageMode.Parameter is null)
                            {
                                togglePageMode.Parameter = new TogglePageModeCommandParameter() { IsLoop = false };
                            }
                        }
                        if (self.Commands.TryGetValue("TogglePageModeReverse", out var togglePageModeReverse))
                        {
                            if (togglePageModeReverse.Parameter is null)
                            {
                                togglePageModeReverse.Parameter = new TogglePageModeCommandParameter() { IsLoop = false };
                            }
                        }
                    }
                }
            }

            // ver.41.0
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 41, 0, 0)) < 0)
            {
                if (self.Commands != null)
                {
                    // 新規追加されたAutoScrollOnCommand のショートカットの衝突を解決する
                    ResolveCommandShortCutKeyConflicts(self.Commands, new AutoScrollOnCommand());
                }
            }

            // ver 42.0
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 42, 0, 6)) < 0)
            {
                // コピーパラメータのシステム化
                if (self.Commands != null)
                {
                    if (self.Commands.TryGetValue("CopyFile", out var copyFile))
                    {
                        if (copyFile.Parameter is CopyFileCommandParameter parameter)
                        {
                            self.Config.System.ArchiveCopyPolicy = parameter.ArchivePolicy_Legacy;
                            self.Config.System.TextCopyPolicy = parameter.TextCopyPolicy_Legacy;
                        }
                    }
                }
            }

            // ver 43.0
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 43, 0, 3661)) < 0)
            {
                if (self.Commands != null)
                {
                    ResolveCommandShortCutKeyConflicts(self.Commands, new CutFileCommand());
                }
            }

            // ver 45.0
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 45, 0, 3973)) < 0)
            {
                if (self.Config.Panels.TryGetIsVisibleItemsCount(out var isVisible))
                {
                    self.Config.Bookshelf.IsVisibleItemsCount = isVisible;
                    self.Config.Bookmark.IsVisibleItemsCount = isVisible;
                    self.Config.PageList.IsVisibleItemsCount = isVisible;
                    self.Config.History.IsVisibleItemsCount = isVisible;
                }
            }


            // NOTE: ver.99 (バージョン変更処理テスト)
#if false
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 99, 0, 0)) < 0)
            {
                self.Commands?.ValidateRename(CommandNameValidator.RenameMap_99_0_0);
                self.ContextMenu?.ValidateRename(CommandNameValidator.RenameMap_99_0_0);
            }
#endif

            return self;
        }


        /// <summary>
        /// CommandCollection に含まれているショートカットキーと衝突しないように新しいコマンドを登録する
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="command">登録するコマンド</param>
        private static void ResolveCommandShortCutKeyConflicts(CommandCollection commands, CommandElement command)
        {
            if (commands.ContainsKey(command.Name)) return;
            if (command.ShortCutKey.IsEmpty) return;

            command.ShortCutKey = ResolveShortCutKeyConflicts(commands, command.ShortCutKey);
            commands[command.Name] = command.CreateMemento();
        }

        /// <summary>
        /// CommandCollection に含まれているショートカットキーを除外したショートカットキー列を作る
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="shortCutKey">要求するショートカットキー</param>
        /// <returns></returns>
        private static ShortcutKey ResolveShortCutKeyConflicts(CommandCollection commands, ShortcutKey shortCutKey)
        {
            if (shortCutKey.IsEmpty) return ShortcutKey.Empty;

            return new ShortcutKey(shortCutKey.Gestures.Where(e => !ContainsShortcutGesture(commands, e)));
        }

        /// <summary>
        /// CommandCollection に特定のショートカットキー設定が含まれているかをチェックする
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="gesture">判定するショートカットキー</param>
        /// <returns></returns>
        private static bool ContainsShortcutGesture(CommandCollection commands, InputGestureSource gesture)
        {
            return commands.Values
                .Where(e => !e.ShortCutKey.IsEmpty)
                .Any(e => e.ShortCutKey.Gestures.Contains(gesture));
        }

#pragma warning restore CS0612, CS0618 // 型またはメンバーが旧型式です

    }


    /// <summary>
    /// コマンド名変更対応
    /// </summary>
    public static class CommandNameValidator
    {
        public static Dictionary<string, string> RenameMap_38_0_0 { get; } = new Dictionary<string, string>()
        {
            ["TogglePermitFileCommand"] = "TogglePermitFile",
            ["FocusPrevAppCommand"] = "FocusPrevApp",
            ["FocusNextAppCommand"] = "FocusNextApp",
        };

        public static Dictionary<string, string> RenameMap_39_0_0 { get; } = new Dictionary<string, string>()
        {
            ["ToggleVisiblePagemarkList"] = "ToggleVisiblePlaylist",
            ["TogglePagemark"] = "TogglePlaylistMark",
            ["PrevPagemark"] = "PrevPlaylistItem",
            ["NextPagemark"] = "NextPlaylistItem",
            ["PrevPagemarkInBook"] = "PrevPlaylistItemInBook",
            ["NextPagemarkInBook"] = "NextPlaylistItemInBook",
        };

        // NOTE: バージョン変更処理テスト用
#if false
        public static Dictionary<string, string> RenameMap_99_0_0 { get; } = new Dictionary<string, string>()
        {
            ["ExportImageAs"] = "XXX_ExportImageAs",
        };
#endif


        /// <summary>
        /// CommandCollection のコマンド名変更
        /// </summary>
        public static void ValidateRename(this CommandCollection commandCollection, Dictionary<string, string> renameMap)
        {
            foreach (var pair in renameMap)
            {
                Rename(pair.Key, pair.Value);
            }

            void Rename(string oldName, string newName)
            {
                foreach (var command in commandCollection.ToList())
                {
                    var nameSource = CommandNameSource.Parse(command.Key);
                    if (nameSource.Name == oldName)
                    {
                        commandCollection.TryAdd(new CommandNameSource(newName, nameSource.Number).FullName, command.Value);
                        commandCollection.Remove(command.Key);
                    }
                }
            }
        }

        /// <summary>
        /// ContextMenu のコマンド名変更
        /// </summary>
        public static void ValidateRename(this MenuNode contextMenu, Dictionary<string, string> renameMap)
        {
            foreach (var pair in renameMap)
            {
                Rename(pair.Key, pair.Value);
            }

            void Rename(string oldName, string newName)
            {
                foreach (var node in contextMenu.GetEnumerator().Where(e => e.MenuElementType == MenuElementType.Command))
                {
                    Debug.Assert(node.CommandName != null);
                    var nameSource = CommandNameSource.Parse(node.CommandName);
                    if (nameSource.Name == oldName)
                    {
                        node.CommandName = new CommandNameSource(newName, nameSource.Number).FullName;
                    }
                }
            }
        }
    }

    /// <summary>
    /// LayoutPanel 名前変更対応
    /// </summary>
    public static class LayoutPanelValidator
    {
        public static void ValidateRename(this LayoutPanelManager.Memento self, string oldName, string newName)
        {
            if (self is null) return;

            if (self.Panels != null && self.Panels.TryGetValue(oldName, out var value))
            {
                self.Panels.Remove(oldName);
                self.Panels.Add(newName, value);
            }

            if (self.Docks != null)
            {
                foreach (var dock in self.Docks.Values)
                {
                    dock.SelectedItem = dock.SelectedItem == oldName ? newName : dock.SelectedItem;

#pragma warning disable CS0612 // 型またはメンバーが旧型式です
                    if (dock.Panels is not null)
                    {
                        dock.Panels = dock.Panels
                            .Select(e => e.Select(x => x == oldName ? newName : x).ToList())
                            .ToList();
                    }
#pragma warning restore CS0612 // 型またはメンバーが旧型式です

                    foreach (var panelLayout in dock.PanelLayout)
                    {
                        panelLayout.Panels = panelLayout.Panels
                            .Select(x => x == oldName ? newName : x).ToList()
                            .ToList();
                    }
                }
            }

            if (self.Windows?.Panels != null)
            {
                self.Windows.Panels = self.Windows.Panels
                    .Select(x => x == oldName ? newName : x)
                    .ToList();
            }
        }

    }

}
