//#define LOCAL_DEBUG

using System;
using System.Collections.Generic;

namespace NeeView
{
    public interface IPlaylistEditor : IDisposable
    {
        void Delete(IEnumerable<PlaylistSourceItem> items);
        string? Rename(PlaylistSourceItem item, string newName);
    }
}
