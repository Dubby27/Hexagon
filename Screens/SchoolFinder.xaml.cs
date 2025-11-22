using System.Diagnostics;

namespace Hexagon.Screens;

public partial class SchoolFinder : ContentPage
{
	public LogIn ParentPage { get; set; }
	public static SchoolFinder Instance { get; set; }

    public SchoolFinder(LogIn parent)
	{
		InitializeComponent();

		Instance = this;
        ParentPage = parent;
		if(Bakalari.schoolList == null)
		{
            Task<bool> schoolListTask = Task.Run(() => Bakalari.GetSchoolList());
            schoolListTask.ContinueWith((schoolListTask) =>
            {
                Trace.WriteLine(schoolListTask.Result);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SchoolFinder.Instance.ListLoadFinished(schoolListTask.Result);
                });
            });
        }
        else
        {
            LoadingLayout.IsVisible = false;
            SchoolListView.IsVisible = true;
            SchoolSearchEntry.IsVisible = true;
        }
	}

	public void ListLoadFinished(bool state)
	{
        if (state)
		{
			LoadingLayout.IsVisible = false;
            SchoolListView.IsVisible = true;
            SchoolSearchEntry.IsVisible = true;
        }
		else
		{
			LoadingLabel.Text = "Nepodaøilo se naèíst seznam škol. Zkontroluj své pøipojení k internetu a zkus to znovu.";
			SchoolFinderActivityIndicator.IsVisible = false;
        }
        NetworkStatusLabel.Text = "Odhlášen";
        NetworkGoodImage.IsVisible = false;
        NetworkBadImage.IsVisible = true;
    }

    public void BackButton_Clicked(object sender, EventArgs e)
	{
		Shell.Current.Navigation.PopAsync();
    }

    private void SchoolListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        ParentPage.SchoolURL = ((SchoolListEntry)e.SelectedItem).url;
        Shell.Current.Navigation.PopAsync();
    }

    public async Task<List<SchoolListEntry>> SearchForSchool()
    {
        List<SchoolListEntry> results = new List<SchoolListEntry>();
        results = Bakalari.schoolList.Where(school => school.name.Contains(SchoolFinder.Instance.SchoolSearchEntry.Text, StringComparison.OrdinalIgnoreCase)).ToList();
        return results;
    }

    public Task? SearchTask;

    private void SchoolSearchEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(SearchTask != null && !SearchTask.IsCompleted)
        {
            
        }

        SearchTask = Task.Run(async () =>
        {
            List<SchoolListEntry> results = await SearchForSchool();
            if(results.Count < 10)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SchoolListView.ItemsSource = results;
                });
            }
        });
    }
}