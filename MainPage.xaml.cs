using Hexagon.Screens;

namespace Hexagon
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

            Navigation.PushModalAsync(new LogIn());
        }
    }
}
