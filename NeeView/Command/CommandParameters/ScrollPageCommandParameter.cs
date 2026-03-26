using Generator.Equals;
using NeeLaboratory;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// スクロール＋ページ移動用パラメータ
    /// </summary>
    [Equatable(Explicit = true)]
    public partial class ScrollPageCommandParameter : ReversibleCommandParameter, IScrollNTypeParameter, IScrollNTypeEndMargin
    {
        [DefaultEquality] private NScrollType _scrollType = NScrollType.NType;
        [DefaultEquality] private double _scroll = 1.0;
        [DefaultEquality] private double _endMargin = 10.0;
        [DefaultEquality] private double _lineBreakStopTime;
        [DefaultEquality] private LineBreakStopMode _lineBreakStopMode = LineBreakStopMode.Line;
        [DefaultEquality] private bool _pagesAsOne;


        [PropertyMember]
        public NScrollType ScrollType
        {
            get { return _scrollType; }
            set { SetProperty(ref _scrollType, value); }
        }

        [PropertyPercent]
        public double Scroll
        {
            get => _scroll;
            set => SetProperty(ref _scroll, AppMath.Round(MathUtility.Clamp(value, 0.1, 1.0)));
        }


        [PropertyMember]
        public double EndMargin
        {
            get => _endMargin;
            set => SetProperty(ref _endMargin, AppMath.Round(Math.Max(value, 0.0)));
        }

        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double LineBreakStopTime
        {
            get { return _lineBreakStopTime; }
            set { SetProperty(ref _lineBreakStopTime, AppMath.Round(value)); }
        }

        [PropertyMember]
        public LineBreakStopMode LineBreakStopMode
        {
            get { return _lineBreakStopMode; }
            set { SetProperty(ref _lineBreakStopMode, value); }
        }

        [PropertyMember]
        public bool PagesAsOne
        {
            get { return _pagesAsOne; }
            set { SetProperty(ref _pagesAsOne, value); }
        }


        #region Obsolete

        [Obsolete("no used"), Alternative(nameof(ScrollType), 39)] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsNScroll
        {
            get { return false; }
            set { ScrollType = value ? NScrollType.NType : NScrollType.Diagonal; }
        }

        [Obsolete("no used"), Alternative(nameof(LineBreakStopTime), 39)] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double PageMoveMargin
        {
            get { return 0.0; }
            set { LineBreakStopTime = value; LineBreakStopMode = LineBreakStopMode.Page; }
        }

        [Obsolete("no used"), Alternative("nv.Config.View.ScrollDuration", 40, ScriptErrorLevel.Warning, IsFullName = true)] // ver.40
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public double ScrollDuration
        {
            get { return 0.0; }
            set { }
        }

        #endregion Obsolete
    }


    public enum NScrollType
    {
        NType,
        ZType,
        Diagonal,
        Horizontal,
        Vertical,
    }

    public enum LineBreakStopMode
    {
        Line,
        Page,
    }
}
