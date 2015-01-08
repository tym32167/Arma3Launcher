using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Arma3LauncherWPF.Config
{
    public class SettingsDto
    {
        public List<Profile> Profiles { get; set; }

        [XmlIgnore]
        public List<ModInfo> InstalledMods { get; set; }

        public string DefaultProfileName { get; set; }

        public bool NoSplashScreen { get; set; }
        public bool EmptyDefaultWorld { get; set; }
        public bool ShowScriptErrors { get; set; }
        public bool NoPauseOnTaskSwitch { get; set; }
        public bool SkipIntro { get; set; }
        public bool Windowed { get; set; }

        public bool MaxMemory { get; set; }
        public bool MaxVRAM { get; set; }
        public bool CPUCount { get; set; }
        public bool ExtraThreads { get; set; }

        public int MaxMemoryInt { get; set; }
        public int MaxVRAMInt { get; set; }
        public int CPUCountInt { get; set; }
        public int ExtraThreadsInt { get; set; }

        public SettingsDto()
        {
            Profiles = new List<Profile>();
            InstalledMods = new List<ModInfo>();
            DefaultProfileName = "Default";
        }
        
        public class Profile : ProfileBase
        {
            public Profile()
            {
                
            }

            public Profile(string profileName, bool canDelete)
            {
                
                this.ProfileName = profileName;
                this.CanDelete = canDelete;

                this.Mods = new List<ModInfo>();
            }
            
            public bool CanDelete { get; set; }
            public bool CanEdit { get; set; }

            public List<ModInfo> Mods { get; set; }

            public override string ToString()
            {
                return this.ProfileName;
            }
        }

        public class ModInfo
        {
            public string ModName { get; set; }

            public override string ToString()
            {
                return this.ModName;
            }
        }
    }
}