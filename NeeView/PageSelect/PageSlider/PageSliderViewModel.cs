﻿using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// PageSlider : ViewModel
    /// </summary>
    public class PageSliderViewModel : BindableBase
    {
        private PageSlider _model;
        private readonly MouseWheelDelta _mouseWheelDelta = new();


        public PageSliderViewModel(PageSlider model)
        {
            _model = model ?? throw new InvalidOperationException();

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.SliderIndexLayout),
                (s, e) => RaisePropertyChanged(null));

            BookOperation.Current.BookChanged +=
                (s, e) => RaisePropertyChanged(nameof(PageSliderVisibility));

            FontParameters.Current.AddPropertyChanged(nameof(FontParameters.DefaultFontSize),
                (s, e) => RaisePropertyChanged(nameof(FontSize)));

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.Thickness),
                (s, e) => RaisePropertyChanged(nameof(FontSize)));
        }


        public PageSlider Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public bool IsSliderWithIndex => _model != null && Config.Current.Slider.SliderIndexLayout != SliderIndexLayout.None;

        public Dock SliderIndexDock => _model != null && Config.Current.Slider.SliderIndexLayout == SliderIndexLayout.Left ? Dock.Left : Dock.Right;

        public Visibility PageSliderVisibility => _model != null && BookOperation.Current.Control.Pages.Count > 0 ? Visibility.Visible : Visibility.Hidden;

        public double FontSize => Math.Min(FontParameters.Current.DefaultFontSize, Config.Current.Slider.Thickness);


        public void MouseWheel(object? sender, MouseWheelEventArgs e)
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

        // ページ番号を決定し、コンテンツを切り替える
        public void Jump(bool force)
        {
            _model.Jump(force);
        }

    }
}

