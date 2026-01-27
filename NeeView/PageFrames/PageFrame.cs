using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NeeView.PageFrames
{


    /// <summary>
    /// [Immutable]
    /// ページフレーム構成
    /// </summary>
    public class PageFrame : IEquatable<PageFrame>
    {
        private readonly List<PageFrameElement> _elements;
        private readonly PageRange _range;
        private readonly PageTerminal _terminal;
        private readonly int _direction;
        private readonly double _angle;
        private readonly double _scale;
        private readonly double _span;
        private readonly double _totalSpan;
        private readonly Size _stretchedSize;
        private readonly Size _size;

        public PageFrame(PageFrameElement source, int direction, ContentSizeCalculator stretchCalculator)
            : this(new[] { source }, direction, stretchCalculator)
        {
        }

        public PageFrame(PageFrameElement source0, PageFrameElement source1, int direction, ContentSizeCalculator stretchCalculator)
            : this(new[] { source0, source1 }, direction, stretchCalculator)
        {
        }

        /// <summary>
        /// PageFrame コンストラクタ
        /// </summary>
        /// <param name="sources">フレームを構成するページ要素</param>
        /// <param name="direction">ページ要素の並び順</param>
        /// <param name="autorotate">自動回転フラグ。Defaultでない場合に自動回転するかを判断する</param>
        public PageFrame(IEnumerable<PageFrameElement> sources, int direction, ContentSizeCalculator calculator)
        {
            AssertSources(sources);
            _elements = sources.ToList();
            _range = PageRange.Marge(_elements.Select(e => e.PageRange));
            _terminal = _elements.Select(e => e.Terminal).Aggregate((e, next) => e | next);
            _direction = direction;

            // auto rotate
            var rawSize = GetRawContentSize();
            var isFileContent = _elements.Any(e => e.Page.Content.IsFileContent);
            _angle = calculator.CalcAutoRotate(rawSize, isFileContent);

            // stretch
            _span = calculator.ContentsSpace;
            _totalSpan = Math.Max(0.0, (_elements.Count - 1) * _span);
            _scale = calculator.CalcFrameStretchScale(rawSize, _totalSpan, RotateTransform);

            // frame size
            _stretchedSize = GetStretchedContentSize(calculator);
            _size = GetContentSize(calculator);
        }


        public List<PageFrameElement> Elements => _elements;
        public PageRange FrameRange => _range;
        public PageTerminal Terminal => _terminal;
        public int Direction => _direction;

        /// <summary>
        /// 自動回転角度
        /// </summary>
        public double Angle => _angle;

        /// <summary>
        /// ストレッチスケール
        /// </summary>
        public double Scale => _scale;

        /// <summary>
        /// コンテンツ間のスペース
        /// </summary>
        public double Span => _span;

        /// <summary>
        /// コンテンツ間のスペース (合計)
        /// </summary>
        public double TotalSpan => _totalSpan;

        /// <summary>
        /// ストレッチケールを適用したサイズ
        /// </summary>
        public Size StretchedSize => _stretchedSize;

        /// <summary>
        /// ストレッチスケールと自動回転を適用したサイズ
        /// </summary>
        /// <remarks>
        /// PageFrame の表示サイズはこれを基準とする
        /// </remarks>
        public Size Size => _size;

        /// <summary>
        /// 自動回転
        /// </summary>
        public RotateTransform RotateTransform => new(_angle);

        /// <summary>
        /// ストレッチケール
        /// </summary>
        public ScaleTransform ScaleTransform => new(_scale, _scale);


        public IEnumerable<PageFrameElement> GetDirectedSources()
        {
            return _direction > 0 ? _elements : _elements.Reverse<PageFrameElement>();
        }

        public bool IsEmpty()
        {
            return !_elements.Any();
        }

        public bool Contains(Page page)
        {
            return _elements.Any(e => e.Contains(page));
        }

        public bool Contains(PagePosition index)
        {
            return _elements.Any(e => e.Contains(index));
        }

        public Size GetRawContentSize()
        {
            if (!_elements.Any()) return new Size(0.0, 0.0);
            var width = Math.Max(_elements.Sum(e => e.Width), 0.0);
            var height = Math.Max(_elements.Any() ? _elements.Max(e => e.Height) : 0.0, 0.0);
            return new Size(width, height);
        }

        private Size GetStretchedContentSize(ContentSizeCalculator calculator)
        {
            var size = GetRawContentSize();
            var width = Math.Max(size.Width * _scale + _totalSpan, 0.0);
            var height = Math.Max(size.Height * _scale, 0.0);
            return new Size(width, height);
        }

        private Size GetContentSize(ContentSizeCalculator calculator)
        {
            var rect = GetStretchedContentSize(calculator).ToRect();
            var transform = new TransformGroup();
            transform.Children.Add(RotateTransform);
            var bounds = transform.TransformBounds(rect);
            return bounds.Size;
        }

        /// <summary>
        /// 縦長１ページのフレーム？
        /// </summary>
        /// <remarks>
        /// ダミーページは除外します。
        /// </remarks>
        /// <returns>縦長1ページのフレーム</returns>
        public bool IsSinglePortraitElement()
        {
            if (!_elements.Any())
            {
                return false;
            }
            if (_elements.Count == 1)
            {
                return !_elements.First().IsLandscape();
            }
            if (_elements.Any(e => e.IsDummy))
            {
                Debug.Assert(_elements.Where(e => !e.IsDummy).Count() == 1); // 有効なページは１ページだけ
                Debug.Assert(!_elements.Where(e => !e.IsDummy).First().IsLandscape()); // ダミーページは縦長の場合にだけ存在する
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return _range.ToString();
        }


        /// <summary>
        /// Frameを構成するPageSource[]の整合性チェック
        /// TODO: どこで実装する？
        /// </summary>
        /// <param name="sources"></param>
        [Conditional("DEBUG")]
        private static void AssertSources(IEnumerable<PageFrameElement> sources)
        {
            PageFrameElement? prev = null;
            foreach (var source in sources.Where(e => !e.IsDummy))
            {
                Debug.Assert(!source.PageRange.IsEmpty());
                if (prev is not null)
                {
                    Debug.Assert(prev.PageRange.Next() == source.PageRange.Min, $"{nameof(sources)} must be continued.");
                }
                prev = source;
            }
        }

        /// <summary>
        /// [開発用]
        /// フレーム正常性チェック
        /// TODO: 実装場所
        /// </summary>
        [Conditional("DEBUG")]
        public void AssertValid(PageFrameContext context)
        {
            if (context.FramePageSize == 2)
            {
                if (_elements.Count == 1)
                {
                    Debug.Assert(FrameRange.IsOnePage());
                    Debug.Assert(_elements[0].PageRange.PartSize == 2);
                }
                else if (_elements.Count == 2)
                {
                    Debug.Assert(_elements[0].PageRange.IsOnePage());
                    Debug.Assert(_elements[1].PageRange.IsOnePage());
                    Debug.Assert(_elements[0].PageRange.PartSize == 2);
                    Debug.Assert(_elements[1].PageRange.PartSize == 2);
                    if (context.IsSupportedWidePage)
                    {
                        Debug.Assert(!_elements[0].IsLandscape());
                        Debug.Assert(!_elements[1].IsLandscape());
                    }
                    else
                    {
                    }
                }
                else
                {
                    Debug.Assert(false, "wrong sources.Count");
                }
            }
            else
            {
                Debug.Assert(FrameRange.IsOnePage());
                Debug.Assert(_elements.Count == 1);
                var src = _elements[0];
                if (context.IsSupportedDividePage)
                {
                    if (src.IsPageLandscape())
                    {
                        Debug.Assert(src.PageRange.PartSize == 1);
                    }
                    else
                    {
                        Debug.Assert(src.PageRange.PartSize == 2);
                    }
                }
                else
                {
                    Debug.Assert(src.PageRange.PartSize == 2);
                }
            }
        }

        public bool IsMatch(PageFrame? frame)
        {
            if (frame is null) return false;

            if (this.Elements.Count != frame.Elements.Count) return false;

            return this.Elements
                .Zip(frame.Elements, (first, second) => first.IsMatch(second))
                .All(e => e);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PageFrame);
        }

        public bool Equals(PageFrame? frame)
        {
            if (frame is null) return false;

            return _elements.SequenceEqual(frame._elements) &&
               _range.Equals(frame._range) &&
               _terminal == frame._terminal &&
               _direction == frame._direction &&
               _angle == frame._angle &&
               _scale == frame._scale &&
               _span == frame._span &&
               _totalSpan == frame._totalSpan &&
               EqualityComparer<Size>.Default.Equals(_stretchedSize, frame._stretchedSize) &&
               EqualityComparer<Size>.Default.Equals(_size, frame._size);
        }

        public static bool operator ==(PageFrame? left, PageFrame? right)
        {
            return EqualityComparer<PageFrame>.Default.Equals(left, right);
        }

        public static bool operator !=(PageFrame? left, PageFrame? right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            // NOTE: 負荷軽減のため、FrameRangeのハッシュ値のみ使用する
            return FrameRange.GetHashCode();
        }
    }
}
