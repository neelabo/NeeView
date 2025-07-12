using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Media.Imaging;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Printing;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// 印刷モード
    /// </summary>
    public enum PrintMode
    {
        [AliasName]
        RawImage,

        [AliasName]
        View,

        [AliasName]
        ViewFill,

        [AliasName]
        ViewStretch,
    }


    /// <summary>
    /// Print Model
    /// </summary>
    public class PrintModel : BindableBase
    {
        // NOTE: アプリ起動中の PrintDialog 設定を維持するため、静的にする
        private static PrintDialog _printDialog = new PrintDialog();
        private static bool _printDialogShown = false;

        private readonly PrintContext _context;
        private double _printableAreaWidth;
        private double _printableAreaHeight;
        private PageImageableArea? _area;
        private double _originWidth;
        private double _originHeight;
        private double _extentWidth;
        private double _extentHeight;
        private PageOrientation _pageOrientation;
        private PrintMode _printMode = PrintMode.View;
        private bool _isBackground;
        private bool _isDotScale;
        private PrintQueue? _printQueue;
        private int _columns = 1;
        private int _rows = 1;
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Center;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
        private Margin _margin = new();


        public PrintModel(PrintContext context)
        {
            _context = context;
            Initialize();
        }


        public bool PrintDialogShown => _printDialogShown;

        public PageOrientation PageOrientation
        {
            get { return _pageOrientation; }
            set
            {
                if (_pageOrientation != value)
                {
                    _pageOrientation = value switch
                    {
                        PageOrientation.Landscape or PageOrientation.ReverseLandscape => PageOrientation.Landscape,
                        _ => PageOrientation.Portrait,
                    };
                    UpdatePrintOrientation();
                    RaisePropertyChanged();
                }
            }
        }

        public static Dictionary<PageOrientation, string> PageOrientationList { get; } = new Dictionary<PageOrientation, string>()
        {
            [PageOrientation.Portrait] = TextResources.GetString("PageOperation.Portrait"),
            [PageOrientation.Landscape] = TextResources.GetString("PageOperation.Landscape"),
        };

        public PrintMode PrintMode
        {
            get { return _printMode; }
            set { if (_printMode != value) { _printMode = value; RaisePropertyChanged(); } }
        }

        public Dictionary<PrintMode, string> PrintModeList
        {
            get { return AliasNameExtensions.GetAliasNameDictionary<PrintMode>(); }
        }

        public bool IsBackground
        {
            get { return _isBackground; }
            set { if (_isBackground != value) { _isBackground = value; RaisePropertyChanged(); } }
        }

        public bool IsDotScale
        {
            get { return _isDotScale; }
            set { if (_isDotScale != value) { _isDotScale = value; RaisePropertyChanged(); } }
        }

        public PrintQueue? PrintQueue
        {
            get { return _printQueue; }
            set { if (_printQueue != value) { _printQueue = value; RaisePropertyChanged(); } }
        }

        public int Columns
        {
            get { return _columns; }
            set { if (_columns != value) { _columns = MathUtility.Clamp(value, 1, 4); RaisePropertyChanged(); } }
        }

        public int Rows
        {
            get { return _rows; }
            set { if (_rows != value) { _rows = MathUtility.Clamp(value, 1, 4); ; RaisePropertyChanged(); } }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get { return _horizontalAlignment; }
            set { if (_horizontalAlignment != value) { _horizontalAlignment = value; RaisePropertyChanged(); } }
        }

        public Dictionary<HorizontalAlignment, string> HorizontalAlignmentList { get; } = new Dictionary<HorizontalAlignment, string>()
        {
            [HorizontalAlignment.Left] = TextResources.GetString("HorizontalAlignment.Left"),
            [HorizontalAlignment.Center] = TextResources.GetString("HorizontalAlignment.Center"),
            [HorizontalAlignment.Right] = TextResources.GetString("HorizontalAlignment.Right"),
        };

        public VerticalAlignment VerticalAlignment
        {
            get { return _verticalAlignment; }
            set { if (_verticalAlignment != value) { _verticalAlignment = value; RaisePropertyChanged(); } }
        }

        public Dictionary<VerticalAlignment, string> VerticalAlignmentList { get; } = new Dictionary<VerticalAlignment, string>()
        {
            [VerticalAlignment.Top] = TextResources.GetString("VerticalAlignment.Top"),
            [VerticalAlignment.Center] = TextResources.GetString("VerticalAlignment.Center"),
            [VerticalAlignment.Bottom] = TextResources.GetString("VerticalAlignment.Bottom"),
        };

        public Margin Margin
        {
            get { return _margin; }
            set { if (_margin != value) { _margin = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// パラメーター初期化
        /// </summary>
        private void Initialize()
        {
            // PageOrientation は PrintDialog から取得する
            PrintMode = PrintMode.View;
            IsBackground = false;
            IsDotScale = false;
            Columns = 1;
            Rows = 1;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            Margin = new();
            UpdatePrintDialog();
        }

        /// <summary>
        /// 設定リセット
        /// </summary>
        /// <remarks>
        /// PrintDialog を作り直すことでプリンター設定もリセットする
        /// </remarks>
        public void ResetDialog()
        {
            if (_printDialogShown) return;

            _printDialog = new PrintDialog();
            Initialize();
        }

        /// <summary>
        /// プリント方向更新
        /// </summary>
        private void UpdatePrintOrientation()
        {
            var printCapabilities = _printDialog.PrintQueue?.GetPrintCapabilities();
            if (printCapabilities is not null && printCapabilities.PageOrientationCapability.Contains(PageOrientation))
            {
                _printDialog.PrintTicket.PageOrientation = PageOrientation;
            }
        }

        /// <summary>
        /// ミリメートルをピクセル数に変換(96DPI)
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        private static double MillimeterToPixel(double mm)
        {
            return mm * 0.039370 * 96.0; // mm -> inch -> 96dpi
        }

        /// <summary>
        /// 印刷ダイアログを表示して、プリンタ選択と印刷設定を行う。
        /// </summary>
        /// <returns></returns>
        public bool? ShowPrintDialog()
        {
            if (_printDialogShown) return null;
            _printDialogShown = true;

            try
            {
                var result = _printDialog.ShowDialog();
                UpdatePrintDialog();
                return result;
            }
            finally
            {
                _printDialogShown = false;
            }
        }

        /// <summary>
        /// ダイアログ情報からパラメータ更新
        /// </summary>
        private void UpdatePrintDialog()
        {
            // プリンター
            PrintQueue = _printDialog.PrintQueue;
            ////Debug.WriteLine($"Printer: {PrintQueue.FullName}");

            // 用紙の方向 (縦/横限定)
            PageOrientation = _printDialog.PrintTicket.PageOrientation ?? PageOrientation.Unknown;

            // 用紙の印刷可能領域
            // NOTE: ドライバによっては null になる可能性がある
            _area = _printDialog.PrintQueue?.GetPrintCapabilities()?.PageImageableArea;
            ////Debug.WriteLine($"Origin: {_area.OriginWidth}x{_area.OriginHeight}");
            ////Debug.WriteLine($"Extent: {_area.ExtentWidth}x{_area.ExtentHeight}");
            ////Debug.WriteLine($"PrintableArea: {_printDialog.PrintableAreaWidth}x{_printDialog.PrintableAreaHeight}");

            _printableAreaWidth = _printDialog.PrintableAreaWidth;
            _printableAreaHeight = _printDialog.PrintableAreaHeight;
        }

        /// <summary>
        /// 画像コンテンツ生成
        /// </summary>
        /// <returns></returns>
        private FrameworkElement CreateRawImageContent()
        {
            if (_context.RawImage == null)
            {
                return new Rectangle();
            }

            var brush = new ImageBrush(_context.RawImage);
            brush.Stretch = Stretch.Fill;
            brush.TileMode = TileMode.None;

            var rectangle = new Rectangle();
            rectangle.Width = _context.RawImage.GetPixelWidth();
            rectangle.Height = _context.RawImage.GetPixelHeight();
            rectangle.Fill = brush;
            RenderOptions.SetBitmapScalingMode(rectangle, IsDotScale ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality);

            return rectangle;
        }

        /// <summary>
        /// 表示コンテンツ生成
        /// </summary>
        /// <returns></returns>
        private FrameworkElement CreateViewContent()
        {
            // スケールモード設定
            foreach (var viewContent in _context.Contents.OfType<IHasScalingMode>())
            {
                viewContent.ScalingMode = IsDotScale ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality;
            }

            // 表示サイズ計算。
            var viewRect = _context.PageFrameContent.GetRawContentRect();

            var rectangle = new Rectangle();
            rectangle.Width = viewRect.Width;
            rectangle.Height = viewRect.Height;
            var brush = new VisualBrush(_context.View);
            brush.Stretch = Stretch.None;
            rectangle.Fill = brush;
            rectangle.RenderTransformOrigin = new Point(0.5, 0.5);
            rectangle.RenderTransform = _context.ViewTransform;

            return rectangle;
        }


        /// <summary>
        /// 印刷領域パラメータ更新
        /// </summary>
        private void UpdateImageableArea()
        {
            var margin = new Margin();

            if (_area != null)
            {
                bool isLandscape = PageOrientation == PageOrientation.Landscape;
                double originWidth = isLandscape ? _area.OriginHeight : _area.OriginWidth;
                double originHeight = isLandscape ? _area.OriginWidth : _area.OriginHeight;
                double extentWidth = isLandscape ? _area.ExtentHeight : _area.ExtentWidth;
                double extentHeight = isLandscape ? _area.ExtentWidth : _area.ExtentHeight;

                // 既定の余白
                margin.Left = originWidth;
                margin.Right = _printableAreaWidth - extentWidth - originWidth;
                margin.Top = originHeight;
                margin.Bottom = _printableAreaHeight - extentHeight - originHeight;
            }

            // 余白補正
            margin.Left = Math.Max(0, margin.Left + MillimeterToPixel(Margin.Left));
            margin.Right = Math.Max(0, margin.Right + MillimeterToPixel(Margin.Right));
            margin.Top = Math.Max(0, margin.Top + MillimeterToPixel(Margin.Top));
            margin.Bottom = Math.Max(0, margin.Bottom + MillimeterToPixel(Margin.Bottom));

            // 領域補正
            _originWidth = margin.Left;
            _originHeight = margin.Top;

            _extentWidth = Math.Max(1, _printableAreaWidth - margin.Left - margin.Right);
            _extentHeight = Math.Max(1, _printableAreaHeight - margin.Top - margin.Bottom);
        }

        /// <summary>
        /// 印刷ビュー生成(全体)
        /// </summary>
        /// <returns></returns>
        private FrameworkElement CreateVisualElement()
        {
            bool isView = PrintMode != PrintMode.RawImage;

            bool isViewAll = PrintMode == PrintMode.ViewStretch || !isView;
            bool isViewPaperArea = isView && PrintMode == PrintMode.ViewFill;
            bool isEffect = isView;
            bool isBackground = IsBackground;

            double printWidth = _extentWidth * Columns;
            double printHeight = _extentHeight * Rows;

            var target = isView ? CreateViewContent() : CreateRawImageContent();

            var canvas = new Canvas();
            canvas.Width = target.Width;
            canvas.Height = target.Height;
            canvas.HorizontalAlignment = HorizontalAlignment.Center;
            canvas.VerticalAlignment = VerticalAlignment.Center;
            canvas.Children.Add(target);

            var gridClip = new Grid();
            gridClip.Name = "GridClip";
            gridClip.Width = _context.ViewWidth;
            gridClip.Height = _context.ViewHeight;
            gridClip.ClipToBounds = true;
            gridClip.Children.Add(canvas);
            gridClip.Background = Brushes.Transparent;

            if (isViewAll)
            {
                // 描画矩形を拡大
                var rect = new Rect(0, 0, target.Width, target.Height);
                rect.Offset(rect.Width * -0.5, rect.Height * -0.5);
                rect = target.RenderTransform.TransformBounds(rect);
                gridClip.Width = rect.Width;
                gridClip.Height = rect.Height;

                // 原点を基準値に戻す補正
                var offset = target.RenderTransform.Transform(new Point(0, 0));
                canvas.RenderTransform = new TranslateTransform(-offset.X, -offset.Y);
            }
            else if (isViewPaperArea)
            {
                var paperAspectRatio = printWidth / printHeight;
                var viewAspectRatio = _context.ViewWidth / _context.ViewHeight;
                if (viewAspectRatio > paperAspectRatio)
                {
                    gridClip.Height = _context.ViewWidth / paperAspectRatio;

                    double offset = (gridClip.Height - _context.ViewHeight) * VerticalAlignment.Direction() * 0.5;
                    canvas.RenderTransform = new TranslateTransform(0, offset);
                }
                else
                {
                    gridClip.Width = _context.ViewHeight * paperAspectRatio;

                    double offset = (gridClip.Width - _context.ViewWidth) * HorizontalAlignment.Direction() * 0.5;
                    canvas.RenderTransform = new TranslateTransform(offset, 0);
                }
            }

            var gridEffect = new Grid();
            gridEffect.Name = "GridEffect";
            gridEffect.Effect = isEffect ? _context.ViewEffect : null;
            gridEffect.Children.Add(gridClip);

            var viewbox = new Viewbox();
            viewbox.Child = gridEffect;
            viewbox.HorizontalAlignment = HorizontalAlignment;
            viewbox.VerticalAlignment = VerticalAlignment;

            var gridArea = new Grid();
            gridArea.Width = printWidth;
            gridArea.Height = printHeight;
            gridArea.Background = isBackground ? _context.Background : null;
            gridArea.Children.Add(viewbox);

            if (isBackground && _context.BackgroundFront != null)
            {
                var backgroundFront = new Rectangle();
                backgroundFront.Fill = _context.BackgroundFront;
                RenderOptions.SetBitmapScalingMode(backgroundFront, BitmapScalingMode.HighQuality);
                gridArea.Children.Insert(0, backgroundFront);
            }

            return gridArea;
        }

        /// <summary>
        /// 印刷ビュー生成(分割)
        /// </summary>
        /// <param name="visual">印刷全体ビュー</param>
        /// <param name="column">列</param>
        /// <param name="row">行</param>
        /// <returns></returns>
        private FrameworkElement CreateVisual(FrameworkElement visual, int column, int row)
        {
            var ox = _extentWidth * column;
            var oy = _extentHeight * row;

            var canvas = new Canvas();
            canvas.ClipToBounds = true;
            canvas.Width = _extentWidth;
            canvas.Height = _extentHeight;
            Canvas.SetLeft(visual, -ox);
            Canvas.SetTop(visual, -oy);
            canvas.Children.Add(visual);

            return canvas;
        }

        /// <summary>
        /// 印刷ページ群生成
        /// </summary>
        /// <returns></returns>
        public List<FixedPage> CreatePageCollection()
        {
            UpdatePrintDialog();

            UpdateImageableArea();

            var collection = new List<FixedPage>();

            for (int row = 0; row < Rows; ++row)
            {
                for (int column = 0; column < Columns; ++column)
                {
                    var fullVisual = CreateVisualElement();

                    var visual = CreateVisual(fullVisual, column, row);

                    var page = new FixedPage();
                    page.Width = _printableAreaWidth; // 既定値上書きのため必須
                    page.Height = _printableAreaHeight;

                    FixedPage.SetLeft(visual, _originWidth);
                    FixedPage.SetTop(visual, _originHeight);

                    page.Children.Add(visual);

                    collection.Add(page);
                }
            }

            return collection;
        }

        /// <summary>
        /// 印刷ドキュメント生成
        /// </summary>
        /// <returns></returns>
        public FixedDocument CreateDocument()
        {
            var document = new FixedDocument();
            foreach (var page in CreatePageCollection().Select(e => new System.Windows.Documents.PageContent() { Child = e }))
            {
                document.Pages.Add(page);
            }
            return document;
        }

        /// <summary>
        /// 印刷実行
        /// </summary>
        public void Print()
        {
            var name = "NeeView - " + GetPrintName();
            Debug.WriteLine($"Print {name}...");
            _printDialog.PrintDocument(CreateDocument().DocumentPaginator, name);
        }

        /// <summary>
        /// 印刷JOB名
        /// </summary>
        /// <returns></returns>
        private string GetPrintName()
        {
            if (PrintMode == PrintMode.RawImage)
            {
                return GetFileName(_context.MainContent);
            }
            else
            {
                return string.Join(" | ", _context.Contents.Where(e => !e.Element.IsDummy).Select(e => GetFileName(e)));
            }

            string GetFileName(ViewContent? content)
            {
                return LoosePath.GetFileName(content?.Element.Page.EntryName.TrimEnd('\\'));
            }
        }


        #region Memento

        [Memento]
        public class Memento
        {
            public Memento()
            {
                PageOrientation = PageOrientation.Portrait;
                PrintMode = PrintMode.View;
                HorizontalAlignment = HorizontalAlignment.Center;
                VerticalAlignment = VerticalAlignment.Center;
                Margin = new Margin();
            }

            public PageOrientation PageOrientation { get; set; }
            public PrintMode PrintMode { get; set; }
            public bool IsBackground { get; set; }
            public bool IsDotScale { get; set; }
            public int Columns { get; set; }
            public int Rows { get; set; }
            public HorizontalAlignment HorizontalAlignment { get; set; }
            public VerticalAlignment VerticalAlignment { get; set; }
            public Margin Margin { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.PageOrientation = PageOrientation;
            memento.PrintMode = PrintMode;
            memento.IsBackground = IsBackground;
            memento.IsDotScale = IsDotScale;
            memento.Columns = Columns;
            memento.Rows = Rows;
            memento.HorizontalAlignment = HorizontalAlignment;
            memento.VerticalAlignment = VerticalAlignment;
            memento.Margin = Margin;

            return memento;
        }

        public void Restore(Memento? memento)
        {
            if (memento == null) return;

            PageOrientation = memento.PageOrientation;
            PrintMode = memento.PrintMode;
            IsBackground = memento.IsBackground;
            IsDotScale = memento.IsDotScale;
            Columns = memento.Columns;
            Rows = memento.Rows;
            HorizontalAlignment = memento.HorizontalAlignment;
            VerticalAlignment = memento.VerticalAlignment;
            Margin = memento.Margin;
        }

        #endregion
    }


    /// <summary>
    /// 余白
    /// </summary>
    public class Margin : BindableBase
    {
        private double _top;
        private double _bottom;
        private double _left;
        private double _right;


        public double Top
        {
            get { return _top; }
            set { if (_top != value) { _top = value; RaisePropertyChanged(); } }
        }

        public double Bottom
        {
            get { return _bottom; }
            set { if (_bottom != value) { _bottom = value; RaisePropertyChanged(); } }
        }

        public double Left
        {
            get { return _left; }
            set { if (_left != value) { _left = value; RaisePropertyChanged(); } }
        }

        public double Right
        {
            get { return _right; }
            set { if (_right != value) { _right = value; RaisePropertyChanged(); } }
        }
    }

    /// <summary>
    /// HorizontalAlignment 拡張
    /// </summary>
    internal static class HorizontalAlignmentExtensions
    {
        /// <summary>
        /// 方向
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static double Direction(this HorizontalAlignment self)
        {
            return self switch
            {
                HorizontalAlignment.Left => -1.0,
                HorizontalAlignment.Right => 1.0,
                _ => 0.0,
            };
        }
    }

    /// <summary>
    /// VerticalAlignment 拡張
    /// </summary>
    internal static class VerticalAlignmentExtensions
    {
        /// <summary>
        /// 方向
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static double Direction(this VerticalAlignment self)
        {
            return self switch
            {
                VerticalAlignment.Top => -1.0,
                VerticalAlignment.Bottom => 1.0,
                _ => 0.0,
            };
        }
    }
}
