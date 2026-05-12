using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NeeView
{
    public static class AppRemoteCommandTools
    {
        public static string CreatePipeName(Process process)
        {
            return CreatePipeName(process.ProcessName, process.Id);
        }

        public static string CreatePipeName(string name, int id)
        {
            return name + ".p" + id;
        }

        public static async Task<string?> GetPipeNameAsync(RemoteCommandDelivery delivery, bool hasWindow)
        {
            var process = await RemoteCommandDeliveryTools.GetProcessAsync(delivery, hasWindow);
            if (process is null)
            {
                return null;
            }

            return CreatePipeName(process);
        }
    }


    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(bool))]
    internal partial class AppRemoteCommandJsonContext : JsonSerializerContext
    {
    }
}
