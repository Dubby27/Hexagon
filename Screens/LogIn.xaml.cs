namespace Hexagon.Screens;

public partial class LogIn : ContentPage
{
	public LogIn()
	{
		InitializeComponent();
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

    private void LogInSubmit_Clicked(object sender, EventArgs e)
    {
        //check URL validity
        Uri uri;
        bool result = Uri.TryCreate(SchoolEntry.Text, UriKind.Absolute, out uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        if(result == false)
        {
            Shell.Current.DisplayAlert("Špatná URL Školy", "Adresa, kterou jsi zadal nevypadá jako URL." +
                " Správná URL by mìla vypadat asi takto: https://bakalari.example.cz nebo https://www.example.cz," +
                " pøipadnì s èíslem portu na konci jako https://bakalari.example.cz:444", "Ok");
            return;
        }
    }
}