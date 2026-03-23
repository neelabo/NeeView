using NeeLaboratory;
using NeeView.Windows.Property;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// ビュー拡大コマンド用パラメータ
    /// </summary>
    public class ViewScaleCommandParameter : CommandParameter
    {
        private double _scale = 0.2;
        private bool _isSnapDefaultScale = true;

        [PropertyPercent]
        public double Scale
        {
            get { return _scale; }
            set { SetProperty(ref _scale, AppMath.Round(MathUtility.Clamp(value, 0.0, 1.0))); }
        }

        [DefaultValue(true)]
        [PropertyMember]
        public bool IsSnapDefaultScale
        {
            get => _isSnapDefaultScale;
            set => SetProperty(ref _isSnapDefaultScale, value);
        }
    }
}
