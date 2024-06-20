
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace ViciousGPT;

public partial class SubtitleWindow : Window
{
    public string FollowWindowTitle { get; set; } = "Baldur";

    public SubtitleWindow()
    {
        InitializeComponent();
    }

    public void Show(string text)
    {
        subtitleText.Text = text;
        Screen? targetScreen = WindowMover.GetScreenContaining(FollowWindowTitle);
        if (targetScreen == null)
        {
            Trace.TraceWarning("Could not find window containing '{0}'. Showing on primary screen.", FollowWindowTitle);
            targetScreen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
        }
        AdjustSizes(targetScreen.Bounds);
        base.Show();
        WindowMover.MoveToScreenBottom(this, targetScreen);
        Topmost = true;
    }

    new public void Hide()
    {
        base.Hide();
        Topmost = false;
    }

    private void AdjustSizes(Rectangle screenBounds)
    {
        Height = (double)screenBounds.Height / 4;
        subtitleText.FontSize = Height / 7;
    }
}
