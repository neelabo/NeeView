using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace NeeView.Collections.ObjectModel
{
    /// <summary>
    /// OvervableCollection を Model から ViewModel に変換して同期する
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    public partial class ObservableCollectionSync<TModel, TViewModel> : IReadOnlyList<TViewModel>, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
    {
        private readonly ObservableCollection<TViewModel> _viewModelCollection;
        private readonly ObservableCollection<TModel> _modelCollection;
        private readonly Func<TModel, TViewModel> _converter;
        private bool _disposedValue;


        public ObservableCollectionSync(ObservableCollection<TModel> modelCollection, Func<TModel, TViewModel> converter)
        {
            _viewModelCollection = new();
            _modelCollection = modelCollection;
            _converter = converter;

            foreach (var model in _modelCollection)
            {
                _viewModelCollection.Add(_converter(model));
            }

            _modelCollection.CollectionChanged += OnCollectionChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => ((INotifyPropertyChanged)_viewModelCollection).PropertyChanged += value;
            remove => ((INotifyPropertyChanged)_viewModelCollection).PropertyChanged -= value;
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add => ((INotifyCollectionChanged)_viewModelCollection).CollectionChanged += value;
            remove => ((INotifyCollectionChanged)_viewModelCollection).CollectionChanged -= value;
        }

        public int Count => ((IReadOnlyCollection<TViewModel>)_viewModelCollection).Count;

        public TViewModel this[int index] => ((IReadOnlyList<TViewModel>)_viewModelCollection)[index];


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _modelCollection.CollectionChanged -= OnCollectionChanged;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<TViewModel> GetEnumerator()
        {
            return ((IEnumerable<TViewModel>)_viewModelCollection).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_viewModelCollection).GetEnumerator();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        int insertIndex = e.NewStartingIndex;
                        foreach (TModel newModel in e.NewItems)
                        {
                            _viewModelCollection.Insert(insertIndex++, _converter(newModel));
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        int removeIndex = e.OldStartingIndex;
                        if (removeIndex >= 0)
                        {
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                if (removeIndex < _viewModelCollection.Count)
                                {
                                    var viewModel = _viewModelCollection[removeIndex];
                                    _viewModelCollection.RemoveAt(removeIndex);
                                    if (viewModel is IDisposable disposable)
                                    {
                                        disposable.Dispose();
                                    }
                                }
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null && e.NewItems != null)
                    {
                        int replaceIndex = e.OldStartingIndex;
                        foreach (TModel newModel in e.NewItems)
                        {
                            var oldItem = _viewModelCollection[replaceIndex];

                            _viewModelCollection[replaceIndex] = _converter(newModel);
                            replaceIndex++;

                            if (oldItem is IDisposable disposable)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex >= 0 && e.NewStartingIndex >= 0)
                    {
                        _viewModelCollection.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach(var disposable in _viewModelCollection.OfType<IDisposable>())
                    {
                        disposable.Dispose();
                    }

                    _viewModelCollection.Clear();

                    foreach (var model in _modelCollection)
                    {
                        _viewModelCollection.Add(_converter(model));
                    }
                    break;
            }
        }

    }
}
