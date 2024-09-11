using System.Windows;
using System.Windows.Input;
using ViciousGPT.Properties;
using static ViciousGPT.KeyboardHotkeyManager;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Controls;


namespace ViciousGPT;

public partial class MainWindow : Window
{
    private KeyboardHotkeyManager? triggerHotkey;
    private KeyboardHotkeyManager? cancelHotkey;
    private readonly ControllerHotkeyManager controllerHotkeys = new() { PlayerIndex = Settings.Default.ControllerPlayerIndex };
    private readonly GlobalController globalController;

    public MainWindow()
    {
        IntializeTrace();
        InitializeComponent();
        Trace.TraceInformation("Starting GlobalController");
        globalController = new GlobalController(this);
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;

        openAiKeyBox.Password = Settings.Default.OpenAiKey;
        openAiKeyBox.PasswordChanged += (sender, e) => Settings.Default.OpenAiKey = openAiKeyBox.Password;
        CheckGoogleCloudCredential();
    }

    private void CheckGoogleCloudCredential()
    {
        if (HasGoogleCloudCredential())
        {
            googleCloudFoundLabel.Visibility = Visibility.Visible;
            googleCloudNotFoundLabel.Visibility = Visibility.Collapsed;
        }
        else
        {
            googleCloudFoundLabel.Visibility = Visibility.Collapsed;
            googleCloudNotFoundLabel.Visibility = Visibility.Visible;
        }
    }

    private static bool HasGoogleCloudCredential()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return File.Exists(appData + @"\gcloud\application_default_credentials.json");
    }

    private static void IntializeTrace()
    {
        if (GlobalController.OutputLogFile)
        {
            Trace.Listeners.Add(new TextWriterTraceListener("ViciousGPT.log"));
            Trace.AutoFlush = true;
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Settings.Default.PropertyChanged += (sender, e) => Settings.Default.Save();
        triggerHotkey = new KeyboardHotkeyManager(this, Modifier.Control, Key.Enter, () => globalController.OnTriggered());
        cancelHotkey = new KeyboardHotkeyManager(this, Modifier.Control|Modifier.Alt, Key.Enter, () => globalController.OnCancelled());
        controllerHotkeys.OnSticksPressed = () => globalController.OnTriggered();
        controllerHotkeys.OnTriggersPressed = () => globalController.OnCancelled();
        controllerHotkeys.Start();
        UpdateControllerRadio();
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

    private void controllerRadio_Checked(object sender, RoutedEventArgs e)
    {
        uint index = sender switch
        {
            _ when ReferenceEquals(sender, controllerRadio1) => 0,
            _ when ReferenceEquals(sender, controllerRadio2) => 1,
            _ when ReferenceEquals(sender, controllerRadio3) => 2,
            _ when ReferenceEquals(sender, controllerRadio4) => 3,
            _ => throw new ArgumentException("Invalid sender"),
        };
        controllerHotkeys.PlayerIndex = index;
        Settings.Default.ControllerPlayerIndex = index;
    }

    private void UpdateControllerRadio()
    {
        (Settings.Default.ControllerPlayerIndex switch
        {
            3 => controllerRadio4,
            2 => controllerRadio3,
            1 => controllerRadio2,
            _ => controllerRadio1,
        }).IsChecked = true;
    }
}
