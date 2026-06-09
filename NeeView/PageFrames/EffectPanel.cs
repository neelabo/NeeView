using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView.PageFrames
{
    public class EffectPanel : Border, IDisposable
    {
        private readonly UIElement _content;
        private Border? _contentParent;
        private readonly EffectLayerCollectionViewModel _collection;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;


        public EffectPanel(EffectLayerCollection layers, UIElement content)
        {
            _collection = new EffectLayerCollectionViewModel(layers);

            _content = content;

            _disposables.Add(_collection.SubscribeCollectionChanged((s, e) => Update()));
            _disposables.Add(Config.Current.ImageEffect.SubscribePropertyChanged(nameof(ImageEffectConfig.IsEnabled), (s, e) => Update()));

            Update();
        }

        private void Update()
        {
            _contentParent?.Child = null;

            Border border = this;
            if (Config.Current.ImageEffect.IsEnabled)
            {
                for (int i = 0; i < _collection.Count; i++)
                {
                    var effect = _collection[i];
                    var child = new Border() { Name = $"Layer_{i}" };
                    child.Loaded += (s, e) => ((Border)s).SetBinding(Border.EffectProperty, new Binding("Adapter.Effect") { Source = effect });
                    child.Unloaded += (s, e) => BindingOperations.ClearBinding((Border)s, Border.EffectProperty);
                    border.Child = child;
                    border = child;
                }
            }
            border.Child = _content;

            _contentParent = border;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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
}

