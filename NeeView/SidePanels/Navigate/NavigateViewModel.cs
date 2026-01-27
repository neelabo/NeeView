using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// Navigate : ViewModel
    /// </summary>
    public class NavigateViewModel : BindableBase
    {
        private readonly NavigateModel _model;


        public NavigateViewModel(NavigateModel model)
        {
            _model = model;
            _model.PropertyChanged += Model_PropertyChanged;

            Config.Current.BookSetting.PropertyChanged += BookSetting_PropertyChanged;
            Config.Current.View.PropertyChanged += ViewConfig_PropertyChanged;
            Config.Current.Navigator.PropertyChanged += NavigatorConfig_PropertyChanged;

            RotateLeftCommand = new RelayCommand(_model.RotateLeft);
            RotateRightCommand = new RelayCommand(_model.RotateRight);
            RotateResetCommand = new RelayCommand(_model.RotateReset);
            ScaleDownCommand = new RelayCommand(_model.ScaleDown);
            ScaleUpCommand = new RelayCommand(_model.ScaleUp);
            ScaleResetCommand = new RelayCommand(_model.ScaleReset);
            StretchCommand = new RelayCommand(_model.Stretch);

            MoreMenuDescription = new NavigateMoreMenuDescription();
        }


        public bool IsVisibleThumbnail
        {
            get => Config.Current.Navigator.IsVisibleThumbnail;
        }

        public double ThumbnailHeight
        {
            get => Config.Current.Navigator.ThumbnailHeight;
            set => Config.Current.Navigator.ThumbnailHeight = value;
        }

        public bool IsVisibleControlBar
        {
            get => Config.Current.Navigator.IsVisibleControlBar;
        }

        public double Angle
        {
            get => _model.Angle;
            set => _model.Angle = value;
        }

        public double SliderAngle
        {
            get => _model.Angle;
            set => _model.Angle = MathUtility.Snap(value, SliderAngle, 0.0, 20.0);
        }

        public AutoRotateType AutoRotate
        {
            get => Config.Current.BookSetting.AutoRotate;
            set => Config.Current.BookSetting.AutoRotate = value;
        }

        public Dictionary<AutoRotateType, string> AutoRotateTypeList { get; } = AliasNameExtensions.GetAliasNameDictionary<AutoRotateType>();

        public bool AllowFileContentAutoRotate
        {
            get => Config.Current.View.AllowFileContentAutoRotate;
            set => Config.Current.View.AllowFileContentAutoRotate = value;
        }

        public double Scale
        {
            get => _model.Scale * 100;
            set => _model.Scale = value * 0.01;
        }

        public double ScaleLog
        {
            get => _model.Scale > 0.0 ? Math.Log(_model.Scale, 2.0) : -5.0;
            set => _model.Scale = Math.Pow(2, MathUtility.Snap(value, ScaleLog, 0.0, 0.5));
        }

        public bool IsFlipHorizontal
        {
            get => _model.IsFlipHorizontal;
            set => _model.IsFlipHorizontal = value;
        }

        public bool IsFlipVertical
        {
            get => _model.IsFlipVertical;
            set => _model.IsFlipVertical = value;
        }


        public PageStretchMode StretchMode
        {
            get => Config.Current.View.StretchMode;
            set => Config.Current.View.StretchMode = value;
        }

        public Dictionary<PageStretchMode, string> StretchModeList { get; } = AliasNameExtensions.GetAliasNameDictionary<PageStretchMode>();


        public bool IsRotateStretchEnabled
        {
            get { return _model.IsRotateStretchEnabled; }
            set { _model.IsRotateStretchEnabled = value; }
        }

        public bool IsKeepAngle
        {
            get => _model.IsKeepAngle;
            set => _model.IsKeepAngle = value;
        }

        public bool IsKeepAngleBooks
        {
            get => _model.IsKeepAngleBooks;
            set => _model.IsKeepAngleBooks = value;
        }

        public bool IsScaleStretchTracking
        {
            get => _model.IsScaleStretchTracking;
            set => _model.IsScaleStretchTracking = value;
        }

        public bool IsKeepScale
        {
            get => _model.IsKeepScale;
            set => _model.IsKeepScale = value;
        }

        public bool IsKeepScaleBooks
        {
            get => _model.IsKeepScaleBooks;
            set => _model.IsKeepScaleBooks = value;
        }

        public bool IsKeepFlip
        {
            get => _model.IsKeepFlip;
            set => _model.IsKeepFlip = value;
        }

        public bool IsKeepFlipBooks
        {
            get => _model.IsKeepFlipBooks;
            set => _model.IsKeepFlipBooks = value;
        }

        public bool AllowStretchScaleUp
        {
            get => Config.Current.View.AllowStretchScaleUp;
            set => Config.Current.View.AllowStretchScaleUp = value;
        }

        public bool AllowStretchScaleDown
        {
            get => Config.Current.View.AllowStretchScaleDown;
            set => Config.Current.View.AllowStretchScaleDown = value;
        }

        public bool IsBaseScaleEnabled
        {
            get => Config.Current.View.IsBaseScaleEnabled;
            set => Config.Current.View.IsBaseScaleEnabled = value;
        }

        public double BaseScale
        {
            get => Config.Current.BookSetting.BaseScale * 100.0;
            set => Config.Current.BookSetting.BaseScale = value / 100.0;
        }

        public double SliderBaseScale
        {
            get => Config.Current.BookSetting.BaseScale * 100.0;
            set => Config.Current.BookSetting.BaseScale = MathUtility.Snap(value, SliderBaseScale, 100.0, 10.0) / 100.0;
        }


        public RelayCommand RotateLeftCommand { get; private set; }
        public RelayCommand RotateRightCommand { get; private set; }
        public RelayCommand RotateResetCommand { get; private set; }
        public RelayCommand ScaleDownCommand { get; private set; }
        public RelayCommand ScaleUpCommand { get; private set; }
        public RelayCommand ScaleResetCommand { get; private set; }
        public RelayCommand StretchCommand { get; private set; }



        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;
                case nameof(DragTransform.Angle):
                    RaisePropertyChanged(nameof(Angle));
                    RaisePropertyChanged(nameof(SliderAngle));
                    break;
                case nameof(DragTransform.Scale):
                    RaisePropertyChanged(nameof(Scale));
                    RaisePropertyChanged(nameof(ScaleLog));
                    break;
                case nameof(DragTransform.IsFlipHorizontal):
                    RaisePropertyChanged(nameof(IsFlipHorizontal));
                    break;
                case nameof(DragTransform.IsFlipVertical):
                    RaisePropertyChanged(nameof(IsFlipVertical));
                    break;
                case nameof(NavigateModel.IsRotateStretchEnabled):
                    RaisePropertyChanged(nameof(IsRotateStretchEnabled));
                    break;
                case nameof(NavigateModel.IsKeepAngle):
                    RaisePropertyChanged(nameof(IsKeepAngle));
                    break;
                case nameof(NavigateModel.IsKeepAngleBooks):
                    RaisePropertyChanged(nameof(IsKeepAngleBooks));
                    break;
                case nameof(NavigateModel.IsKeepScale):
                    RaisePropertyChanged(nameof(IsKeepScale));
                    break;
                case nameof(NavigateModel.IsKeepScaleBooks):
                    RaisePropertyChanged(nameof(IsKeepScaleBooks));
                    break;
                case nameof(NavigateModel.IsKeepFlip):
                    RaisePropertyChanged(nameof(IsKeepFlip));
                    break;
                case nameof(NavigateModel.IsKeepFlipBooks):
                    RaisePropertyChanged(nameof(IsKeepFlipBooks));
                    break;
            }
        }

        private void BookSetting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

                case nameof(BookSettingConfig.AutoRotate):
                    RaisePropertyChanged(nameof(AutoRotate));
                    break;

                case nameof(BookSettingConfig.BaseScale):
                    RaisePropertyChanged(nameof(BaseScale));
                    RaisePropertyChanged(nameof(SliderBaseScale));
                    break;
            }
        }

        private void ViewConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

                case nameof(ViewConfig.AllowFileContentAutoRotate):
                    RaisePropertyChanged(nameof(AllowFileContentAutoRotate));
                    break;

                case nameof(ViewConfig.StretchMode):
                    RaisePropertyChanged(nameof(StretchMode));
                    break;

                case nameof(ViewConfig.AllowStretchScaleUp):
                    RaisePropertyChanged(nameof(AllowStretchScaleUp));
                    break;

                case nameof(ViewConfig.AllowStretchScaleDown):
                    RaisePropertyChanged(nameof(AllowStretchScaleDown));
                    break;

                case nameof(ViewConfig.IsBaseScaleEnabled):
                    RaisePropertyChanged(nameof(IsBaseScaleEnabled));
                    break;
            }
        }

        private void NavigatorConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

                case nameof(NavigatorConfig.IsVisibleThumbnail):
                    RaisePropertyChanged(nameof(IsVisibleThumbnail));
                    break;

                case nameof(NavigatorConfig.ThumbnailHeight):
                    RaisePropertyChanged(nameof(ThumbnailHeight));
                    break;

                case nameof(NavigatorConfig.IsVisibleControlBar):
                    RaisePropertyChanged(nameof(IsVisibleControlBar));
                    break;
            }
        }

        public void AddBaseScaleTick(int delta)
        {
            var tick = 1.0;
            BaseScale = MathUtility.Snap(BaseScale + delta * tick, tick);
        }

        public void AddScaleTick(int delta)
        {
            var tick = 1.0;
            Scale = MathUtility.Snap(Scale + delta * tick, tick);
        }

        public void AddAngleTick(int delta)
        {
            var tick = 1.0;
            Angle = MathUtility.Snap(Angle + delta * tick, tick);
        }



        #region MoreMenu

        public NavigateMoreMenuDescription MoreMenuDescription { get; }

        public class NavigateMoreMenuDescription : MoreMenuDescription
        {
            public override ContextMenu Create()
            {
                var menu = new ContextMenu();
                menu.Items.Add(CreateCheckMenuItem(TextResources.GetString("Navigator.MoreMenu.IsVisibleThumbnail"), new Binding(nameof(NavigatorConfig.IsVisibleThumbnail)) { Source = Config.Current.Navigator }));
                menu.Items.Add(CreateCheckMenuItem(TextResources.GetString("Navigator.MoreMenu.IsVisibleControlBar"), new Binding(nameof(NavigatorConfig.IsVisibleControlBar)) { Source = Config.Current.Navigator }));
                return menu;
            }
        }

        #endregion
    }
}
