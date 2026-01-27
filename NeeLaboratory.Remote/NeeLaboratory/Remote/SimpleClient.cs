using NeeLaboratory.IO;
using NeeLaboratory.Runtime.Serialization;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Remote
{
    public class SimpleClient
    {
        private readonly string _serverPipeName;
        private readonly SemaphoreSlim _semaphore = new(1);


        public SimpleClient(string serverPipeName)
        {
            _serverPipeName = serverPipeName;
        }

        public async ValueTask<List<Chunk>> CallAsync(List<Chunk> args, CancellationToken token)
        {
            // セマフォで排他処理。通信は同時に１つだけ
            _semaphore.Wait(token);
            try
            {
                // 接続 2 秒タイムアウト
                return await CallInnerAsync(args, 2000, token);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask<List<Chunk>> CallInnerAsync(List<Chunk> args, int timeout, CancellationToken token)
        {
            ////Debug.WriteLine($"Client: Start");
            using (var pipeClient = new NamedPipeClientStream(".", _serverPipeName, PipeDirection.InOut))
            {
                ////Debug.WriteLine($"Client: Connect {_serverPipeName} ...");
                await pipeClient.ConnectAsync(timeout, token);

                using (var stream = new ChunkStream(pipeClient, true))
                {
                    // call
                    ////Debug.WriteLine($"Client: Call: {args[0].Id}");
                    stream.WriteChunkArray(args);

                    // result
                    var result = await stream.ReadChunkArrayAsync(token);
                    ////Debug.WriteLine($"Client: Result.Id: {result[0].Id}");

                    if (result[0].Id < 0)
                    {
                        var data = result[0].Data;
                        var message = data != null ? DefaultSerializer.Deserialize<string>(data, BasicJsonSerializerContext.Default) : "Susie Exception";
                        throw new IOException(message);
                    }

                    return result;
                }
            }
        }
    }
}


