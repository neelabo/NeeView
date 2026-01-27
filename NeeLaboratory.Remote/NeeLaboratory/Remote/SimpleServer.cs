using NeeLaboratory.IO;
using NeeLaboratory.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;

namespace NeeLaboratory.Remote
{
    public class SimpleServer
    {
        public delegate List<Chunk> SimpleReceiver(List<Chunk> chunks);

        private readonly string _name;

        private readonly Dictionary<int, SimpleReceiver> _receivers = new();

        public SimpleServer(string name)
        {
            _name = name;
        }

        public void AddReceiver(int id, SimpleReceiver receiver)
        {
            _receivers.Add(id, receiver);
        }

        public void ServerProcess()
        {
            while (true)
            {
                ServerProcessTurn();
                GC.Collect();
            }
        }

        public void ServerProcessTurn()
        {
            ////Trace.WriteLine($"Server: Open");
            using (var pipeServer = new NamedPipeServerStream(_name, PipeDirection.InOut))
            {
                ////Trace.WriteLine($"Server: Wait for connect...");
                pipeServer.WaitForConnection();

                using (var stream = new ChunkStream(pipeServer, true))
                {
                    var command = stream.ReadChunkArray();
                    var result = CommandExecute(command);
                    stream.WriteChunkArray(result);
                }
            }

            ////Trace.WriteLine($"Server: Closed");
        }

        private List<Chunk> CommandExecute(List<Chunk> command)
        {
            try
            {
                ////Trace.WriteLine($"Server: Execute: ChunkCount={command.Count} CommandId={command[0].Id}");
                var result = _receivers[command[0].Id].Invoke(command);
                ////Trace.WriteLine($"Server: Result: ChunkCount={result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Server: Execute.Exception: " + ex.Message);
                Trace.Indent();
                Trace.WriteLine(ex);
                Trace.Unindent();
                return new List<Chunk>() { new Chunk(-1, DefaultSerializer.Serialize(ex.Message, BasicJsonSerializerContext.Default)) };
            }
        }

    }

}
