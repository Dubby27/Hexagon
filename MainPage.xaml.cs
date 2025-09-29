using Hexagon.Screens;

namespace Hexagon
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

            Navigation.PushModalAsync(new LogIn());
            /*if(SecureStorage.GetAsync("LoggedIn").Result == "true")
            {
                Task.Run(() =>
                {
                    Bakalari.LogInRefresh();
                }).Wait();
            }
            else
            {
                Navigation.PushModalAsync(new LogIn());
            }*/
        }
    }
}
