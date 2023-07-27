﻿using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class PrevScrollPageCommand : CommandElement
    {
        public PrevScrollPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.ShortCutKey = "WheelUp";
            this.IsShowMessage = false;
            this.PairPartner = "NextScrollPage";

            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.PrevScrollPage(sender, e.Parameter.Cast<ScrollPageCommandParameter>());
        }
    }


    /// <summary>
    /// スクロール＋ページ移動用パラメータ
    /// </summary>
    public class ScrollPageCommandParameter : ReversibleCommandParameter, IScrollNTypeParameter, IScrollNTypeEndMargin
    {
        private NScrollType _scrollType = NScrollType.NType;
        private double _scroll = 1.0;
        private double _scrollDuration = 0.2;
        private double _endMargin = 10.0;
        private double _lineBreakStopTime;
        private LineBreakStopMode _lineBreakStopMode = LineBreakStopMode.Line;


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
            set => SetProperty(ref _scroll, MathUtility.Clamp(value, 0.1, 1.0));
        }

        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double ScrollDuration
        {
            get { return _scrollDuration; }
            set { SetProperty(ref _scrollDuration, Math.Max(value, 0.0)); }
        }

        [PropertyMember]
        public double EndMargin
        {
            get => _endMargin;
            set => SetProperty(ref _endMargin, Math.Max(value, 0.0));
        }

        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double LineBreakStopTime
        {
            get { return _lineBreakStopTime; }
            set { SetProperty(ref _lineBreakStopTime, value); }
        }

        [PropertyMember]
        public LineBreakStopMode LineBreakStopMode
        {
            get { return _lineBreakStopMode; }
            set { SetProperty(ref _lineBreakStopMode, value); }
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

        #endregion Obsolete
    }


    public enum NScrollType
    {
        NType,
        ZType,
        Diagonal,
    }

    public enum LineBreakStopMode
    {
        Line,
        Page,
    }
}
