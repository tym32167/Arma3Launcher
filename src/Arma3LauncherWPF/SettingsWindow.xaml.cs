using System.IO;
using System.Windows;
using System.Windows.Forms;
using Arma3LauncherWPF.Config;
using GalaSoft.MvvmLight;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Arma3LauncherWPF
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly Settings _settings;
        private readonly SettingsWindowViewModel _model;

        public SettingsWindow( Settings settings)
        {
            
            _settings = settings;
            InitializeComponent();
            _model = new SettingsWindowViewModel();
            _model.Arma3FilePath = AppSettingsHelper.ArmaFilePath;
            this.DataContext = _model;

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFilePath(_model.Arma3FilePath))
            {
                AppSettingsHelper.ArmaFilePath = _model.Arma3FilePath;
            }
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool CheckFilePath(string path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path) &&
                   System.String.Compare(System.IO.Path.GetFileName(path).ToLower(), "arma3.exe".ToLower(), System.StringComparison.Ordinal) == 0;
        }

        public class SettingsWindowViewModel : ViewModelBase
        {
            private string _selectedLanguage;
            private string _arma3FilePath;

            public string SelectedLanguage
            {
                get { return _selectedLanguage; }
                set { _selectedLanguage = value; RaisePropertyChanged("SelectedLanguage"); }
            }

            public string Arma3FilePath
            {
                get { return _arma3FilePath; }
                set { _arma3FilePath = value; RaisePropertyChanged("Arma3FilePath"); }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
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
                    MessageBox.Show(Properties.Resources.Incorrect_File, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    _model.Arma3FilePath = file;
                }
            }
        }
    }
}
