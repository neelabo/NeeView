using NeeView.Effects;

namespace NeeView
{
    [WordNodeMember]
    public class EffectLayerAccessor
    {
        private readonly EffectLayerCollection _layers;
        private readonly EffectLayer _layer;

        public EffectLayerAccessor(EffectLayerCollection layers, EffectLayer layer)
        {
            _layers = layers;
            _layer = layer;
        }

        [WordNodeMember]
        public bool IsEnabled
        {
            get => _layer.IsEnabled;
            set => AppDispatcher.Invoke(() => _layer.IsEnabled = value);
        }

        [WordNodeMember(DocumentType = typeof(EffectType))]
        public string EffectType
        {
            get => _layer.EffectType.ToString();
            set => AppDispatcher.Invoke(() => _layer.EffectType = value.ToEnum<EffectType>());
        }

        [WordNodeMember]
        public EffectUnit? Effect
        {
            get => _layer.Effect;
        }

        [WordNodeMember]
        public void Remove()
        {
            AppDispatcher.Invoke(() => _layers.Delete(_layer));
        }

        [WordNodeMember]
        public void MoveUp()
        {
            AppDispatcher.Invoke(() => _layers.MoveUp(_layer));
        }

        [WordNodeMember]
        public void MoveDown()
        {
            AppDispatcher.Invoke(() => _layers.MoveDown(_layer));
        }

        [WordNodeMember]
        public void Reset()
        {
            _layer.Reset();
        }
    }
}