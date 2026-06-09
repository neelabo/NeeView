using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using NeeView.Collections.ObjectModel;
using NeeView.Effects;
using System;
using System.Collections.ObjectModel;

namespace NeeView
{
    public partial class EffectLayerViewModel : ObservableObject, IDisposable
    {
        private EffectLayer _source;
        private bool _disposedValue;
        private DisposableCollection _disposables = new();

        [ObservableProperty]
        public partial EffectAdapter? Adapter { get; set; }

        public EffectLayerViewModel(EffectLayer source)
        {
            _source = source;

            _disposables.Add(_source.SubscribePropertyChanged(nameof(EffectLayer.IsEnabled), (s, e) => Update()));
            _disposables.Add(_source.SubscribePropertyChanged(nameof(EffectLayer.Effect), (s, e) => Update()));

            Update();
        }

        public void Update()
        {
            Adapter = _source.IsEnabled ? EffectUnitExtensions.CreateEffectAdapter(_source.Effect) : null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: LayerModel と LayerViewModel は 1vs1 の関係でモデルが消えればViewModelが消える関係なので、Disposeいらないかも？
                    _disposables.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


    public partial class EffectLayerCollectionViewModel : ObservableCollectionSync<EffectLayer, EffectLayerViewModel>
    {
        public EffectLayerCollectionViewModel(ObservableCollection<EffectLayer> source) : base(source, ViewModelConverter)
        {
        }

        private static EffectLayerViewModel ViewModelConverter(EffectLayer layer)
        {
            return new EffectLayerViewModel(layer);
        }
    }

}