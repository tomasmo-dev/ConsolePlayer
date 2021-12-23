using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace NotePlayer
{
    class Program
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        const int WM_SETTEXT = 0x000C;

        public static void exec()
        {
            Process[] nPads = Process.GetProcessesByName("Notepad");
            if (nPads.Length == 0) return;

            if(nPads[0] != null)
            {
                IntPtr child = FindWindowEx(nPads[0].MainWindowHandle, new IntPtr(0), "Edit", null);
                SendMessage(child, WM_SETTEXT, 0, "X");
            }

        }

        public static void DrawFrame(string fr)
        {
            IntPtr hwnd = ConsolePlayer.Program.ChildHandle;

            SendMessage(hwnd, WM_SETTEXT, 0, fr);
        }

        public static IntPtr GetChildHandle()
        {

            Process[] nPads = Process.GetProcessesByName("Notepad");
            if (nPads.Length == 0) Environment.Exit(123);

            if (nPads[0] != null)
            {
                IntPtr child = FindWindowEx(nPads[0].MainWindowHandle, new IntPtr(0), "Edit", null);
            
                return child;
            }
            else
            {
                return IntPtr.Zero;
            }

        }

        private static string getMessage()
        {
            string msg = "";

            for (int i = 0; i < 44; i++)
            {
                for (int x = 0; x < 168; x++)
                {
                    msg += "X";
                }

                msg += "\r\n";
            }

            return msg;
        }
    }
}