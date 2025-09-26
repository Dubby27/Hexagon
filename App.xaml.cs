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
            Shell shell = DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone ? new PhoneShell() : new DesktopShell();
            window = new Window(shell);

            window.TitleBar = new TitleBar
            {
                Title = "Hexagon",
                /*BackgroundColor = Current.RequestedTheme == AppTheme.Light ?
                    Current.Resources["GradientStart"] as Color :
                    Current.Resources["GradientStartDark"] as Color,
                ForegroundColor = Current.RequestedTheme == AppTheme.Light ?
                    Current.Resources["Black"] as Color :
                    Current.Resources["White"] as Color*/
            };

            return window;
        }
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