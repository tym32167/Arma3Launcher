using System;
using System.Globalization;
using System.Windows;
using Arma3LauncherWPF.Logging;

namespace Arma3LauncherWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ILog _logger = new Log();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            log4net.Config.XmlConfigurator.Configure();
            _logger.Info("Startup");


            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            try
            {
                var window = new MainWindow();
                window.Show();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
            }


        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _logger.Info("Exit");
        }
    }
}
