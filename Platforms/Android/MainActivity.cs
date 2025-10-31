using Android.App;
using Android.Content.PM;
using Android.OS;

namespace Hexagon
{
    [Activity(Theme = "@style/LaunchTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.SetTheme(Resource.Style.MainTheme);
            base.OnCreate(savedInstanceState);

        }
    }
}
