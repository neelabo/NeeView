using NeeView.Susie;
using NeeView.Text;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: FileTypes
    /// </summary>
    public class SettingPageFileTypes : SettingPage
    {
        private class SettingItemCollectionDescription : ISettingItemCollectionDescription
        {
            public StringCollection GetDefaultCollection()
            {
                return PictureFileExtensionTools.CreateDefaultSupportedFileTypes(Config.Current.Image.Standard.UseWicInformation);
            }
        }

        public SettingPageFileTypes() : base(Properties.TextResources.GetString("SettingPage.Archive"))
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageArchiveZip(),
                new SettingPageArchiveSevenZip(),
                new SettingPageArchivePdf(),
                new SettingPageArchiveMedia(),
                new SettingPageSusie(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.FileTypes"));

            var supportFileTypeEditor = new SettingItemCollectionControl() { Collection = (FileTypeCollection)PictureProfile.Current.SupportFileTypes.Clone(), AddDialogHeader = Properties.TextResources.GetString("Word.Extension"), IsAlwaysResetEnabled = true, Description = new SettingItemCollectionDescription() };
            supportFileTypeEditor.CollectionChanged += SupportFileTypeEditor_CollectionChanged;
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.SupportFileTypes)), supportFileTypeEditor));

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.UseWicInformation))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAllFileSupported))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAnimatedGifEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAnimatedPngEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAspectRatioEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsIgnoreImageDpi))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Background, nameof(BackgroundConfig.PageBackgroundColor))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Background, nameof(BackgroundConfig.IsPageBackgroundChecker))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.FileTypes.Svg"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Svg, nameof(ImageSvgConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Svg, nameof(ImageSvgConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Image.Svg.SupportFileTypes, AddDialogHeader = Properties.TextResources.GetString("Word.Extension"), DefaultCollection = ImageSvgConfig.DefaultSupportFileTypes }));
            this.Items.Add(section);
        }

        private void SupportFileTypeEditor_CollectionChanged(object? sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            var editor = (SettingItemCollectionControl?)sender;
            if (editor is null) throw new InvalidOperationException();

            PictureProfile.Current.SupportFileTypes = (FileTypeCollection)editor.Collection;
        }
    }


    /// <summary>
    /// SettingPage: Archive ZIP
    /// </summary>
    public class SettingPageArchiveZip : SettingPage
    {
        public SettingPageArchiveZip() : base(Properties.TextResources.GetString("SettingPage.Archive.Zip"))
        {
            var encodingMap = typeof(ZipEncoding).VisibleAliasNameDictionary();
            encodingMap[ZipEncoding.Local] = encodingMap[ZipEncoding.Local] + " - " + Environment.Encoding.EncodingName;

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Archive.ZipFeature"));

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Zip, nameof(ZipArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Zip, nameof(ZipArchiveConfig.IsFileWriteAccessEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Zip, nameof(ZipArchiveConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Archive.Zip.SupportFileTypes, AddDialogHeader = Properties.TextResources.GetString("Word.Extension"), DefaultCollection = ZipArchiveConfig.DefaultSupportFileTypes }));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Zip, nameof(ZipArchiveConfig.Encoding), new PropertyMemberElementOptions() { EnumMap = encodingMap })));

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// SettingPage: Archive 7-Zip
    /// </summary>
    public class SettingPageArchiveSevenZip : SettingPage
    {
        public SettingPageArchiveSevenZip() : base(Properties.TextResources.GetString("SettingPage.Archive.SevenZip"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Archive.SevenZipFeature"));

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Archive.SevenZip.SupportFileTypes, AddDialogHeader = Properties.TextResources.GetString("Word.Extension"), DefaultCollection = SevenZipArchiveConfig.DefaultSupportFileTypes }));

            if (!Environment.IsX64)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.X86DllPath))) { IsStretch = true, });
            }

            if (Environment.IsX64)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.X64DllPath))) { IsStretch = true });
            }

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// SettingPage: Archive PDF
    /// </summary>
    public class SettingPageArchivePdf : SettingPage
    {
        public SettingPageArchivePdf() : base(Properties.TextResources.GetString("SettingPage.Archive.Pdf"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Archive.PdfFeature"));

            if (PdfArchiveConfig.IsPdfArchiveSupported)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.IsEnabled))));
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.SupportFileTypes)),
                     new SettingItemCollectionControl() { Collection = Config.Current.Archive.Pdf.SupportFileTypes, AddDialogHeader = Properties.TextResources.GetString("Word.Extension"), DefaultCollection = PdfArchiveConfig.DefaultSupportFileTypes }));
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.RenderSize))));
            }
            else
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.IsEnabled))) { IsEnabled = new IsEnabledPropertyValue(false) });
                section.Children.Add(new SettingItemHeader("Not supported on this OS."));
            }

            this.Items = new List<SettingItem>() { section };

        }
    }


    /// <summary>
    /// SettingPage: Archive Media
    /// </summary>
    public class SettingPageArchiveMedia : SettingPage
    {
        public SettingPageArchiveMedia() : base(Properties.TextResources.GetString("SettingPage.Archive.Media"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Archive.MediaFeature"));

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.IsMediaPageEnabled))));

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Archive.Media.SupportFileTypes, AddDialogHeader = Properties.TextResources.GetString("Word.Extension"), DefaultCollection = MediaArchiveConfig.DefaultSupportFileTypes }));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.PageSeconds))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.MediaStartDelaySeconds))));

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.IsLibVlcEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.LibVlcPath)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Archive.Media, nameof(MediaArchiveConfig.IsLibVlcEnabled)),
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.DefaultSubtitle)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Archive.Media, nameof(MediaArchiveConfig.IsLibVlcEnabled)),
            });

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: Susie
    /// </summary>
    public class SettingPageSusie : SettingPage
    {
        public SettingPageSusie() : base(Properties.TextResources.GetString("SettingPage.Susie"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Susie.GeneralGeneral"), Properties.TextResources.GetString("SettingPage.Susie.GeneralGeneral.Remarks"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.SusiePluginPath)))
            {
                IsStretch = true,
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Susie, nameof(SusieConfig.IsEnabled)),
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsFirstOrderSusieImage))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsFirstOrderSusieArchive))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Susie.ImagePlugin"));
            section.Children.Add(new SettingItemSusiePlugin(SusiePluginType.Image));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Susie.ArchivePlugin"));
            section.Children.Add(new SettingItemSusiePlugin(SusiePluginType.Archive));
            this.Items.Add(section);
        }
    }

}
