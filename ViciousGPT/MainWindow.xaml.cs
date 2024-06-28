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
        RefreshSelectFileDoneVisibility();
    }

    private void RefreshSelectFileDoneVisibility()
    {
        selectFileDone.Visibility = Settings.Default.GoogleServiceAccountPath.Length > 0 ? Visibility.Visible : Visibility.Hidden;
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

    private void selectFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog();
        bool success = dialog.ShowDialog() ?? false;
        if (success)
        {
            string newFilePath = Path.Combine(GetPersistentFolder(), "service-account.json");
            File.Copy(dialog.FileName, newFilePath, true);
            Settings.Default.GoogleServiceAccountPath = newFilePath;
        }
        RefreshSelectFileDoneVisibility();
    }

    private static string GetPersistentFolder()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string persistentFolder = Path.Combine(appData, "ViciousGPT");
        if (!Directory.Exists(persistentFolder))
        {
            Directory.CreateDirectory(persistentFolder);
        }
        return persistentFolder;
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
