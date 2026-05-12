using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO
{
    [LocalDebug]
    public partial class RemoteCommandServer
    {
        private readonly string _pipeName;
        private readonly Dictionary<string, Func<byte[], byte[]>> _handlers = new();
        private readonly JsonSerializerContext _jsonContext;

        public RemoteCommandServer(string pipeName, JsonSerializerContext jsonContext)
        {
            _pipeName = pipeName;
            _jsonContext = jsonContext;
        }

        public void RegisterMethod<TArgs, TResult>(string methodName, Func<TArgs, TResult> handler)
        {
            _handlers[methodName] = (inputBytes) =>
            {
                var jsonTypeInfo = (JsonTypeInfo<TArgs>)_jsonContext.GetTypeInfo(typeof(TArgs))!;
                var args = JsonSerializer.Deserialize(inputBytes, jsonTypeInfo)!;
                var result = handler(args);
                var resultTypeInfo = (JsonTypeInfo<TResult>)_jsonContext.GetTypeInfo(typeof(TResult))!;
                return JsonSerializer.SerializeToUtf8Bytes(result, resultTypeInfo);
            };
        }

        public void RegisterMethod<TArgs>(string methodName, Action<TArgs> handler)
        {
            _handlers[methodName] = (inputBytes) =>
            {
                var jsonTypeInfo = (JsonTypeInfo<TArgs>)_jsonContext.GetTypeInfo(typeof(TArgs))!;
                var args = JsonSerializer.Deserialize(inputBytes, jsonTypeInfo)!;
                handler(args);
                var resultTypeInfo = (JsonTypeInfo<string>)_jsonContext.GetTypeInfo(typeof(string))!;
                return JsonSerializer.SerializeToUtf8Bytes("OK", resultTypeInfo);
            };
        }

        public async Task StartAsync(CancellationToken ct = default)
        {
            LocalDebug.WriteLine($"Start: {_pipeName}");
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var serverStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    await serverStream.WaitForConnectionAsync(ct);

                    // Close each request individually (stateless design)
                    await ProcessRequestAsync(serverStream, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LocalDebug.WriteLine($"Connection error: {ex.Message}");
                }
            }
        }

        private async Task ProcessRequestAsync(NamedPipeServerStream stream, CancellationToken ct)
        {
            try
            {
                byte[] sizeBuffer = new byte[4];

                // read header
                await stream.ReadExactlyAsync(sizeBuffer, ct);
                int headerSize = BitConverter.ToInt32(sizeBuffer, 0);
                if (headerSize > 1024) throw new InvalidDataException("Header too large");

                byte[] headerBuffer = new byte[headerSize];
                await stream.ReadExactlyAsync(headerBuffer, ct);
                string methodName = Encoding.UTF8.GetString(headerBuffer);

                // read payload
                await stream.ReadExactlyAsync(sizeBuffer, ct);
                int payloadSize = BitConverter.ToInt32(sizeBuffer, 0);

                byte[] payloadBuffer = new byte[payloadSize];
                await stream.ReadExactlyAsync(payloadBuffer, ct);

                if (_handlers.TryGetValue(methodName, out var handler))
                {
                    byte[] responsePayload = handler(payloadBuffer);

                    // result: [1 (Success)][Size][Payload]
                    await stream.WriteAsync(new byte[] { 1 }, ct);
                    await stream.WriteAsync(BitConverter.GetBytes(responsePayload.Length), ct);
                    await stream.WriteAsync(responsePayload, ct);
                }
                else
                {
                    throw new Exception($"Method '{methodName}' not found.");
                }
            }
            catch (Exception ex)
            {
                // error: [0 (Failed)][Size][ErrorMessage]
                byte[] errorBytes = Encoding.UTF8.GetBytes(ex.Message);
                stream.WriteByte(0);
                await stream.WriteAsync(BitConverter.GetBytes(errorBytes.Length), ct);
                await stream.WriteAsync(errorBytes, ct);
            }
        }
    }

}


