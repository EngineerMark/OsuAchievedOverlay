using OsuAchievedOverlay.Managers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            PopulateData();
        }

        private void PopulateData(){
            DropdownGamemode.ItemsSource = Enum.GetValues(typeof(OsuApiHelper.OsuMode)).Cast<OsuApiHelper.OsuMode>();

            InputApiKey.Password = SettingsManager.Instance.Settings["api"]["key"];
            InputUsername.Text = SettingsManager.Instance.Settings["api"]["user"];
            InputUpdaterate.Text = int.TryParse(SettingsManager.Instance.Settings["api"]["updateRate"], out _) ? SettingsManager.Instance.Settings["api"]["updateRate"] : "60";
            DropdownGamemode.SelectedIndex = (int)Enum.Parse(typeof(OsuApiHelper.OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]);
        }

        private void Btn_SaveSettings(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.Settings["api"]["key"] = WindowManager.Instance.SettingsWin.InputApiKey.Password;
            SettingsManager.Instance.Settings["api"]["user"] = WindowManager.Instance.SettingsWin.InputUsername.Text;
            SettingsManager.Instance.Settings["api"]["gamemode"] = "" + ((OsuApiHelper.OsuMode)WindowManager.Instance.SettingsWin.DropdownGamemode.SelectedIndex);
            int updateRate = int.Parse(WindowManager.Instance.SettingsWin.InputUpdaterate.Text);
            updateRate = Math.Min(120, Math.Max(5, updateRate));
            SettingsManager.Instance.Settings["api"]["updateRate"] = "" + updateRate;

            SettingsManager.Instance.SettingsSave();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Btn_Close(object sender, RoutedEventArgs e)
        {
            Close();
            WindowManager.Instance.SettingsWin = null;
        }

        private void Btn_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void InputNumericOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Link_OpenURL(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
