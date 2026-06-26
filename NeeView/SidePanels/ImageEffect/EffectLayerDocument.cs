using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public partial class EffectLayerDocument : ObservableObject, IDisposable
    {
        private static readonly Dictionary<EffectType, string> _effectTypeList = AliasNameExtensions.GetAliasNameDictionary<EffectType>();

        private EffectLayerCollection _layers;
        private EffectLayer _layer;
        private DisposableCollection _disposables = new();
        private bool _disposedValue;

        public EffectLayerDocument(EffectLayer layer)
        {
            _layers = Config.Current.ImageEffect.Layers;
            _layer = layer;
            Debug.Assert(_layers.Contains(layer));

            _disposables.Add(layer.SubscribePropertyChanged(nameof(EffectLayer.IsEnabled),
                (s, e) => OnPropertyChanged(nameof(IsEnabled))));

            _disposables.Add(layer.SubscribePropertyChanged(nameof(EffectLayer.EffectType),
                (s, e) => OnPropertyChanged(nameof(EffectType))));

            _disposables.Add(layer.SubscribePropertyChanged(nameof(EffectLayer.Effect),
                (s, e) => Update()));

            _disposables.Add(_layers.SubscribeCollectionChanged(
                (s, e) => UpdateCanExeucute()));

            UpdateCanExeucute();
            Update();
        }


        public Dictionary<EffectType, string> EffectTypeList => _effectTypeList;

        public bool IsEnabled
        {
            get => _layer.IsEnabled;
            set => _layer.IsEnabled = value;
        }

        public EffectType EffectType
        {
            get => _layer.EffectType;
            set => _layer.EffectType = value;
        }

        public EffectUnit? EffectUnit
        {
            get => _layer.Effect;
        }

        [ObservableProperty]
        public partial PropertyDocument? EffectParameters { get; private set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveUpCommand))]
        public partial bool CanMoveUp { get; private set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveDownCommand))]
        public partial bool CanMoveDown { get; private set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        public partial bool CanDelete { get; private set; }


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

        [RelayCommand(CanExecute = nameof(CanMoveUp))]
        public void MoveUp()
        {
            _layers.MoveUp(_layer);
        }

        [RelayCommand(CanExecute = nameof(CanMoveDown))]
        public void MoveDown()
        {
            _layers.MoveDown(_layer);
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        public void Delete()
        {
            _layers.Delete(_layer);
        }

        private void UpdateCanExeucute()
        {
            CanMoveUp = _layers.CanMoveUp(_layer);
            CanMoveDown = _layers.CanMoveDown(_layer);
            CanDelete = _layers.CanDelete(_layer);
        }

        private void Update()
        {
            if (_layer.Effect is null)
            {
                EffectParameters = null;
            }
            else
            {
                EffectParameters = new PropertyDocument(_layer.Effect);
            }

            OnPropertyChanged(nameof(EffectUnit));
        }
    }


    public class EffectUnitToSampleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EffectUnit effectUnit && effectUnit.SampleType != EffectSampleType.None)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class EffectUnitToBushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EffectUnit effectUnit)
            {
                return effectUnit.SampleType switch
                {
                    EffectSampleType.Luminance
                        => BrushTools.LuminanceBrush,
                    EffectSampleType.Tone
                        => BrushTools.ToneBrush,
                    _
                        => Brushes.Black,
                };
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class EffectUnitToEffectConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EffectUnit effectUnit && effectUnit.SampleType != EffectSampleType.None)
            {
                return effectUnit.CreateEffectAdapter()?.Effect;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
