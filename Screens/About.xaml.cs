namespace Hexagon.Screens;

public partial class About : ContentPage
{
	public About()
	{
		InitializeComponent();

		VersionLabel.Text = "Hexagon Client " + AppInfo.VersionString;
	}
}