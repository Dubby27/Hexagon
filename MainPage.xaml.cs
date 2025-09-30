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
                await Bakalari.LogInRefresh();
            }
        }
    }
}
