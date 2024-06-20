using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace ViciousGPT;
public partial class WindowMover
{
    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public static void MoveToScreenBottom(Window moveWindow, Screen targetScreen)
    {
        Rectangle screen = targetScreen.Bounds;
        nint windowToMove = new WindowInteropHelper(moveWindow).Handle;
        int windowHeight = (int)moveWindow.Height;
        MoveWindow(windowToMove, screen.Left, screen.Top + screen.Height - windowHeight, screen.Width, windowHeight, true);
    }

    public static Screen? GetScreenContaining(string windowTitle)
    {
        nint? windowHandle = FindWindowContaining(windowTitle);
        if (windowHandle.HasValue)
        {
            return Screen.FromHandle(windowHandle.Value);
        }
        return null;
    }

    private static nint? FindWindowContaining(string searchText)
    {
        nint? targetHandle = null;
        EnumWindows((hWnd, lParam) =>
        {
            if (IsWindowVisible(hWnd))
            {
                StringBuilder sb = new(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                if (sb.ToString().Contains(searchText))
                {
                    targetHandle = hWnd;
                    return false; // Stop enumeration
                }
            }
            return true; // Continue enumeration
        }, IntPtr.Zero);
        return targetHandle;
    }
}
