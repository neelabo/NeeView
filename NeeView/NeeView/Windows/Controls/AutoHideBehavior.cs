using Microsoft.Xaml.Behaviors;
using NeeView.Windows.Data;
using NeeView.Windows.Media;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// 自動非表示のフォーカスロックモード
    /// </summary>
    public enum AutoHideFocusLockMode
    {
        [AliasName]
        None,

        [AliasName]
        LogicalFocusLock,

        [AliasName]
        LogicalTextBoxFocusLock,

        [AliasName]
        FocusLock,

        [AliasName]
        TextBoxFocusLock,
    }


    /// <summary>
    /// 自動非表示ビヘイビア
    /// </summary>
    public class AutoHideBehavior : Behavior<FrameworkElement>
    {
        #region DependencyProperties

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(AutoHideBehavior), new PropertyMetadata(null));


        public FrameworkElement Screen
        {
            get { return (FrameworkElement)GetValue(ScreenProperty); }
            set { SetValue(ScreenProperty, value); }
        }

        public static readonly DependencyProperty ScreenProperty =
            DependencyProperty.Register("Screen", typeof(FrameworkElement), typeof(AutoHideBehavior), new PropertyMetadata(null, OnScreenPropertyChanged));

        private static void OnScreenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior)
            {
                if (e.OldValue != null)
                {
                    throw new InvalidOperationException("Screen set is once.");
                }
            }
        }


        /// <summary>
        /// 追加のマウス判定領域
        /// </summary>
        public FrameworkElement SubTarget
        {
            get { return (FrameworkElement)GetValue(SubTargetProperty); }
            set { SetValue(SubTargetProperty, value); }
        }

        public static readonly DependencyProperty SubTargetProperty =
            DependencyProperty.Register("SubTarget", typeof(FrameworkElement), typeof(AutoHideBehavior), new PropertyMetadata(null));


        public Dock Dock
        {
            get { return (Dock)GetValue(DockProperty); }
            set { SetValue(DockProperty, value); }
        }

        public static readonly DependencyProperty DockProperty =
            DependencyProperty.Register("Dock", typeof(Dock), typeof(AutoHideBehavior), new PropertyMetadata(Dock.Left));


        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(AutoHideBehavior), new PropertyMetadata(false, OnIsEnabledPropertyChanged));

        private static void OnIsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                control.UpdateVisibility(UpdateVisibilityOption.All);
            }
        }

        public double DelayTime
        {
            get { return (double)GetValue(DelayTimeProperty); }
            set { SetValue(DelayTimeProperty, value); }
        }

        public static readonly DependencyProperty DelayTimeProperty =
            DependencyProperty.Register("DelayTime", typeof(double), typeof(AutoHideBehavior), new PropertyMetadata(1.0, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                control.UpdateVisibility();
            }
        }


        public double DelayVisibleTime
        {
            get { return (double)GetValue(DelayVisibleTimeProperty); }
            set { SetValue(DelayVisibleTimeProperty, value); }
        }

        public static readonly DependencyProperty DelayVisibleTimeProperty =
            DependencyProperty.Register("DelayVisibleTime", typeof(double), typeof(AutoHideBehavior), new PropertyMetadata(0.0, OnPropertyChanged));


        public bool IsVisibleLocked
        {
            get { return (bool)GetValue(IsVisibleLockedProperty); }
            set { SetValue(IsVisibleLockedProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleLockedProperty =
            DependencyProperty.Register("IsVisibleLocked", typeof(bool), typeof(AutoHideBehavior), new PropertyMetadata(false, OnPropertyChanged));


        public double HitTestMargin
        {
            get { return (double)GetValue(HitTestMarginProperty); }
            set { SetValue(HitTestMarginProperty, value); }
        }

        public static readonly DependencyProperty HitTestMarginProperty =
            DependencyProperty.Register("HitTestMargin", typeof(double), typeof(AutoHideBehavior), new PropertyMetadata(16.0));


        public bool IsMouseEnabled
        {
            get { return (bool)GetValue(IsMouseEnabledProperty); }
            set { SetValue(IsMouseEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsMouseEnabledProperty =
            DependencyProperty.Register("IsMouseEnabled", typeof(bool), typeof(AutoHideBehavior), new PropertyMetadata(true, OnIsMouseEnablePropertyChanged));

        private static void OnIsMouseEnablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                control.UpdateVisibility(UpdateVisibilityOption.UpdateMouseOver);
            }
        }

        public AutoHideFocusLockMode FocusLockMode
        {
            get { return (AutoHideFocusLockMode)GetValue(FocusLockModeProperty); }
            set { SetValue(FocusLockModeProperty, value); }
        }

        public static readonly DependencyProperty FocusLockModeProperty =
            DependencyProperty.Register("FocusLockMode", typeof(AutoHideFocusLockMode), typeof(AutoHideBehavior), new PropertyMetadata(AutoHideFocusLockMode.None, OnFocusLockModePropertyChanged));

        private static void OnFocusLockModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                control.UpdateVisibility(UpdateVisibilityOption.UpdateFocusLock);
            }
        }


        public bool IsKeyDownDelayEnabled
        {
            get { return (bool)GetValue(IsKeyDownDelayEnabledProperty); }
            set { SetValue(IsKeyDownDelayEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsKeyDownDelayEnabledProperty =
            DependencyProperty.Register("IsKeyDownDelayEnabled", typeof(bool), typeof(AutoHideBehavior), new PropertyMetadata(true));


        public AutoHideDescription Description
        {
            get { return (AutoHideDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(AutoHideDescription), typeof(AutoHideBehavior), new PropertyMetadata(null, OnDescriptionPropertyChanged));

        private static void OnDescriptionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoHideBehavior control)
            {
                if (e.OldValue is AutoHideDescription description)
                {
                    description.VisibleOnceCall -= control.Description_VisibleOnce;
                }
                if (control.Description != null)
                {
                    control.Description.VisibleOnceCall += control.Description_VisibleOnce;
                }
            }
        }

        #endregion DependencyProperties


        private Window? _window;
        private readonly DelayValue<Visibility> _delayVisibility = new();
        private bool _isAttached;
        private bool _isMouseOver;
        private bool _isFocusLock;
        private bool _isVisibilityHoled;


        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.Screen == null) throw new InvalidOperationException("Screen property is required.");

            _delayVisibility.SetValue(this.AssociatedObject.Visibility);
            _delayVisibility.ValueChanged += DelayVisibility_ValueChanged;
            this.Description?.RaiseVisibilityChanged(this, new VisibilityChangedEventArgs(_delayVisibility.Value));

            this.AssociatedObject.IsKeyboardFocusWithinChanged += AssociatedObject_IsKeyboardFocusWithinChanged;
            this.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus += AssociatedObject_LostFocus;
            this.AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            this.AssociatedObject.Loaded += AssociatedObject_Loaded;

            _isAttached = true;
            UpdateVisibility(UpdateVisibilityOption.All);
        }

        protected override void OnDetaching()
        {
            _isAttached = false;

            base.OnDetaching();

            _delayVisibility.ValueChanged -= DelayVisibility_ValueChanged;

            this.AssociatedObject.IsKeyboardFocusWithinChanged -= AssociatedObject_IsKeyboardFocusWithinChanged;
            this.AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            this.AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
            this.AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            this.AssociatedObject.Unloaded += AssociatedObject_Unloaded;

            BindingOperations.ClearBinding(this.AssociatedObject, FrameworkElement.VisibilityProperty);
        }

        private void AssociatedObject_Loaded(object? sender, RoutedEventArgs e)
        {
            _window = Window.GetWindow(this.AssociatedObject);
            _window.MouseMove += Screen_MouseMove;
            _window.MouseLeave += Screen_MouseLeave;
            _window.StateChanged += Window_StateChanged;
        }

        private void AssociatedObject_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (_window != null)
            {
                _window.MouseMove -= Screen_MouseMove;
                _window.MouseLeave -= Screen_MouseLeave;
                _window.StateChanged -= Window_StateChanged;
                _window = null;
            }
        }


        /// <summary>
        /// ウィンドウ最小化中にパネルが消えるとキーボードフォーカスが失われる現象の対策
        /// </summary>
        private void Window_StateChanged(object? sender, EventArgs e)
        {
            if (!IsEnabled) return;
            if (sender is null) return;

            if (_isVisibilityHoled && ((Window)sender).WindowState != WindowState.Minimized)
            {
                _isVisibilityHoled = false;
                AppDispatcher.BeginInvoke(() =>
                {
                    FlushVisibility();
                    UpdateVisibility(UpdateVisibilityOption.UpdateMouseOver);
                });
            }
        }

        private void DelayVisibility_ValueChanged(object? sender, EventArgs e)
        {
            if (_window?.WindowState != WindowState.Minimized)
            {
                FlushVisibility();
            }
            else
            {
                _isVisibilityHoled = true;
            }
        }

        private void FlushVisibility()
        {
            var visibility = _delayVisibility.Value;
            this.AssociatedObject.Visibility = visibility;
            this.Description?.RaiseVisibilityChanged(this, new VisibilityChangedEventArgs(visibility));
        }

        private void AssociatedObject_IsKeyboardFocusWithinChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsEnabled) return;
            UpdateVisibility(UpdateVisibilityOption.UpdateFocusLock);
        }

        private void AssociatedObject_GotFocus(object? sender, RoutedEventArgs e)
        {
            if (!IsEnabled) return;
            UpdateVisibility(UpdateVisibilityOption.UpdateFocusLock);
        }

        private void AssociatedObject_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!IsEnabled) return;
            UpdateVisibility(UpdateVisibilityOption.UpdateFocusLock);
        }

        private void AssociatedObject_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (!IsEnabled) return;
            if (!this.IsKeyDownDelayEnabled) return;

            // 非表示要求の場合に遅延表示を再発行することで表示状態を延長する
            if (!CanVisible())
            {
                SetVisibility(isVisible: false, isVisibleDelay: false, now: false, isForce: true);
            }
        }

        private void Screen_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;
            UpdateVisibility(UpdateVisibilityOption.UpdateMouseOver);
        }

        private void Screen_MouseLeave(object? sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;
            UpdateVisibility(UpdateVisibilityOption.UpdateMouseOver);
        }

        private void Description_VisibleOnce(object? sender, VisibleOnceEventArgs e)
        {
            VisibleOnce(e.IsVisible);
        }

        /// <summary>
        /// 表示更新用フラグ
        /// </summary>
        [Flags]
        private enum UpdateVisibilityOption
        {
            None,
            Now = 0x0001,
            IsForce = 0x0002,
            UpdateMouseOver = 0x0004,
            UpdateFocusLock = 0x0008,

            All = Now | IsForce | UpdateMouseOver | UpdateFocusLock
        }

        /// <summary>
        /// 表示更新
        /// </summary>
        private void UpdateVisibility(UpdateVisibilityOption options = UpdateVisibilityOption.None)
        {
            if (!_isAttached) return;

            if ((options & UpdateVisibilityOption.UpdateMouseOver) == UpdateVisibilityOption.UpdateMouseOver)
            {
                UpdateMouseOver();
            }

            if ((options & UpdateVisibilityOption.UpdateFocusLock) == UpdateVisibilityOption.UpdateFocusLock)
            {
                UpdateFocusLock();
            }

            var now = (options & UpdateVisibilityOption.Now) == UpdateVisibilityOption.Now;
            var isForce = (options & UpdateVisibilityOption.IsForce) == UpdateVisibilityOption.IsForce;
            SetVisibility(CanVisible(), CanVisibleDelay(), now, isForce);
        }

        /// <summary>
        /// マウスカーソル位置による表示開始判定
        /// </summary>
        private void UpdateMouseOver()
        {
            _isMouseOver = IsMouseOver();

            bool IsMouseOver()
            {
                if (!this.IsEnabled || !this.IsMouseEnabled)
                {
                    return false;
                }

                if (_window?.IsMouseOver != true)
                {
                    return false;
                }

                if (this.AssociatedObject.IsMouseOver)
                {
                    return true;
                }

                if (this.SubTarget != null && this.SubTarget.IsMouseOver)
                {
                    return true;
                }

                if (this.Description?.IsIgnoreMouseOverAppendix() == true)
                {
                    return false;
                }

                var point = Mouse.GetPosition(this.Screen);

                return this.Dock switch
                {
                    Dock.Left => point.X < this.HitTestMargin,
                    Dock.Top => point.Y < this.HitTestMargin,
                    Dock.Right => point.X > this.Screen.ActualWidth - this.HitTestMargin - 1.0,
                    Dock.Bottom => point.Y > this.Screen.ActualHeight - this.HitTestMargin - 1.0,
                    _ => false,
                };
            }
        }

        /// <summary>
        /// フォーカスによる表示判定
        /// </summary>
        private void UpdateFocusLock()
        {
            _isFocusLock = IsFocusLock();

            bool IsFocusLock()
            {
                switch (this.FocusLockMode)
                {
                    case AutoHideFocusLockMode.FocusLock:
                        return this.AssociatedObject.IsKeyboardFocusWithin;

                    case AutoHideFocusLockMode.TextBoxFocusLock:
                        if (Keyboard.FocusedElement is not TextBox textbox) return false;
                        return this.AssociatedObject.IsKeyboardFocusWithin && VisualTreeUtility.HasParentElement(textbox, this.AssociatedObject);

                    case AutoHideFocusLockMode.LogicalFocusLock:
                        return GetFocusedElement() != null;

                    case AutoHideFocusLockMode.LogicalTextBoxFocusLock:
                        return GetFocusedElement() is TextBox;

                    default:
                        return false;
                }
            }

            FrameworkElement? GetFocusedElement()
            {
                if (FocusManager.GetFocusedElement(FocusManager.GetFocusScope(this.AssociatedObject)) is not FrameworkElement focusedElement) return null;
                return VisualTreeUtility.HasParentElement(focusedElement, this.AssociatedObject) ? focusedElement : null;
            }
        }

        private bool CanVisible()
        {
            return CanVisibleNow() || CanVisibleDelay();
        }

        private bool CanVisibleNow()
        {
            return !this.IsEnabled || this.IsVisibleLocked || _isFocusLock || Description?.IsVisibleLocked() == true;
        }

        private bool CanVisibleDelay()
        {
            return this.IsEnabled && _isMouseOver;
        }

        private void SetVisibility(bool isVisible, bool isVisibleDelay, bool now, bool isForce)
        {
            if (isVisible)
            {
                var option = isForce ? DelayValueOverwriteOption.Force : DelayValueOverwriteOption.Shorten;
                var ms = isVisibleDelay ? DelayVisibleTime * 1000.0 : 0.0;
                _delayVisibility.SetValue(Visibility.Visible, ms, option);
            }
            else
            {
                var option = isForce ? DelayValueOverwriteOption.Force : DelayValueOverwriteOption.Shorten;
                var ms = now ? 0.0 : Math.Max(this.DelayTime * 1000.0, 1.0); // NOTE: コンテキストメニューを閉じた瞬間に IsMouseOver が取得できないことがあるので最小の遅延時間を保証する
                _delayVisibility.SetValue(Visibility.Collapsed, ms, option);
            }
        }


        public void VisibleOnce(bool isVisible)
        {
            if (!_isAttached) return;
            if (!IsEnabled) return;

            SetVisibility(isVisible: isVisible, isVisibleDelay: false, now: true, isForce: true);
            UpdateVisibility();
        }

        /// <summary>
        /// コントロールからBehavior取得
        /// </summary>
        public static AutoHideBehavior? GetBehavior(FrameworkElement target)
        {
            var behavior = Interaction.GetBehaviors(target)
                .OfType<AutoHideBehavior>()
                .FirstOrDefault();

            return behavior;
        }
    }


    /// <summary>
    /// VisibilityChangedイベントの引数
    /// </summary>
    public class VisibilityChangedEventArgs : EventArgs
    {
        public VisibilityChangedEventArgs(Visibility visibility)
        {
            Visibility = visibility;
        }

        public Visibility Visibility { get; set; }
    }


    /// <summary>
    /// AutoHideBehavior補足
    /// </summary>
    public class AutoHideDescription
    {
        public event EventHandler<VisibilityChangedEventArgs>? VisibilityChanged;
        public event EventHandler<VisibleOnceEventArgs>? VisibleOnceCall;

        /// <summary>
        /// 表示ロック追加フラグ
        /// </summary>
        public virtual bool IsVisibleLocked()
        {
            return false;
        }

        /// <summary>
        /// VisibilityChangedイベント発行用。AutoHideBehaviorから呼ばれる
        /// </summary>
        public void RaiseVisibilityChanged(object sender, VisibilityChangedEventArgs args)
        {
            VisibilityChanged?.Invoke(sender, args);
        }

        /// <summary>
        /// 一度だけ表示させる命令をBehaviorに送る
        /// </summary>
        public void VisibleOnce(bool isVisible)
        {
            VisibleOnceCall?.Invoke(this, new VisibleOnceEventArgs(isVisible));
        }

        /// <summary>
        /// 追加のマウス無効判定
        /// </summary>
        public virtual bool IsIgnoreMouseOverAppendix()
        {
            return false;
        }
    }

    public class VisibleOnceEventArgs : EventArgs
    {
        public VisibleOnceEventArgs()
        {
        }

        public VisibleOnceEventArgs(bool isVisible)
        {
            IsVisible = isVisible;
        }

        public bool IsVisible { get; } = true;
    }
}
