using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// FileCard.xaml の相互作用ロジック
    /// </summary>
    public partial class AudioCard : UserControl
    {
        public AudioCard()
        {
            InitializeComponent();
            this.Root.DataContext = this;
            this.Root.Loaded += (s, e) => Update();
        }


        public AudioInfo? AudioInfo
        {
            get { return (AudioInfo)GetValue(AudioInfoProperty); }
            set { SetValue(AudioInfoProperty, value); }
        }

        public static readonly DependencyProperty AudioInfoProperty =
            DependencyProperty.Register("AudioInfo", typeof(AudioInfo), typeof(AudioCard), new PropertyMetadata(null, OnAudioInfoPropertyChanged));

        private static void OnAudioInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioCard control)
            {
                control.Update();
            }
        }





        private void Update()
        {
            if (this.AudioInfo is not null)
            {
                var isSimle = string.IsNullOrEmpty(AudioInfo.Title);
                this.Body.Visibility = Visibility.Visible;
                this.Title.Text = isSimle ? LoosePath.GetFileNameWithoutExtension(AudioInfo.ArchiveEntry.EntryLastName) : AudioInfo.Title;
                this.Description.Text = AudioInfo.Artist + (string.IsNullOrEmpty(AudioInfo.Album) ? "" : " / " + AudioInfo.Album);
                this.Description.Visibility = isSimle ? Visibility.Collapsed : Visibility.Visible;
                this.DefaultImage.Visibility = this.AudioInfo.CoverImage != null ? Visibility.Hidden : Visibility.Visible;
                this.CoverImage.Visibility = this.AudioInfo.CoverImage != null ? Visibility.Visible : Visibility.Hidden;
                this.CoverImage.Source = this.AudioInfo.CoverImage;
            }
            else
            {
                this.Body.Visibility = Visibility.Collapsed;
            }
        }
    }
}
