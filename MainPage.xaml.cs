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

        public async void StartLoginProcess()
        {
            try
            {
                Bakalari.ProcessDetails = await SecureStorage.GetAsync("ProcessDetails") == "True";
                Bakalari.BetaQuickTimetable = await SecureStorage.GetAsync("BetaQuickTimetable") == "True";
            }
            catch(Exception ex) { }
            if (await SecureStorage.GetAsync("LoggedIn") != "true")
            {
                await Shell.Current.Navigation.PushModalAsync(new LogIn()); 
            }
            if (await SecureStorage.GetAsync("LoggedIn") == "true" && Bakalari.credentials == null)
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
                await Bakalari.LogInRefresh();
                try
                {
                    RefreshQuickPanel();
                }
                catch
                {
                    //fail
                }
            }
            if(await SecureStorage.GetAsync("LoggedIn") == "true" && Bakalari.credentials != null)
            {
                await Bakalari.RefreshAll();
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
                await Bakalari.RefreshAll();
                RefreshQuickPanel();
                button.IsVisible = true;
            }
        }
    }
}
