using System.Diagnostics;

namespace Hexagon;

public partial class User : ContentPage
{
    bool appearedOnce = false;

    public User()
	{
		InitializeComponent();

        try
        {
            UserName.Text = Bakalari.userData.FullName;
            SchoolURL.Text = Bakalari.school.ToString();
            ProcessDetailSwitch.IsChecked = Bakalari.ProcessDetails;
            TimetableStylePicker.SelectedIndex = Bakalari.BetaQuickTimetable ? 1 : 0;
        }
        catch(Exception ex) { }

        appearedOnce = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (appearedOnce)
        {
            try
            {
                UserName.Text = Bakalari.userData.FullName;
                SchoolURL.Text = Bakalari.school.ToString();
                ProcessDetailSwitch.IsChecked = Bakalari.ProcessDetails;
                TimetableStylePicker.SelectedIndex = Bakalari.BetaQuickTimetable ? 1 : 0;
            }
            catch (Exception ex) { }
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

        await Navigation.PushModalAsync(new Hexagon.Screens.LogIn());
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Shell.Current.DisplayAlert("Pokroèilé informace:",
            "UserUID: " + Bakalari.userData.UserUID + ", " +
            "Je Rodiè: " + (Bakalari.userData.UserType == "parents" ? "ano" : "ne"),
            "Zavøít");
    }

    private void ProcessDetailSwitch_Toggled(object sender, CheckedChangedEventArgs e)
    {
        Bakalari.ProcessDetails = e.Value;
        SecureStorage.SetAsync("ProcessDetails", e.Value.ToString());
    }

    private void TimetableStylePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        Bakalari.BetaQuickTimetable = TimetableStylePicker.SelectedIndex == 1;
        SecureStorage.SetAsync("BetaQuickTimetable", (TimetableStylePicker.SelectedIndex == 1).ToString());
        MainPage.RefreshOnAppear();
    }

    private void Button_Clicked_2(object sender, EventArgs e)
    {
        Browser.Default.OpenAsync("https://forms.gle/Q6Ex43wiMrZVnF9WA");
    }
}