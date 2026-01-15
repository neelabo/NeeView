using System;

namespace NeeView
{
    public static class PlaylistSourceValidator
    {
        public static PlaylistSource Validate(this PlaylistSource self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("Playlist.Format must not be null.");

            // ver 45.0
            if (self.Format.CompareTo(new FormatVersion(PlaylistSource.FormatName, 45, 0, 3978)) < 0)
            {
                // UNCパスの正規化
                foreach (var item in self.Items)
                {
                    if (item.Path is not null)
                    {
                        item.Path = UncPathTools.ConvertPathToNormalized(item.Path);
                    }
                }
            }

            return self;
        }
    }
}
