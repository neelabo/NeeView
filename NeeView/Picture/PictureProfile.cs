using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    public class PictureProfile : BindableBase
    {
        static PictureProfile() => Current = new PictureProfile();
        public static PictureProfile Current { get; }

        public static string[] _animatedGifExtensions = new string[] { ".gif" };
        public static string[] _animatedPngExtensions = new string[] { ".png", ".apng" };
        public static string[] _animatedWebpExtensions = new string[] { ".webp" };


        private PictureProfile()
        {
        }


        [PropertyMember]
        public FileTypeCollection SupportFileTypes
        {
            get
            {
                if (Config.Current.Image.Standard.SupportFileTypes is null)
                {
                    // NOTE: fall through. don't come here!
                    Config.Current.Image.Standard.SupportFileTypes = PictureFileExtensionTools.CreateDefaultSupportedFileTypes(Config.Current.Image.Standard.UseWicInformation);
                }
                return Config.Current.Image.Standard.SupportFileTypes;
            }
            set { Config.Current.Image.Standard.SupportFileTypes = value; }
        }


        // 対応拡張子判定 (ALL)
        public bool IsSupported(string fileName, bool includeMedia)
        {
            string ext = LoosePath.GetExtension(fileName);

            if (SupportFileTypes.Contains(ext)) return true;

            if (Config.Current.Susie.IsEnabled)
            {
                if (SusiePluginManager.Current.ImageExtensions.Contains(ext)) return true;
            }

            if (Config.Current.Image.Svg.IsEnabled)
            {
                if (Config.Current.Image.Svg.SupportFileTypes.Contains(ext)) return true;
            }

            if (Config.Current.Archive.Media.IsMediaPageEnabled && includeMedia)
            {
                if (Config.Current.Archive.Media.SupportFileTypes.Contains(ext)) return true;
            }

            return false;
        }

        public IEnumerable<string> GetFileTypes(bool includeMedia)
        {
            foreach (var item in SupportFileTypes.Items)
            {
                yield return item;
            }

            if (Config.Current.Susie.IsEnabled)
            {
                foreach (var item in SusiePluginManager.Current.ImageExtensions.Items)
                {
                    yield return item;
                }
            }

            if (Config.Current.Image.Svg.IsEnabled)
            {
                foreach (var item in Config.Current.Image.Svg.SupportFileTypes.Items)
                {
                    yield return item;
                }
            }

            if (Config.Current.Archive.Media.IsMediaPageEnabled && includeMedia)
            {
                foreach (var item in Config.Current.Archive.Media.SupportFileTypes.Items)
                {
                    yield return item;
                }
            }
        }

        // 対応拡張子判定 (標準)
        public bool IsDefaultSupported(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);
            return SupportFileTypes.Contains(ext);
        }

        // 対応拡張子判定 (Susie)
        public bool IsSusieSupported(string fileName)
        {
            if (!Config.Current.Susie.IsEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return SusiePluginManager.Current.ImageExtensions.Contains(ext);
        }

        // 対応拡張子判定 (Svg)
        public bool IsSvgSupported(string fileName)
        {
            if (!Config.Current.Image.Svg.IsEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return Config.Current.Image.Svg.SupportFileTypes.Contains(ext);
        }

        // 対応拡張子判定 (Media)
        public bool IsMediaSupported(string fileName)
        {
            if (!Config.Current.Archive.Media.IsMediaPageEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return Config.Current.Archive.Media.SupportFileTypes.Contains(ext);
        }

        // 拡張子判定 (Animated Gif)
        public bool IsAnimatedGifSupported(string fileName)
        {
            if (!Config.Current.Image.Standard.IsAnimatedGifEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return _animatedGifExtensions.Contains(ext);
        }

        // 拡張子判定 (Animated Png)
        public bool IsAnimatedPngSupported(string fileName)
        {
            if (!Config.Current.Image.Standard.IsAnimatedPngEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return _animatedPngExtensions.Contains(ext);
        }

        // 拡張子判定 (Animated Webp)
        public bool IsAnimatedWebpSupported(string fileName)
        {
            if (!Config.Current.Image.Standard.IsAnimatedWebpEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return _animatedWebpExtensions.Contains(ext);
        }

        // 最大サイズ内におさまるサイズを返す
        public static Size CreateFixedSize(Size size)
        {
            if (size.IsEmpty) return size;

            return size.Limit(Config.Current.Performance.MaximumSize);
        }

    }
}
