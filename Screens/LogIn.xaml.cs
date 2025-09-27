namespace Hexagon.Screens;

public partial class LogIn : ContentPage
{
	public LogIn()
	{
	    InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        // Return true to consume the event and prevent default back navigation
        return true;
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

    }
}
