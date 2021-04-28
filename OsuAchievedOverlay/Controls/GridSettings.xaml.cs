using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OsuAchievedOverlay.Controls
{
    /// <summary>
    /// Interaction logic for GridSettings.xaml
    /// </summary>
    public partial class GridSettings : UserControl
    {
        public event EventHandler GridClosed;

        public GridSettings()
        {
            InitializeComponent();
        }

        private void Btn_Close(object sender, RoutedEventArgs e)
        {
            GridClosed?.Invoke(this, EventArgs.Empty);
        }

        public void PopulateData()
        {
            DropdownGamemode.ItemsSource = Enum.GetValues(typeof(OsuApiHelper.OsuMode)).Cast<OsuApiHelper.OsuMode>();

            InputApiKey.Password = SettingsManager.Instance.Settings["api"]["key"];
            InputUsername.Text = SettingsManager.Instance.Settings["api"]["user"];
            InputUpdaterate.Text = int.TryParse(SettingsManager.Instance.Settings["api"]["updateRate"], out _) ? SettingsManager.Instance.Settings["api"]["updateRate"] : "60";
            DropdownGamemode.SelectedIndex = (int)Enum.Parse(typeof(OsuApiHelper.OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]);
        }

        private void Btn_SaveSettings(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.Settings["api"]["key"] = InputApiKey.Password;
            SettingsManager.Instance.Settings["api"]["user"] = InputUsername.Text;
            SettingsManager.Instance.Settings["api"]["gamemode"] = "" + ((OsuApiHelper.OsuMode)DropdownGamemode.SelectedIndex);
            int updateRate = int.Parse(InputUpdaterate.Text);
            updateRate = Math.Min(120, Math.Max(5, updateRate));
            SettingsManager.Instance.Settings["api"]["updateRate"] = "" + updateRate;

            SettingsManager.Instance.SettingsSave();
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
