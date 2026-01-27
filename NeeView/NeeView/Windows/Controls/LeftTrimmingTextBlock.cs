using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// 左からトリミングするテキストボックス
    /// </summary>
    /// <remarks>
    /// 親コントロールの幅に収まるように文字列を編集します。
    /// </remarks>
    public class LeftTrimmingTextBlock : TextBlock
    {
        private FrameworkElement? _parentElement;


        public LeftTrimmingTextBlock()
        {
            this.Loaded += new RoutedEventHandler(PathTrimmingTextBlock_Loaded);
        }


        public string TextSource
        {
            get { return (string)GetValue(TextSourceProperty); }
            set { SetValue(TextSourceProperty, value); }
        }

        public static readonly DependencyProperty TextSourceProperty =
            DependencyProperty.Register("TextSource", typeof(string), typeof(LeftTrimmingTextBlock), new UIPropertyMetadata("", TextSourcePropertyChanged));

        private static void TextSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LeftTrimmingTextBlock control)
            {
                control.UpdateText();
            }
        }


        private void PathTrimmingTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Parent is null) throw new InvalidOperationException();

            _parentElement = (FrameworkElement)this.Parent;
            _parentElement.SizeChanged += new SizeChangedEventHandler(ParentElement_SizeChanged);

            UpdateText();
        }

        private void ParentElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (_parentElement != null)
            {
                this.Text = GetLeftTrimmingText(_parentElement.ActualWidth);
            }
            else
            {
                this.Text = this.TextSource;
            }
        }

        private string GetLeftTrimmingText(double width)
        {
            const double margin = 2.0;

            if (string.IsNullOrEmpty(this.TextSource))
            {
                return this.TextSource;
            }

            var originalFormattedText = CreateFormattedText(this.TextSource);
            if (originalFormattedText.Width < width - margin)
            {
                return this.TextSource;
            }

            for (int index = 2; index < this.TextSource.Length; ++index)
            {
                var s = string.Concat("…", this.TextSource.AsSpan(index));
                var formattedText = CreateFormattedText(s);
                if (formattedText.Width < width - margin)
                {
                    return s;
                }
            }

            return "…";
        }

        private FormattedText CreateFormattedText(string s)
        {
            var formatted = new FormattedText(
                s,
                CultureInfo.CurrentCulture,
                this.FlowDirection,
                new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
                this.FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip
             );

            return formatted;
        }
    }
}
