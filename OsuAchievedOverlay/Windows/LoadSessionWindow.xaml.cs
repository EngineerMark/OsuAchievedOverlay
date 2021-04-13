using Humanizer;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for LoadSessionWindow.xaml
    /// </summary>
    public partial class LoadSessionWindow : Window
    {
        private UIElement _sessionPrefab;

        public UIElement SessionPrefab
        {
            get
            {
                return InterfaceManager.Instance.CloneElement(_sessionPrefab);
            }
        }

        public LoadSessionWindow()
        {
            InitializeComponent();
            _sessionPrefab = InterfaceManager.Instance.CloneElement(PrefabSessionListItem);

            RebuildList();

            Closed += (object sender, EventArgs e) =>
            {
                GameManager.Instance.SessionWin = null;
            };
        }

        private void RebuildList(){
            SessionList.Children.Clear();

            int id = 0;
            foreach (SessionFileData sessionData in SessionManager.Instance.GetSortedList())
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
            }
        }

        private void Btn_ClickSessionItem(string identifier)
        {
            //Button b = (Button)sender;
            //Grid parentGrid = (Grid)b.Parent;
            //string base64Path = "";
            //foreach (UIElement child in parentGrid.Children)
            //{
            //    if (child is TextBlock textblock)
            //    {
            //        if ((string)textblock.Tag == "SessionIdentifier")
            //        {
            //            base64Path = textblock.Text;
            //            break;
            //        }
            //    }
            //}

            if (!string.IsNullOrEmpty(identifier))
            {
                SessionFileData sessionFileData = SessionManager.Instance.FindByIdentifier(identifier);
                if (sessionFileData != null)
                {
                    OpenSessionFile(StringHelper.Base64Decode(identifier));
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
            //var fileStream = openFileDialog.OpenFile();
            var fileStream = new FileStream(path, FileMode.Open);

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string data = reader.ReadToEnd();
                if (data.Length > 0)
                {
                    Session newSession = null;
                    bool validSession = true;
                    try
                    {
                        newSession = JsonConvert.DeserializeObject<Session>(data);
                    }
                    catch (Exception)
                    {
                        validSession = false;
                        MessageBox.Show("Seems like the opened file is an invalid session file.", "Error opening session", MessageBoxButton.OK);
                    }
                    if (validSession)
                    {

                        SessionManager.Instance.AddFile(new SessionFileData()
                        {
                            FileName = System.IO.Path.GetFileNameWithoutExtension(path),
                            FileExtension = System.IO.Path.GetExtension(path),
                            FileLocation = System.IO.Path.GetDirectoryName(path),
                            FileDate = DateTimeOffset.Now.ToUnixTimeSeconds()
                        });

                        GameManager.Instance.CurrentSession = newSession;
                        GameManager.Instance.RefreshTimer(null, null);

                        Close();
                        GameManager.Instance.SessionWin = null;
                    }
                }
            }
        }

        private void Btn_LoadFromList(object sender, RoutedEventArgs e)
        {

        }
    }
}
