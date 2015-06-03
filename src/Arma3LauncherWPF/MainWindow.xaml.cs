using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;
using Arma3LauncherWPF.Config;
using Arma3LauncherWPF.ViewModel;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;
using System.Security.Cryptography;
using System.IO; 

namespace Arma3LauncherWPF

{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            Instance = Window.GetWindow(this);
        }
        
        private void LoadSettingsCheck()
        {
            //var set = Settings.Instance.Value;
            //if (!set.Profiles.Where(x => x.ProfileName == "Default").Any())
            //{
            //    var def = new SettingsDto.Profile("Default", false);
            //    set.Profiles.Add(def);
            //    Settings.Save(set);
            //    //LoadSettingsCheck();
            //}

            
        }
        Guid GetHashString(string path)
        {
            //переводим строку в байт-массим 
 
            FileStream flStream = File.OpenRead(path);
            byte[] bytes = new byte[flStream.Length]; //Encoding.Unicode.GetBytes(s);
            flStream.Read(bytes, 0, (int)flStream.Length);
            //создаем объект для получения средст шифрования  
            MD5CryptoServiceProvider CSP =
                new MD5CryptoServiceProvider();
            //вычисляем хеш-представление в байтах  
            byte[] byteHash = CSP.ComputeHash(bytes);
            string hash = string.Empty;
            //формируем одну цельную строку из массива  
            foreach (byte b in byteHash)
                hash += string.Format("{0:x2}", b);
            return new Guid(hash);
        }
 
        private void buttonEquals_Click(object sender, EventArgs e)
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            this.ShowInTaskbar = this.WindowState != WindowState.Minimized;
        }

        private void LoadFilePathCkeck()
        {
            var path = AppSettingsHelper.ArmaFilePath;
            if (!CheckFilePath(path))
            {
                System.Windows.Forms.MessageBox.Show(Properties.Resources.arma3_place_question, Properties.Resources.arma3_file,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                var ofd = new OpenFileDialog();
                ofd.CheckFileExists = true;
                ofd.Multiselect = false;
                ofd.Filter = Properties.Resources.exe_dialog_filter;

                var result = ofd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var file = ofd.FileName;
                    if (!CheckFilePath(file))
                    {
                        MessageBox.Show(Properties.Resources.Incorrect_File,Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LoadFilePathCkeck();
                    }
                        
        {
            // берем файл с САЙТА
            string usingPath = ("http://saite.ru/tester.exe");
 
            
            //сравниваем их хеш. 
            if (GetHashString(defaultPath).ToString() == GetHashString(usingPath).ToString())
                MessageBox.Show("OK");
            else
                MessageBox.Show("Error");
        }
                    else
                    {
                        AppSettingsHelper.ArmaFilePath = file;
                    }
                }
                else
                {
                    Application.Current.Shutdown();
                }


            }
        }

        private int GetNumOfCores()
        {
            return Environment.ProcessorCount;
        }

        private bool CheckFilePath(string path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path) &&
                   System.String.Compare(Path.GetFileName(path).ToLower(), "arma3.exe".ToLower(), System.StringComparison.Ordinal) == 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilePathCkeck();
            LoadSettingsCheck();

            CPUCountInt.Minimum = 1;
            CPUCountInt.Maximum = GetNumOfCores();

            var locator = new ViewModelLocator();
            _mainViewModel = locator.Main;
            DataContext = _mainViewModel;
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            _mainViewModel.Save();
        }

        private void Button_Add(object sender, RoutedEventArgs e)
        {
            var dialog = new Prompt();
            dialog.Title = Properties.Resources.AddProfile;
            
            if (dialog.ShowDialog() == true)
            {
               _mainViewModel.AddProfile(dialog.ResponseText);
            }
            
        }

        private void Button_Run(object sender, RoutedEventArgs e)
        {
            if (_mainViewModel.AllNeededModsDownloaded())
            {
                _mainViewModel.Start();
            }
            else
            {
                var res = MessageBox.Show(Properties.Resources.Not_All_Downloaded_Warn, Properties.Resources.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    _mainViewModel.Start();
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _mainViewModel.DownloadMod((sender as System.Windows.Controls.Button).DataContext as MainViewModel.ModInfoAdv, Window.GetWindow(this));
        }

        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            _mainViewModel.DeleteCurrentProfile();
        }

        private void ButtonBase_OnClickRefresh(object sender, RoutedEventArgs e)
        {
            _mainViewModel.Refresh();
        }

        private void RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ButtonBase_Update(object sender, RoutedEventArgs e)
        {
            _mainViewModel.UpdateMod((sender as System.Windows.Controls.Button).DataContext as MainViewModel.ModInfoAdv, Window.GetWindow(this));
        }

        private void ButtonBase_Delete(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                var mod = button.DataContext as MainViewModel.ModInfoAdv;
                var res = MessageBox.Show(string.Format(Properties.Resources.Delete_Mod_Question_Template, mod.ModInfo.ModName), Properties.Resources.Deleting, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    _mainViewModel.RemoveMod(mod);
            }
        }

        private void ButtonBase_DownloadAll(object sender, RoutedEventArgs e)
        {
            _mainViewModel.DownloadAll(Window.GetWindow(this));
        }

        private void ButtonBase_UpdateAll(object sender, RoutedEventArgs e)
        {
            _mainViewModel.UpdateAll(Window.GetWindow(this));
        }

        private void Tray_Dbl_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Minimized ? WindowState.Normal : WindowState.Minimized;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void Button_Settings(object sender, RoutedEventArgs e)
        {
            var set = new SettingsWindow(_mainViewModel._settings);
            set.ShowDialog();
        }

        private void Button_Connect(object sender, RoutedEventArgs e)
        {
            if (_mainViewModel.AllNeededModsDownloaded())
            {
                _mainViewModel.StartConnect();
            }
            else
            {
                var res = MessageBox.Show(Properties.Resources.Not_All_Downloaded_Warn, Properties.Resources.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == System.Windows.Forms.DialogResult.Yes)
                    _mainViewModel.StartConnect();
            }
        }

        public static Window Instance;
    }
}
