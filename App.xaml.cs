using System.ComponentModel;

namespace Hexagon
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            AppShell shell = new AppShell();
            Window window = new Window(shell);

            return window;
        }
    }
}