using CommunityToolkit.Maui.Storage;
using System.Text;

namespace Hexagon.Screens;

public partial class DataExport : ContentPage
{
	public DataExport()
	{
		InitializeComponent();
	}

    private async void ExportPermanentScheduleButton_Clicked(object sender, EventArgs e)
    {
		try
		{
            var scheduleStream = new MemoryStream(Encoding.UTF8.GetBytes(await SecureStorage.GetAsync("PermanentTimetable")));
            await FileSaver.Default.SaveAsync("hexagon_permanent_timetable.txt", scheduleStream);
        }
        catch (Exception ex)
        {
            Shell.Current.DisplayAlert("Chyba", "Nepodaøilo se exportovat trvalý rozvrh: " + ex.Message, "Zavøít");
        }
    }
}