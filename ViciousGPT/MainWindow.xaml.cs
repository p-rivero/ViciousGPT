using System.Windows;
using System.Windows.Input;
using ViciousGPT.Properties;
using static ViciousGPT.KeyboardHotkeyManager;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;


namespace ViciousGPT;

public partial class MainWindow : Window
{
    private const uint CONTROLLER_NUMBER = 0; // Between 0 and 3

    private KeyboardHotkeyManager? triggerHotkey;
    private KeyboardHotkeyManager? cancelHotkey;
    private readonly ControllerHotkeyManager controllerHotkeys = new(CONTROLLER_NUMBER);
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
}
