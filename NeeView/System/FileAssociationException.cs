//#define LOCAL_DEBUG

using System;

namespace NeeView
{
    public class FileAssociationException : Exception
    {
        public FileAssociationException()
        {
        }

        public FileAssociationException(string? message) : base(message)
        {
        }

        public FileAssociationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
