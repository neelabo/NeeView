﻿using NeeView.PageFrames;
using System;
using System.Windows;

namespace NeeView
{
    public static class MediaPlayerCanvasFactory
    {
        public static MediaPlayerCanvas Create(PageFrameElement element, MediaViewData source, ViewContentSize contentSize, Rect viewbox, IMediaPlayer player)
        {
            switch (player)
            {
                case DefaultMediaPlayer mediaPlayer:
                    return new DefaultMediaPlayerCanvas(element, source, contentSize, viewbox, mediaPlayer);
                case VlcMediaPlayer vlcMediaPlayer:
                    return new VlcMediaPlayerCanvas(element, source, contentSize, viewbox, vlcMediaPlayer);
                case AnimatedMediaPlayer animatedMediaPlayer:
                    return new AnimatedMediaPlayerCanvas(element, source, contentSize, viewbox, animatedMediaPlayer);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
