namespace NeeView
{
    [DocumentableBaseClass(typeof(PageAccessor))]
    public record class ViewPageAccessor : PageAccessor
    {
        public ViewPageAccessor(Page page) : base(page)
        {
        }

        [WordNodeMember]
        public double Width => this.Source.GetContentPictureInfo()?.OriginalSize.Width ?? 0.0;

        [WordNodeMember]
        public double Height => this.Source.GetContentPictureInfo()?.OriginalSize.Height ?? 0.0;

        [WordNodeMember]
        public MediaPlayerAccessor? Player
        {
            get
            {
                var player = BookOperation.Current.GetMediaPlayer(Source);
                return player is not null ? new MediaPlayerAccessor(player) : null;
            }
        }


        [WordNodeMember]
        public PageAccessor GetPageAccessor() => new PageAccessor(Source);
    }
}
