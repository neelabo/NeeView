using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NeeView
{
    public static class WebPDecoder
    {
        private static class NativeMethods
        {
            [DllImport("libwebp", CallingConvention = CallingConvention.Cdecl)]
            public static extern int WebPGetInfo(IntPtr data, UIntPtr data_size, out int width, out int height);

            [DllImport("libwebp", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr WebPDecodeBGRAInto(IntPtr data, UIntPtr data_size, IntPtr output_buffer, UIntPtr output_buffer_size, int output_stride);

            [DllImport("libwebp", CallingConvention = CallingConvention.Cdecl)]
            public static extern void WebPFree(IntPtr ptr);

            [DllImport("libwebp", CallingConvention = CallingConvention.Cdecl)]
            public static extern int WebPGetFeaturesInternal(IntPtr data, UIntPtr data_size, ref WebPBitstreamFeatures features, int abi_version);

            public const int WEBP_DECODER_ABI_VERSION = 0x0209;

            [StructLayout(LayoutKind.Sequential)]
            public struct WebPBitstreamFeatures
            {
                public int width;
                public int height;
                public int has_alpha;
                public int has_animation;
                public int format;
                private readonly uint pad1;
                private readonly uint pad2;
                private readonly uint pad3;
                private readonly uint pad4;
                private readonly uint pad5;
            }

            [DllImport("libwebpdemux", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr WebPAnimDecoderNewInternal(ref WebPData webp_data, ref WebPAnimDecoderOptions dec_options, int abi_version);

            public const int WEBP_DEMUX_ABI_VERSION = 0x0107;

            [StructLayout(LayoutKind.Sequential)]
            public struct WebPData
            {
                public IntPtr data;
                public UIntPtr size;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct WebPAnimDecoderOptions
            {
                public int color_mode;
                public int use_threads;
                private readonly uint pad1;
                private readonly uint pad2;
                private readonly uint pad3;
                private readonly uint pad4;
                private readonly uint pad5;
                private readonly uint pad6;
                private readonly uint pad7;
            };

            public const int WEBP_CSP_MODE_BGRA = 3;

            [DllImport("libwebpdemux", CallingConvention = CallingConvention.Cdecl)]
            public static extern int WebPAnimDecoderGetInfo(IntPtr dec, out WebPAnimInfo info);

            [StructLayout(LayoutKind.Sequential)]
            public struct WebPAnimInfo
            {
                public uint canvas_width;
                public uint canvas_height;
                public uint loop_count;
                public uint bgcolor;
                public uint frame_count;
                private readonly uint pad1;
                private readonly uint pad2;
                private readonly uint pad3;
                private readonly uint pad4;
            }

            [DllImport("libwebpdemux", CallingConvention = CallingConvention.Cdecl)]
            public static extern int WebPAnimDecoderGetNext(IntPtr dec, out IntPtr buf, out int timestamp);

            [DllImport("libwebpdemux", CallingConvention = CallingConvention.Cdecl)]
            public static extern void WebPAnimDecoderDelete(IntPtr dec);
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

        public static bool IsWebP(byte[] bytes)
        {
            return AnimatedImageChecker.IsWebp(new MemoryStream(bytes, false));
        }

        public static RawImage Decode(byte[] bytes)
        {
            Initialize();

            var webpDataHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var webpData = new NativeMethods.WebPData();
            webpData.data = webpDataHandle.AddrOfPinnedObject();
            webpData.size = (UIntPtr)bytes.Length;

            try
            {
                var features = new NativeMethods.WebPBitstreamFeatures();
                if (NativeMethods.WebPGetFeaturesInternal(webpData.data, webpData.size, ref features, NativeMethods.WEBP_DECODER_ABI_VERSION) != 0)
                {
                    throw new WebPDecoderException("WebPGetFeatures failed");
                }

                if (features.has_animation == 0)
                {
                    return DecodeImage(webpData, features.width, features.height);
                }
                else
                {
                    return DecodeFirstFrame(webpData);
                }
            }
            finally
            {
                webpDataHandle.Free();
            }
        }

        private static RawImage DecodeImage(NativeMethods.WebPData webpData, int width, int height)
        {
            int stride = width * 4;
            int bufferSize = stride * height;
            byte[] buffer = new byte[bufferSize];
            var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var bufferPtr = bufferHandle.AddrOfPinnedObject();

            try
            {
                var ptr = NativeMethods.WebPDecodeBGRAInto(webpData.data, webpData.size, bufferPtr, (UIntPtr)buffer.Length, stride);
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

        private static RawImage DecodeFirstFrame(NativeMethods.WebPData webpData)
        {
            var options = new NativeMethods.WebPAnimDecoderOptions();
            options.color_mode = NativeMethods.WEBP_CSP_MODE_BGRA;

            IntPtr decoder = IntPtr.Zero;
            try
            {
                decoder = NativeMethods.WebPAnimDecoderNewInternal(ref webpData, ref options, NativeMethods.WEBP_DEMUX_ABI_VERSION);
                if (decoder == 0)
                {
                    throw new WebPDecoderException("WebPAnimDecoderNew failed");
                }

                if (NativeMethods.WebPAnimDecoderGetInfo(decoder, out NativeMethods.WebPAnimInfo info) == 0)
                {
                    throw new WebPDecoderException("WebPAnimDecoderGetInfo failed");
                }

                if (NativeMethods.WebPAnimDecoderGetNext(decoder, out IntPtr rgbaPtr, out int timestamp) == 0)
                {
                    throw new WebPDecoderException("WebPAnimDecoderGetNext failed");
                }

                int stride = (int)info.canvas_width * 4;
                int bufferSize = stride * (int)info.canvas_height;
                byte[] buffer = new byte[bufferSize];
                Marshal.Copy(rgbaPtr, buffer, 0, buffer.Length);
                return new RawImage(buffer, (int)info.canvas_width, (int)info.canvas_height);
            }
            finally
            {
                if (decoder != IntPtr.Zero)
                {
                    NativeMethods.WebPAnimDecoderDelete(decoder);
                }
            }
        }
    }
}