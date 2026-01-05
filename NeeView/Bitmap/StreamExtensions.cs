using System;
using System.IO;

namespace NeeView
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream stream)
        {
            if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> segment))
            {
                return segment.Array ?? [];
            }

            using (var memory = new MemoryStream())
            {
                stream.CopyTo(memory);
                return memory.ToArray();
            }
        }
    }
}