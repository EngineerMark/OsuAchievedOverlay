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
    /// Interaction logic for LocalApiWindow.xaml
    /// </summary>
    public partial class LocalApiWindow : Window
    {
        private UIElement _apifilePrefab;
        public UIElement ApiFilePrefab
        {
            get
            {
                return InterfaceManager.Instance.CloneElement(_apifilePrefab);
            }
        }

        //Follows the order of the prefab in the xaml window, make sure to adjust when modifying the grid contents
        private enum FileItemId{
            FileName = 0,
            StringData = 1,
            RemovalButton = 2,
            PrefixPositive = 3,
            PrefixNegative = 4
        }

        private Session ExampleSession = new Session()
        {
            InitialData = new SessionData(){
                RankSilverSS = 10,
                RankGoldSS = 15,
                RankSilverS = 100,
                RankGoldS = 300,
                RankA = 1100,
                TotalScore = 5000000,
                RankedScore = 1000000,
                Playcount = 550,
                Playtime = 200
            },
            CurrentData = new SessionData()
            {
                RankSilverSS = 40,
                RankGoldSS = 19,
                RankSilverS = 150,
                RankGoldS = 320,
                RankA = 1200,
                TotalScore = 5900000,
                RankedScore = 1400000,
                Playcount = 950,
                Playtime = 400
            },
            DifferenceData = new SessionData()
            {
                RankSilverSS = 10,
                RankGoldSS = 4,
                RankSilverS = 50,
                RankGoldS = 20,
                RankA = 100,
                TotalScore = 900000,
                RankedScore = 400000,
                Playcount = 400,
                Playtime = 200
            },
        };

        public LocalApiWindow()
        {
            InitializeComponent();
            WindowManager.Instance.ApiWin = this;

            _apifilePrefab = InterfaceManager.Instance.CloneElement(PrefabAPIFilesItem);
            ListAPIFiles.Children.Clear();

            Closed += (object sender, EventArgs e) =>
            {
                WindowManager.Instance.ApiWin = null;
            };

            LocalAPIManager.Instance.Start();
        }

        public void AddItem(LocalApiFile apiFile){
            AddItem(apiFile.FileName, apiFile.StringData, apiFile.PositivePrefix, apiFile.NegativePrefix);
        }

        public void AddItem(string fileName, string stringData, string positive, string negative){
            UIElement newFile = ApiFilePrefab;
            ((TextBox)((Grid)newFile).Children[(int)FileItemId.FileName]).Text = fileName;
            ((TextBox)((Grid)newFile).Children[(int)FileItemId.StringData]).Text = stringData;
            ((TextBox)((Grid)newFile).Children[(int)FileItemId.PrefixPositive]).Text = positive;
            ((TextBox)((Grid)newFile).Children[(int)FileItemId.PrefixNegative]).Text = negative;
            ((TextBox)((Grid)newFile).Children[(int)FileItemId.StringData]).TextChanged += Input_ChangeStringData;
            ((Button)((Grid)newFile).Children[(int)FileItemId.RemovalButton]).Click += Btn_RemoveItem;
            ListAPIFiles.Children.Add(newFile);
            Input_ChangeStringData(((TextBox)((Grid)newFile).Children[(int)FileItemId.StringData]), null);

            ((Grid)newFile).Tag = new LocalApiFile()
            {
                FileName = fileName,
                StringData = stringData,
                PositivePrefix = positive,
                NegativePrefix = negative
            };
        }

        private void Btn_NewItem(object sender, RoutedEventArgs e)
        {
            UIElement newFile = ApiFilePrefab;
            ((TextBox)((Grid)newFile).Children[(int)FileItemId.StringData]).TextChanged += Input_ChangeStringData;
            ((Button)((Grid)newFile).Children[(int)FileItemId.RemovalButton]).Click += Btn_RemoveItem;
            ListAPIFiles.Children.Add(newFile);
        }

        private void Btn_RemoveItem(object sender, RoutedEventArgs e){
            Button src = (Button)sender;
            Grid currentGridItem = (Grid)src.Parent;
            ((StackPanel)currentGridItem.Parent).Children.Remove(currentGridItem);
        }

        private void Input_ChangeStringData(object sender, TextChangedEventArgs e)
        {
            TextBox src = (TextBox)sender;
            Grid currentGridItem = (Grid)src.Parent;
            LocalApiFile fileObject = (LocalApiFile)currentGridItem.Tag;
            if (fileObject != null)
            {
                TextBox stringData = (TextBox)currentGridItem.Children[(int)FileItemId.StringData];
                stringData.ToolTip = LocalAPIManager.Instance.Parse(fileObject, ExampleSession);
            }
        }

        private void Btn_SaveData(object sender, RoutedEventArgs e)
        {
            LocalAPIManager.Instance.ApiDataList.Clear();
            UIElementCollection elements = ListAPIFiles.Children;
            foreach(UIElement APIFileItem in elements){
                Grid APIFileItemGrid = (Grid)APIFileItem;

                TextBox fileName = (TextBox)APIFileItemGrid.Children[(int)FileItemId.FileName];
                TextBox stringData = (TextBox)APIFileItemGrid.Children[(int)FileItemId.StringData];
                TextBox positivePrefix = (TextBox)APIFileItemGrid.Children[(int)FileItemId.PrefixPositive];
                TextBox negativePrefix = (TextBox)APIFileItemGrid.Children[(int)FileItemId.PrefixNegative];

                LocalApiFile fileObject = new LocalApiFile()
                {
                    FileName = fileName.Text,
                    StringData = stringData.Text,
                    PositivePrefix = positivePrefix.Text,
                    NegativePrefix = negativePrefix.Text
                };

                APIFileItemGrid.Tag = fileObject;

                LocalAPIManager.Instance.ApiDataList.Add(fileObject);
            }

            if (File.Exists(LocalAPIManager.ApiDataFile))
                File.Delete(LocalAPIManager.ApiDataFile);

            File.WriteAllText(LocalAPIManager.ApiDataFile, JsonConvert.SerializeObject(LocalAPIManager.Instance.ApiDataList));
        }
    }
}
