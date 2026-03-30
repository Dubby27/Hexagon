using Hexagon.Screens;

namespace Hexagon
{
    public partial class MainPage : ContentPage
    {
        public static MainPage Instance { get; set; }

        public static void RefreshQuickPanelExt()
        {
            if(Instance != null)
            {
                Instance.StartLoginProcess();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            Instance = this;
        }

        public static bool refreshOnAppear = false;

        public static void RefreshOnAppear()
        {
            refreshOnAppear = true;
        }

        protected override void OnAppearing()
        {
            if (refreshOnAppear)
            {
                StartLoginProcess();
            }
            base.OnAppearing();
        }

        public static bool loggingIn = false;

        public async void StartLoginProcess()
        {
            try
            {
                if(await SecureStorage.GetAsync("LoggedIn") == "true")
                {
                    Bakalari.ProcessDetails = await SecureStorage.GetAsync("ProcessDetails") == "True";
                    Bakalari.BetaQuickTimetable = await SecureStorage.GetAsync("BetaQuickTimetable") == "True";
                    if (!string.IsNullOrEmpty(await SecureStorage.GetAsync("DataSaver")))
                    {
                        Bakalari.DataSaver = int.Parse(await SecureStorage.GetAsync("DataSaver"));
                    }
                }
            }
            catch(Exception ex) { }
            if (await SecureStorage.GetAsync("LoggedIn") != "true")
            {
                if (!loggingIn)
                {
                    loggingIn = true;
                    await Shell.Current.Navigation.PushModalAsync(new LogIn());
                }
            }
            else if (await SecureStorage.GetAsync("LoggedIn") == "true" && Bakalari.credentials == null)
            {
                bool r = await Bakalari.LoadOfflineTimetables();
                if (r)
                {
                    try
                    {
                        RefreshQuickPanel();
                    }
                    catch
                    {
                        //fail
                    }
                }
                await Bakalari.LoadOfflineData();
                if(Bakalari.DataSaver < 2)
                {
                    await Bakalari.LogInRefresh();
                }
                else
                {
                    Bakalari.StartTask("data_saver", "");
                    Bakalari.EndTask(false);
                }
                try
                {
                    RefreshQuickPanel();
                }
                catch
                {
                    //fail
                }
            }
            else if(await SecureStorage.GetAsync("LoggedIn") == "true" && Bakalari.credentials != null)
            {
                RefreshQuickPanel();
                if (Bakalari.DataSaver == 0)
                {
                    await Bakalari.RefreshAll();
                }
                RefreshQuickPanel();
            }
            QuickTitle.HorizontalTextAlignment = TextAlignment.Center;
        }

        public void RefreshQuickPanel()
        {
            QuickPanelStruct panelStruct = Hexagon.QuickPanel.EvaluateQuickPanel();

            QuickUpper.IsVisible = panelStruct.upper != null;
            if (QuickUpper.IsVisible)
            {
                QuickUpper.Text = panelStruct.upper;
            }
            QuickTitle.IsVisible = panelStruct.title != null;
            if (QuickTitle.IsVisible)
            {
                QuickTitle.Text = panelStruct.title;
            }
            QuickLower.IsVisible = panelStruct.lower != null;
            if (QuickLower.IsVisible)
            {
                QuickLower.Text = panelStruct.lower;
            }
            QuickTimetable.IsVisible = panelStruct.timetable != null;
            if (panelStruct.timetable != null)
            {
                QuickTimetable.Clear();
                QuickTimetable.Add(panelStruct.timetable);
                //QuickTitle.Text = QuickTimetable.Children[0].Background.BackgroundColor.ToString();
            }
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            if(Bakalari.TaskCount == 0)
            {
                Button button = (Button)sender;
                button.IsVisible = false;
                RefreshQuickPanel();
                await Bakalari.RefreshAll();
                RefreshQuickPanel();
                button.IsVisible = true;
            }
        }

        public async void ShowUpdateAvailable(Release details)
        {
            bool answer = await DisplayAlert("Aktualizace k dispozici", $"Je dostupná aktualizace {details.tag_name}, chcete ji stáhnout?", "Ne", "Ano");
            if(!answer)
            {
                Browser.Default.OpenAsync("https://github.com/Dubby27/Hexagon/releases/latest");
            }
            else
            {
                SecureStorage.SetAsync("DismissedVersion", details.tag_name);
            }
        }
    }
}
