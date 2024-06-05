using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ViciousGPT.GlobalHotkeyManager;


namespace ViciousGPT
{
    public partial class MainWindow : Window
    {
        private GlobalHotkeyManager? triggerHotkey;
        private GlobalHotkeyManager? cancelHotkey;
        private readonly GlobalController globalController = new();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            triggerHotkey = new GlobalHotkeyManager(this, Modifier.Control, Key.Enter, () => globalController.OnTriggered());
            cancelHotkey = new GlobalHotkeyManager(this, Modifier.Control|Modifier.Alt, Key.Enter, () => globalController.OnCancelled());
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            triggerHotkey?.Dispose();
            cancelHotkey?.Dispose();
        }
    }
}
