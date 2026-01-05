using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NeeView
{
    public static class WebPDecoder
    {
        private static class NativeMethods
        {
            [DllImport("libwebp.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int WebPGetInfo(IntPtr data, UIntPtr data_size, out int width, out int height);

            [DllImport("libwebp.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr WebPDecodeBGRAInto(IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride);

            [DllImport("libwebp.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern void WebPFree(IntPtr ptr);
        }


        public static void Initialize()
        {
            // NOTE: AnimatedImage で libwebp.dll を読み込む
            var isSuccess = AnimatedImage.Config.CheckWebpSupport();
            if (!isSuccess)
            {
                throw new WebPDecoderException("Failed to load libwebp");
            }
        }

        public static bool IsWebP(byte[] webpData)
        {
            return AnimatedImageChecker.IsWebp(new MemoryStream(webpData, false));
        }

        public static RawImage Decode(byte[] webpData)
        {
            Initialize();

            var webpDataHandle = GCHandle.Alloc(webpData, GCHandleType.Pinned);
            var webpDataPtr = webpDataHandle.AddrOfPinnedObject();
            try
            {
                var result = NativeMethods.WebPGetInfo(webpDataPtr, (UIntPtr)webpData.Length, out int width, out int height);
                if (result == 0)
                {
                    throw new WebPDecoderException("WebPGetInfo failed");
                }

                int stride = width * 4;
                int bufferSize = stride * height;
                byte[] buffer = new byte[bufferSize];

                var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var bufferPtr = bufferHandle.AddrOfPinnedObject();
                try
                {
                    var ptr = NativeMethods.WebPDecodeBGRAInto(webpDataPtr, (UIntPtr)webpData.Length, bufferPtr, (UIntPtr)buffer.Length, stride);
                    if (ptr == IntPtr.Zero)
                    {
                        throw new WebPDecoderException("WebPDecode failed");
                    }

                    return new RawImage(buffer, width, height);
                }
                finally
                {
                    bufferHandle.Free();
                }
            }
            finally
            {
                webpDataHandle.Free();
            }
        }
    }
}