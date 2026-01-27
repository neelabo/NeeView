using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// ImageEffect : ViewModel
    /// </summary>
    public class ImageEffectViewModel : BindableBase
    {
        public ImageEffectViewModel(ImageEffect model)
        {
            _model = model;

            this.UnsharpMaskProfile = new PropertyDocument(Config.Current.ImageResizeFilter.UnsharpMask);

            this.CustomSizeProfile = new PropertyDocument(Config.Current.ImageCustomSize);
            this.CustomSizeProfile.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);

            this.TrimProfile = new PropertyDocument(Config.Current.ImageTrim);
            this.TrimProfile.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);

            this.GridLineProfile = new PropertyDocument(Config.Current.ImageGrid);
            this.GridLineProfile.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);
            this.GridLineProfile.SetVisualType<PropertyValue_Color>(PropertyVisualType.ComboColorPicker);
        }


        /// <summary>
        /// Model property.
        /// </summary>
        private ImageEffect _model;
        public ImageEffect Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        // PictureProfile
        public PictureProfile PictureProfile => PictureProfile.Current;

        public PropertyDocument UnsharpMaskProfile { get; set; }

        public PropertyDocument CustomSizeProfile { get; set; }

        public PropertyDocument TrimProfile { get; set; }

        public PropertyDocument GridLineProfile { get; set; }

        public Dictionary<EffectType, string> EffectTypeList { get; } = AliasNameExtensions.GetAliasNameDictionary<EffectType>();


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

    }

}
