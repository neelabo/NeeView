using System;

namespace NeeView
{
    public static class PlaylistSourceValidator
    {
        public static PlaylistSource Validate(this PlaylistSource self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("Playlist.Format must not be null.");

            // 45.0 alpha.4 でのフォーマットバージョンを修正
            if (self.Format.CompareTo(new FormatVersion(PlaylistSource.FormatName, 45, 0, 3981)) == 0)
            {
                self.Format = new FormatVersion(PlaylistSource.FormatName, 2, 0, 0);
            }

            // playlist ver 2.0.1
            if (self.Format.CompareTo(new FormatVersion(PlaylistSource.FormatName, VersionNumber.Playlist2_0_1)) < 0)
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
