using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class ColorSelectEffect : ShaderEffect
    {
        private static readonly PixelShader _pixelShader = new()
        {
            UriSource = Tools.MakePackUri(typeof(ColorSelectEffect).Assembly, "NeeView/Effects/Shaders/ColorSelectEffect.ps")
        };

        public ColorSelectEffect()
        {
            PixelShader = _pixelShader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(HueProperty);
            UpdateShaderValue(RangeProperty);
            UpdateShaderValue(CurveProperty);
        }

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ColorSelectEffect), 0);
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }


        //
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(double), typeof(ColorSelectEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));
        public double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        //
        public static readonly DependencyProperty RangeProperty = DependencyProperty.Register("Range", typeof(double), typeof(ColorSelectEffect), new UIPropertyMetadata(0.1, PixelShaderConstantCallback(1)));
        public double Range
        {
            get { return (double)GetValue(RangeProperty); }
            set { SetValue(RangeProperty, value); }
        }

        //
        public static readonly DependencyProperty CurveProperty = DependencyProperty.Register("Curve", typeof(double), typeof(ColorSelectEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(2)));
        public double Curve
        {
            get { return (double)GetValue(CurveProperty); }
            set { SetValue(CurveProperty, value); }
        }
    }
}
