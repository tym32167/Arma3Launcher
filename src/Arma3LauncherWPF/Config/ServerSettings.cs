using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Arma3LauncherWPF.Extensions;
using Arma3LauncherWPF.Logging;

namespace Arma3LauncherWPF.Config
{
    public class ServerSettings : SettingsBase<ModInfoServer>
    {
        private readonly ILog _log;

        public ServerSettings(ILog log)
            : base("serverSettings", log)
        {
            _log = log;
        }


        public IEnumerable<ModInfo> AvailibleMods
        {
            get { return Instance.Mods; }
        }

        public IEnumerable<Profile> AvailibleProfiles
        {
            get { return Instance.Profiles; }
        }

        public void Refresh()
        {
            _instance = null;
        }

        public DateTime DateOfMod(string modName)
        {
            var mod = Instance.Mods.FirstOrDefault(x => x.Name.EqualIgnoreCase(modName));
            if (mod == null) return DateTime.MinValue;
            return mod.ModDate;
        }


        public bool CanDownloadMod(string modName)
        {
            var mod = Instance.Mods.FirstOrDefault(x => x.Name.EqualIgnoreCase(modName));
            if (mod != null) return !string.IsNullOrEmpty(mod.Url);
            return false;
        }


        public new void Save(ModInfoServer set)
        {
            base.Save(set);
        }

        private new ModInfoServer Load()
        {
            try
            {
                using (var reader = new XmlTextReader(AppSettingsHelper.ServerFileUrl))
                {
                    var xmlSer = new XmlSerializer(typeof(ModInfoServer));
                    var set = xmlSer.Deserialize(reader) as ModInfoServer;
                    //base.Set(set);
                    base.Save(set);
                    return set;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                var frombase = base.Load();
                return frombase ?? new ModInfoServer();
            }
        }

        private ModInfoServer _instance;
        private ModInfoServer Instance
        {
            get { return _instance ?? (_instance = Load()); }
        }
        
        #region Nested

        public class ModInfo
        {
            public string Name { get; set; }
            public string FileName { get; set; }
            public string Url { get; set; }
            public DateTime ModDate { get; set; }
        }

        public class ProfileModInfo
        {
            public string Name { get; set; }
        }

        public class Profile : ProfileBase
        {
            public Profile()
            {
                this.Mods = new List<ProfileModInfo>();
            }

            public List<ProfileModInfo> Mods { get; set; }
        }

        #endregion 
    }

    public class ModInfoServer
    {
        public ModInfoServer()
        {
            Mods = new List<ServerSettings.ModInfo>();
            Profiles = new List<ServerSettings.Profile>();
        }

        public List<ServerSettings.ModInfo> Mods { get; set; }
        public List<ServerSettings.Profile> Profiles { get; set; }
    }
}