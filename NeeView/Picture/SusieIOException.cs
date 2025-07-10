using NeeView.Properties;
using System;

namespace NeeView
{
    /// <summary>
    /// Susie 例外
    /// </summary>
    public class SusieIOException : Exception
    {
        public SusieIOException() : base(TextResources.GetString("SusieLoadFailedException.Message"))
        {
        }

        public SusieIOException(string message) : base(message)
        {
        }

        public SusieIOException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
