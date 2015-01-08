using System.Configuration;
using System.IO;
using System;
using System.Linq;

namespace Arma3LauncherWPF.Config
{
    public class AppSettingsHelper
    {
        private static string GetSetting(string key)
        {
            return Properties.Settings.Default[key] as String;
        }

        private static void SetSetting(string key, string value)
        {
            Properties.Settings.Default[key] = value;
            Properties.Settings.Default.Save();
        }

        private static string GetAppSetting(string key)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!config.AppSettings.Settings.AllKeys.Contains(key)) config.AppSettings.Settings.Add(key, null);
            var ckey = config.AppSettings.Settings[key];
            return ckey == null ? null : ckey.Value;
        }

        private static void SetAppSetting(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(key);
            config.Save();
            config.AppSettings.Settings.Add(key, value);
            config.Save();
        }

        private const string FPathKey = "arma3file";

        public static string ArmaFilePath
        {
            get { return GetSetting(FPathKey); }
            set { SetSetting(FPathKey, value);}
        }


        // @"https://dl.dropboxusercontent.com/u/38076493/games/server.xml"

        private const string FServerFileUrl = "FServerFileUrl";
        public static string ServerFileUrl 
        {
            get
            {
                return GetAppSetting(FServerFileUrl);
            }
            set { SetAppSetting(FServerFileUrl, value); }
        }

        public static string Arc7ZipPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs", "7z.exe"); }
        }

        public static string Arc7ZipLogsPath
        {
            get { return string.Format(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs", "logs_{0}.txt"), DateTime.Now.Date.ToString("yyyy_MM_dd")); }
        }
    }
}