using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Server
{
    internal class SelectAreaHook
    {
        //TODO
        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private ServerState _state;
        private static Point _last;
        private static Rectangle[] _area = new Rectangle[2];

        //public static void Main()
        //{
        //    _hookID = SetHook(_proc);
        //    Application.Run();
        //    UnhookWindowsHookEx(_hookID);
        //}

        public SelectAreaHook(ServerState pState)
        {
            this._state = pState;
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        internal static void MySetHook()
        {
            _hookID = SetHook(_proc);
        }

        internal static void UnSetHook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        internal static Rectangle GetArea()
        {
            return _area[1];
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    _last = new Point(hookStruct.pt.x, hookStruct.pt.y);
                }
                else if (MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    _area[0] = _area[1];

                    if (_last.X == hookStruct.pt.x || _last.Y == hookStruct.pt.y)
                    {}    // TODO same point -> window handle
                    else if (_last.X > hookStruct.pt.x && _last.Y > hookStruct.pt.y)
                        _area[1] = new Rectangle(hookStruct.pt.x, hookStruct.pt.y, _last.X - hookStruct.pt.x, _last.Y - hookStruct.pt.y);

                    else if (_last.X > hookStruct.pt.x && _last.Y < hookStruct.pt.y)
                        _area[1] = new Rectangle(hookStruct.pt.x, _last.Y, _last.X - hookStruct.pt.x, hookStruct.pt.y - _last.Y);
                    
                    else if (_last.X < hookStruct.pt.x && _last.Y > hookStruct.pt.y)
                        _area[1] = new Rectangle(_last.X, hookStruct.pt.y, hookStruct.pt.x - _last.X, _last.Y - hookStruct.pt.y);
                    
                    else if (_last.X < hookStruct.pt.x && _last.Y < hookStruct.pt.y)
                        _area[1] = new Rectangle(_last.X, _last.Y, hookStruct.pt.x - _last.X, hookStruct.pt.y - _last.Y);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
