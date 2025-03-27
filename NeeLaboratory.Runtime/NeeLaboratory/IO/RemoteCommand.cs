using System.Text;
using System.Text.Json.Serialization;

namespace NeeLaboratory.IO
{

    public class RemoteCommand
    {
        public RemoteCommand()
        {
            Id = "";
            Args = System.Array.Empty<string>();
        }

        public RemoteCommand(string id)
        {
            Id = id;
            Args = System.Array.Empty<string>();
        }

        public RemoteCommand(string id, params string[] args)
        {
            Id = id;
            Args = args;
        }

        public string Id { get; set; }

        public string[] Args { get; set; }
    }


    [JsonSerializable(typeof(RemoteCommand))]
    internal partial class RemoteCommandJsonSerializerContext : JsonSerializerContext
    {
    }
}
