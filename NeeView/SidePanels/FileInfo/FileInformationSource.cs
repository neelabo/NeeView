using NeeLaboratory.ComponentModel;
using NeeView.Media.Imaging.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView
{

    public class FileInformationSource : BindableBase, IRenameable
    {
        private List<FileInformationRecord>? _properties;


        public FileInformationSource(ViewContent viewContent)
        {
            ViewContent = viewContent;

            Update();
        }

        public ViewContent ViewContent { get; private set; }

        public Page Page => ViewContent.Element.Page;

        public PictureInfo? PictureInfo => Page.Content.PictureInfo;

        public BitmapMetadataDatabase? Metadata => PictureInfo?.Metadata;

        public double IconMaxSize => 96.0;

        public FrameworkElement? Icon => CreateIcon();

        public List<FileInformationRecord>? Properties
        {
            get { return _properties; }
            set { SetProperty(ref _properties, value); }
        }

        public GpsLocation? GpsLocation { get; private set; }


        public static List<FileInformationRecord> CreatePropertiesTemplate()
        {
            return new List<FileInformationRecord>(InformationKeyExtensions.DefaultKeys
                .Select(e => new FileInformationRecord(e, null)));
        }

        public void Update()
        {
            this.GpsLocation = CreateGpsLocate();
            this.Properties = CreateProperties();
        }

        public List<FileInformationRecord> CreateProperties()
        {
            var factory = new InformationValueFactory(Page);
            var defaults = InformationKeyExtensions.DefaultKeys.Select(e => new FileInformationRecord(e, factory.Create(e)));
            var extras = factory.GetExtraMap().Select(e => new FileInformationRecord(e.Key, InformationGroup.Extras, e.Value));
            return defaults.Concat(extras).ToList();
        }

        private GpsLocation? CreateGpsLocate()
        {
            if (Metadata != null && Metadata[BitmapMetadataKey.GPSLatitude] is ExifGpsDegree lat && Metadata[BitmapMetadataKey.GPSLongitude] is ExifGpsDegree lon)
            {
                return new GpsLocation(lat, lon);
            }
            return null;
        }


        public FrameworkElement? CreateIcon()
        {
            var imageSource = (ViewContent.ViewSource as IHasImageSource)?.ImageSource;

            if (imageSource != null)
            {
                return CreateBitmapContentIcon(imageSource);
            }
            else if (Page?.ArchiveEntry != null)
            {
                var entry = Page.ArchiveEntry;

                if (entry.IsDirectory)
                {
                    if (entry.IsFileSystem)
                    {
                        return CreateSymbolFolderIcon();
                    }
                    else
                    {
                        return CreateSymbolIcon("/Archive");
                    }
                }
                else
                {
                    return CreateSymbolIcon(LoosePath.GetExtension(entry.EntryName).ToUpperInvariant());
                }
            }

            return null;
        }

        private FrameworkElement? CreateBitmapContentIcon(ImageSource imageSource)
        {
            var length = imageSource.Width > imageSource.Height ? imageSource.Width : imageSource.Height;
            var retio = IconMaxSize / length;

            var image = new Image()
            {
                Source = imageSource,
                Width = imageSource.Width * retio,
                Height = imageSource.Height * retio,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                UseLayoutRounding = true,
                Effect = new DropShadowEffect()
                {
                    ShadowDepth = 2.0,
                    Opacity = 0.5
                },
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            return image;
        }

        private FrameworkElement CreateSymbolFolderIcon()
        {
            var imageSource = FileIconCollection.Current.CreateDefaultFolderIcon().GetBitmapSource(256.0);

            var image = new Image()
            {
                Source = imageSource,
                Width = 64.0,
                Height = 64.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                UseLayoutRounding = true,
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            return CreateSymbolIcon(image);
        }

        private FrameworkElement CreateSymbolIcon(string text)
        {
            var border = new Border()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = IconMaxSize,
                Child = new TextBlock()
                {
                    Text = text,
                    FontSize = 20.0,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5),
                },
            };
            return CreateSymbolIcon(border);
        }

        private FrameworkElement CreateSymbolIcon(UIElement content)
        {
            var border = new Border()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = IconMaxSize,
                Height = IconMaxSize,
                Background = new SolidColorBrush(Color.FromArgb(0x10, 0x80, 0x80, 0x80)),
                Child = content,
            };

            return border;
        }

        public bool CanOpenPlace()
        {
            var place = Page?.GetFolderOpenPlace();
            return !string.IsNullOrWhiteSpace(place);
        }

        public void OpenPlace()
        {
            var place = Page?.GetFolderOpenPlace();
            if (!string.IsNullOrWhiteSpace(place))
            {
                ExternalProcess.OpenWithFileManager(place);
            }
        }

        public bool CanOpenMap()
        {
            return GpsLocation != null;
        }

        public void OpenMap()
        {
            GpsLocation?.OpenMap(Config.Current.Information.MapProgramFormat);
        }

        #region IRenameable

        public string GetRenameText()
        {
            return ((IRenameable)Page).GetRenameText();
        }

        public bool CanRename()
        {
            return ((IRenameable)Page).CanRename();
        }

        public ValueTask<bool> RenameAsync(string name)
        {
            return ((IRenameable)Page).RenameAsync(name);
        }

        #endregion IRenameable
    }
}
