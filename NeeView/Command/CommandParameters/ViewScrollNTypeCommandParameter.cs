using Generator.Equals;
using NeeLaboratory;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// N字スクロール
    /// </summary>
    [Equatable(Explicit = true)]
    public partial class ViewScrollNTypeCommandParameter : ReversibleCommandParameter, IScrollNTypeParameter
    {
        [DefaultEquality] private NScrollType _scrollType = NScrollType.NType;
        [DefaultEquality] private double _scroll = 1.0;
        [DefaultEquality] private double _lineBreakStopTime;
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

        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double LineBreakStopTime
        {
            get { return _lineBreakStopTime; }
            set { SetProperty(ref _lineBreakStopTime, AppMath.Round(value)); }
        }

        [PropertyMember]
        public bool PagesAsOne
        {
            get { return _pagesAsOne; }
            set { SetProperty(ref _pagesAsOne, value); }
        }


        #region Obsolete

        [Obsolete("no used"), Alternative("nv.Config.View.ScrollDuration", 40, ScriptErrorLevel.Warning, IsFullName = true)] // ver.40
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public double ScrollDuration
        {
            get { return 0.0; }
            set { }
        }

        #endregion Obsolete
    }

}
