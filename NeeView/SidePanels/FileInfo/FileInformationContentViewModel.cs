using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    // TODO: 今のところ破棄されないので不要であるが、正しく上位からDispose()を呼ぶようにしておく
    public class FileInformationContentViewModel : BindableBase, IDisposable
    {
        private static readonly FileInformationRecord _extraEmptyRecord = new FileInformationRecord(InformationKey.ExtraEmpty, null);
        private readonly MappedCollection<FileInformationKey, FileInformationRecord> _collection;
        private FileInformationSource? _source;
        private CollectionViewSource _collectionViewSource;
        private FileInformationRecord? _selectedItem;
        private bool _isVisibleImage;
        private bool _isVisibleMetadata;
        private bool _anyExtraValues;
        private IDisposable? _subscribeDisposer;
        private bool _disposedValue;


        public FileInformationContentViewModel()
        {
            _collection = new MappedCollection<FileInformationKey, FileInformationRecord>(InformationKeyExtensions.DefaultKeys.Select(e => new FileInformationRecord(e, null)));
            _collection.Add(_extraEmptyRecord.Key, _extraEmptyRecord);

            _collectionViewSource = new CollectionViewSource();
            _collectionViewSource.Source = _collection.Collection;
            _collectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FileInformationRecord.Group)) { Converter = new EnumToAliasNameConverter() });
            _collectionViewSource.Filter += CollectionViewSource_Filter;

            Config.Current.Information.PropertyChanged += Information_PropertyChanged;
        }


        public FileInformationSource? Source
        {
            get { return _source; }
            set
            {
                if (SetProperty(ref _source, value))
                {
                    _subscribeDisposer?.Dispose();
                    UpdateDatabase();
                    _subscribeDisposer = _source?.SubscribePropertyChanged(nameof(Source.Properties), (s, e) => AppDispatcher.BeginInvoke(() => UpdateDatabase()));
                }
            }
        }

        public CollectionViewSource CollectionViewSource
        {
            get { return _collectionViewSource; }
            set { SetProperty(ref _collectionViewSource, value); }
        }

        public FileInformationRecord? SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }


        private void Information_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case nameof(InformationConfig.IsVisibleFile):
                case nameof(InformationConfig.IsVisibleImage):
                case nameof(InformationConfig.IsVisibleDescription):
                case nameof(InformationConfig.IsVisibleOrigin):
                case nameof(InformationConfig.IsVisibleCamera):
                case nameof(InformationConfig.IsVisibleAdvancedPhoto):
                case nameof(InformationConfig.IsVisibleGps):
                    UpdateFilter();
                    break;
                case nameof(InformationConfig.IsVisibleExtras):
                    UpdateDatabase();
                    UpdateFilter();
                    break;
                case nameof(InformationConfig.DateTimeFormat):
                    UpdateFilter();
                    break;
            }
        }

        private void UpdateDatabase()
        {
            if (_source?.Properties != null)
            {
                using (_collectionViewSource.DeferRefresh())
                {
                    // Clear extra values
                    foreach (var item in _collection.Where(e => e.Key.IsExtraValue()))
                    {
                        item.Value = null;
                    }

                    foreach (var item in _source.Properties)
                    {
                        if (!item.Key.IsExtraValue() || _collection.ContainsKey(item.Key))
                        {
                            // NOTE: UI高速化のため、表示値だけを変更
                            _collection[item.Key].Value = item.Value;
                        }
                        else if (Config.Current.Information.IsVisibleGroup(InformationGroup.Extras))
                        {
                            var newItem = item.Clone();
                            _collection.Add(newItem.Key, newItem);
                        }
                    }

                    var removes = _collection.Where(e => e.Key.IsExtraValue() && e.Value is null).ToList();
                    foreach (var item in removes)
                    {
                        _collection.Remove(item.Key);
                    }

                    if (UpdateDatabaseState())
                    {
                        UpdateFilter();
                    }
                }
            }
        }

        /// <summary>
        /// データによるプロパティ表示状態更新
        /// </summary>
        /// <returns>表示範囲が変化してフィルターの更新が必要な場合は true</returns>
        private bool UpdateDatabaseState()
        {
            if (_source is null) return false;

            bool isChanged = false;

            var isVisibleImage = _source.PictureInfo != null;
            if (_isVisibleImage != isVisibleImage)
            {
                _isVisibleImage = isVisibleImage;
                isChanged = true;
            }

            var isVisibleMetadata = _source.Metadata != null;
            if (_isVisibleMetadata != isVisibleMetadata)
            {
                _isVisibleMetadata = isVisibleMetadata;
                isChanged = true;
            }

            var anyExtraValues = _collection.Any(e => e.Key.IsExtraValue());
            if (_anyExtraValues != anyExtraValues)
            {
                _anyExtraValues = anyExtraValues;
                isChanged = true;
            }

            return isChanged;
        }

        private void UpdateFilter()
        {
            var selectedItem = SelectedItem;
            _collectionViewSource.View?.Refresh();
            SelectedItem = selectedItem;
        }


        private void CollectionViewSource_Filter(object? sender, FilterEventArgs e)
        {
            if (e.Item is FileInformationRecord record)
            {
                var category = record.Group.ToInformationCategory();
                if (category == InformationCategory.Image && !_isVisibleImage)
                {
                    e.Accepted = false;
                }
                else if (category == InformationCategory.Metadata && !_isVisibleMetadata)
                {
                    e.Accepted = false;
                }
                else if (record.Key.Key == InformationKey.ExtraEmpty)
                {
                    e.Accepted = Config.Current.Information.IsVisibleGroup(record.Group) && !_anyExtraValues;
                }
                else
                {
                    e.Accepted = Config.Current.Information.IsVisibleGroup(record.Group);
                }
            }
            else
            {
                e.Accepted = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _subscribeDisposer?.Dispose();
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
