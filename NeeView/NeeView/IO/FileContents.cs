// from Stack Overflow 
// URL: http://ja.stackoverflow.com/questions/5670/c%E3%81%AB%E3%81%A6%E3%82%A2%E3%83%97%E3%83%AA%E3%81%8B%E3%82%89%E3%83%89%E3%83%A9%E3%83%83%E3%82%B0%E3%83%89%E3%83%AD%E3%83%83%E3%83%97%E3%82%92%E5%8F%97%E3%81%91%E5%85%A5%E3%82%8C%E3%81%9F%E3%81%84%E3%81%AE%E3%81%A7%E3%81%99%E3%81%8C-filecontents%E3%81%AE%E7%B5%90%E6%9E%9C%E3%81%8Call-0%E3%81%AB%E3%81%AA%E3%81%A3%E3%81%A6%E3%81%97%E3%81%BE%E3%81%84%E3%81%BE%E3%81%99
// License: CC BY-SA 3.0
//
// 変更点：
//
// - System.Window.IDataObjectに対応
// - GetFileDescriptor() の format.lindex を -1 に変更

using NeeView.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;


namespace NeeView.IO
{
    public class FileContents
    {
        private FileContents(string name, byte[] bytes)
        {
            Name = name;
            Bytes = bytes;
        }
        public string Name { get; private set; }
        public byte[] Bytes { get; private set; }

        private static readonly uint CF_FILEDESCRIPTORW = NativeMethods.RegisterClipboardFormat("FileGroupDescriptorW");
        private static readonly uint CF_FILECONTENTS = NativeMethods.RegisterClipboardFormat("FileContents");

        public static FileContents[] Get(System.Windows.IDataObject dataObject)
        {
            return Get((IDataObject)dataObject);
        }

        public static FileContents[] Get(IDataObject dataObject)
        {
            var fileDescriptor = GetFileDescriptor(dataObject);
            return fileDescriptor.fgd.Select((fgd, i) => new FileContents(fgd.cFileName, GetFileContent(dataObject, i))).ToArray();
        }

        private static FILEGROUPDESCRIPTORW GetFileDescriptor(IDataObject dataObject)
        {
            var format = new FORMATETC
            {
                cfFormat = unchecked((short)(CF_FILEDESCRIPTORW & 0xFFFF)),
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                ptd = IntPtr.Zero,
                lindex = -1,
                tymed = TYMED.TYMED_HGLOBAL
            };
            dataObject.GetData(ref format, out STGMEDIUM medium);
            Debug.Assert(medium.tymed == TYMED.TYMED_HGLOBAL && medium.unionmember != IntPtr.Zero && medium.pUnkForRelease == null);
            if (medium.unionmember == IntPtr.Zero) throw new FormatException("Wrong DataObject.FileDescriptorW format.");
            try
            {
                var fileGroupDescriptor = new FILEGROUPDESCRIPTORW();

                IntPtr ptr = NativeMethods.GlobalLock(medium.unionmember);
                fileGroupDescriptor.cItems = Marshal.PtrToStructure<int>(ptr);
                ptr += Marshal.SizeOf(fileGroupDescriptor.cItems);

                fileGroupDescriptor.fgd = new FILEDESCRIPTORW[fileGroupDescriptor.cItems];
                for (int index = 0; index < fileGroupDescriptor.cItems; index++)
                {
                    fileGroupDescriptor.fgd[index] = Marshal.PtrToStructure<FILEDESCRIPTORW>(ptr);
                    ptr += Marshal.SizeOf<FILEDESCRIPTORW>();
                }

                return fileGroupDescriptor;
            }
            finally
            {
                NativeMethods.GlobalFree(medium.unionmember);
            }
        }

        private static byte[] GetFileContent(IDataObject dataObject, int i)
        {
            var format = new FORMATETC
            {
                cfFormat = unchecked((short)(CF_FILECONTENTS & 0xFFFF)),
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                ptd = IntPtr.Zero,
                lindex = i,
                tymed = TYMED.TYMED_HGLOBAL | TYMED.TYMED_ISTREAM
            };
            dataObject.GetData(ref format, out STGMEDIUM medium);
            if (medium.unionmember == IntPtr.Zero) throw new FormatException("Wrong DataObject.FileContent format.");
            try
            {
                switch (medium.tymed)
                {
                    case TYMED.TYMED_HGLOBAL:
                        {
                            var size = (long)NativeMethods.GlobalSize(medium.unionmember);
                            Debug.Assert(size <= Int32.MaxValue);
                            var buffer = new byte[size];
                            Marshal.Copy(NativeMethods.GlobalLock(medium.unionmember), buffer, 0, buffer.Length);
                            NativeMethods.GlobalUnlock(medium.unionmember);
                            NativeMethods.GlobalFree(medium.unionmember);
                            return buffer;
                        }
                    case TYMED.TYMED_ISTREAM:
                        {
                            var stream = (IStream)Marshal.GetObjectForIUnknown(medium.unionmember);
                            Marshal.Release(medium.unionmember);
                            stream.Stat(out STATSTG statstg, 0);
                            Debug.Assert(statstg.cbSize <= Int32.MaxValue);
                            var buffer = new byte[statstg.cbSize];
                            stream.Read(buffer, buffer.Length, IntPtr.Zero);
                            return buffer;
                        }
                    default:
                        throw new NotSupportedException($"Not supported TYMED: {medium.tymed}");
                }
            }
            finally
            {
                NativeMethods.ReleaseStgMedium(ref medium);
            }
        }
    }
}

