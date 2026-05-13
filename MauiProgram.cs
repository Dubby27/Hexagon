using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

#if ANDROID
using Android.App;
using Android.OS;
#endif

namespace Hexagon
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SpaceGrotesk-VariableFont_wght.ttf", "SpaceGrotesk");
                })
                .UseMauiCommunityToolkit()
                .UseSentry(options =>
                {
                    options.Dsn = "https://b64e38c62eb479c42ce466a9a391f9d3@o4510159806988288.ingest.de.sentry.io/4510159817277520";
                    options.Debug = true;
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

    public class PlatformThemeService
    {
#if ANDROID
        // Source: https://stackoverflow.com/a/39164921/10083577
        public void SetStatusBarColor(Color color)
        {
            // The SetStatusBarcolor is new since API 21
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var androidColor = Android.Graphics.Color.ParseColor(color.ToHex());
                Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window.SetStatusBarColor(androidColor);
            }
            else
            {
                // Here you will just have to set your 
                // color in styles.xml file as shown below.
            }
        }
        public void SetNavBarColor(Color color)
        {
            // The SetStatusBarcolor is new since API 21
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var androidColor = Android.Graphics.Color.ParseColor(color.ToHex());
                Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window.SetNavigationBarColor(androidColor);
            }
            else
            {
                // Here you will just have to set your 
                // color in styles.xml file as shown below.
            }
        }
#endif
    }
}
