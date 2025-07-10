using NeeView.Properties;
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace NeeView
{
    public class NotSupportedFileTypeException : Exception
    {
        public NotSupportedFileTypeException(string extension) : base(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("Notice.NotSupportedFileType"), extension))
        {
            Extension = extension;
        }

        public NotSupportedFileTypeException(string extension, string message) : base(message)
        {
            Extension = extension;
        }

        public NotSupportedFileTypeException(string extension, string message, Exception inner) : base(message)
        {
            Extension = extension;
        }

        public string Extension { get; set; }
    }
}
