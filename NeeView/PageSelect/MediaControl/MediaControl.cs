using NeeLaboratory.ComponentModel;
using System;

namespace NeeView
{


    public class MediaControl : BindableBase
    {
        public MediaControl()
        {
        }

        public event EventHandler<MediaPlayerChanged>? Changed;

        public MediaPlayerChanged LastChangedArgs { get; private set; } = new();


        public void RaiseContentChanged(object sender, MediaPlayerChanged e)
        {
            LastChangedArgs = e;
            Changed?.Invoke(sender, e);
        }
    }


    /// <summary>
    /// MediaPlayer変更通知パラメータ
    /// </summary>
    public class MediaPlayerChanged : EventArgs
    {
        public MediaPlayerChanged()
        {
        }

        public MediaPlayerChanged(ViewContentMediaPlayer player, bool isLastStart)
        {
            MediaPlayer = player;
            IsLastStart = isLastStart;
        }

        public ViewContentMediaPlayer? MediaPlayer { get; set; }
        public bool IsLastStart { get; set; }
        public bool IsValid => MediaPlayer != null;
        public bool IsMainMediaPlayer { get; init; }
    }
}
