using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeView.Collections.ObjectModel;
using NeeView.Effects;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// ImageEffect : ViewModel
    /// </summary>
    public partial class ImageEffectViewModel : ObservableObject
    {
        public ImageEffectViewModel()
        {
            this.UnsharpMaskProfile = new PropertyDocument(Config.Current.ImageResizeFilter.UnsharpMask);

            this.CustomSizeProfile = new PropertyDocument(Config.Current.ImageCustomSize);
            this.CustomSizeProfile.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);

            this.TrimProfile = new PropertyDocument(Config.Current.ImageTrim);
            this.TrimProfile.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);

            this.GridLineProfile = new PropertyDocument(Config.Current.ImageGrid);
            this.GridLineProfile.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);
            this.GridLineProfile.SetVisualType<PropertyValue_Color>(PropertyVisualType.ComboColorPicker);

            EffectLayers = new(Config.Current.ImageEffect.Layers, e => new EffectLayerDocument(e));
            EffectLayers.CollectionChanged += (s, e) => UpdateCanExecute();

            UpdateCanExecute();
        }


        // PictureProfile
        public PictureProfile PictureProfile => PictureProfile.Current;

        public PropertyDocument UnsharpMaskProfile { get; set; }

        public PropertyDocument CustomSizeProfile { get; set; }

        public PropertyDocument TrimProfile { get; set; }

        public PropertyDocument GridLineProfile { get; set; }

        public Dictionary<EffectType, string> EffectTypeList { get; } = AliasNameExtensions.GetAliasNameDictionary<EffectType>();

        public ObservableCollectionSync<EffectLayer, EffectLayerDocument> EffectLayers { get; set; }


        public void ResetValue()
        {
            var viewComponent = MainViewComponent.Current;

            //using (var lockerKey = viewComponent.ContentRebuild.Locker.Lock())
            {
                Config.Current.ImageResizeFilter.ResizeInterpolation = ResizeInterpolation.Lanczos;
                Config.Current.ImageResizeFilter.IsUnsharpMaskEnabled = true;
                this.UnsharpMaskProfile.Reset();
            }
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddEffectLayerCommand))]
        public partial bool CanAddEffectLayer { get; private set; }

        private void UpdateCanExecute()
        {
            CanAddEffectLayer = Config.Current.ImageEffect.Layers.CanCreateNew();
        }

        [RelayCommand(CanExecute = nameof(CanAddEffectLayer))]
        public void AddEffectLayer()
        {
            Config.Current.ImageEffect.Layers.CreateNew();
        }
    }

}
