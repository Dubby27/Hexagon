using CommunityToolkit.Maui.Storage;
using System.Text;

namespace Hexagon.Screens;

public partial class DataExport : ContentPage
{
	public DataExport()
	{
		InitializeComponent();

        ActualScheduleUpdateLabel.Text = "Aktualizováno " + Bakalari.actualValid.ToString();
        NextScheduleUpdateLabel.Text = "Aktualizováno " + Bakalari.nextValid.ToString();
    }

    public async Task Export(string data, string fileName)
    {
        try
        {
            var scheduleStream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            await FileSaver.Default.SaveAsync(fileName, scheduleStream);
        }
        catch (Exception ex)
        {
            Shell.Current.DisplayAlert("Chyba", "Nepodaøilo se exportovat data: " + ex.Message, "Zavøít");
        }
    }

    private async void ExportPermanentScheduleButton_Clicked(object sender, EventArgs e)
    {
        await Export(await SecureStorage.GetAsync("PermanentTimetable"), "hexagon_permanent_timetable.txt");
    }

    private async void ExportActualScheduleButton_Clicked(object sender, EventArgs e)
    {
        await Export(await SecureStorage.GetAsync("ActualTimetable"), "hexagon_actual_timetable.txt");
    }

    private async void ExportNextScheduleButton_Clicked(object sender, EventArgs e)
    {
        await Export(await SecureStorage.GetAsync("NextTimetable"), "hexagon_next_timetable.txt");
    }

    private async void ExportUserDataButton_Clicked(object sender, EventArgs e)
    {
        await Export(await SecureStorage.GetAsync("UserData"), "hexagon_user_data.txt");
    }

    private async void ExportEventsButton_Clicked(object sender, EventArgs e)
    {
        await Export(await SecureStorage.GetAsync("UserEventData"), "hexagon_event_data.txt");
    }
}