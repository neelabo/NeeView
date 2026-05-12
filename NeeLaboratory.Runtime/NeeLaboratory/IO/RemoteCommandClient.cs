using System;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO
{

    public class RemoteCommandClient
    {
        private readonly JsonSerializerContext _jsonContext;
        private readonly int _timeout = 500;

        public RemoteCommandClient(JsonSerializerContext jsonContext)
        {
            _jsonContext = jsonContext;
        }

        public async Task CallAsync<TArgs>(string pipeName, string methodName, TArgs args)
        {
            await CallAsync<TArgs, string>(pipeName, methodName, args);
        }

        public async Task<TResult> CallAsync<TArgs, TResult>(string pipeName,  string methodName, TArgs args)
        {
            using var clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

            using var cts = new CancellationTokenSource(_timeout);
            await clientStream.ConnectAsync(cts.Token);

            // Send
            var argsTypeInfo = (JsonTypeInfo<TArgs>)_jsonContext.GetTypeInfo(typeof(TArgs))!;
            byte[] headerBytes = Encoding.UTF8.GetBytes(methodName);
            byte[] payloadBytes = JsonSerializer.SerializeToUtf8Bytes(args, argsTypeInfo);

            await clientStream.WriteAsync(BitConverter.GetBytes(headerBytes.Length));
            await clientStream.WriteAsync(headerBytes);
            await clientStream.WriteAsync(BitConverter.GetBytes(payloadBytes.Length));
            await clientStream.WriteAsync(payloadBytes);
            await clientStream.FlushAsync();

            // Receive
            byte[] sizeBuffer = new byte[4];
            int status = clientStream.ReadByte(); // 1:Success, 0:Failed

            await clientStream.ReadExactlyAsync(sizeBuffer);
            int responseSize = BitConverter.ToInt32(sizeBuffer, 0);
            byte[] responseBuffer = new byte[responseSize];
            await clientStream.ReadExactlyAsync(responseBuffer);

            if (status == 1)
            {
                var resTypeInfo = (JsonTypeInfo<TResult>)_jsonContext.GetTypeInfo(typeof(TResult))!;
                return JsonSerializer.Deserialize(responseBuffer, resTypeInfo) ?? throw new InvalidOperationException("Response payload was null.");
            }
            else
            {
                string errorMsg = Encoding.UTF8.GetString(responseBuffer);
                throw new Exception($"Server Error: {errorMsg}");
            }
        }
    }
}
