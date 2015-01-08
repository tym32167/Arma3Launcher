using System;
using System.IO;
using System.Threading.Tasks;
using Arma3LauncherWPF.Config;
using Arma3LauncherWPF.Logging;

namespace Arma3LauncherWPF.Core
{
    public class ModInstaller
    {
        private readonly ILog _log;
        private readonly Settings _settings;
        private readonly ServerSettings _serverSettings;
        private readonly ModDownloader _modDownloader;

        public ModInstaller(ILog log, Settings settings, ServerSettings serverSettings, ModDownloader modDownloader)
        {
            _log = log;
            _settings = settings;
            _serverSettings = serverSettings;
            _modDownloader = modDownloader;
        }









        public async Task InstallModAsync(string modName)
        {
            var result = await _modDownloader.DownloadModAsync(modName);

            if (!string.IsNullOrEmpty(result.Fname) && File.Exists(result.Fname))
            {
                var dirName = Path.GetDirectoryName(result.Fname);
                try
                {
                    if (result.IsSuccess)
                    {
                        
                        var instDir = Path.Combine(dirName, Guid.NewGuid().ToString());
                        Directory.CreateDirectory(instDir);
                        if (FileSystemHelper.UnpackZipFile(result.Fname, instDir))
                        {
                            var dir = Directory.GetDirectories(instDir);
                            var dest = Path.GetDirectoryName(AppSettingsHelper.ArmaFilePath);

                            foreach (var d in dir)
                            {
                                var nameOfDir = new DirectoryInfo(d).Name;
                                var destDirectory = Path.Combine(dest, nameOfDir);
                                if (Directory.Exists(destDirectory) && nameOfDir.StartsWith("@"))
                                    if (!FileSystemHelper.DeleteDirectory(destDirectory)) return;
                                FileSystemHelper.CopyDirectory(d, destDirectory);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
                finally
                {
                    FileSystemHelper.DeleteDirectory(dirName);
                }
            }
        }














        public void RemoveMod(string modName)
        {
            var dir = Path.Combine(Path.GetDirectoryName(AppSettingsHelper.ArmaFilePath), modName);
            if (Directory.Exists(dir)) FileSystemHelper.DeleteDirectory(dir);
        }
    }
}