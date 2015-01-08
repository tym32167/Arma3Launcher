using Arma3LauncherWPF.Logging;

namespace Arma3LauncherWPF.Config
{
    public abstract class SettingsBase<T> 
        where T : class, new()
    {
        private readonly string _filename;
        private readonly ILog _log;
        private T _cache;

        protected SettingsBase(string filename, ILog log)
        {
            _filename = filename;
            _log = log;
        }

        protected void Set(T settings)
        {
            _cache = settings;
        }

        protected void Save(T settings)
        {
            Set(settings);
            Properties.Settings.Default[_filename] = settings;
            Properties.Settings.Default.Save();
        }

        protected void Reload()
        {
            Properties.Settings.Default.Reload();
            _cache = null;
        }

        protected T Load()
        {
            return _cache ?? (_cache = (Properties.Settings.Default[_filename] as T) ?? new T());
        }
    }
}