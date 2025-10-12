using NeeLaboratory.ComponentModel;
using System.Runtime.Serialization;
using System.ComponentModel;
using NeeView.Windows.Property;
using System;
using NeeLaboratory.Generators;
using NeeView.Data;

namespace NeeView
{
    /// <summary>
    /// PageSlider : Model
    /// </summary>
    public partial class PageSlider : BindableBase
    {
        static PageSlider() => Current = new PageSlider();
        public static PageSlider Current { get; }


        private bool _isSliderDirectionReversed;


        private PageSlider()
        {
            this.PageMarkers = new PageMarkers(BookOperation.Current);

            Config.Current.BookSetting.SubscribePropertyChanged(nameof(BookSettingConfig.BookReadOrder), (s, e) => UpdateIsSliderDirectionReversed()); 

            ThumbnailList.Current.IsSliderDirectionReversed = this.IsSliderDirectionReversed;

            PageSelector.Current.SelectionChanged += PageSelector_SelectionChanged;

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.SliderDirection), (s, e) =>
            {
                UpdateIsSliderDirectionReversed();
            });

            UpdateIsSliderDirectionReversed();
        }


        [Subscribable]
        public event EventHandler<ValueChangedEventArgs<bool>>? SliderDirectionChanged;


        /// <summary>
        /// ページマーカー表示のモデル
        /// </summary>
        public PageMarkers PageMarkers { get; private set; }

        /// <summary>
        /// 実際のスライダー方向
        /// </summary>
        public bool IsSliderDirectionReversed
        {
            get { return _isSliderDirectionReversed; }
            private set
            {
                if (_isSliderDirectionReversed != value)
                {
                    _isSliderDirectionReversed = value;
                    RaisePropertyChanged();
                    ThumbnailList.Current.IsSliderDirectionReversed = _isSliderDirectionReversed;
                    this.PageMarkers.IsSliderDirectionReversed = _isSliderDirectionReversed;
                    SliderDirectionChanged?.Invoke(this, new ValueChangedEventArgs<bool>() { NewValue = _isSliderDirectionReversed });
                }
            }
        }

        public PageSelector PageSelector => PageSelector.Current;

        // NOTE: スライダーとリンク。ページモード補正を行う
        public int SelectedIndex
        {
            get { return PageSelector.SelectedIndex; }
            set
            {
                var newValue = GetFixedIndex(value);
                if (newValue != PageSelector.SelectedIndex)
                {
                    SetSelectedIndex(newValue);
                    RaisePropertyChanged(nameof(SelectedIndexRaw));
                }
            }
        }

        // NOTE: スライダーテキストボックスとリンク
        public int SelectedIndexRaw
        {
            get { return PageSelector.SelectedIndex; }
            set
            {
                if (value != PageSelector.SelectedIndex)
                {
                    SetSelectedIndex(value);
                    RaisePropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        private void SetSelectedIndex(int value)
        {
            if (!IsThumbnailLinked())
            {
                PageSelector.SetSelectedIndex(this, value, false);
                PageSelector.Jump(this);
            }
            else
            {
                PageSelector.SetSelectedIndex(this, value, true);
            }
        }

        private int GetFixedIndex(int value)
        {
            if (Config.Current.GetFramePageSize(PageSelector.PageMode) != 2)
            {
                return value;
            }

            // 変更なければそのまま
            if (value == PageSelector.SelectedIndex)
            {
                return value;
            }

            // 先頭ページは常に優先
            if (value == 0)
            {
                return value;
            }

            // 「終端１ページを単独表示」対応
            if (value == PageSelector.MaxIndex && PageSelector.IsSupportedSingleLastPage)
            {
                return value;
            }

            // ２ページアライメント
            if (Config.Current.Book.IsStaticWidePage)
            {
                if (PageSelector.IsSupportedSingleFirstPage)
                {
                    return ((value-1) & ~1) + 1;
                }
                else
                {
                    return (value & ~1);
                }
            }
            
            // スライダーの移動量をページモードに従う
            if (Config.Current.Slider.IsSyncPageMode)
            {
                var baseIndex = PageSelector.SelectedIndex;
                if (PageSelector.ViewPageCount < 2)
                {
                    baseIndex = Math.Min(PageSelector.MaxIndex, baseIndex + (value > baseIndex ? 1 : 0));
                }
                else if (Math.Abs(value - baseIndex) < 2)
                {
                    return baseIndex;
                }

                var delta = value - baseIndex;
                var newDelta = delta - (delta % 2);
                var newValue = baseIndex + newDelta;
                return newValue;
            }

            return value;
        }



        private void PageSelector_SelectionChanged(object? sender, EventArgs e)
        {
            if (sender == this) return;
            RaisePropertyChanged(nameof(SelectedIndex));
            RaisePropertyChanged(nameof(SelectedIndexRaw));
        }

        // スライダー方向更新
        private void UpdateIsSliderDirectionReversed()
        {
            IsSliderDirectionReversed = Config.Current.Slider.SliderDirection switch
            {
                SliderDirection.RightToLeft => true,
                SliderDirection.SyncBookReadDirection => Config.Current.BookSetting.BookReadOrder == PageReadOrder.RightToLeft,
                _ => false,
            };
        }

        /// <summary>
        /// スライドとフィルムストリップを連動させるかを判定
        /// </summary>
        public bool IsThumbnailLinked() => Config.Current.FilmStrip.IsEnabled && Config.Current.Slider.IsSliderLinkedFilmStrip;


        // ページ番号を決定し、コンテンツを切り替える
        public void Jump(bool force)
        {
            if (force || IsThumbnailLinked())
            {
                PageSelector.Jump(this);
            }
        }

    }
}

