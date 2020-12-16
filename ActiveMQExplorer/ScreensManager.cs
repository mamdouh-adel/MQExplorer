using ActiveMQExplorer.Views;

namespace ActiveMQExplorer
{
    public class ScreensManager
    {
        public static MainWindowView MainWindow { get; set; }
        public static SettingsWindowView SettingsWindow { get; set; }
        public static bool IsClosing { get; set; } = false;
    }
}
