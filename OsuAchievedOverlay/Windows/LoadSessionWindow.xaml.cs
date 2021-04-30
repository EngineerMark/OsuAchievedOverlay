using Humanizer;
using Microsoft.Win32;
using Newtonsoft.Json;
using OsuAchievedOverlay.Managers;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for LoadSessionWindow.xaml
    /// </summary>
    public partial class LoadSessionWindow : Window
    {
        private UIElement _sessionPrefab;
        private UIElement _seperatorPrefab;

        public UIElement SessionPrefab
        {
            get
            {
                return InterfaceManager.Instance.CloneElement(_sessionPrefab);
            }
        }

        public UIElement SeperatorPrefab
        {
            get
            {
                return InterfaceManager.Instance.CloneElement(_seperatorPrefab);
            }
        }

        public LoadSessionWindow()
        {
            InitializeComponent();
            _sessionPrefab = InterfaceManager.Instance.CloneElement(PrefabSessionListItem);
            _seperatorPrefab = InterfaceManager.Instance.CloneElement(PrefabSeperator);

            RebuildList();

            Closed += (object sender, EventArgs e) =>
            {
                WindowManager.Instance.SessionWin = null;
            };
        }

        private void RebuildList()
        {
            SessionList.Children.Clear();

            int id = 0;
            foreach (SessionFileData sessionData in SessionManager.Instance.GetSortedList())
            {
                Session temp = null;
                try
                {
                    temp = LoadSession(System.IO.Path.Combine(sessionData.FileLocation, sessionData.FileName + sessionData.FileExtension));
                }catch(Exception e){
                    
                }

                if (temp != null)
                {
                    Grid clonedPrefab = (Grid)SessionPrefab;
                    foreach (UIElement child in clonedPrefab.Children)
                    {
                        if (child is TextBlock textblock)
                        {
                            switch (textblock.Text)
                            {
                                case " {label_filename}":
                                    textblock.Text = sessionData.FileName + sessionData.FileExtension;
                                    break;
                                case " {label_filepath}":
                                    textblock.Text = sessionData.FileLocation;
                                    break;
                                case " {label_date}":
                                    textblock.Text = "" + DateTimeOffset.FromUnixTimeSeconds(sessionData.FileDate).UtcDateTime.Humanize();
                                    break;
                                case " {label_fullpath}":
                                    textblock.Text = System.IO.Path.Combine(sessionData.FileLocation, sessionData.FileName + sessionData.FileExtension);
                                    break;
                                default:
                                    textblock.Text = "";
                                    break;
                            }

                            if ((string)textblock.Tag == "SessionIdentifier")
                            {
                                textblock.Text = sessionData.Identifier;
                            }
                        }
                        else if (child is Button button)
                        {
                            if ((string)button.Tag == "SessionOpen")
                            {
                                if (temp.Version != Session.CurrentVersion)
                                {
                                    //Tooltip doesn't display when button is disabled... why wpf?
                                    //button.IsEnabled = false;
                                    button.ToolTip = "This session is made in a different version than supported (Session is " + temp.Version + ", but you are running " + Session.CurrentVersion + ")";
                                }
                                button.Click += (object sender, RoutedEventArgs e) =>
                                {
                                    Btn_ClickSessionItem(sessionData.Identifier);
                                };
                            }
                            else if ((string)button.Tag == "SessionRemove")
                            {
                                button.Click += (object sender, RoutedEventArgs e) =>
                                {
                                    Btn_RemoveSessionItem(sessionData.Identifier);
                                };
                            }
                        }
                    }
                    id++;
                    SessionList.Children.Add(clonedPrefab);
                    SessionList.Children.Add(SeperatorPrefab);
                }
            }
        }

        private void Btn_ClickSessionItem(string identifier)
        {
            if (!string.IsNullOrEmpty(identifier))
            {
                SessionFileData sessionFileData = SessionManager.Instance.FindByIdentifier(identifier);
                if (sessionFileData != null)
                {
                    string path = StringHelper.Base64Decode(identifier);
                    OpenSessionFile(path);
                }
            }
        }

        private void Btn_RemoveSessionItem(string identifier)
        {
            SessionManager.Instance.SessionFiles.RemoveAll(a => a.Identifier == identifier);
            RebuildList();
        }

        private void Btn_LoadFromFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                //File.WriteAllText(saveFileDialog.FileName, json);
                OpenSessionFile(openFileDialog.FileName);
            }
        }

        private void OpenSessionFile(string path)
        {
            Session newSession = null;
            bool validSession = true;
            try
            {
                newSession = LoadSession(path);
            }
            catch (Exception e)
            {
                validSession = false;
                MessageBox.Show("Seems like the opened file is an invalid session file.\n\n"+e.Message, "Error opening session", MessageBoxButton.OK);
            }
            if (validSession)
            {

                SessionManager.Instance.AddFile(path);

                SessionManager.Instance.CurrentSession = newSession;
                if (newSession.Username != null && newSession.Username != GameManager.Instance.OsuUser.Name)
                {
                    SettingsManager.Instance.Settings["api"]["user"] = newSession.Username;
                    GameManager.Instance.OsuUser = OsuApiHelper.OsuApi.GetUser(newSession.Username, (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]));
                }

                GameManager.Instance.RefreshTimer();

                Close();
                WindowManager.Instance.SessionWin = null;
            }
        }

        public Session LoadSession(string path)
        {
            var fileStream = new FileStream(path, FileMode.Open);

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string data = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Session>(data);
            }
        }

        private void Btn_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Btn_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
