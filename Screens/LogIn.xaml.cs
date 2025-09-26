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

    }
}