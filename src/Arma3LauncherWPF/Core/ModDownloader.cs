using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Arma3LauncherWPF.Config;
using Arma3LauncherWPF.Logging;

namespace Arma3LauncherWPF.Core
{
    public class DownloadResult
    {
        public string Fname { get; set; }
        public bool IsSuccess { get; set; }
    }


    public class ModDownloader
    {
        private readonly ILog _log;
        private readonly ServerSettings _serverSettings;
        private readonly Progress _progress;
        private readonly WebClient _webClient = new WebClient();


        public ModDownloader(ILog log, ServerSettings serverSettings, Progress progress)
        {
            _log = log;
            _serverSettings = serverSettings;
            _progress = progress;

            _webClient.DownloadProgressChanged += wc_DownloadProgressChanged;
        }

        public void CancelDownload()
        {
            _webClient.CancelAsync();
        }



        public async Task<DownloadResult> DownloadModAsync(string modName)
        {
            var tempDir = Path.GetTempPath();
            var dirName = Path.Combine(tempDir, Guid.NewGuid().ToString());
            var fname = string.Empty;

            try
            {
                if (!_serverSettings.CanDownloadMod(modName))
                {
                    return new DownloadResult { Fname = fname, IsSuccess = false };
                }

                var mod = _serverSettings.AvailibleMods.FirstOrDefault(x => x.Name == modName);

                if (mod != null)
                {

                    Directory.CreateDirectory(dirName);
                    fname = Path.Combine(dirName, mod.FileName);

                    await DownloadFileAsync(new Uri(mod.Url), fname);
                }

                return new DownloadResult {Fname = fname, IsSuccess = true};
                
            }
            catch (Exception e)
            {
                _log.Error(e);
                return new DownloadResult { Fname = fname, IsSuccess = false };
            }

        }

        private async Task DownloadFileAsync(Uri uri, string filename)
        {
            try
            {
                await _webClient.DownloadFileTaskAsync(uri, filename);
            }
            catch (Exception e)
            {
                _log.Error(e);
                _log.ErrorFormat("uri {0} filename {1}", uri, filename);
                throw;
            }
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _progress.Dispatcher.Invoke(() =>
            {
                if (_progress != null)
                {
                    _progress.progress.Value = e.ProgressPercentage;
                    _progress.Title = string.Format("Downloaded {0} MB", e.BytesReceived/1024/1024);
                }
            });
        }
    }
}