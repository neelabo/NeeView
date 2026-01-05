using System;

namespace NeeView
{
    public class WebPDecoderException : Exception
    {
        public WebPDecoderException()
        {
        }
        public WebPDecoderException(string message)
            : base(message)
        {
        }
        public WebPDecoderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}