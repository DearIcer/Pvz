using System.Runtime.InteropServices;
public static class Win32DllImport
{
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

    [DllImport("user32.dll")]
    public static extern int EnumWindows(EnumWindowsProc ewp, int lParam);
    public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(
    IntPtr hProcess,
    IntPtr lpBaseAddress,
    byte[] lpBuffer,
    int dwSize,
    out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int nSize,
        out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    public const int PROCESS_ALL_ACCESS = 0xFFFF;
    public const int PROCESS_VM_READ = 0x0010;
    public const int PROCESS_VM_WRITE = 0x0020;
    public const int PROCESS_VM_OPERATION = 0x0008;

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    public const int SW_SHOWDEFAULT = 10;

    [DllImport("user32.dll")]
    public static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongA")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    public const int GWL_EXSTYLE = -20; // 表示窗口扩展样式
    public const int WS_EX_LAYERED = 0x80000; // 表示窗口为分层窗口
    public const int LWA_ALPHA = 0x2; // 表示设置窗口透明度

    public const int GWL_STYLE = -16;
    public const int WS_BORDER = 0x00800000;
    public const int WS_CAPTION = 0x00C00000;
    public const int WS_SYSMENU = 0x00080000;
    public const int WS_MINIMIZEBOX = 0x00020000;
    public const int WS_MAXIMIZEBOX = 0x00010000;

    public const int SWP_FRAMECHANGED = 0x0020;
    public const int SWP_NOSIZE = 0x0001;
    public const int SWP_NOMOVE = 0x0002;
    public const int SWP_NOZORDER = 0x0004;
    public const int SWP_NOACTIVATE = 0x0010;

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("dwmapi.dll")]
    public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    #region 刷新窗口用
     [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }
    #endregion

    [DllImport("user32.dll")]
    public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

    public const int WM_QUIT = 0x0012;

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public System.Drawing.Point pt;
    }
}

