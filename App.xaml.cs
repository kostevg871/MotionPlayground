using Microsoft.Maui.Controls;

namespace MotionPlayground
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}
