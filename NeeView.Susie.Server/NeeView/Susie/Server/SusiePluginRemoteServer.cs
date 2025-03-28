using NeeLaboratory;
using NeeLaboratory.Remote;
using NeeLaboratory.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NeeView.Susie.Server
{
    public class SusiePluginRemoteServer
    {
        private readonly SimpleServer _server;
        private readonly SusiePluginServer _process;


        public SusiePluginRemoteServer()
        {
            var name = SusiePluginRemote.CreateServerName(Process.GetCurrentProcess());
            Trace.WriteLine($"ServerName: {name}");
            _server = new SimpleServer(name);
            _server.AddReceiver(SusiePluginCommandId.Initialize, Initialize);
            _server.AddReceiver(SusiePluginCommandId.GetPlugin, GetPlugin);
            _server.AddReceiver(SusiePluginCommandId.SetPlugin, SetPlugin);
            _server.AddReceiver(SusiePluginCommandId.SetPluginOrder, SetPluginOrder);
            _server.AddReceiver(SusiePluginCommandId.ShowConfigurationDlg, ShowConfigurationDlg);
            _server.AddReceiver(SusiePluginCommandId.GetArchivePlugin, GetArchivePlugin);
            _server.AddReceiver(SusiePluginCommandId.GetImagePlugin, GetImagePlugin);
            _server.AddReceiver(SusiePluginCommandId.GetImage, GetImage);
            _server.AddReceiver(SusiePluginCommandId.GetArchiveEntries, GetArchiveEntries);
            _server.AddReceiver(SusiePluginCommandId.ExtractArchiveEntry, ExtractArchiveEntry);
            _server.AddReceiver(SusiePluginCommandId.ExtractArchiveEntryToFolder, ExtractArchiveEntryToFolder);


            _process = new SusiePluginServer();
        }

        public void Run()
        {
            _server.ServerProcess();
        }

        private static TResult DeserializeChunk<TResult>(Chunk chunk)
            where TResult : class
        {
            if (chunk.Data is null) throw new InvalidOperationException("chunk.Data must not be null");

            return SusieCommandSerializer.Deserialize<TResult>(chunk.Data);
        }

        private static List<Chunk> CreateResult<T>(int id, T result)
        {
            var chunk = new Chunk(id, SusieCommandSerializer.Serialize(result));
            return new List<Chunk>() { chunk };
        }

        private static List<Chunk> CreateResultIsSuccess(int id, bool isSuccess)
        {
            var chunk = new Chunk(id, SusieCommandSerializer.Serialize(new SusiePluginCommandResult(isSuccess)));
            return new List<Chunk>() { chunk };
        }


        private List<Chunk> Initialize(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandInitialize>(command[0]);
            Trace.WriteLine($"Remote.Initialize: {args.PluginFolder}, {args.Plugins}");
            _process.Initialize(args.PluginFolder, args.Plugins);
            return CreateResultIsSuccess(SusiePluginCommandId.Initialize, true);
        }

        private List<Chunk> GetPlugin(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandGetPlugin>(command[0]);
            Trace.WriteLine($"Remote.GetPlugin: {args.PluginNames}");
            var pluginInfoList = _process.GetPlugin(args.PluginNames);
            return CreateResult(SusiePluginCommandId.GetPlugin, new SusiePluginCommandGetPluginResult(pluginInfoList));
        }

        private List<Chunk> SetPlugin(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandSetPlugin>(command[0]);
            Trace.WriteLine($"Remote.SetPlugin: {args.Plugins}");
            _process.SetPlugin(args.Plugins);
            return CreateResultIsSuccess(SusiePluginCommandId.SetPlugin, true);
        }

        private List<Chunk> SetPluginOrder(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandSetPluginOrder>(command[0]);
            Trace.WriteLine($"Remote.SetPluginOrder: {args.Order}");
            _process.SetPluginOrder(args.Order);
            return CreateResultIsSuccess(SusiePluginCommandId.SetPluginOrder, true);
        }

        private List<Chunk> ShowConfigurationDlg(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandShowConfigurationDlg>(command[0]);
            Trace.WriteLine($"Remote.ShowConfigurationDlg: {args.PluginName}");
            _process.ShowConfigurationDlg(args.PluginName, args.HWnd);
            return CreateResultIsSuccess(SusiePluginCommandId.ShowConfigurationDlg, true);
        }


        private List<Chunk> GetArchivePlugin(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandGetArchivePlugin>(command[0]);
            Trace.WriteLine($"Remote.GetArchivePlugin: {args.FileName}");
            var buff = command[1].Data;
            var pluginInfo = _process.GetArchivePlugin(args.FileName, buff, args.IsCheckExtension);
            return CreateResult(SusiePluginCommandId.GetArchivePlugin, new SusiePluginCommandGetArchivePluginResult(pluginInfo));
        }

        private List<Chunk> GetImagePlugin(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandGetImagePlugin>(command[0]);
            Trace.WriteLine($"Remote.GetImagePlugin: {args.FileName}");
            var buff = command[1].Data;
            var pluginInfo = _process.GetImagePlugin(args.FileName, buff, args.IsCheckExtension);
            return CreateResult(SusiePluginCommandId.GetImagePlugin, new SusiePluginCommandGetImagePluginResult(pluginInfo));
        }

        private List<Chunk> GetImage(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandGetImage>(command[0]);
            Trace.WriteLine($"Remote.GetImage: {args.FileName}");
            var buff = command[1].Data;
            var susieImage = _process.GetImage(args.PluginName, args.FileName, buff, args.IsCheckExtension);
            if (susieImage != null)
            {
                var result = CreateResult(SusiePluginCommandId.GetImage, new SusiePluginCommandGetImageResult(susieImage.Plugin));
                result.Add(new Chunk(SusiePluginCommandId.GetImage, susieImage.BitmapData));
                return result;
            }
            else
            {
                var result = CreateResult(SusiePluginCommandId.GetImage, new SusiePluginCommandGetImageResult(null));
                result.Add(new Chunk(SusiePluginCommandId.GetImage, null));
                return result;
            }
        }


        private List<Chunk> GetArchiveEntries(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandGetArchiveEntries>(command[0]);
            Trace.WriteLine($"Remote.GetArchiveEntries: {args.FileName}");
            var entries = _process.GetArchiveEntries(args.PluginName, args.FileName);
            return CreateResult(SusiePluginCommandId.GetArchiveEntries, new SusiePluginCommandGetArchiveEntriesResult(entries));
        }

        private List<Chunk> ExtractArchiveEntry(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandExtractArchiveEntry>(command[0]);
            Trace.WriteLine($"Remote.ExtractArchiveEntry: {args.FileName}, {args.Position}");
            var buff = _process.ExtractArchiveEntry(args.PluginName, args.FileName, args.Position);
            return new List<Chunk>() { new Chunk(SusiePluginCommandId.ExtractArchiveEntry, buff) };
        }

        private List<Chunk> ExtractArchiveEntryToFolder(List<Chunk> command)
        {
            var args = DeserializeChunk<SusiePluginCommandExtractArchiveEntryToFolder>(command[0]);
            Trace.WriteLine($"Remote.ExtractArchiveEntryToFolder: {args.FileName}, {args.Position}, {args.ExtractFolder}");
            _process.ExtractArchiveEntryToFolder(args.PluginName, args.FileName, args.Position, args.ExtractFolder);
            return CreateResultIsSuccess(SusiePluginCommandId.ExtractArchiveEntryToFolder, true);
        }
    }
}


