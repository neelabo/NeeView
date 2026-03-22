using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(IgnoreInheritedMembers = true)]
    public partial class NavigatorConfig : BindableBase
    {
        private bool _isVisibleThumbnail;
        private double _thumbnailHeight = 256.0;
        private bool _isVisibleControlBar;

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
            set { SetProperty(ref _thumbnailHeight, value); }
        }

        [PropertyMember]
        public bool IsVisibleControlBar
        {
            get { return _isVisibleControlBar; }
            set { SetProperty(ref _isVisibleControlBar, value); }
        }


    }
}


