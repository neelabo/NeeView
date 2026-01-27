using System.Windows.Input;

namespace NeeView
{

    /// <summary>
    /// 拡張マウスアクション
    /// 拡張ボタン対応
    /// </summary>
    public class MouseExGesture : InputGesture
    {
        // メインアクション
        public MouseExAction Action { get; private set; }

        // 修飾キー
        public ModifierKeys Modifiers { get; private set; }

        // 修飾マウスボタン
        public ModifierMouseButtons ModifierButtons { get; private set; }

        // コンストラクタ
        public MouseExGesture(MouseExAction action, ModifierKeys modifiers, ModifierMouseButtons modifierButtons)
        {
            this.Action = action;
            this.Modifiers = modifiers;
            this.ModifierButtons = modifierButtons;
        }

        // 入力判定
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is not MouseButtonEventArgs mouseEventArgs) return false;

            MouseExAction action = MouseExAction.None;

            switch (mouseEventArgs.ChangedButton)
            {
                case MouseButton.Left:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.LeftDoubleClick : MouseExAction.LeftClick;
                    break;

                case MouseButton.Right:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.RightDoubleClick : MouseExAction.RightClick;
                    break;

                case MouseButton.Middle:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.MiddleDoubleClick : MouseExAction.MiddleClick;
                    break;

                case MouseButton.XButton1:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.XButton1DoubleClick : MouseExAction.XButton1Click;
                    break;

                case MouseButton.XButton2:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.XButton2DoubleClick : MouseExAction.XButton2Click;
                    break;
            }

            if (action == MouseExAction.None) return false;

            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.Left)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (mouseEventArgs.RightButton == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.Right)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (mouseEventArgs.MiddleButton == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.Middle)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (mouseEventArgs.XButton1 == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.XButton1)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (mouseEventArgs.XButton2 == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.XButton2)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;

            return this.Action == action && this.ModifierButtons == modifierMouseButtons && Modifiers == Keyboard.Modifiers;
        }


        public string GetDisplayString()
        {
            return new MouseGestureSource(MouseActionExtensions.ConvertFrom(Action), Modifiers, ModifierButtons).GetDisplayString();
        }
    }
}
