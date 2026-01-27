using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    public class HsvEffect : ShaderEffect
    {
        private static readonly PixelShader _pixelShader = new()
        {
            UriSource = Tools.MakePackUri(typeof(HsvEffect).Assembly, "NeeView/Effects/Shaders/HsvEffect.ps")
        };

        public HsvEffect()
        {
            PixelShader = _pixelShader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(HueProperty);
            UpdateShaderValue(SaturationProperty);
            UpdateShaderValue(ValueProperty);
        }

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(HsvEffect), 0);
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }


        //
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(double), typeof(HsvEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));
        public double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        //
        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(double), typeof(HsvEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(1)));
        public double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }

        //
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(HsvEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(2)));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}
