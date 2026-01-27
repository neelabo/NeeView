using System;
using System.IO;

namespace NeeLaboratory.IO
{
    public static class MemoryStreamExtensions
    {
        /// <summary>
        /// stream to byte span (MemoryStream)
        /// </summary>
        /// <param name="stream">source stream</param>
        /// <returns></returns>
        public static ReadOnlySpan<byte> ToSpan(this MemoryStream stream)
        {
            return new ReadOnlySpan<byte>(stream.GetBuffer(), 0, (int)stream.Length);
        }

    }
}
