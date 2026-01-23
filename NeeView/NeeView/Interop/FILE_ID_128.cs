using System;
using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct FILE_ID_128
        {
            public byte b0;
            public byte b1;
            public byte b2;
            public byte b3;
            public byte b4;
            public byte b5;
            public byte b6;
            public byte b7;
            public byte b8;
            public byte b9;
            public byte b10;
            public byte b11;
            public byte b12;
            public byte b13;
            public byte b14;
            public byte b15;

            public FILE_ID_128()
            {
            }

            public FILE_ID_128(byte[] src)
            {
                if (src.Length != 16)
                    throw new ArgumentException("FileId must be 16 bytes");

                //return new FILE_ID_128
                {
                    b0 = src[0];
                    b1 = src[1];
                    b2 = src[2];
                    b3 = src[3];
                    b4 = src[4];
                    b5 = src[5];
                    b6 = src[6];
                    b7 = src[7];
                    b8 = src[8];
                    b9 = src[9];
                    b10 = src[10];
                    b11 = src[11];
                    b12 = src[12];
                    b13 = src[13];
                    b14 = src[14];
                    b15 = src[15];
                }
                ;
            }
        }

    }
}
