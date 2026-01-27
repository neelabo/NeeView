//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.IO;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// ZIPファイル解析。
    /// 現状では簡易UTF8判定処理のみ実装。
    /// </summary>
    [LocalDebug]
    public partial class ZipAnalyzer : IDisposable
    {
        private readonly ZipBinaryReader _reader;
        private bool _disposedValue;


        public ZipAnalyzer(Stream stream, bool leaveOpen)
        {
            _reader = new ZipBinaryReader(stream, Encoding.UTF8, leaveOpen);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _reader.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// ZIP の Encoding が UTF-8 かを判定
        /// </summary>
        /// <remarks>
        /// UTF-8 フラグのないエントリ名の UTF-8 チェックを行う
        /// </remarks>
        /// <returns>全て UTF-8 であるなら true</returns>
        public bool IsEncodingUTF8()
        {
            // check header (PK34)
            //LocalDebug.WriteLine("[PK34] LocalFileHeader:");
            //_reader.Seek(0, SeekOrigin.Begin);
            //if (_reader.ReadSignature() != ZipSignatures.LocalFileHeader) throw new FormatException("Signature is not PK34");

            // read end of central directory (PK56)
            LocalDebug.WriteLine("[PK56] End of central directory:");
            var pos = _reader.SeekEndOfCentralDirectory();
            if (pos < 0) throw new FormatException();

            // get number of central directory entries
            _reader.Seek(pos + 8, SeekOrigin.Begin);
            long numOfEntries = _reader.ReadUInt16();
            LocalDebug.WriteLine($"numOfEntries={numOfEntries}");

            // get central directory offset
            _reader.Seek(pos + 16, SeekOrigin.Begin);
            long centralDirectoryOffset = _reader.ReadUInt32();
            LocalDebug.WriteLine($"centralDirectoryOffset={centralDirectoryOffset}");

            // Zip64 ?
            if (numOfEntries == 0xffff || centralDirectoryOffset == 0xffffffff)
            {
                LocalDebug.WriteLine($"Zip64 now.");
                const int zip64EndOfCentralDirectoryLocatorSize = 20;
                pos = _reader.Seek(pos - zip64EndOfCentralDirectoryLocatorSize, SeekOrigin.Begin);

                LocalDebug.WriteLine($"[PK67] Zip64 central directory locator:");
                if (_reader.ReadSignature() != ZipSignatures.Zip64EndOfCentralDirectoryLocator) throw new FormatException("Signature is not PK67");

                _reader.Seek(pos + 8, SeekOrigin.Begin);
                long zip64EndOfCentralDirectoryOffset = _reader.ReadInt64();
                pos = _reader.Seek(zip64EndOfCentralDirectoryOffset, SeekOrigin.Begin);
                LocalDebug.WriteLine($"zip64EndOfCentralDirectoryOffset={zip64EndOfCentralDirectoryOffset}");

                LocalDebug.WriteLine($"[PK66] Zip64 central directory record:");
                if (_reader.ReadSignature() != ZipSignatures.Zip64EndOfCentralDirectoryRecord) throw new FormatException("Signature is not PK66");

                _reader.Seek(pos + 24, SeekOrigin.Begin);
                numOfEntries = _reader.ReadInt64();
                LocalDebug.WriteLine($"numOfEntries={numOfEntries}");

                _reader.Seek(pos + 48, SeekOrigin.Begin);
                centralDirectoryOffset = _reader.ReadInt64();
                LocalDebug.WriteLine($"centralDirectoryOffset={centralDirectoryOffset}");
            }

            // read central directory entries (PK12)
            LocalDebug.WriteLine("Read central directory entries...");
            _reader.Seek(centralDirectoryOffset, SeekOrigin.Begin);

            // for:
            for (var i = 0; i < numOfEntries; i++)
            {
                pos = _reader.Position;

                // check signature
                LocalDebug.WriteLine($"[PK12] central directory entry[{i}]:");
                if (_reader.ReadSignature() != ZipSignatures.CentralDirectoryEntry) throw new FormatException("Signature is not PK12");

                // get entry bitflag
                _reader.Seek(pos + 8, SeekOrigin.Begin);
                var bitFlags = (ZipGeneralBitFlags)_reader.ReadUInt16();
                LocalDebug.WriteLine($"bitFlags={bitFlags}");

                // has bitflag.UTF8
                var isUTF8 = bitFlags.HasFlag(ZipGeneralBitFlags.UnicodeText);

                // get name length
                _reader.Seek(pos + 28, SeekOrigin.Begin);
                var nameLength = _reader.ReadUInt16();
                LocalDebug.WriteLine($"nameLen={nameLength}");
                var extraFieldLength = _reader.ReadUInt16();
                LocalDebug.WriteLine($"extraFieldLength={extraFieldLength}");
                var commentLength = _reader.ReadUInt16();
                LocalDebug.WriteLine($"commentLength={commentLength}");

                // check string
                if (isUTF8)
                {
                    LocalDebug.WriteLine($"name(UTF8) skip.");
                }
                else
                {
                    // get name.bytes[]
                    _reader.Seek(pos + 46, SeekOrigin.Begin);
                    var name = _reader.ReadBytes(nameLength);

                    // if (name.bytes is not UTF8) return false;
                    if (!IsUTF8Binary(name))
                    {
                        LocalDebug.WriteLine($"name(???)={Environment.Encoding.GetString(name)}");
                        LocalDebug.WriteLine($"[Result] Unknown encoding.");
                        return false;
                    }

                    LocalDebug.WriteLine($"name(UTF8)={Encoding.UTF8.GetString(name)}");
                }

                // seek next
                _reader.Seek(pos + 46 + nameLength + extraFieldLength + commentLength, SeekOrigin.Begin);
            }

            // result
            LocalDebug.WriteLine($"[Result] UTF8 encoding.");
            return true;
        }


        /// <summary>
        /// check UTF8 binary
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private bool IsUTF8Binary(byte[] bytes)
        {
            int count = 0;
            foreach (var b in bytes)
            {
                if (count > 0)
                {
                    if ((b & 0b1100_0000) == 0b1000_0000)
                    {
                        count--;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if ((b & 0b1000_0000) == 0b0000_0000)
                    {
                        count = 0;
                    }
                    else if ((b & 0b1110_0000) == 0b1100_0000)
                    {
                        count = 1;
                    }
                    else if ((b & 0b1111_0000) == 0b1110_0000)
                    {
                        count = 2;
                    }
                    else if ((b & 0b1111_1000) == 0b1111_0000)
                    {
                        count = 3;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Zip General Bit Flags
        /// </summary>
        [Flags]
        public enum ZipGeneralBitFlags
        {
            // bit 11
            UnicodeText = 0x0800,
        }


        /// <summary>
        /// Zip Signatures
        /// </summary>
        public class ZipSignatures
        {
            // PK12 : Central directory entry
            public const int CentralDirectoryEntry = 0x02014B50;

            // PK34 : Local file header
            public const int LocalFileHeader = 0x04034B50;

            // PK56 : End of central directory
            public const int EndOfCentralDirectory = 0x06054B50;

            // PK66 : Zip64 end of central directory record
            public const int Zip64EndOfCentralDirectoryRecord = 0x06064B50;

            // PK67 : Zip64 end of central directory locator
            public const int Zip64EndOfCentralDirectoryLocator = 0x07064B50;
        }


        /// <summary>
        /// Zip Binary Reader
        /// </summary>
        public class ZipBinaryReader : BinaryReader
        {
            public ZipBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
            {
            }


            public long Position => BaseStream.Position;

            public long Length => BaseStream.Length;


            public long Seek(long offset, SeekOrigin origin)
            {
                return BaseStream.Seek(offset, origin);
            }

            public long SeekEndOfCentralDirectory()
            {
                const int endOfCentralDirectoryBaseSize = 22;

                var pos = Length - endOfCentralDirectoryBaseSize;
                var posLimit = Math.Max(pos - 0xffff, 0);

                while (posLimit < pos)
                {
                    Seek(pos, SeekOrigin.Begin);
                    if (ReadSignature() == ZipSignatures.EndOfCentralDirectory)
                    {
                        return Seek(pos, SeekOrigin.Begin);
                    }
                    pos--;
                }

                throw new FormatException("Cannot found PK56");
            }

            public int ReadSignature()
            {
                return ReadInt32();
            }
        }
    }
}
