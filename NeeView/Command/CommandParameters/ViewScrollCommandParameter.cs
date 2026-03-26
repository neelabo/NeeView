using Generator.Equals;
using NeeLaboratory;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// ビュースクロールコマンド用パラメータ
    /// </summary>
    [Equatable(Explicit = true)]
    public partial class ViewScrollCommandParameter : CommandParameter
    {
        [DefaultEquality] private double _scroll = 0.25;
        [DefaultEquality] private bool _allowCrossScroll = true;

        // 属性に説明文
        [PropertyPercent]
        public double Scroll
        {
            get { return _scroll; }
            set { SetProperty(ref _scroll, AppMath.Round(MathUtility.Clamp(value, 0.0, 1.0))); }
        }

        [PropertyMember]
        public bool AllowCrossScroll
        {
            get => _allowCrossScroll;
            set => SetProperty(ref _allowCrossScroll, value);
        }


        #region Obsolete

        // スクロール速度(秒)
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
