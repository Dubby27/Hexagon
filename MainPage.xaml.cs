using Hexagon.Screens;

namespace Hexagon
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartLoginProcess();
        }

        public async void StartLoginProcess()
        {
            if (await SecureStorage.GetAsync("LoggedIn") != "true")
            {
                await Navigation.PushModalAsync(new LogIn());
            }
            if (await SecureStorage.GetAsync("LoggedIn") == "true" && Bakalari.credentials == null)
            {
                await Bakalari.LoadOfflineData();
                RefreshQuickPanel();
                await Bakalari.LogInRefresh();
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

        private async void Button_Clicked(object sender, EventArgs e)
        {
            //log out for testing purposes
            Bakalari.credentials = null;
            Bakalari.IsSynced = false;

            Bakalari.StatusLabel = "Odhlášen";
            Bakalari.GoodImage = false;
            Bakalari.StatusActivity = false;
            Bakalari.BadImage = true;

            //delete credentials
            SecureStorage.Remove("LoggedIn");
            SecureStorage.Remove("School");
            SecureStorage.Remove("RefreshToken");

            await Navigation.PushModalAsync(new LogIn());
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            await Bakalari.RefreshAll();
            RefreshQuickPanel();
        }
    }
}
