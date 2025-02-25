using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView.Setting
{
    /// <summary>
    /// InputGestureSettingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class InputGestureSettingControl : UserControl
    {
        private static Key[] _ignoreKeys = new Key[]
        {
            Key.System, Key.LeftShift, Key.LeftCtrl, Key.RightShift, Key.RightCtrl, Key.LWin, Key.RWin, Key.LeftAlt, Key.RightAlt,
            Key.ImeProcessed, Key.ImeModeChange, Key.ImeAccept,
            Key.Apps, Key.NumLock
        };

        private InputGestureSettingViewModel? _vm;


        public InputGestureSettingControl()
        {
            InitializeComponent();

            this.Loaded += InputGestureSettingControl_Loaded;
        }

        private void InputGestureSettingControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode()) return;

            var window = Window.GetWindow(this) as INotifyMouseHorizontalWheelChanged ?? throw new InvalidOperationException();

            var source = new MouseHorizontalWheelSource(this.MouseGestureBox, window);
            source.MouseHorizontalWheelChanged += MouseGestureBox_MouseHorizontalWheelChanged;
        }

        public void Initialize(IReadOnlyDictionary<string, CommandElement> commandMap, string key)
        {
            _vm = new InputGestureSettingViewModel(commandMap, key);
            this.DataContext = _vm;
        }

        public void Flush()
        {
            _vm?.Flush();
        }


        private void KeyGestureBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is IInputElement element)
            {
                Keyboard.Focus(element);
            }
        }


        // キー入力処理
        // 押されているキーの状態からショートカットテキスト作成
        private void KeyGestureBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_vm is null) return;
            if (e.IsRepeat) return;

            // TAB, ALT+にも対応
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            // 一部 IME Key 対応
            if (e.Key == Key.ImeProcessed && e.ImeProcessedKey.IsImeKey())
            {
                key = e.ImeProcessedKey;
            }

            ////Debug.WriteLine($"{Keyboard.Modifiers}+{e.Key}({key})");

            if (_ignoreKeys.Contains(key))
            {
                _vm.KeyGesture = null;
                return;
            }

            _vm.KeyGesture = new KeyGestureSource(key, Keyboard.Modifiers); ;

            e.Handled = true;
        }

        // 追加ボタン処理
        private void AddKeyGestureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vm is null) return;

            _vm.AddGesture(_vm.KeyGesture);
            _vm.KeyGesture = null;
        }

        // 削除ボタン処理
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vm is null) return;

            if (this.InputGestureList.SelectedValue is InputGestureToken token)
            {
                _vm.RemoveGesture(token.Gesture);
            }
        }


        // マウス入力処理
        // マウスの状態からショートカットテキスト作成
        private void MouseGestureBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_vm is null) return;

            MouseAction action = MouseAction.None;
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    action = e.ClickCount >= 2 ? MouseAction.LeftDoubleClick : MouseAction.LeftClick;
                    break;
                case MouseButton.Right:
                    action = e.ClickCount >= 2 ? MouseAction.RightDoubleClick : MouseAction.RightClick;
                    break;
                case MouseButton.Middle:
                    action = e.ClickCount >= 2 ? MouseAction.MiddleDoubleClick : MouseAction.MiddleClick;
                    break;
                case MouseButton.XButton1:
                    action = e.ClickCount >= 2 ? MouseAction.XButton1DoubleClick : MouseAction.XButton1Click;
                    break;
                case MouseButton.XButton2:
                    action = e.ClickCount >= 2 ? MouseAction.XButton2DoubleClick : MouseAction.XButton2Click;
                    break;
            }

            SetMouseGesture(action, Keyboard.Modifiers, GetModifierMouseButtons(e));
        }


        // マウスホイール入力処理
        // マウスの状態からショートカットテキスト作成
        private void MouseGestureBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_vm is null) return;

            MouseAction wheelAction = MouseAction.None;
            if (e.Delta > 0)
            {
                wheelAction = MouseAction.WheelUp;
            }
            else if (e.Delta < 0)
            {
                wheelAction = MouseAction.WheelDown;
            }

            SetMouseGesture(wheelAction, Keyboard.Modifiers, GetModifierMouseButtons(e));
        }

        // マウス水平ホイール入力処理
        // マウスの状態からショートカットテキスト作成
        private void MouseGestureBox_MouseHorizontalWheelChanged(object sender, MouseWheelEventArgs e)
        {
            if (_vm is null) return;

            MouseAction wheelAction = MouseAction.None;
            if (e.Delta > 0)
            {
                wheelAction = MouseAction.WheelRight;
            }
            else if (e.Delta < 0)
            {
                wheelAction = MouseAction.WheelLeft;
            }

            SetMouseGesture(wheelAction, Keyboard.Modifiers, GetModifierMouseButtons(e));
        }

        private void SetMouseGesture(MouseAction key, ModifierKeys modifiers, ModifierMouseButtons modifierMouseButtons)
        {
            if (_vm is null) return;
            if (key == MouseAction.None) return;

            _vm.MouseGesture = new MouseGestureSource(key, modifiers, modifierMouseButtons);
            this.AddMouseGestureButton.Focus();
        }

        private ModifierMouseButtons GetModifierMouseButtons(MouseButtonEventArgs e)
        {
            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (e.LeftButton == MouseButtonState.Pressed && e.ChangedButton != MouseButton.Left)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (e.RightButton == MouseButtonState.Pressed && e.ChangedButton != MouseButton.Right)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (e.MiddleButton == MouseButtonState.Pressed && e.ChangedButton != MouseButton.Middle)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (e.XButton1 == MouseButtonState.Pressed && e.ChangedButton != MouseButton.XButton1)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (e.XButton2 == MouseButtonState.Pressed && e.ChangedButton != MouseButton.XButton2)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;
            return modifierMouseButtons;
        }

        private ModifierMouseButtons GetModifierMouseButtons(MouseEventArgs e)
        {
            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (e.LeftButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (e.RightButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (e.MiddleButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (e.XButton1 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (e.XButton2 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;
            return modifierMouseButtons;
        }


        // マウスショートカット追加ボタン処理
        private void AddMouseGestureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vm is null) return;

            _vm.AddGesture(_vm.MouseGesture);
            _vm.MouseGesture = null;
        }

        /// <summary>
        /// 競合の解消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConflictButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vm is null) return;

            if (this.InputGestureList.SelectedValue is InputGestureToken item)
            {
                _vm.ResolveConflict(item, Window.GetWindow(this));
            }
        }

    }
}
