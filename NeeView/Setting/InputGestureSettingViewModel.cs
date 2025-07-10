using NeeLaboratory.ComponentModel;
using NeeLaboratory.Linq;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    public class InputGestureSettingViewModel : BindableBase
    {
        // すべてのコマンドのショートカット
        private readonly IReadOnlyDictionary<string, CommandElement> _commandMap;
        private KeyGestureSource? _keyGesture;
        private MouseGestureSource? _mouseGesture;
        private ObservableCollection<InputGestureToken> _gestureTokens;


        public InputGestureSettingViewModel(IReadOnlyDictionary<string, CommandElement> commandMap, string command)
        {
            _commandMap = commandMap;
            Command = command;
            Header = $"{CommandTable.Current.GetElement(Command).Text} - {TextResources.GetString("InputGestureControl.Title")}";

            _gestureTokens = CreateGestures();
        }

        // 編集するコマンド
        public string Command { get; set; }

        public KeyGestureSource? KeyGesture
        {
            get { return _keyGesture; }
            set { SetProperty(ref _keyGesture, value); }
        }

        public MouseGestureSource? MouseGesture
        {
            get { return _mouseGesture; }
            set { SetProperty(ref _mouseGesture, value); }
        }

        public ObservableCollection<InputGestureToken> GestureTokens
        {
            get { return _gestureTokens; }
            set { if (_gestureTokens != value) { _gestureTokens = value; RaisePropertyChanged(); } }
        }

        // ウィンドウタイトル？
        public string Header { get; set; }


        /// <summary>
        /// ジェスチャーリスト更新
        /// </summary>
        public void UpdateGestures()
        {
            GestureTokens = CreateGestures();
        }

        private ObservableCollection<InputGestureToken> CreateGestures()
        {
            var items = new ObservableCollection<InputGestureToken>();
            if (!_commandMap[Command].ShortCutKey.IsEmpty)
            {
                foreach (var gesture in _commandMap[Command].ShortCutKey.Gestures)
                {
                    var element = CreateShortcutElement(gesture);
                    items.Add(element);
                }
            }
            return items;
        }

        /// <summary>
        /// GestureToken 作成
        /// </summary>
        /// <param name="gesture"></param>
        /// <returns></returns>
        public InputGestureToken CreateShortcutElement(InputGestureSource gesture)
        {
            var element = new InputGestureToken(gesture);

            var overlaps = _commandMap
                .Where(e => !e.Value.ShortCutKey.IsEmpty && e.Key != Command && e.Value.ShortCutKey.Gestures.Contains(gesture))
                .Select(e => e.Key)
                .ToList();

            if (overlaps.Count > 0)
            {
                element.Conflicts = overlaps;
                element.OverlapsText = string.Format(CultureInfo.InvariantCulture, TextResources.GetString("Notice.Conflict"), TextResources.Join(overlaps.Select(e => CommandTable.Current.GetElement(e).Text)));
            }

            return element;
        }

        /// <summary>
        /// ジェスチャーの追加
        /// </summary>
        /// <param name="gesture"></param>
        public void AddGesture(InputGestureSource? gesture)
        {
            if (gesture is null) return;

            if (!GestureTokens.Any(item => item.Gesture == gesture))
            {
                var element = CreateShortcutElement(gesture);
                GestureTokens.Add(element);
            }
        }

        /// <summary>
        /// ジェスチャーの削除
        /// </summary>
        /// <param name="gesture"></param>
        public void RemoveGesture(InputGestureSource? gesture)
        {
            if (gesture is null) return;

            var token = GestureTokens.FirstOrDefault(e => e.Gesture == gesture);
            if (token != null)
            {
                GestureTokens.Remove(token);
            }
        }

        /// <summary>
        /// GestureTokensから元の情報に書き戻し
        /// </summary>
        public void Flush()
        {
            _commandMap[Command].ShortCutKey = GestureTokens.Count > 0
                ? new ShortcutKey(GestureTokens.Select(e => e.Gesture).WhereNotNull())
                : ShortcutKey.Empty;
        }


        /// <summary>
        /// 競合の解決
        /// </summary>
        public void ResolveConflict(InputGestureToken item, System.Windows.Window owner)
        {
            Flush();

            if (item.Conflicts is null) return;
            if (item.Gesture is null) return;

            var conflicts = new List<string>(item.Conflicts);
            conflicts.Insert(0, Command);
            var context = new ResolveConflictDialogContext(item.Gesture, conflicts, Command);

            // 競合解消用ダイアログ表示。本来はViewで行うべき
            var dialog = new ResolveConflictDialog(context);
            dialog.Owner = owner;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                foreach (var conflictItem in context.Conflicts)
                {
                    if (!conflictItem.IsChecked)
                    {
                        var newGesture = new ShortcutKey(_commandMap[conflictItem.CommandName].ShortCutKey.Gestures.Where(i => i != item.Gesture));
                        _commandMap[conflictItem.CommandName].ShortCutKey = newGesture;
                    }
                }
                UpdateGestures();
            }
        }
    }
}
