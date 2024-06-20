namespace ViciousGPT;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

public partial class KeyboardHotkeyManager : IDisposable
{
    public static class Modifier
    {
        // Combine the modifiers with a bitwise OR: Modifier.Control | Modifier.Alt
        public const uint None = 0x0000;
        public const uint Alt = 0x0001;
        public const uint Control = 0x0002;
        public const uint Shift = 0x0004;
        public const uint Win = 0x0008;
    }

    private const int WM_HOTKEY = 0x0312;

    private readonly IntPtr windowHandle;
    private readonly HwndSource source;
    private readonly int hotkeyId;
    private readonly Action callback;
    private bool disposed = false;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    public KeyboardHotkeyManager(Window window, uint modifiers, Key key, Action callback)
    {
        this.callback = callback;
        hotkeyId = new Random().Next(0x0000, 0xBFFF);

        windowHandle = new WindowInteropHelper(window).Handle;
        source = HwndSource.FromHwnd(windowHandle);
        source.AddHook(HwndHook);

        uint keyInt = (uint)KeyInterop.VirtualKeyFromKey(key);
        RegisterHotKey(windowHandle, hotkeyId, modifiers, keyInt);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                UnregisterHotKey(windowHandle, hotkeyId);
                source.RemoveHook(HwndHook);
            }
            disposed = true;
        }
    }

    private IntPtr HwndHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == hotkeyId)
        {
            callback?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }
}
