using System.Threading.Tasks;

namespace Hexagon.Screens;

public partial class LogIn : ContentPage
{

    public string? SchoolURL;

    public LogIn()
	  {
	    InitializeComponent();
        NetworkActivityIndicator.IsVisible = false;
        NetworkGoodImage.IsVisible = false;
        NetworkStatusLabel.Text = "Odhlášen";
      }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if(SchoolURL != null)
        {
            SchoolEntry.Text = SchoolURL;
            SchoolURL = null;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (Bakalari.refreshInvalid)
        {
            base.OnBackButtonPressed();
            return false;
        }
        else
        {
            // Return true to consume the event and prevent default back navigation
            return true;
        }
    }

    private void SchoolEntry_Focused(object sender, FocusEventArgs e)
    {
		if(SchoolEntry.Text == "")
        {
            SchoolEntry.Text = "https://";
        }
    }

    private void SchoolEntry_Unfocused(object sender, FocusEventArgs e)
    {
        if (SchoolEntry.Text == "https://")
        {
            SchoolEntry.Text = "";
        }
    }

    private void LogInButton_Clicked(object sender, EventArgs e)
    {
        //check URL validity
        Uri uri;
        bool result = Uri.TryCreate(SchoolEntry.Text, UriKind.Absolute, out uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        if(result == false)
        {
            Shell.Current.DisplayAlert("Špatná URL školy", "Adresa, kterou jsi zadal, nevypadá jako URL." +
                " Správná URL by měla vypadat asi takto: https://bakalari.example.cz nebo https://www.example.cz," +
                " případně s číslem portu na konci jako https://bakalari.example.cz:444", "Ok");
            return;
        }
        else
        {
            Bakalari.OnLogInFinished += LoginFinished;
            if(UsernameEntry.Text == "")
            {
                Shell.Current.DisplayAlert("Neznámé údaje", "Uživatelské jméno nebylo zadáno", "Ok");
            }
            else
            {
                if (PasswordEntry.Text == "")
                {
                    Shell.Current.DisplayAlert("Neznámé údaje", "Heslo nebylo zadáno", "Ok");
                }
                else
                {
                    Bakalari.LogIn(uri, UsernameEntry.Text, PasswordEntry.Text);
                }
            }
        }
    }

    public async void LoginFinished(bool success)
    {
        if(success)
        {
            if (RememberCheck.IsChecked)
            {
                _ = SecureStorage.SetAsync("Username", UsernameEntry.Text);
                _ = SecureStorage.SetAsync("Password", PasswordEntry.Text);
            }
            _ = Shell.Current.Navigation.PopToRootAsync(true);
        }
        else
        {
            //Shell.Current.DisplayAlert("Chyba přihlášení", "Přihlášení se nezdařilo. Zkontroluj, zda máš správně zadanou adresu školy, uživatelské jméno a heslo.", "Ok");
        }
    }

    private void SchoolFinderButton_Clicked(object sender, EventArgs e)
    {
        Shell.Current.Navigation.PushAsync(new SchoolFinder(this));
    }
}
