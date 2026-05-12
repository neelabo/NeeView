using NeeLaboratory.IO;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NeeView
{
    public class AppRemoteCommandClient
    {
        private readonly static Lazy<AppRemoteCommandClient> _current = new();
        public static AppRemoteCommandClient Current => _current.Value;


        private readonly RemoteCommandClient _client = new(AppRemoteCommandJsonContext.Default);


        public async Task RestartAsync(RemoteCommandDelivery delivery, string[] args)
        {
            var pipes = await AppRemoteCommandTools.GetPipeNameAsync(delivery, false);
            if (pipes is null) return;
            try
            {
                await _client.CallAsync(pipes, nameof(AppRemoteCommandServer.Restart), args);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task<bool> IsHideWindowAsync(RemoteCommandDelivery delivery)
        {
            Debug.Assert(delivery.Type == RemoteCommandDeliveryType.Custom);

            var pipes = await AppRemoteCommandTools.GetPipeNameAsync(delivery, false);
            if (pipes is null) return false;
            try
            {
                return await _client.CallAsync<int, bool>(pipes, nameof(AppRemoteCommandServer.IsHideWindow), 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }

}
