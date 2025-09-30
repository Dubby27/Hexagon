using Hexagon.Screens;

namespace Hexagon
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

            if(SecureStorage.GetAsync("LoggedIn").Result != "true")
            {
                Navigation.PushModalAsync(new LogIn());
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (SecureStorage.GetAsync("LoggedIn").Result == "true" && Bakalari.credentials == null)
            {
                Bakalari.LogInRefresh();
            }
        }
    }
}
