using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// PageSlider : ViewModel
    /// </summary>
    public class PageSliderViewModel : ObservableObject
    {
        private PageSlider _model;
        private readonly MouseWheelDelta _mouseWheelDelta = new();


        public PageSliderViewModel(PageSlider model)
        {
            _model = model ?? throw new InvalidOperationException();

            Config.Current.Slider.SubscribePropertyChanged(nameof(SliderConfig.SliderIndexLayout),
                (s, e) => OnPropertyChanged(""));

            BookOperation.Current.BookChanged +=
                (s, e) => OnPropertyChanged(nameof(PageSliderVisibility));

            FontParameters.Current.SubscribePropertyChanged(nameof(FontParameters.DefaultFontSize),
                (s, e) => OnPropertyChanged(nameof(FontSize)));

            Config.Current.Slider.SubscribePropertyChanged(nameof(SliderConfig.Thickness),
                (s, e) => OnPropertyChanged(nameof(FontSize)));
        }


        public PageSlider Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }

        public bool IsSliderWithIndex => _model != null && Config.Current.Slider.SliderIndexLayout != SliderIndexLayout.None;

        public Dock SliderIndexDock => _model != null && Config.Current.Slider.SliderIndexLayout == SliderIndexLayout.Left ? Dock.Left : Dock.Right;

        public Visibility PageSliderVisibility => _model != null && BookOperation.Current.Control.Pages.Count > 0 ? Visibility.Visible : Visibility.Hidden;

        public double FontSize => Math.Min(FontParameters.Current.DefaultFontSize, Config.Current.Slider.Thickness);


        public void MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            if (Config.Current.Slider.MouseWheelAction == SliderMouseWheelAction.CommandDependent)
            {
                MainViewComponent.Current.MouseInput.RaiseMouseWheelChanged(sender, e);
            }
            else
            {
                int turn = _mouseWheelDelta.NotchCount(e);
                if (turn == 0) return;

                for (int i = 0; i < Math.Abs(turn); ++i)
                {
                    if (turn < 0)
                    {
                        BookOperation.Current.Control.MoveNext(this);
                    }
                    else
                    {
                        BookOperation.Current.Control.MovePrev(this);
                    }
                }
            }
        }

        // ページ番号を決定し、コンテンツを切り替える
        public void Jump(bool force)
        {
            _model.Jump(force);
        }

    }
}

