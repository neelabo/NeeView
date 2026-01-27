using NeeLaboratory.Generators;
using System;
using System.Windows;

namespace NeeView.Windows
{
    public partial class DpiScaleProvider : IDpiScaleProvider
    {
        [Subscribable]
        public event EventHandler? DpiChanged;


        public DpiScale DpiScale { get; private set; } = new DpiScale(1, 1);


        public bool SetDipScale(DpiScale dpi)
        {
            if (DpiScale.DpiScaleX != dpi.DpiScaleX || DpiScale.DpiScaleY != dpi.DpiScaleY)
            {
                DpiScale = dpi;
                DpiChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            else
            {
                return false;
            }
        }

        public DpiScale GetDpiScale()
        {
            return DpiScale;
        }
    }

}
