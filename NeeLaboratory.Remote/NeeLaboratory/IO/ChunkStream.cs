using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO
{
    public class ChunkStream : IDisposable
    {
        private readonly Stream _stream;
        private readonly bool _leaveOpen;

        public ChunkStream(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public ChunkStream(Stream stream, bool leaveOpen)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _leaveOpen = leaveOpen;
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (!_leaveOpen)
                    {
                        _stream.Dispose();
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public byte[] ReadData(int length)
        {
            var data = new byte[length];
            int answer = _stream.Read(data, 0, length);
            if (answer != length) throw new IOException($"Cannot read enough: request={length}, answer={answer}");
            return data;
        }

        public async ValueTask<byte[]> ReadDataAsync(int length, CancellationToken token)
        {
            var data = new byte[length];
            int answer = await _stream.ReadAsync(data.AsMemory(0, length), token);
            if (answer != length) throw new IOException($"Cannot read enough: request={length}, answer={answer}");
            return data;
        }

        public void WriteData(byte[] buffer)
        {
            _stream.Write(buffer, 0, buffer.Length);
        }

        public async ValueTask WriteDataAsync(byte[] buffer, CancellationToken token)
        {
            await _stream.WriteAsync(buffer, token);
        }

        public int ReadByte()
        {
            return _stream.ReadByte();
        }

        public async ValueTask<int> ReadByteAsync(CancellationToken token)
        {
            var buffer = await ReadDataAsync(1, token);
            return buffer[0];
        }

        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        public async ValueTask WriteByteAsync(byte value, CancellationToken token)
        {
            var buffer = new byte[] { value };
            await WriteDataAsync(buffer, token);
        }

        public int ReadInt32()
        {
            var buffer = ReadData(4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public async ValueTask<int> ReadInt32Async(CancellationToken token)
        {
            var buffer = await ReadDataAsync(4, token);
            return BitConverter.ToInt32(buffer, 0);
        }

        public void WriteInt32(int value)
        {
            WriteData(BitConverter.GetBytes(value));
        }

        public async ValueTask WriteInt32Async(int value, CancellationToken token)
        {
            await WriteDataAsync(BitConverter.GetBytes(value), token);
        }

        public Chunk ReadChunk()
        {
            var id = ReadInt32();
            var length = ReadInt32();
            var data = (length > 0) ? ReadData(length) : null;
            return new Chunk(id, data);
        }

        public async ValueTask<Chunk> ReadChunkAsync(CancellationToken token)
        {
            var id = await ReadInt32Async(token);
            var length = await ReadInt32Async(token);
            var data = (length > 0) ? await ReadDataAsync(length, token) : null;
            return new Chunk(id, data);
        }


        public void WriteChunk(Chunk chunk)
        {
            WriteInt32(chunk.Id);
            WriteInt32(chunk.Length);
            if (chunk.Data != null && chunk.Length > 0)
            {
                WriteData(chunk.Data);
            }
        }

        public async ValueTask WriteChunkAsync(Chunk chunk, CancellationToken token)
        {
            await WriteInt32Async(chunk.Id, token);
            await WriteInt32Async(chunk.Length, token);
            if (chunk.Data != null && chunk.Length > 0)
            {
                await WriteDataAsync(chunk.Data, token);
            }
        }

        public List<Chunk> ReadChunkArray()
        {
            var count = ReadInt32();
            var chunks = new List<Chunk>(count);
            for(int i=0; i<count; ++i)
            {
                chunks.Add(ReadChunk());
            }
            return chunks;
        }

        public async ValueTask<List<Chunk>> ReadChunkArrayAsync(CancellationToken token)
        {
            var count = await ReadInt32Async(token);
            var chunks = new List<Chunk>(count);
            for (int i = 0; i < count; ++i)
            {
                chunks.Add(await ReadChunkAsync(token));
            }
            return chunks;
        }

        public void WriteChunkArray(List<Chunk> chunks)
        {
            WriteInt32(chunks.Count);
            foreach(var chunk in chunks)
            {
                WriteChunk(chunk);
            }
        }

        public async ValueTask WriteChunkArrayAsync(List<Chunk> chunks, CancellationToken token)
        {
            await WriteInt32Async(chunks.Count, token);
            foreach (var chunk in chunks)
            {
                await WriteChunkAsync(chunk, token);
            }
        }
    }
}
