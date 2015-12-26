﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public enum BookCommandType
    {
        OpenSettingWindow,

        LoadAs,

        PrevPage,
        NextPage,

        PrevFolder,
        NextFolder,

        FullScreen,

        ToggleStretchMode,
        SetStretchModeNone,
        SetStretchModeInside,
        SetStretchModeOutside,
        SetStretchModeUniform,
        SetStretchModeUniformToFill,

        TogglePageMode,
        SetPageMode1,
        SetPageMode2,

        ToggleBookReadOrder,
        SetBookReadOrderRight,
        SetBookReadOrderLeft,

        ToggleIsSupportedTitlePage,
        ToggleIsSupportedWidePage,

        ToggleIsRecursiveFolder,

        ToggleSortMode,
        SetSortModeFileName,
        SetSortModeFileNameDictionary,
        SetSortModeTimeStamp,
        SetSortModeRandom,

        ToggleIsReverseSort,
        SetIsReverseSortFalse,
        SetIsReverseSortTrue,

        ViewScrollUp,
        ViewScrollDown,
        ViewScaleUp,
        ViewScaleDown,
    }


    public static class BookCommandExtension
    {
        public class Header
        {
            public string Group { get; set; }
            public string Text { get; set; }
            public Header(string group, string text)
            {
                Group = group;
                Text = text;
            }
        }

        public static Dictionary<BookCommandType, Header> Headers { get; } = new Dictionary<BookCommandType, Header>
        {
            [BookCommandType.OpenSettingWindow] = new Header("設定", "設定ウィンドウを開く"),

            [BookCommandType.LoadAs] = new Header("ファイル", "ファイルを開く"),

            [BookCommandType.PrevPage] = new Header("移動", "前のページに戻る"),
            [BookCommandType.NextPage] = new Header("移動", "次のページへ進む"),
            [BookCommandType.PrevFolder] = new Header("移動", "前のフォルダ(書庫)に戻る"),
            [BookCommandType.NextFolder] = new Header("移動", "次のフォルダ(書庫)へ進む"),

            [BookCommandType.FullScreen] = new Header("ウィンドウ", "フルスクリーンにする"),

            [BookCommandType.ToggleStretchMode] = new Header("スケール", "スケール方法を切り替える"),
            [BookCommandType.SetStretchModeNone] = new Header("スケール", "元のサイズで表示する"),
            [BookCommandType.SetStretchModeInside] = new Header("スケール", "大きい場合、ウィンドウサイズに合わせる"),
            [BookCommandType.SetStretchModeOutside] = new Header("スケール", "小さい場合、ウィンドウサイズに広げる"),
            [BookCommandType.SetStretchModeUniform] = new Header("スケール", "ウィンドウサイズに合わせる"),
            [BookCommandType.SetStretchModeUniformToFill] = new Header("スケール", "ウィンドウいっぱいに広げる"),

            [BookCommandType.TogglePageMode] = new Header("ページ表示", "ページ表示を切り替える"),
            [BookCommandType.SetPageMode1] = new Header("ページ表示", "１ページ表示にする"),
            [BookCommandType.SetPageMode2] = new Header("ページ表示", "２ページ表示にする"),

            [BookCommandType.ToggleBookReadOrder] = new Header("２ページ設定", "右開き、左開きを切り替える"),
            [BookCommandType.SetBookReadOrderRight] = new Header("２ページ設定", "右開きにする"),
            [BookCommandType.SetBookReadOrderLeft] = new Header("２ページ設定", "左開きにする"),

            [BookCommandType.ToggleIsSupportedTitlePage] = new Header("２ページ設定", "最初のページをタイトルとみなす"),
            [BookCommandType.ToggleIsSupportedWidePage] = new Header("２ページ設定", "横長のページを２ページ分とみなす"),

            [BookCommandType.ToggleIsRecursiveFolder] = new Header("本設定", "サブフォルダ読み込みON/OFF"),

            [BookCommandType.ToggleSortMode] = new Header("ページ整列", "ページ並び順を切り替える"),
            [BookCommandType.SetSortModeFileName] = new Header("ページ整列", "ファイル名順にする"),
            [BookCommandType.SetSortModeFileNameDictionary] = new Header("ページ整列", "ファイル名(辞書)順にする"),
            [BookCommandType.SetSortModeTimeStamp] = new Header("ページ整列", "ファイル日付順にする"),
            [BookCommandType.SetSortModeRandom] = new Header("ページ整列", "ランダムに並べる"),

            [BookCommandType.ToggleIsReverseSort] = new Header("ページ整列", "正順逆順を切り替える"),
            [BookCommandType.SetIsReverseSortFalse] = new Header("ページ整列", "正順にする"),
            [BookCommandType.SetIsReverseSortTrue] = new Header("ページ整列", "逆順にする"),

            [BookCommandType.ViewScrollUp] = new Header("ビュー操作", "スクロール↑"),
            [BookCommandType.ViewScrollDown] = new Header("ビュー操作", "スクロール↓"),
            [BookCommandType.ViewScaleUp] = new Header("ビュー操作", "拡大"),
            [BookCommandType.ViewScaleDown] = new Header("ビュー操作", "縮小"),
        };
    }



    //
    public class BookCommandCollection : Dictionary<BookCommandType, BookCommand>
    {
        public BookCommandShortcutSource ShortcutSource { get; private set; }

        public void Initialize(Book book, BookCommandShortcutSource source)
        {
            Add(BookCommandType.OpenSettingWindow, new BookCommand(null));
            Add(BookCommandType.LoadAs, new BookCommand(e => book.Load((string)e)));
            Add(BookCommandType.PrevPage, new BookCommand(e => book.PrevPage()));
            Add(BookCommandType.NextPage, new BookCommand(e => book.NextPage()));
            Add(BookCommandType.PrevFolder, new BookCommand(e => book.PrevFolder()));
            Add(BookCommandType.NextFolder, new BookCommand(e => book.NextFolder()));
            Add(BookCommandType.FullScreen, new BookCommand(null));
            Add(BookCommandType.ToggleStretchMode, new BookCommand(e => book.StretchMode = book.StretchMode.GetToggle()));
            Add(BookCommandType.SetStretchModeNone, new BookCommand(e => book.StretchMode = PageStretchMode.None));
            Add(BookCommandType.SetStretchModeInside, new BookCommand(e => book.StretchMode = PageStretchMode.Inside));
            Add(BookCommandType.SetStretchModeOutside, new BookCommand(e => book.StretchMode = PageStretchMode.Outside));
            Add(BookCommandType.SetStretchModeUniform, new BookCommand(e => book.StretchMode = PageStretchMode.Uniform));
            Add(BookCommandType.SetStretchModeUniformToFill, new BookCommand(e => book.StretchMode = PageStretchMode.UniformToFill));
            Add(BookCommandType.TogglePageMode, new BookCommand(e => book.PageMode = 3 - book.PageMode));
            Add(BookCommandType.SetPageMode1, new BookCommand(e => book.PageMode = 1));
            Add(BookCommandType.SetPageMode2, new BookCommand(e => book.PageMode = 2));
            Add(BookCommandType.ToggleBookReadOrder, new BookCommand(e => book.BookReadOrder = book.BookReadOrder.GetToggle()));
            Add(BookCommandType.SetBookReadOrderRight, new BookCommand(e => book.BookReadOrder = BookReadOrder.RightToLeft));
            Add(BookCommandType.SetBookReadOrderLeft, new BookCommand(e => book.BookReadOrder = BookReadOrder.LeftToRight));
            Add(BookCommandType.ToggleIsSupportedTitlePage, new BookCommand(e => book.IsSupportedTitlePage = !book.IsSupportedTitlePage));
            Add(BookCommandType.ToggleIsSupportedWidePage, new BookCommand(e => book.IsSupportedWidePage = !book.IsSupportedWidePage));
            Add(BookCommandType.ToggleIsRecursiveFolder, new BookCommand(e => book.IsRecursiveFolder = !book.IsRecursiveFolder));
            Add(BookCommandType.ToggleSortMode, new BookCommand(e => book.SortMode = book.SortMode.GetToggle()));
            Add(BookCommandType.SetSortModeFileName, new BookCommand(e => book.SortMode = BookSortMode.FileName));
            Add(BookCommandType.SetSortModeFileNameDictionary, new BookCommand(e => book.SortMode = BookSortMode.FileNameDictionary));
            Add(BookCommandType.SetSortModeTimeStamp, new BookCommand(e => book.SortMode = BookSortMode.TimeStamp));
            Add(BookCommandType.SetSortModeRandom, new BookCommand(e => book.SortMode = BookSortMode.Random));
            Add(BookCommandType.ToggleIsReverseSort, new BookCommand(e => book.IsReverseSort = !book.IsReverseSort));
            Add(BookCommandType.SetIsReverseSortFalse, new BookCommand(e => book.IsReverseSort = false));
            Add(BookCommandType.SetIsReverseSortTrue, new BookCommand(e => book.IsReverseSort = true));
            Add(BookCommandType.ViewScrollUp, new BookCommand(null));
            Add(BookCommandType.ViewScrollDown, new BookCommand(null));
            Add(BookCommandType.ViewScaleUp, new BookCommand(null));
            Add(BookCommandType.ViewScaleDown, new BookCommand(null));

            SetShortcut(source ?? BookCommandShortcutSource.CreateDefaultShortcutSource());
        }


        public void SetShortcut(BookCommandShortcutSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            ShortcutSource = source;

            foreach (var pair in this)
            {
                var a = source[pair.Key].ShortCutKey;
                pair.Value.ShortCutKey = a;
                pair.Value.MouseGesture = source[pair.Key].MouseGesture;
            }
        }
    }

    [DataContract]
    public class BookCommandIntpuGesture
    {
        [DataMember]
        public string ShortCutKey;
        [DataMember]
        public string MouseGesture;

        public BookCommandIntpuGesture()
        {
        }

        public BookCommandIntpuGesture(string shortCutKey, string mouseGesture)
        {
            ShortCutKey = shortCutKey;
            MouseGesture = mouseGesture;
        }

        public BookCommandIntpuGesture Clone()
        {
            return new BookCommandIntpuGesture(ShortCutKey, MouseGesture);
        }
    }

    //
    public class BookCommandShortcutSource : Dictionary<BookCommandType, BookCommandIntpuGesture>
    {
        public static BookCommandShortcutSource CreateDefaultShortcutSource()
        {
            var source = new BookCommandShortcutSource();
            source.ResetToDefault();
            return source;
        }

        private void ResetToDefault()
        {
            Add(BookCommandType.LoadAs, new BookCommandIntpuGesture("Ctrl+O", null));
            Add(BookCommandType.PrevPage, new BookCommandIntpuGesture("Right,RightClick", "R"));
            Add(BookCommandType.NextPage, new BookCommandIntpuGesture("Left,LeftClick", "L"));
            Add(BookCommandType.PrevFolder, new BookCommandIntpuGesture("Shift+Right", "UR"));
            Add(BookCommandType.NextFolder, new BookCommandIntpuGesture("Shift+Left", "UL"));
            Add(BookCommandType.FullScreen, new BookCommandIntpuGesture("F12", "U"));
            Add(BookCommandType.TogglePageMode, new BookCommandIntpuGesture("LeftButton+WheelUp", null));
            Add(BookCommandType.SetPageMode1, new BookCommandIntpuGesture("Ctrl+1", null));
            Add(BookCommandType.SetPageMode2, new BookCommandIntpuGesture("Ctrl+2", null));
            Add(BookCommandType.ViewScrollUp, new BookCommandIntpuGesture("WheelUp", null));
            Add(BookCommandType.ViewScrollDown, new BookCommandIntpuGesture("WheelDown", null));
            Add(BookCommandType.ViewScaleUp, new BookCommandIntpuGesture("RightButton+WheelUp", null));
            Add(BookCommandType.ViewScaleDown, new BookCommandIntpuGesture("RightButton+WheelDown", null));

            foreach(BookCommandType type in Enum.GetValues(typeof(BookCommandType)))
            {
                if (!this.ContainsKey(type))
                {
                    Add(type, new BookCommandIntpuGesture(null, null));
                }
            }
        }

        public void Store(BookCommandCollection commands)
        {
            commands.ShortcutSource.CopyTo(this);
        }

        public void Restore(BookCommandCollection commands)
        {
            var source = commands.ShortcutSource;

            // 上書き
            foreach (var pair in this)
            {
                source[pair.Key] = pair.Value;
            }

            commands.SetShortcut(source);
        }

        public void CopyTo(BookCommandShortcutSource target)
        {
            target.Clear();
            foreach (var pair in this)
            {
                target.Add(pair.Key, pair.Value.Clone());
            }
        }
    }

    //
    public class BookCommand
    {
        public Action<object> Command { get; set; }
        public string ShortCutKey { get; set; }
        public string MouseGesture { get; set; }

        /*
        public BookCommand(Action<object> command, string shotcut, string mouseGesture)
        {
            Command = command;
            ShortCutKey = shotcut;
            MouseGesture = mouseGesture;
        }
        */

        public BookCommand(Action<object> command)
        {
            Command = command;
        }

        public void Execute(object param)
        {
            Command(param);
        }
    }


#if false
    public class MouseGestureCustom : MouseGesture
    {
        public MouseGestureCustom()
        { }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var mouseEventArgs = inputEventArgs as MouseEventArgs;
            if (mouseEventArgs == null)
                return false;

           
            if (!IsDefinedKey(mouseEventArgs.))
                return false;

            return this.Key == mouseEventArgs.Key;
        }
    }
#endif

    /*
    public class MouseWheelEventArgs : InputEventArgs
    {
        public MouseWheelEventArgs(InputDevice inputDevice, int timestamp) : base(inputDevice, timestamp)
        {
        }
    }
    */

    //MouseGestureConverter converter = new MouseGestureConverter();
    //MouseGesture mouseGesture = (MouseGesture)converter.ConvertFromString(key);

    public class InputGestureConverter
    {
        public InputGesture ConvertFromString(string source)
        {
            try
            {
                KeyGestureConverter converter = new KeyGestureConverter();
                return (KeyGesture)converter.ConvertFromString(source);
            }
            catch { }

            try
            {
                MouseGestureConverter converter = new MouseGestureConverter();
                return (MouseGesture)converter.ConvertFromString(source);
            }
            catch { }

            try
            {
                CustomGestureConverter converter = new CustomGestureConverter();
                return (InputGesture)converter.ConvertFromString(source);
            }
            catch { }

            Debug.WriteLine("no support gesture: " + source);
            return null;
        }
    }

    public class CustomGestureConverter
    {
        public InputGesture ConvertFromString(string source)
        {
            var keys = source.Split('+');

            MouseWheelAction action = MouseWheelAction.None;
            ModifierKeys modifierKeys = ModifierKeys.None;
            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;

            if (!Enum.TryParse(keys.Last(), out action))
            {
                throw new NotSupportedException();
            }

            for(int i=0; i<keys.Length-1; ++i)
            {
                var key = keys[i];

                ModifierKeys modifierKeysOne;
                if (Enum.TryParse<ModifierKeys>(key, out modifierKeysOne))
                {
                    modifierKeys |= modifierKeysOne;
                    continue;
                }

                ModifierMouseButtons modifierMouseButtonsOne;
                if (Enum.TryParse<ModifierMouseButtons>(key, out modifierMouseButtonsOne))
                {
                    modifierMouseButtons |= modifierMouseButtonsOne;
                    continue;
                }

                throw new NotSupportedException();
            }

            return new MouseWheelGesture(action, modifierKeys, modifierMouseButtons);
        }
    }


    public enum MouseWheelAction
    {
        None,
        WheelUp,
        WheelDown,
    }

    [Flags]
    public enum ModifierMouseButtons
    {
        None = 0,
        LeftButton = (1<<0),
        MiddleButton = (1<<1),
        RightButton = (1<<2),
        XButton1 = (1<<3),
        XButton2 = (1<<4),
    }

    public class MouseWheelGesture : InputGesture
    {
        public MouseWheelAction MouseWheelAction { get; private set; }
        public ModifierKeys ModifierKeys { get; private set; }
        public ModifierMouseButtons ModifierMouseButtons { get; private set; }


        public MouseWheelGesture(MouseWheelAction wheelAction, ModifierKeys modifierKeys, ModifierMouseButtons modifierMouseButtons)
        {
            this.MouseWheelAction = wheelAction;
            this.ModifierKeys = modifierKeys;
            this.ModifierMouseButtons = modifierMouseButtons;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var mouseEventArgs = inputEventArgs as MouseWheelEventArgs;
            if (mouseEventArgs == null) return false;

            MouseWheelAction wheelAction = MouseWheelAction.None;
            if (mouseEventArgs.Delta > 0)
            {
                wheelAction = MouseWheelAction.WheelUp;
            }
            else if (mouseEventArgs.Delta < 0)
            {
                wheelAction = MouseWheelAction.WheelDown;
            }

            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (mouseEventArgs.RightButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (mouseEventArgs.MiddleButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (mouseEventArgs.XButton1 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (mouseEventArgs.XButton2 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;

            return this.MouseWheelAction == wheelAction && ModifierKeys == Keyboard.Modifiers && ModifierMouseButtons == modifierMouseButtons;
        }
    }

    public class KeyGestureNoModifier : KeyGesture
    {
        // baseクラスのコンストラクタにはダミーのModifierKeys.Controlを渡す。
        // そうしないと修飾キー無しでははじかれるキーがあるため。
        public KeyGestureNoModifier(Key key) : base(key, ModifierKeys.Control)
        {
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)
                    || Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
                    || Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)
                    || Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
            {
                return false;
            }

            var keyEventArgs = inputEventArgs as KeyEventArgs;
            if (keyEventArgs == null)
                return false;
            if (!IsDefinedKey(keyEventArgs.Key))
                return false;

            return this.Key == keyEventArgs.Key;
        }

        private bool IsDefinedKey(Key key)
        {
            return Key.None <= key && key <= Key.OemClear;
        }
    }

}
