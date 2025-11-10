namespace Hexagon;

public partial class User : ContentPage
{
	public User()
	{
		InitializeComponent();

        try
        {
            UserName.Text = Bakalari.userData.FullName;
            SchoolURL.Text = Bakalari.school.ToString();
        }
        catch { }
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

        await Navigation.PushModalAsync(new Hexagon.Screens.LogIn());
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Shell.Current.DisplayAlert("Pokroèilé informace:",
            "UserUID: " + Bakalari.userData.UserUID + ", " +
            "Je Rodiè: " + (Bakalari.userData.UserType == "parents" ? "ano" : "ne"),
            "Zavøít");
    }
}