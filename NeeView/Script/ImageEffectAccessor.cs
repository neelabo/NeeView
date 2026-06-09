using System.Linq;

namespace NeeView
{
    [WordNodeMember]
    public class ImageEffectAccessor
    {
        public ImageEffectAccessor()
        {
        }

        [WordNodeMember]
        public bool IsEnabled
        {
            get => Config.Current.ImageEffect.IsEnabled;
            set => AppDispatcher.Invoke(() => Config.Current.ImageEffect.IsEnabled = value);
        }

        [WordNodeMember]
        public EffectLayerAccessor[] Layers
        {
            get
            {
                var layers = Config.Current.ImageEffect.Layers;
                return layers.Select(e => new EffectLayerAccessor(layers, e)).ToArray();
            }
        }

        [WordNodeMember]
        public EffectLayerAccessor? CreateNew()
        {
            return AppDispatcher.Invoke(() =>
            {
                var layers = Config.Current.ImageEffect.Layers;
                var layer = layers.CreateNew();
                return layer is not null ? new EffectLayerAccessor(layers, layer) : null;
            });
        }
    }
}