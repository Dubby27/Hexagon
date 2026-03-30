using Hexagon.Screens;
using System.ComponentModel;
using System.Diagnostics;
#if ANDROID
using Android.App;
using Android.OS;
#endif

namespace Hexagon
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public static Window window;

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Shell shell = DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone ? new PhoneShell() : new DesktopShell();
            window = new Window(shell);

            window.TitleBar = new TitleBar
            {
                Title = "Hexagon"
            };

            window.Activated += (s, e) =>
            {
                Hexagon.MainPage.RefreshQuickPanelExt();
            };
#if ANDROID
            Current.RequestedThemeChanged += (s, a) =>
            {
                UpdateThemeColor(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, null);
                if(Shell.Current.CurrentPage.ClassId == "LoginPage")
                {
                    Shell.Current.Navigation.PopToRootAsync();
                    Shell.Current.Navigation.PushModalAsync(new LogIn());
                }
            };
#endif

            return window;
        }

#if ANDROID
        static void UpdateThemeColor(Android.App.Activity activity, Bundle bundle)
        {
            bool success = App.Current.RequestedTheme == AppTheme.Light ? App.Current.Resources.TryGetValue("GradientStart", out object color) : App.Current.Resources.TryGetValue("GradientStartDark", out color);
            Color clr = (Color)color;
            new PlatformThemeService().SetStatusBarColor(clr);
            success = App.Current.RequestedTheme == AppTheme.Light ? App.Current.Resources.TryGetValue("GradientEnd", out color) : App.Current.Resources.TryGetValue("GradientEndDark", out color);
            clr = (Color)color;
            new PlatformThemeService().SetNavBarColor(clr);
        }
#endif
    }
}

namespace Hexagon.Drawable
{
    public class GradientTextDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Create a horizontal gradient
            var gradient = new LinearGradientPaint
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops =
                [
                new PaintGradientStop(0, Colors.Aqua),
                new PaintGradientStop(1, Colors.Blue)
                ]
            };

            // Use the gradient to draw the text
            canvas.SetFillPaint(gradient, dirtyRect);
            canvas.FontSize = 64;
            canvas.DrawString("Gradient Text", dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}