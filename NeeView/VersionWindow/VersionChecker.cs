using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NeeLaboratory.ComponentModel;
using System.Net.Http;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// バージョンチェッカー
    /// </summary>
    public class VersionChecker : BindableBase
    {
        private volatile bool _isChecking = false;
        private volatile bool _isChecked = false;
        private string _latestVersionUrl = "https://github.com/neelabo/NeeView/releases";
        private bool _isExistNewVersion;
        private string? _message;


        public VersionChecker()
        {
            CurrentVersion = Environment.CheckVersion;
            LatestVersion = new FormatVersion(Environment.SolutionName, 0, 0, 0);
        }

        public bool IsEnabled => Config.Current.System.IsNetworkEnabled && !Environment.IsAppxPackage && !Environment.IsCanaryPackage && !Environment.IsBetaPackage;

        public FormatVersion CurrentVersion { get; set; }
        public FormatVersion LatestVersion { get; set; }

        public string LatestVersionUrl
        {
            get { return _latestVersionUrl; }
            set { SetProperty(ref _latestVersionUrl, value); }
        }

        public bool IsExistNewVersion
        {
            get { return _isExistNewVersion; }
            set { SetProperty(ref _isExistNewVersion, value); }
        }

        public string? Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(); }
        }


        public void CheckStart()
        {
            if (_isChecked || _isChecking) return;

            if (IsEnabled)
            {
                // チェック開始
                LatestVersion = new FormatVersion(Environment.SolutionName, 0, 0, 0);
                Message = Properties.TextResources.GetString("VersionChecker.Message.Checking");
                Task.Run(async () => await CheckVersionAsync(CancellationToken.None));
            }
        }

        private async Task CheckVersionAsync(CancellationToken token)
        {
            _isChecking = true;

            try
            {
                var release = await GetLatestReleaseAsync(token);
                if (release is not null)
                {
                    LatestVersion = new FormatVersion(Environment.SolutionName, release.TagName);
                    LatestVersionUrl = release.HtmlUrl;

                    Debug.WriteLine($"Current: {CurrentVersion}, Latest: {LatestVersion}");

                    if (LatestVersion == CurrentVersion)
                    {
                        Message = Properties.TextResources.GetString("VersionChecker.Message.Latest");
                    }
                    else if (LatestVersion.CompareTo(CurrentVersion) < 0)
                    {
                        Message = Properties.TextResources.GetString("VersionChecker.Message.Unknown");
                    }
                    else
                    {
                        Message = Properties.TextResources.GetString("VersionChecker.Message.New");
                        IsExistNewVersion = true;
                    }
                }
                else
                {
                    Message = Properties.TextResources.GetString("VersionChecker.Message.Failed");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Message = Properties.TextResources.GetString("VersionChecker.Message.Failed");
            }
            finally
            {
                _isChecked = true;
                _isChecking = false;
            }
        }

        private static async Task<GitHubRelease?> GetLatestReleaseAsync(CancellationToken token)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.UserAgent.ParseAdd("NeeView");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

            var requestUrl = "https://api.github.com/repos/neelabo/NeeView/releases/latest";

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, };
            var response = await client.GetFromJsonAsync<GitHubRelease>(requestUrl, options, token);
            return response;
        }


        public class GitHubRelease
        {
            public string TagName { get; set; } = "";
            public string Name { get; set; } = "";
            public string Body { get; set; } = "";
            public string HtmlUrl { get; set; } = "";
        }
    }
}
