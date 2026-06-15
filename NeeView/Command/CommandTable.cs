using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.Text;
using NeeView.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

// TODO: コマンド引数にコマンドパラメータを渡せないだろうか。（現状メニュー呼び出しであることを示すタグが指定されることが有る)

namespace NeeView
{
    // Typo: InputScheme ... 保存データの互換性を確認後に修正
    public enum InputScheme
    {
        TypeA, // 標準
        TypeB, // ホイールでページ送り
        TypeC, // クリックでページ送り
    };


    /// <summary>
    /// コマンド設定テーブル
    /// </summary>
    public partial class CommandTable : ObservableObject, IReadOnlyDictionary<string, CommandElement>
    {
        static CommandTable() => Current = new CommandTable();
        public static CommandTable Current { get; }


        private Dictionary<string, CommandElement> _elements;
        private Dictionary<string, string> _pairs;
        private CommandCollection _baseCollection;
        private DefaultCommandCollection _defaultCollection;


        private CommandTable()
        {
            InitializeCommandTable();

            Changed += CommandTable_Changed;
        }


        /// <summary>
        /// コマンドテーブルが変更された
        /// </summary>
        [Subscribable]
        public event EventHandler<CommandChangedEventArgs>? Changed;


        public int ChangeCount { get; private set; }

        public Dictionary<string, ObsoleteCommandItem> AlternativeCommands { get; private set; }

        public Dictionary<string, ObsoleteCommandItem> ObsoleteCommands { get; private set; }


        #region IReadOnlyDictionary Support

        public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, CommandElement>)_elements).Keys;

        public IEnumerable<CommandElement> Values => ((IReadOnlyDictionary<string, CommandElement>)_elements).Values;

        public int Count => ((IReadOnlyCollection<KeyValuePair<string, CommandElement>>)_elements).Count;

        public CommandElement this[string key] => ((IReadOnlyDictionary<string, CommandElement>)_elements)[key];

        public bool ContainsKey(string key)
        {
            return ((IReadOnlyDictionary<string, CommandElement>)_elements).ContainsKey(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out CommandElement value)
        {
            return ((IReadOnlyDictionary<string, CommandElement>)_elements).TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, CommandElement>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, CommandElement>>)_elements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_elements).GetEnumerator();
        }

        #endregion

        #region Methods: Initialize

        /// <summary>
        /// コマンドテーブル初期化
        /// </summary>
        [MemberNotNull(nameof(_elements), nameof(_pairs), nameof(_baseCollection), nameof(_defaultCollection), nameof(AlternativeCommands), nameof(ObsoleteCommands))]
        private void InitializeCommandTable()
        {
#pragma warning disable CS0612 // 型またはメンバーが旧型式です
            var list = new List<CommandElement>()
            {
                new LoadAsCommand(),
                new LoadRecentBookCommand(),
                new ReLoadCommand(),
                new UnloadCommand(),

                new OpenExplorerCommand(),
                new OpenExternalAppCommand(),
                new OpenExternalAppAsCommand(),
                new CutFileCommand(),
                new CopyFileCommand(),
                new CopyImageCommand(),
                new PasteCommand(),
                new CopyToFolderAsCommand(),
                new MoveToFolderAsCommand(),

                new ExportImageAsCommand(),
                new ExportImageCommand(),
                new ExportBookAsCommand(),
                new PrintCommand(),
                new DeleteFileCommand(),

                new OpenBookExplorerCommand(),
                new OpenBookExternalAppAsCommand(),
                new CutBookCommand(),
                new CopyBookCommand(),
                new CopyBookToFolderAsCommand(),
                new MoveBookToFolderAsCommand(),
                new DeleteBookCommand(),
                new RenameBookCommand(),

                new SelectArchiverCommand(),

                new ClearHistoryCommand(),
                new ClearHistoryInPlaceCommand(),
                new RemoveUnlinkedHistoryCommand(),

                new ToggleStretchModeCommand(),
                new ToggleStretchModeReverseCommand(),
                new SetStretchModeNoneCommand(),
                new SetStretchModeUniformCommand(),
                new SetStretchModeUniformToFillCommand(),
                new SetStretchModeUniformToSizeCommand(),
                new SetStretchModeUniformToVerticalCommand(),
                new SetStretchModeUniformToHorizontalCommand(),
                new ToggleStretchAllowScaleUpCommand(),
                new ToggleStretchAllowScaleDownCommand(),
                new ToggleNearestNeighborCommand(),
                new ToggleBackgroundCommand(),
                new SetBackgroundBlackCommand(),
                new SetBackgroundWhiteCommand(),
                new SetBackgroundAutoCommand(),
                new SetBackgroundCheckCommand(),
                new SetBackgroundCheckDarkCommand(),
                new SetBackgroundCustomCommand(),
                new ToggleTopmostCommand(),
                new ToggleVisibleAddressBarCommand(),
                new ToggleHideMenuCommand(),
                new ToggleVisibleSideBarCommand(),
                new ToggleHidePanelCommand(),
                new ToggleHideLeftPanelCommand(),
                new ToggleHideRightPanelCommand(),
                new ToggleVisiblePageSliderCommand(),
                new ToggleHidePageSliderCommand(),

                new ToggleVisibleBookshelfCommand(),
                new ToggleVisiblePageListCommand(),
                new ToggleVisibleBookmarkListCommand(),
                new ToggleVisiblePlaylistCommand(),
                new ToggleVisibleHistoryListCommand(),
                new ToggleVisibleFileInfoCommand(),
                new ToggleVisibleNavigatorCommand(),
                new ToggleVisibleEffectInfoCommand(),
                new ToggleVisibleFoldersTreeCommand(),
                new ToggleVisibleContentsTreeCommand(),
                new FocusFolderSearchBoxCommand(),
                new FocusBookmarkSearchBoxCommand(),
                new FocusPageListSearchBoxCommand(),
                new FocusHistorySearchBoxCommand(),
                new FocusBookmarkListCommand(),
                new FocusMainViewCommand(),
                new ToggleVisibleFilmStripCommand(),
                new ToggleHideFilmStripCommand(),
                new ToggleMainViewFloatingCommand(),

                new ToggleFullScreenCommand(),
                new SetFullScreenCommand(),
                new CancelFullScreenCommand(),
                new ToggleFullDesktopCommand(),
                new ToggleWindowMinimizeCommand(),
                new ToggleWindowMaximizeCommand(),
                new ShowHiddenPanelsCommand(),

                new ToggleSlideShowCommand(),

                new ViewScrollNTypeUpCommand(),
                new ViewScrollNTypeDownCommand(),
                new ViewScrollUpCommand(),
                new ViewScrollDownCommand(),
                new ViewScrollLeftCommand(),
                new ViewScrollRightCommand(),
                new ViewPresetScrollCommand(),
                new ViewScaleUpCommand(),
                new ViewScaleDownCommand(),
                new ViewScaleStretchCommand(),
                new ViewBaseScaleUpCommand(),
                new ViewBaseScaleDownCommand(),
                new ViewRotateLeftCommand(),
                new ViewRotateRightCommand(),
                new ToggleIsAutoRotateLeftCommand(),
                new ToggleIsAutoRotateRightCommand(),
                new ToggleIsAutoRotateForcedLeftCommand(),
                new ToggleIsAutoRotateForcedRightCommand(),

                new ToggleViewFlipHorizontalCommand(),
                new ViewFlipHorizontalOnCommand(),
                new ViewFlipHorizontalOffCommand(),

                new ToggleViewFlipVerticalCommand(),
                new ViewFlipVerticalOnCommand(),
                new ViewFlipVerticalOffCommand(),
                new ViewResetCommand(),

                new PrevPageCommand(),
                new NextPageCommand(),
                new PrevOnePageCommand(),
                new NextOnePageCommand(),

                new PrevScrollPageCommand(),
                new NextScrollPageCommand(),
                new JumpPageCommand(),
                new JumpRandomPageCommand(),
                new PrevSizePageCommand(),
                new NextSizePageCommand(),
                new PrevFolderPageCommand(),
                new NextFolderPageCommand(),
                new FirstPageCommand(),
                new LastPageCommand(),
                new PrevHistoryPageCommand(),
                new NextHistoryPageCommand(),

                new ToggleBookLockCommand(),
                new PrevBookCommand(),
                new NextBookCommand(),
                new RandomBookCommand(),
                new PrevHistoryCommand(),
                new NextHistoryCommand(),

                new PrevBookHistoryCommand(),
                new NextBookHistoryCommand(),
                new MoveToParentBookCommand(),
                new MoveToChildBookCommand(),

                new ToggleMediaPlayCommand(),
                new PrevMediaPositionCommand(),
                new NextMediaPositionCommand(),

                new ToggleBookOrderCommand(),
                new SetBookOrderByFileNameACommand(),
                new SetBookOrderByFileNameDCommand(),
                new SetBookOrderByPathACommand(),
                new SetBookOrderByPathDCommand(),
                new SetBookOrderByFileTypeACommand(),
                new SetBookOrderByFileTypeDCommand(),
                new SetBookOrderByTimeStampACommand(),
                new SetBookOrderByTimeStampDCommand(),
                new SetBookOrderByEntryTimeACommand(),
                new SetBookOrderByEntryTimeDCommand(),
                new SetBookOrderBySizeACommand(),
                new SetBookOrderBySizeDCommand(),
                new SetBookOrderByRandomCommand(),
                new TogglePageModeCommand(),
                new TogglePageModeReverseCommand(),
                new SetPageModeOneCommand(),
                new SetPageModeTwoCommand(),
                new ToggleIsPanoramaCommand(),
                new TogglePageOrientationCommand(),
                new SetPageOrientationHorizontalCommand(),
                new SetPageOrientationVerticalCommand(),
                new ToggleBookReadOrderCommand(),
                new SetBookReadOrderRightCommand(),
                new SetBookReadOrderLeftCommand(),
                new ToggleIsSupportedDividePageCommand(),
                new ToggleIsSupportedWidePageCommand(),
                new ToggleIsSupportedSingleFirstPageCommand(),
                new ToggleIsSupportedSingleLastPageCommand(),
                new ToggleIsRecursiveFolderCommand(),
                new ToggleSortModeCommand(),
                new SetSortModeFileNameCommand(),
                new SetSortModeFileNameDescendingCommand(),
                new SetSortModeTimeStampCommand(),
                new SetSortModeTimeStampDescendingCommand(),
                new SetSortModeSizeCommand(),
                new SetSortModeSizeDescendingCommand(),
                new SetSortModeEntryCommand(),
                new SetSortModeEntryDescendingCommand(),
                new SetSortModeRandomCommand(),
                new SetDefaultPageSettingCommand(),

                new ToggleBookmarkCommand(),
                new RegisterBookmarkCommand(),

                new NextPlaylistCommand(),
                new PrevPlaylistCommand(),
                new TogglePlaylistItemCommand(),
                new PrevPlaylistItemCommand(),
                new NextPlaylistItemCommand(),
                new PrevPlaylistItemInBookCommand(),
                new NextPlaylistItemInBookCommand(),

                new SetEffectProfileCommand(),
                new NextEffectProfileCommand(),
                new PrevEffectProfileCommand(),
                new ToggleCustomSizeCommand(),
                new ToggleTrimCommand(),
                new ToggleResizeFilterCommand(),
                new ToggleGridCommand(),
                new ToggleEffectCommand(),

                new ToggleIsLoupeCommand(),
                new LoupeOnCommand(),
                new LoupeOffCommand(),
                new LoupeScaleUpCommand(),
                new LoupeScaleDownCommand(),

                new ToggleHoverScrollCommand(),
                new ToggleAutoScrollCommand(),

                // script
                new CancelScriptCommand(),

                // other
                new OpenOptionsWindowCommand(),
                new OpenSettingFilesFolderCommand(),
                new OpenScriptsFolderCommand(),
                new OpenVersionWindowCommand(),
                new CloseApplicationCommand(),

                new TogglePermitFileCommand(),

                new HelpCommandListCommand(),
                new HelpScriptCommand(),
                new HelpMainMenuCommand(),
                new HelpSearchOptionCommand(),
                new OpenContextMenuCommand(),

                new ExportBackupCommand(),
                new ImportBackupCommand(),
                new ReloadSettingCommand(),
                new SaveSettingCommand(),
                new TouchEmulateCommand(),

                new FocusPrevAppCommand(),
                new FocusNextAppCommand(),

                new StretchWindowCommand(),

                new OpenConsoleCommand()
            };
#pragma warning restore CS0612 // 型またはメンバーが旧型式です

            // command list order
            foreach (var item in list.GroupBy(e => e.Group).SelectMany(e => e).Select((e, index) => (e, index)))
            {
                item.e.Order = item.index;
            }

            // to dictionary
            _elements = list.ToDictionary(e => e.Name);

            // share
            _elements["NextPage"].SetShare(_elements["PrevPage"]);
            _elements["NextOnePage"].SetShare(_elements["PrevOnePage"]);
            _elements["NextScrollPage"].SetShare(_elements["PrevScrollPage"]);
            _elements["NextSizePage"].SetShare(_elements["PrevSizePage"]);
            _elements["NextFolderPage"].SetShare(_elements["PrevFolderPage"]);
            _elements["LastPage"].SetShare(_elements["FirstPage"]);
            _elements["TogglePageModeReverse"].SetShare(_elements["TogglePageMode"]);
            _elements["ToggleStretchModeReverse"].SetShare(_elements["ToggleStretchMode"]);
            _elements["SetStretchModeUniformToFill"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToSize"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToVertical"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToHorizontal"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["NextPlaylistItemInBook"].SetShare(_elements["PrevPlaylistItemInBook"]);
            _elements["ViewScrollNTypeDown"].SetShare(_elements["ViewScrollNTypeUp"]);
            _elements["ViewScrollDown"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScrollLeft"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScrollRight"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScaleDown"].SetShare(_elements["ViewScaleUp"]);
            _elements["ViewBaseScaleDown"].SetShare(_elements["ViewBaseScaleUp"]);
            _elements["ViewRotateRight"].SetShare(_elements["ViewRotateLeft"]);

#if DEBUG
            // PairPartner 整合性チェック
            foreach (var element in _elements)
            {
                if (element.Value.PairPartner is not null)
                {
                    var pair0 = element.Value;
                    var pair1 = _elements[pair0.PairPartner];
                    Debug.Assert(pair0.Parameter is ReversibleCommandParameter);
                    Debug.Assert(pair1.PairPartner == element.Key);
                    Debug.Assert(pair1.Parameter?.GetType() == pair0.Parameter?.GetType());
                }
            }
#endif

            // PairPartner 情報収集
            _pairs = new();
            foreach (var element in _elements)
            {
                if (element.Value.PairPartner != null)
                {
                    if (!_pairs.ContainsKey(element.Value.PairPartner))
                    {
                        _pairs.Add(element.Key, element.Value.PairPartner);
                    }
                }
            }

            // コマンド毎のデフォルト設定の生成
            foreach (var element in _elements.Values)
            {
                element.CreateDefaultMemento();
            }

            // デフォルト設定として記憶
            var collection = new CommandCollection();
            foreach (var element in _elements)
            {
                collection.Add(element.Key, element.Value.DefaultMemento);
            }
            _baseCollection = collection;
            _defaultCollection = new DefaultCommandCollection(InputScheme.TypeA, PageReadOrder.RightToLeft, _baseCollection);

            // 廃棄されたコマンドの情報。代替コマンドあり。
            var alternativeCommands = new List<ObsoleteCommandItem>()
            {
                new ObsoleteCommandItem("AutoScrollOn", "ToggleAutoScroll", 46),
                new ObsoleteCommandItem("ToggleVisibleThumbnailList", "ToggleVisibleFilmStrip", 46),
                new ObsoleteCommandItem("ToggleHideThumbnailList", "ToggleHideFilmStrip", 46),
            };
            AlternativeCommands = alternativeCommands.ToDictionary(e => e.Obsolete);

            // 廃棄されたコマンドの情報
            var obsoleteCommands = new List<ObsoleteCommandItem>()
            {
                new ObsoleteCommandItem("ToggleVisibleTitleBar", null, 39),
                new ObsoleteCommandItem("ToggleVisiblePagemarkList", "ToggleVisiblePlaylist", 39),
                new ObsoleteCommandItem("TogglePagemark", "TogglePlaylistMark", 39),
                new ObsoleteCommandItem("PrevPagemark", "PrevPlaylistItem", 39),
                new ObsoleteCommandItem("NextPagemark", "NextPlaylistItem", 39),
                new ObsoleteCommandItem("PrevPagemarkInBook", "PrevPlaylistItemInBook", 39),
                new ObsoleteCommandItem("NextPagemarkInBook", "NextPlaylistItemInBook", 39),
            };
            ObsoleteCommands = obsoleteCommands.ToDictionary(e => e.Obsolete);

#if DEBUG
            // check toggle commands
            {
                foreach (var element in _elements)
                {
                    if (HasToggleArgs(element.Value.GetType()))
                    {
                        //Debug.WriteLine($"Check {element.Key}");
                        Debug.Assert(element.Value.Parameter is ToggleCommandParameter);
                    }
                }

                static bool HasToggleArgs(Type type)
                {
                    var method = type.GetMethod(nameof(CommandElement.Execute), [typeof(object), typeof(CommandContext)]);
                    var methodAttribute = method?.GetCustomAttribute(typeof(MethodArgumentAttribute)) as MethodArgumentAttribute;
                    return methodAttribute?.Note == "ToggleCommand.Execute.Remarks";
                }
            }
#endif
        }

        #endregion

        #region Methods

        public CommandElement GetElement<T>() where T : CommandElement
        {
            var key = CommandElementTools.CreateCommandName<T>();
            return GetElement(key);
        }

        public CommandElement GetElement(string? key)
        {
            if (key is null) return CommandElement.None;

            if (_elements.TryGetValue(key, out CommandElement? command))
            {
                return command;
            }
            else
            {
                return CommandElement.None;
            }
        }

        private void AddCommand(string name, CommandElement command)
        {
            command.CheckDefaultMemento();
            _elements.Add(name, command);
        }

        private bool RemoveCommand(string name)
        {
            if (!_elements.TryGetValue(name, out var command)) return false;

            (command as IDisposable)?.Dispose();
            return _elements.Remove(name);
        }

        public CommandElement CreateCloneCommand(CommandElement source)
        {
            var cloneCommand = CloneCommand(source);

            Changed?.Invoke(this, new CommandChangedEventArgs(false));

            return cloneCommand;
        }

        public void RemoveCloneCommand(CommandElement command)
        {
            if (command.IsCloneCommand())
            {
                RemoveCommand(command.Name);

                Changed?.Invoke(this, new CommandChangedEventArgs(false));
            }
        }

        private CommandElement CloneCommand(CommandElement source)
        {
            var cloneCommandName = CreateUniqueCommandName(source.NameSource);
            return CloneCommand(source, cloneCommandName);
        }

        private CommandElement CloneCommand(CommandElement source, CommandNameSource name)
        {
            var cloneCommand = source.CloneCommand(name);
            AddCommand(cloneCommand.Name, cloneCommand);
            ValidateOrder();
            return cloneCommand;
        }

        private CommandNameSource CreateUniqueCommandName(CommandNameSource name)
        {
            if (!_elements.ContainsKey(name.FullName))
            {
                return name;
            }

            for (int id = 2; ; id++)
            {
                var newName = new CommandNameSource(name.Name, id);
                if (!_elements.ContainsKey(newName.FullName))
                {
                    return newName;
                }
            }
        }

        private void ValidateOrder()
        {
            var sorted = _elements.Values
                .OrderBy(e => e.Order)
                .GroupBy(e => e.GetType())
                .Select(group => group.OrderBy(e => e.NameSource))
                .SelectMany(e => e)
                .ToList();

            foreach (var item in sorted.Select((e, i) => (e, i)))
            {
                item.e.Order = item.i;
            }
        }

        /// <summary>
        /// テーブル更新イベントを発行
        /// </summary>
        public void RaiseChanged()
        {
            Changed?.Invoke(this, new CommandChangedEventArgs(false));
        }

        private void CommandTable_Changed(object? sender, CommandChangedEventArgs e)
        {
            ChangeCount++;
            ClearInputGestureDirty();
        }

        /// <summary>
        /// コマンドセットをリセット
        /// </summary>
        /// <param name="inputScheme"></param>
        /// <param name="pageReadOrder"></param>
        public void ResetCommandCollection(InputScheme inputScheme, PageReadOrder pageReadOrder)
        {
            Config.Current.Command.PresetInputScheme = inputScheme;
            Config.Current.Command.PresetPageReadOrder = pageReadOrder;

            var defaultMemento = GetDefaultMemento();
            RestoreCommandCollection(defaultMemento, true);
        }

        /// <summary>
        /// デフォルトコマンドセットを取得
        /// </summary>
        private CommandCollection GetDefaultMemento()
        {
            var inputScheme = Config.Current.Command.PresetInputScheme;
            var pageReadOrder = Config.Current.Command.PresetPageReadOrder;

            // プロパティが異なる場合は再生成
            if (_defaultCollection is null || _defaultCollection.InputScheme != inputScheme || _defaultCollection.PageReadOrder != pageReadOrder)
            {
                var memento = CreateDefaultMemento(inputScheme, pageReadOrder);
                _defaultCollection = new DefaultCommandCollection(inputScheme, pageReadOrder, memento);
                UpdateDefaultCommandElement(memento);
            }

            return _defaultCollection.CommandCollection;
        }

        /// <summary>
        /// CommandElement が保持するでフォルド設定を更新
        /// </summary>
        /// <param name="defaultMemento"></param>
        private void UpdateDefaultCommandElement(CommandCollection defaultMemento)
        {
            foreach (var item in defaultMemento)
            {
                GetElement(item.Key).SetDefaultMemento(item.Value);
            }
        }

        /// <summary>
        /// 初期設定生成
        /// </summary>
        /// <param name="inputScheme">入力スキーム</param>
        /// <param name="pageReadOrder">ページを開く方向</param>
        /// <returns></returns>
        private CommandCollection CreateDefaultMemento(InputScheme inputScheme, PageReadOrder pageReadOrder)
        {
            if (inputScheme == InputScheme.TypeA && pageReadOrder == PageReadOrder.RightToLeft)
            {
                return _baseCollection;
            }

            var memento = _baseCollection.Clone();

            switch (inputScheme)
            {
                case InputScheme.TypeA: // default
                    break;

                case InputScheme.TypeB: // wheel page, right click context menu
                    memento["NextScrollPage"].ShortCutKey = "";
                    memento["PrevScrollPage"].ShortCutKey = "";
                    memento["NextPage"].ShortCutKey = "Left,WheelDown";
                    memento["PrevPage"].ShortCutKey = "Right,WheelUp";
                    memento["OpenContextMenu"].ShortCutKey = "RightClick";
                    break;

                case InputScheme.TypeC: // click page
                    memento["NextScrollPage"].ShortCutKey = "";
                    memento["PrevScrollPage"].ShortCutKey = "";
                    memento["NextPage"].ShortCutKey = "Left,LeftClick";
                    memento["PrevPage"].ShortCutKey = "Right,RightClick";
                    memento["ViewScrollUp"].ShortCutKey = "WheelUp";
                    memento["ViewScrollDown"].ShortCutKey = "WheelDown";
                    break;
            }

            if (pageReadOrder != PageReadOrder.RightToLeft)
            {
                // ページ移動コマンドのキー操作入れ替え
                foreach (var pair in _pairs)
                {
                    var shortCutKey = memento[pair.Key].ShortCutKey;
                    memento[pair.Key].ShortCutKey = ReplaceSwapWheelGesture(memento[pair.Value].ShortCutKey);
                    memento[pair.Value].ShortCutKey = ReplaceSwapWheelGesture(shortCutKey);

                    var mouseGesture = memento[pair.Key].MouseGesture;
                    memento[pair.Key].MouseGesture = memento[pair.Value].MouseGesture;
                    memento[pair.Value].MouseGesture = mouseGesture;
                }
            }

            return memento;

            static string? ReplaceSwapWheelGesture(string? s)
            {
                return s?.ReplaceSwap("WheelUp", "WheelDown", "****");
            }
        }

        /// <summary>
        /// コマンド実行
        /// </summary>
        public bool TryExecute(object sender, string commandName, object[]? args, CommandOption option)
        {
            if (_elements.TryGetValue(commandName, out CommandElement? command))
            {
                var arguments = new CommandArgs(args, option);
                if (command.CanExecute(sender, arguments, false))
                {
                    command.Execute(sender, arguments, false);
                }
            }

            return false;
        }

        /// <summary>
        /// 入力ジェスチャーが変更されていたらテーブル更新イベントを発行する
        /// </summary>
        public void FlushInputGesture()
        {
            if (_elements.Values.Any(e => e.IsInputGestureDirty))
            {
                Changed?.Invoke(this, new CommandChangedEventArgs(false));
            }
        }

        /// <summary>
        /// 入力ジェスチャー変更フラグをクリア
        /// </summary>
        public void ClearInputGestureDirty()
        {
            foreach (var command in _elements.Values)
            {
                command.IsInputGestureDirty = false;
            }
        }

        // コマンドリストをブラウザで開く
        public void OpenCommandListHelp()
        {
            Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "CommandList.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(CreateCommandListHelp(false));
            }

            ExternalProcess.Start(fileName);
        }

        public string CreateCommandListHelp(bool version)
        {
            var sb = new StringBuilder();

            sb.AppendLine(HtmlHelpUtility.CreateHeader("NeeView Command List"));
            sb.AppendLine($"<body><h1>{TextResources.GetString("HelpCommandList.Title")}</h1>");

            if (version)
            {
                sb.AppendLine($"<p>Version {Environment.ApplicationVersion}</p>");
            }

            sb.AppendLine("<!-- section: note -->");
            sb.AppendLine($"<p>{TextResources.GetString("HelpCommandList.Message")}</p>");
            sb.AppendLine("<!-- section_end: note-->");

            foreach (var group in _elements.Values.GroupBy(e => e.Group))
            {
                sb.AppendLine($"<h3>{group.Key}</h3>");
                sb.AppendLine("<table>");
                sb.AppendLine("<tr>");
                sb.AppendLine($"<th class=\"td-middle\">{TextResources.GetString("Word.Command")}</th>");
                sb.AppendLine($"<th class=\"nowrap\">{TextResources.GetString("Word.Shortcut")}</th>");
                sb.AppendLine($"<th class=\"nowrap\">{TextResources.GetString("Word.Gesture")}</th>");
                sb.AppendLine($"<th class=\"nowrap\">{TextResources.GetString("Word.Touch")}</th>");
                sb.AppendLine($"<th class=\"td-middle\">{TextResources.GetString("Word.Summary")}</th>");
                sb.AppendLine("</tr>");
                foreach (var command in group.OrderBy(e => e.Order))
                {
                    sb.AppendLine($"<tr><td>{command.Text}</td><td>{command.ShortCutKey.GetDisplayString()}</td><td>{command.MouseGesture.GetDisplayString()}</td><td>{command.TouchGesture.GetDisplayString()}</td><td>{command.Remarks}</td></tr>");
                }
                sb.AppendLine("</table>");
            }

            sb.AppendLine("</body>");
            sb.AppendLine(HtmlHelpUtility.CreateFooter());

            return sb.ToString();
        }

        #endregion

        #region Scripts

        /// <summary>
        /// スクリプトコマンド更新要求
        /// </summary>
        /// <param name="commands">新しいスクリプトコマンド群</param>
        /// <param name="isReplace">登録済コマンドも置き換える</param>
        public void SetScriptCommands(IEnumerable<ScriptCommand> commands, bool isReplace)
        {
            var news = (commands ?? new List<ScriptCommand>())
                .ToList();

            var oldies = _elements.Values
                .OfType<ScriptCommand>()
                .ToList();

            // 入れ替えの場合は既存の設定をすべて削除
            if (isReplace)
            {
                foreach (var command in oldies)
                {
                    RemoveCommand(command.Name);
                }
                oldies = new List<ScriptCommand>();
            }

            // 存在しないものは削除
            var newPaths = news.Select(e => e.Path).ToList();
            var excepts = oldies.Where(e => !newPaths.Contains(e.Path)).ToList();
            foreach (var command in excepts)
            {
                RemoveCommand(command.Name);
            }

            // 既存のものは情報更新
            var updates = oldies.Except(excepts).ToList();
            foreach (var command in updates)
            {
                command.UpdateDocument(false);
            }

            // 新規のものは追加
            var overwritesPaths = updates.Select(e => e.Path).Distinct().ToList();
            var newcomer = news.Where(e => !overwritesPaths.Contains(e.Path)).ToList();
            foreach (var command in newcomer)
            {
                AddCommand(command.Name, command);
            }

            // re order
            var scripts = _elements.Values.OfType<ScriptCommand>().OrderBy(e => e.NameSource.Name).ThenBy(e => e.NameSource.Number);
            var offset = _elements.Count;
            foreach (var item in scripts.Select((e, i) => (e, i)))
            {
                item.e.Order = offset + item.i;
            }

            Debug.Assert(_elements.Values.GroupBy(e => e.Order).All(e => e.Count() == 1));
            Changed?.Invoke(this, new CommandChangedEventArgs(false));
        }

        #endregion Scripts

        #region Memento CommandCollection

        public CommandCollection CreateCommandCollectionMemento(bool trim)
        {
            // デフォルト値の更新
            _ = GetDefaultMemento();

            var collection = new CommandCollection();
            foreach (var item in _elements)
            {
                var memento = item.Value.CreateMemento(trim);

                // トリミングが有効な場合、クローンコマンド以外でデフォルト設定のコマンドを除外
                if (trim)
                {
                    if (!item.Value.IsCloneCommand() && memento.IsDefault())
                    {
                        continue;
                    }
                }

                collection.Add(item.Key, memento);
            }
            return collection;
        }

        public void RestoreCommandCollection(CommandCollection? collection, bool cleanup)
        {
            collection ??= new();

            ScriptManager.Current.UpdateScriptCommands(isForce: false, isReplace: false);

            // collection は差分の可能性がるので、不足分をデフォルトから補完する
            var defaultMemento = GetDefaultMemento();
            var reconstruct = collection.UnionBy(defaultMemento, e => e.Key);

            foreach (var pair in reconstruct)
            {
                if (_elements.TryGetValue(pair.Key, out CommandElement? element))
                {
                    element.Restore(pair.Value);
                }
                else
                {
                    var cloneName = CommandNameSource.Parse(pair.Key);
                    if (cloneName.IsClone)
                    {
                        if (_elements.TryGetValue(cloneName.Name, out var source))
                        {
                            var command = CloneCommand(source, cloneName);
                            Debug.Assert(command.Name == pair.Key);
                            command.Restore(pair.Value);
                        }
                        else
                        {
                            Debug.WriteLine($"Warning: No such clone source command '{cloneName.Name}'");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Warning: No such command '{pair.Key}'");
                    }
                }
            }

            // cleanup undefined clone commands
            if (cleanup)
            {
                var excepts = _elements.Where(e => e.Value.IsCloneCommand()).Select(e => e.Key).Except(collection.Keys).ToList();
                foreach (var key in excepts)
                {
                    RemoveCommand(key);
                }
            }

            Changed?.Invoke(this, new CommandChangedEventArgs(false));
        }


        #endregion Memento CommmandCollection
    }


    /// <summary>
    /// 保存用コマンドコレクション
    /// </summary>
    public class CommandCollection : Dictionary<string, CommandElementMemento>
    {
        public CommandCollection Clone()
        {
            var clone = new CommandCollection();
            foreach (var item in this)
            {
                clone.Add(item.Key, (CommandElementMemento)item.Value.Clone());
            }
            return clone;
        }
    }

    /// <summary>
    /// 属性つきコマンドコレクション
    /// </summary>
    /// <remarks>
    /// 既定のコマンドコレクションに使われます
    /// </remarks>
    public class DefaultCommandCollection  
    {
        public DefaultCommandCollection(InputScheme inputScheme, PageReadOrder pageReadOrder, CommandCollection commandCollection)
        {
            InputScheme = inputScheme;
            PageReadOrder = pageReadOrder;
            CommandCollection = commandCollection;
        }

        public InputScheme InputScheme { get;  }
        public PageReadOrder PageReadOrder { get;  }
        public CommandCollection CommandCollection { get;  }
    }
}
