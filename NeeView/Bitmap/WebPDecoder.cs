using NeeLaboratory.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public static bool IsWebP(Stream stream)
        {
            return AnimatedImageChecker.IsWebp(stream);
        }

        public static bool IsWebP(Memory<byte> memory)
        {
            throw new NotImplementedException();
        }

        public static WriteableBitmap Decode(Stream stream)
        {
            var memory = stream.ToMemory();
            return Decode(memory);
        }

        public static WriteableBitmap Decode(Memory<byte> memory)
        {
            Initialize();

            using var webpData = new WebPDataHandle(memory);

            var features = new NativeMethods.WebPBitstreamFeatures();
            if (NativeMethods.WebPGetFeaturesInternal(webpData.data, webpData.size, ref features, NativeMethods.WEBP_DECODER_ABI_VERSION) != 0)
            {
                throw new WebPDecoderException("WebPGetFeatures failed");
            }

            if (features.has_animation == 0)
            {
                return DecodeImage(webpData.WebPData, features.width, features.height);
            }
            else
            {
                return DecodeFirstFrame(webpData.WebPData);
            }
        }

        private static unsafe WriteableBitmap DecodeImage(NativeMethods.WebPData webpData, int width, int height)
        {
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            int stride = wb.BackBufferStride;
            int bufferSize = stride * height;
            Debug.Assert(wb.BackBufferStride == width * 4);

            wb.Lock();
            try
            {
                IntPtr result = NativeMethods.WebPDecodeBGRAInto(webpData.data, webpData.size, (IntPtr)wb.BackBuffer, (UIntPtr)bufferSize, stride);
                if (result == IntPtr.Zero)
                {
                    throw new Exception("WebPDecodeBGRAInto failed.");
                }
                wb.AddDirtyRect(new Int32Rect(0, 0, width, height));
            }
            finally
            {
                wb.Unlock();
            }

            return wb;
        }

        private static unsafe WriteableBitmap DecodeFirstFrame(NativeMethods.WebPData webpData)
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

                var width = (int)info.canvas_width;
                var height = (int)info.canvas_height;

                if (NativeMethods.WebPAnimDecoderGetNext(decoder, out IntPtr rgbaPtr, out int timestamp) == 0)
                {
                    throw new WebPDecoderException("WebPAnimDecoderGetNext failed");
                }

                byte* src = (byte*)rgbaPtr;
                
                var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                int stride = wb.BackBufferStride;
                int size = stride * height;
                Debug.Assert(wb.BackBufferStride == width * 4);

                wb.Lock();
                try
                {
                    Buffer.MemoryCopy(src, (void*)wb.BackBuffer, size, size);
                    wb.AddDirtyRect(new Int32Rect(0, 0, width, height));
                }
                finally
                {
                    wb.Unlock();
                }

                return wb;
            }
            finally
            {
                if (decoder != IntPtr.Zero)
                {
                    NativeMethods.WebPAnimDecoderDelete(decoder);
                }
            }
        }

        public static WebPImageInfo GetInfo(Stream stream)
        {
            try
            {
                // 1. 先頭1024byteで情報取得を試みる
                stream.Position = 0;
                var memory = stream.ReadToMemory(1024);
                return GetInfo(memory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Retry...");
                // 2. 全データから情報取得を試みる
                var memory = stream.ToMemory();
                return GetInfo(memory);
            }
        }

        public static WebPImageInfo GetInfo(Memory<byte> memory)
        {
            if (memory.Length == 0)
            {
                throw new ArgumentException($"Memory size is zero");
            }

            Initialize();

            using var webpData = new WebPDataHandle(memory);

            var features = new NativeMethods.WebPBitstreamFeatures();
            if (NativeMethods.WebPGetFeaturesInternal(webpData.data, webpData.size, ref features, NativeMethods.WEBP_DECODER_ABI_VERSION) != 0)
            {
                throw new WebPDecoderException("WebPGetFeatures failed");
            }

            return new WebPImageInfo()
            {
                PixelWidth = features.width,
                PixelHeight = features.height,
                FrameCount = 1, // FrameCount は取得できるけど使用されないので設定していない
                HasAlpha = features.has_alpha != 0,
            };
        }


        private class WebPDataHandle : IDisposable
        {
            private GCHandle _handle;
            private NativeMethods.WebPData _webpData;

            public NativeMethods.WebPData WebPData => _webpData;

            public IntPtr data => _webpData.data;
            public UIntPtr size => _webpData.size;

            public WebPDataHandle(Memory<byte> memory)
            {
                if (!MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> seg))
                {
                    throw new WebPDecoderException("TryGetArray failed");
                }

                _handle = GCHandle.Alloc(seg.Array!, GCHandleType.Pinned);
                _webpData = new NativeMethods.WebPData();
                _webpData.data = _handle.AddrOfPinnedObject();
                _webpData.size = (UIntPtr)memory.Length;
            }

            public void Dispose()
            {
                _handle.Free();
            }
        }

    }
}