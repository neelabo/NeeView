using NeeView.Media.Imaging.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public enum InformationKey
    {
        // File
        FileName = (InformationCategory.File << 24) | (InformationGroup.File << 16) | 0,
        FilePath,
        CreationTime,
        LastWriteTime,
        FileSize,
        ArchivePath,
        Archiver,

        // Image
        Dimensions = (InformationCategory.Image << 24) | (InformationGroup.Image << 16) | 0,
        //Width,
        //Height,
        HorizontalResolution,
        VerticalResolution,
        BitDepth,
        Decoder,

        // Description
        Title = (InformationCategory.Metadata << 24) | (InformationGroup.Description << 16) | BitmapMetadataKey.Title,
        Subject,
        Rating,
        Tags,
        Comments,

        // Origin
        Author = (InformationCategory.Metadata << 24) | (InformationGroup.Origin << 16) | BitmapMetadataKey.Author,
        DateTaken,
        ApplicationName,
        DateAcquired,
        Copyright,

        // Camera
        CameraMaker = (InformationCategory.Metadata << 24) | (InformationGroup.Camera << 16) | BitmapMetadataKey.CameraMaker,
        CameraModel,
        FNumber,
        ExposureTime,
        ISOSpeed,
        ExposureBias,
        FocalLength,
        MaxAperture,
        MeteringMode,
        SubjectDistance,
        FlashMode,
        FlashEnergy,
        FocalLengthIn35mmFilm,

        // Advanced photo
        LensMaker = (InformationCategory.Metadata << 24) | (InformationGroup.AdvancedPhoto << 16) | BitmapMetadataKey.LensMaker,
        LensModel,
        FlashMaker,
        FlashModel,
        CameraSerialNumber,
        Contrast,
        Brightness,
        LightSource,
        ExposureProgram,
        Saturation,
        Sharpness,
        WhiteBalance,
        PhotometricInterpretation,
        DigitalZoom,
        Orientation,
        EXIFVersion,

        // GPS
        GPSLatitude = (InformationCategory.Metadata << 24) | (InformationGroup.Gps << 16) | BitmapMetadataKey.GPSLatitude,
        GPSLongitude,
        GPSAltitude,

        // Extras
        ExtraValue = (InformationCategory.Metadata << 24) | (InformationGroup.Extras << 16) | 0,
        ExtraEmpty,
    }


    public static class InformationKeyExtensions
    {
        private static List<InformationKey> _defaultKeys;
        private static Dictionary<string, InformationKey> _nameMap;

        static InformationKeyExtensions()
        {
            _defaultKeys = Enum.GetValues(typeof(InformationKey))
                .Cast<InformationKey>()
                .Where(e => !IsExtra(e))
                .ToList();

            _nameMap = _defaultKeys.ToDictionary(e => e.ToString().ToLowerInvariant());

            //foreach(var pair in _nameMap)
            //{
            //    Debug.WriteLine($"<tr><td>{pair.Value.ToString()}</td><td>{pair.Value.ToAliasName()}</td></tr>");
            //}

#if DEBUG
            // check
            foreach (InformationCategory category in Enum.GetValues(typeof(InformationKey)).Cast<InformationKey>().Select(e => e.ToInformationCategory()))
            {
                Debug.Assert(Enum.IsDefined(typeof(InformationCategory), category));
            }

            foreach (InformationGroup group in Enum.GetValues(typeof(InformationKey)).Cast<InformationKey>().Select(e => e.ToInformationGroup()))
            {
                Debug.Assert(Enum.IsDefined(typeof(InformationGroup), group));
            }

            {
                var a = _defaultKeys.Where(e => e.ToInformationCategory() == InformationCategory.Metadata).Select(e => e.ToString());
                var b = Enum.GetValues(typeof(BitmapMetadataKey)).Cast<BitmapMetadataKey>().Where(e => e.IsPublic()).Select(e => e.ToString());
                Debug.Assert(!a.Except(b).Any());
                Debug.Assert(!b.Except(a).Any());
            }
#endif
        }


        public static IEnumerable<InformationKey> DefaultKeys => _defaultKeys;


        public static bool IsExtra(this InformationKey key)
        {
            return ToInformationGroup(key) == InformationGroup.Extras;
        }

        public static InformationCategory ToInformationCategory(this InformationKey key)
        {
            return (InformationCategory)(((int)key >> 24) & 0xFF);
        }

        public static InformationGroup ToInformationGroup(this InformationKey key)
        {
            return (InformationGroup)(((int)key >> 16) & 0xFF);
        }

        public static BitmapMetadataKey ToBitmapMetadataKey(this InformationKey key)
        {
            if (key.ToInformationCategory() == InformationCategory.Metadata)
            {
                return (BitmapMetadataKey)(((int)key) & 0xFFFF);
            }

            throw new NotSupportedException();
        }

        public static bool TryParse(string key, out InformationKey value)
        {
            return _nameMap.TryGetValue(key, out value);
        }
    }
}
