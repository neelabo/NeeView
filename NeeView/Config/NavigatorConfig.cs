using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class NavigatorConfig : ObservableObject
    {
        [DefaultEquality] private bool _isVisibleThumbnail;
        [DefaultEquality] private double _thumbnailHeight = 256.0;
        [DefaultEquality] private bool _isVisibleControlBar;
        [DefaultEquality] private bool _isVisibleSlideShow;

        [PropertyMember]
        public bool IsVisibleThumbnail
        {
            get { return _isVisibleThumbnail; }
            set { SetProperty(ref _isVisibleThumbnail, value); }
        }

        [PropertyMember]
        public double ThumbnailHeight
        {
            get { return _thumbnailHeight; }
            set { SetProperty(ref _thumbnailHeight, AppMath.Round(value)); }
        }

        [PropertyMember]
        public bool IsVisibleControlBar
        {
            get { return _isVisibleControlBar; }
            set { SetProperty(ref _isVisibleControlBar, value); }
        }

        [PropertyMember]
        public bool IsVisibleSlideShow
        {
            get { return _isVisibleSlideShow; }
            set { SetProperty(ref _isVisibleSlideShow, value); }
        }
    }
}


