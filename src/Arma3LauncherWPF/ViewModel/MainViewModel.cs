using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Arma3LauncherWPF.Config;
using Arma3LauncherWPF.Core;
using Arma3LauncherWPF.Extensions;
using Arma3LauncherWPF.Logging;
using GalaSoft.MvvmLight;
using Settings = Arma3LauncherWPF.Config.Settings;

namespace Arma3LauncherWPF.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private SettingsDto.Profile _currentProfile;
        

        internal readonly Settings _settings;
        private readonly ServerSettings _serverSettings;
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
        
            _log = new Log();
            _serverSettings = new ServerSettings(_log);
            _settings = new Settings(_log, _serverSettings);
        }

        public SettingsDto SettingsDto
        {
            get { return _settings.Instance; }
        }


        private List<SettingsDto.Profile> _profiles;
        public List<SettingsDto.Profile> Profiles
        {
            get
            {
                if (_profiles == null)
                {
                    var result = new List<SettingsDto.Profile>();

                    result.AddRange(_settings.Instance.Profiles);
                    result.AddRange(_serverSettings.AvailibleProfiles.Select(x => new SettingsDto.Profile()
                    {
                        Mods = x.Mods.Select(y => new SettingsDto.ModInfo() {ModName = y.Name}).ToList(),
                        ProfileName = x.ProfileName,
                        CanDelete = false,
                        CanEdit = false,
                        ServerAddress = x.ServerAddress
                    }).ToList());

                    _profiles = result;
                }
                return _profiles;
            }
        }



        public SettingsDto.Profile CurrentProfile
        {
            get
            {
                if (_currentProfile == null)
                {
                    _currentProfile = LoadCurrentProfile();
                }
                return _currentProfile;

            }
            set
            {
                _currentProfile = value;
                _modsForProfile = null;
                RaisePropertyChanged("ModsForProfile");
                RaisePropertyChanged("CurrentProfile");
            }
        }



        private List<ModInfoAdv> _modsForProfile;
        public List<ModInfoAdv> ModsForProfile
        {
            get
            {
                if (_modsForProfile == null)
                {
                    _modsForProfile = _settings.Instance.InstalledMods.Select(x => x.ModName)
                        .Union(_serverSettings.AvailibleMods.Select(x => x.Name))
                        .Distinct(StringComparer.InvariantCultureIgnoreCase)
                        .OrderBy(x => x)
                        .Select(
                            x =>
                                new ModInfoAdv(_settings, _serverSettings)
                                {
                                    ModInfo = new SettingsDto.ModInfo() {ModName = x},
                                    IsChecked = CurrentProfile.Mods.Any(y => y.ModName == x)
                                })
                        .ToList();
                }
                return _modsForProfile;
            }
            set
            {
                _modsForProfile = value;
                RaisePropertyChanged("ModsForProfile");
            }
        }



        public void Save()
        {
            CurrentProfile.Mods.Clear();
            CurrentProfile.Mods.AddRange(ModsForProfile.Where(x => x.IsChecked).Select(x => x.ModInfo).ToList());
            _settings.Instance.DefaultProfileName = CurrentProfile.ProfileName;
            _settings.Save();

            RaisePropertyChanged("Profiles");
            CurrentProfile = LoadCurrentProfile();
            
            RaisePropertyChanged("");
        }

        public void AddProfile(string profileName)
        {
            if (!string.IsNullOrEmpty(profileName))
            {
                var set = _settings.Instance;
                if (!string.IsNullOrEmpty(profileName))
                {
                    var def = new SettingsDto.Profile(profileName, true);
                    def.CanEdit = true;
                    set.DefaultProfileName = profileName;
                    set.Profiles.Add(def);
                    _profiles = null;
                    RaisePropertyChanged("Profiles");
                    CurrentProfile = def;
                    
                }
            }
        }

        private SettingsDto.Profile LoadCurrentProfile()
        {
            var profile = Profiles.FirstOrDefault(x => x.ProfileName == SettingsDto.DefaultProfileName);
            if (profile == null)
            {
                profile = _settings.Instance.Profiles.FirstOrDefault();
                if (profile == null) AddProfile("Default");
                profile = _settings.Instance.Profiles.FirstOrDefault();
            }
            return profile;
        }

        public void DeleteCurrentProfile()
        {
            if (CurrentProfile != null && CurrentProfile.CanDelete)
            {
                _settings.Instance.Profiles.Remove(CurrentProfile);
                _profiles = null;
                RaisePropertyChanged("Profiles");
                CurrentProfile = LoadCurrentProfile();
                
            }
        }

        public bool AllNeededModsDownloaded()
        {
            return !ModsForProfile.Any(x => x.IsChecked && !x.Downloaded);
        }

        public void Start()
        {
            Save();
            var args = string.Empty;

            args += " -mod=";

            args = CurrentProfile.Mods.Aggregate(args, (current, modInfo) => current + (modInfo.ModName + ";"));

            if (SettingsDto.NoSplashScreen) args += " -nosplash ";
            if (SettingsDto.EmptyDefaultWorld) args += " -world=empty ";
            if (SettingsDto.ShowScriptErrors) args += " -showScriptErrors ";
            if (SettingsDto.NoPauseOnTaskSwitch) args += " -noPause ";
            if (SettingsDto.SkipIntro) args += " -skipIntro ";
            if (SettingsDto.Windowed) args += " -window ";

            if (SettingsDto.MaxMemory) args += string.Format(" -maxMem={0} ", SettingsDto.MaxMemoryInt);
            if (SettingsDto.MaxVRAM) args += string.Format(" -maxVRAM={0} ", SettingsDto.MaxVRAMInt);
            if (SettingsDto.CPUCount) args += string.Format(" -cpuCount={0} ", SettingsDto.CPUCountInt);
            if (SettingsDto.ExtraThreads) args += string.Format(" -exThreads={0} ", SettingsDto.ExtraThreadsInt);
            
            _log.Info(string.Format("run with args {0}", args));
            //MessageBox.Show(args);

            var path = AppSettingsHelper.ArmaFilePath;
            if (!string.IsNullOrEmpty(path))
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path,
                    Arguments = args
                });
        }

        public async void DownloadMod(ModInfoAdv mod, Window owner)
        {
            var progress = new Progress();
            progress.Owner = owner;
            progress.Title = string.Format(Properties.Resources.Download_Mod_Text_Template, mod.ModInfo.ModName);
           

            var modDownloader = new ModDownloader(_log, _serverSettings, progress);
            var modInstaller = new ModInstaller(_log, _settings, _serverSettings, modDownloader);
            var cancel = new CancelEventHandler((sender, args) => modDownloader.CancelDownload());
            progress.Closing += cancel;

            progress.Show();
            await modInstaller.InstallModAsync(mod.ModInfo.ModName);
            progress.Close();
            
            _modsForProfile = null;
            RaisePropertyChanged("ModsForProfile");
        }

        public void Refresh()
        {
            _settings.Refresh();

            _modsForProfile = null;
            _profiles = null;
            _currentProfile = null;

            RaisePropertyChanged("SettingsDto");
            RaisePropertyChanged("ModsForProfile");
            RaisePropertyChanged("Profiles");
            RaisePropertyChanged("CurrentProfile");
        }

        public void UpdateMod(ModInfoAdv modInfoAdv, Window owner)
        {
            DownloadMod(modInfoAdv, owner);
        }

        public void RemoveMod(ModInfoAdv modInfoAdv)
        {
            var modDownloader = new ModDownloader(_log, _serverSettings, null);
            var modInstaller = new ModInstaller(_log, _settings, _serverSettings, modDownloader);

            var dpn = CurrentProfile.ProfileName;
            modInstaller.RemoveMod(modInfoAdv.ModInfo.ModName);
            
            _settings.Instance.DefaultProfileName = dpn;
            CurrentProfile = LoadCurrentProfile();
            
            Refresh();
        }


        public void DownloadAll(Window getWindow)
        {
            var mods = ModsForProfile.Where(x => x.CanDownload && !x.Downloaded);
            foreach (var modInfoAdv in mods)
            {
                DownloadMod(modInfoAdv, getWindow);
            }
        }

        public void UpdateAll(Window getWindow)
        {
            var mods = ModsForProfile.Where(x =>x.CanUpdate);
            foreach (var modInfoAdv in mods)
            {
                UpdateMod(modInfoAdv, getWindow);
            }
        }

        public class ModInfoAdv : ViewModelBase
        {
            private readonly Settings _settings;
            private readonly ServerSettings _serverSettings;
            private bool _isChecked;
            public SettingsDto.ModInfo ModInfo { get; set; }

            public ModInfoAdv(Settings settings, ServerSettings serverSettings)
            {
                _settings = settings;
                _serverSettings = serverSettings;
            }

            public bool Downloaded
            {
                get { return _settings.Instance.InstalledMods.Any(x => x.ModName.EqualIgnoreCase(ModInfo.ModName)); }
            }

            public bool CanDownload
            {
                get { return !Downloaded && _serverSettings.CanDownloadMod(ModInfo.ModName); }
            }

            public bool CanUpdate
            {
                get
                {
                    var modedate = Settings.GetModeDate(ModInfo.ModName).ToUniversalTime();
                    var diff = _serverSettings.DateOfMod(ModInfo.ModName).ToUniversalTime() -
                               modedate;
                    return _serverSettings.CanDownloadMod(ModInfo.ModName)
                    && diff.Minutes > 0;
                }
            }


            public bool IsChecked
            {
                get { return _isChecked; }
                set
                {
                    _isChecked = value;
                    RaisePropertyChanged("IsChecked");
                }
            }
        }

        public void StartConnect()
        {
            Save();
            var args = string.Empty;

            args += " -mod=";

            args = CurrentProfile.Mods.Aggregate(args, (current, modInfo) => current + (modInfo.ModName + ";"));

            if (SettingsDto.NoSplashScreen) args += " -nosplash ";
            if (SettingsDto.EmptyDefaultWorld) args += " -world=empty ";
            if (SettingsDto.ShowScriptErrors) args += " -showScriptErrors ";
            if (SettingsDto.NoPauseOnTaskSwitch) args += " -noPause ";
            if (SettingsDto.SkipIntro) args += " -skipIntro ";
            if (SettingsDto.Windowed) args += " -window ";

            if (SettingsDto.MaxMemory) args += string.Format(" -maxMem={0} ", SettingsDto.MaxMemoryInt);
            if (SettingsDto.MaxVRAM) args += string.Format(" -maxVRAM={0} ", SettingsDto.MaxVRAMInt);
            if (SettingsDto.CPUCount) args += string.Format(" -cpuCount={0} ", SettingsDto.CPUCountInt);
            if (SettingsDto.ExtraThreads) args += string.Format(" -exThreads={0} ", SettingsDto.ExtraThreadsInt);

            if (CurrentProfile != null && CurrentProfile.ServerAddress != null)
            {
                var addr = CurrentProfile.ServerAddress;
                if (!string.IsNullOrEmpty(addr.IP))
                {
                    args += string.Format(" -connect={0} ", addr.IP);

                    if (!string.IsNullOrEmpty(addr.Port)) args += string.Format(" -port={0} ", addr.Port);
                    if (!string.IsNullOrEmpty(addr.Password)) args += string.Format(" -password={0} ", addr.Password);
                }
                
            }

            _log.Info(string.Format("connect with args {0}", args));
            //MessageBox.Show(args);

            var path = AppSettingsHelper.ArmaFilePath;
            if (!string.IsNullOrEmpty(path))
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path,
                    Arguments = args
                });
        }
    }
}