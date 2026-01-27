using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// SidePanelFrame ViewModel
    /// </summary>
    public class SidePanelFrameViewModel : BindableBase
    {
        private SidePanelFrame _model;


        public SidePanelFrameViewModel(SidePanelFrame model, LeftPanelViewModel left, RightPanelViewModel right)
        {
            _model = model;
            _model.VisibleAtOnceRequest += Model_VisibleAtOnceRequest;

            MainLayoutPanelManager = CustomLayoutPanelManager.Current;

            Left = left;
            Left.PropertyChanged += Left_PropertyChanged;

            Right = right;
            Right.PropertyChanged += Right_PropertyChanged;

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.CanHideLeftPanel),
                (s, e) => RaisePropertyChanged(nameof(LeftPanelOpacity)));

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.CanHideRightPanel),
                (s, e) => RaisePropertyChanged(nameof(RightPanelOpacity)));

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.Opacity),
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(LeftPanelOpacity));
                    RaisePropertyChanged(nameof(RightPanelOpacity));
                });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsSideBarEnabled),
                (s, e) => RaisePropertyChanged(nameof(IsSideBarVisible)));

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsLimitPanelWidth),
                (s, e) => RaisePropertyChanged(nameof(IsLimitPanelWidth)));

            MainLayoutPanelManager.DragBegin +=
                (s, e) => DragBegin(this, EventArgs.Empty);
            MainLayoutPanelManager.DragEnd +=
                (s, e) => DragEnd(this, EventArgs.Empty);

            LeftSidePanelIconDescriptor = new SidePanelIconDescriptor(this, MainLayoutPanelManager.LeftDock);
            RightSidePanelIconDescriptor = new SidePanelIconDescriptor(this, MainLayoutPanelManager.RightDock);
        }


        public event EventHandler? PanelVisibilityChanged;


        public SidePanelIconDescriptor LeftSidePanelIconDescriptor { get; }
        public SidePanelIconDescriptor RightSidePanelIconDescriptor { get; }

        public bool IsSideBarVisible
        {
            get => Config.Current.Panels.IsSideBarEnabled;
            set => Config.Current.Panels.IsSideBarEnabled = value;
        }

        public double LeftPanelOpacity
        {
            get => MainWindowModel.Current.CanHideLeftPanel ? Config.Current.Panels.Opacity : 1.0;
        }

        public double RightPanelOpacity
        {
            get => MainWindowModel.Current.CanHideRightPanel ? Config.Current.Panels.Opacity : 1.0;
        }

        public GridLength LeftPanelWidth
        {
            get => new(this.Left.Width);
            set => this.Left.Width = value.Value;
        }

        public GridLength RightPanelWidth
        {
            get => new(this.Right.Width);
            set => this.Right.Width = value.Value;
        }

        public bool IsLeftPanelActive
        {
            get => this.Left.IsPanelActive;
        }

        public bool IsRightPanelActive
        {
            get => this.Right.IsPanelActive;
        }

        public bool IsLimitPanelWidth
        {
            get => Config.Current.Panels.IsLimitPanelWidth;
            set => Config.Current.Panels.IsLimitPanelWidth = value;
        }


        /// <summary>
        /// パネル表示リクエスト
        /// </summary>
        private void Model_VisibleAtOnceRequest(object? sender, VisibleAtOnceRequestEventArgs e)
        {
            VisibleAtOnce(e.Key, e.IsVisible);
        }

        /// <summary>
        /// パネルを一度だけ表示
        /// </summary>
        public void VisibleAtOnce(string key, bool isVisible)
        {
            if (string.IsNullOrEmpty(key))
            {
                Left.VisibleOnce(isVisible);
                Right.VisibleOnce(isVisible);
            }
            else if (Left.SelectedItemContains(key))
            {
                Left.VisibleOnce(isVisible);
            }
            else if (Right.SelectedItemContains(key))
            {
                Right.VisibleOnce(isVisible);
            }
        }


        public SidePanelFrame Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public SidePanelViewModel Left { get; private set; }

        public SidePanelViewModel Right { get; private set; }

        public App App => App.Current;

        public AutoHideConfig AutoHideConfig => Config.Current.AutoHide;

        public CustomLayoutPanelManager MainLayoutPanelManager { get; private set; }


        /// <summary>
        /// ドラッグ開始イベント処理.
        /// 強制的にパネル表示させる
        /// </summary>
        public void DragBegin(object? sender, EventArgs e)
        {
            Left.IsDragged = true;
            Right.IsDragged = true;
        }

        /// <summary>
        /// ドラッグ終了イベント処理
        /// </summary>
        public void DragEnd(object? sender, EventArgs e)
        {
            Left.IsDragged = false;
            Right.IsDragged = false;
        }


        /// <summary>
        /// 右パネルのプロパティ変更イベント処理
        /// </summary>
        private void Right_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Right.Width):
                    RaisePropertyChanged(nameof(RightPanelWidth));
                    break;
                case nameof(Right.PanelVisibility):
                    PanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                    break;
                case nameof(Right.IsAutoHide):
                    PanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                    break;
                case nameof(Right.IsPanelActive):
                    RaisePropertyChanged(nameof(IsRightPanelActive));
                    break;
            }
        }

        /// <summary>
        /// 左パネルのプロパティ変更イベント処理
        /// </summary>
        private void Left_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Left.Width):
                    RaisePropertyChanged(nameof(LeftPanelWidth));
                    break;
                case nameof(Left.PanelVisibility):
                    PanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                    break;
                case nameof(Left.IsAutoHide):
                    PanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                    break;
                case nameof(Left.IsPanelActive):
                    RaisePropertyChanged(nameof(IsLeftPanelActive));
                    break;
            }
        }
    }
}
