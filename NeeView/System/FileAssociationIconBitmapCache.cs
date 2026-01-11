//#define LOCAL_DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public partial class FileAssociationIconBitmapCache
    {
        private readonly Dictionary<string, BitmapSource?> _cache = [];
        private readonly Lock _lock = new();

        public BitmapSource? GetBitmapSource(FileAssociationIcon icon)
        {
            lock (_lock)
            {
                var literal = icon.CreateDefaultIconLiteral();
                if (!_cache.TryGetValue(literal, out BitmapSource? bitmapSource))
                {
                    try
                    {
                        bitmapSource = FileAssociationTools.GetBitmapSource(icon);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        bitmapSource = null;
                    }
                    _cache.Add(literal, bitmapSource);
                }
                return bitmapSource;
            }
        }
    }
}
