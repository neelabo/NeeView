using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;


namespace NeeView.Effects
{
    public class ColorizeEffect : ShaderEffect
    {
        private readonly PixelShader _shader = new PixelShader()
        {
            UriSource = Tools.MakePackUri(typeof(ColorSelectEffect).Assembly, "NeeView/Effects/Shaders/Colorize.ps")
        };


        public ColorizeEffect()
        {
            PixelShader = _shader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(LutTextureProperty);
            UpdateShaderValue(LuminanceWeightProperty);
        }

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ColorizeEffect), 0);
        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public static readonly DependencyProperty LutTextureProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("LutTexture", typeof(ColorizeEffect), 1);
        public Brush LutTexture
        {
            get => (Brush)GetValue(LutTextureProperty);
            set => SetValue(LutTextureProperty, value);
        }

        public static readonly DependencyProperty LuminanceWeightProperty = DependencyProperty.Register("LuminanceWeight", typeof(double), typeof(ColorizeEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));
        public double LuminanceWeight
        {
            get { return (double)GetValue(LuminanceWeightProperty); }
            set { SetValue(LuminanceWeightProperty, value); }
        }
    }
}