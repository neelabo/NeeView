using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    public class InformationConfig : BindableBase
    {
        private static readonly string _defaultDateTimeFormat = TextResources.GetString("Information.DateFormat");
        private static readonly string _defaultMapProgramFormat = @"https://www.google.com/maps/place/$Lat+$Lon/";
        private GridLength _propertyHeaderWidth = new(128.0);

        private readonly Dictionary<InformationGroup, bool> _groupVisibilityMap = new()
        {
            [InformationGroup.File] = true,
            [InformationGroup.Image] = true,
            [InformationGroup.Description] = true,
            [InformationGroup.Origin] = true,
            [InformationGroup.Camera] = true,
            [InformationGroup.AdvancedPhoto] = true,
            [InformationGroup.Gps] = true,
            [InformationGroup.Extras] = false,
        };

        [JsonInclude, JsonPropertyName(nameof(DateTimeFormat))]
        public string? _dateTimeFormat = null;
        [JsonInclude, JsonPropertyName(nameof(MapProgramFormat))]
        public string? _mapProgramFormat;


        private bool SetVisibleGroup(InformationGroup group, bool isVisible, [CallerMemberName] string? propertyName = null)
        {
            if (_groupVisibilityMap[group] == isVisible) return false;

            _groupVisibilityMap[group] = isVisible;
            RaisePropertyChanged(propertyName);
            return true;
        }

        public bool IsVisibleGroup(InformationGroup group)
        {
            return _groupVisibilityMap[group];
        }


        [JsonIgnore]
        [PropertyMember]
        public string DateTimeFormat
        {
            get { return _dateTimeFormat ?? _defaultDateTimeFormat; }
            set { SetProperty(ref _dateTimeFormat, (string.IsNullOrWhiteSpace(value) || value == _defaultDateTimeFormat) ? null : value); }
        }

        [JsonIgnore]
        [PropertyMember]
        public string MapProgramFormat
        {
            get { return _mapProgramFormat ?? _defaultMapProgramFormat; }
            set { SetProperty(ref _mapProgramFormat, (string.IsNullOrWhiteSpace(value) || value == _defaultMapProgramFormat) ? null : value); }
        }


        [PropertyMember]
        public bool IsVisibleFile
        {
            get { return IsVisibleGroup(InformationGroup.File); }
            set { SetVisibleGroup(InformationGroup.File, value); }
        }

        [PropertyMember]
        public bool IsVisibleImage
        {
            get { return IsVisibleGroup(InformationGroup.Image); }
            set { SetVisibleGroup(InformationGroup.Image, value); }
        }

        [PropertyMember]
        public bool IsVisibleDescription
        {
            get { return IsVisibleGroup(InformationGroup.Description); }
            set { SetVisibleGroup(InformationGroup.Description, value); }
        }

        [PropertyMember]
        public bool IsVisibleOrigin
        {
            get { return IsVisibleGroup(InformationGroup.Origin); }
            set { SetVisibleGroup(InformationGroup.Origin, value); }
        }

        [PropertyMember]
        public bool IsVisibleCamera
        {
            get { return IsVisibleGroup(InformationGroup.Camera); }
            set { SetVisibleGroup(InformationGroup.Camera, value); }
        }

        [PropertyMember]
        public bool IsVisibleAdvancedPhoto
        {
            get { return IsVisibleGroup(InformationGroup.AdvancedPhoto); }
            set { SetVisibleGroup(InformationGroup.AdvancedPhoto, value); }
        }

        [PropertyMember]
        public bool IsVisibleGps
        {
            get { return IsVisibleGroup(InformationGroup.Gps); }
            set { SetVisibleGroup(InformationGroup.Gps, value); }
        }

        [PropertyMember]
        public bool IsVisibleExtras
        {
            get { return IsVisibleGroup(InformationGroup.Extras); }
            set { SetVisibleGroup(InformationGroup.Extras, value); }
        }

        #region HiddenParameters

        [JsonIgnore]
        [PropertyMapIgnore]
        public ReadOnlyDictionary<InformationGroup, bool> GroupVisibilityMap => new(_groupVisibilityMap);

        [PropertyMapIgnore]
        public GridLength PropertyHeaderWidth
        {
            get { return _propertyHeaderWidth; }
            set { SetProperty(ref _propertyHeaderWidth, value); }
        }

        #endregion HiddenParameters
    }
}


