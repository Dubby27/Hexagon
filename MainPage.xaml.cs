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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartLoginProcess();
        }

        public async void StartLoginProcess()
        {
            try
            {
                Bakalari.ProcessDetails = await SecureStorage.GetAsync("ProcessDetails") == "True";
            }
            catch(Exception ex) { }
            if (await SecureStorage.GetAsync("LoggedIn") != "true")
            {
                await Navigation.PushModalAsync(new LogIn()); 
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
            if (panelStruct.timetable != null)
            {
                QuickTimetable = panelStruct.timetable;
            }
            else
            {
                QuickTimetable.IsVisible = false;
            }
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            await Bakalari.RefreshAll();
            RefreshQuickPanel();
        }
    }
}
