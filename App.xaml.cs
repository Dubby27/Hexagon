using System.ComponentModel;
using CommunityToolkit.Maui;

namespace Hexagon
{
    public partial class App : Application
    {
        public static Window window;

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Shell shell = DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone ? new Shell() : new DesktopShell();
            window = new Window(shell);

            window.TitleBar = new TitleBar
            {
                Title = "Hexagon",
                BackgroundColor = Current.RequestedTheme == AppTheme.Light ?
                    Application.Current.Resources["GradientStart"] as Color :
                    Application.Current.Resources["GradientStartDark"] as Color,
                ForegroundColor = Current.RequestedTheme == AppTheme.Light ?
                    Application.Current.Resources["Black"] as Color :
                    Application.Current.Resources["White"] as Color
            };

            return window;
        }
    }
}