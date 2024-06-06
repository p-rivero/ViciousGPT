using System.Windows;
using System.Windows.Input;
using static ViciousGPT.GlobalHotkeyManager;


namespace ViciousGPT;

public partial class MainWindow : Window
{
    private GlobalHotkeyManager? triggerHotkey;
    private GlobalHotkeyManager? cancelHotkey;
    private readonly GlobalController globalController;

    public MainWindow()
    {
        InitializeComponent();
        globalController = new GlobalController(this);
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

    protected override void OnClosed(EventArgs e)
    {
        globalController.Dispose();
        base.OnClosed(e);
    }
}
