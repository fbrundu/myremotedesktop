using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static class OpenWindowsGetter
{
    delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

    internal static IDictionary<IntPtr, String> GetOpenWindows()
    {
        IntPtr lShellWindow = GetShellWindow();
        Dictionary<IntPtr, String> lWindows = new Dictionary<IntPtr, String>();

        EnumWindows(delegate(IntPtr hWnd, int lParam)
        {
            if (hWnd == lShellWindow) 
                return true;
            if (!IsWindowVisible(hWnd)) 
                return true;

            int lLength = GetWindowTextLength(hWnd);
            if (lLength == 0) 
                return true;

            StringBuilder lBuilder = new StringBuilder(lLength);
            GetWindowText(hWnd, lBuilder, lLength + 1);

            lWindows[hWnd] = lBuilder.ToString();
            
            return true;

        }, 0);

        return lWindows;
    }
    
    [DllImport("user32.dll")]
    static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern IntPtr GetShellWindow();
}
