using System;
using System.IO;
using System.Linq;
using Arma3LauncherWPF.Logging;

namespace Arma3LauncherWPF.Config
{
    public class Settings : SettingsBase<SettingsDto>
    {
        private readonly ILog _log;
        private readonly ServerSettings _serverSettings;

        public Settings(ILog log, ServerSettings serverSettings)
            //: base(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml"), log)
            : base("settings", log)
        {
            _log = log;
            _serverSettings = serverSettings;
        }


        public void Save()
        {
            Save(Instance);
        }

        public new void Save(SettingsDto set)
        {
            base.Save(set);
            Refresh();
        }

        public void Refresh()
        {
            Reload();
        }

        public new SettingsDto Load()
        {
            var dto = base.Load();
            try
            {
                if (dto != null)
                {
                    var fname = AppSettingsHelper.ArmaFilePath;
                    if (!string.IsNullOrEmpty(fname))
                    {
                        dto.InstalledMods.AddRange(
                            Directory.GetDirectories(Path.GetDirectoryName(fname), "@*")
                                .Select(x => new DirectoryInfo(x).Name)
                                .Select(x => new SettingsDto.ModInfo() { ModName = x }).ToList());
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            return dto;
        }
        
        public SettingsDto Instance
        {
            get { return  Load(); }
        }


        public static DateTime GetModeDate(string modName)
        {
            var fname = AppSettingsHelper.ArmaFilePath;
            if (!string.IsNullOrEmpty(fname))
            {
                var modDir = Directory.GetDirectories(Path.GetDirectoryName(fname), "@*")
                    .Select(x => new DirectoryInfo(x)).FirstOrDefault(x => x.Name == modName);
                if (modDir != null) return modDir.CreationTime;
            }
            return DateTime.MaxValue;
        }
    }
}