using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OnScreenKeyboardManager
{
    class OnScreenKeyboardHandler
    {
        static System.Diagnostics.Process OSK;
        static string OKSFilename = "FreeVK.exe";
        static string OSKWinClass = null; //"TMainForm";
        static string OSKWinTitle = "Free Virtual Keyboard";

        public static void Start()
        {
            TextBoxFocusTracker.TextBoxGetFocus += (sender, e) =>
            {
                if (isClassNameOSK(e.ClassName)) return;
                if (isHwndOsk(e.hwnd)) return;
                TryStartOSK();
                PositionOSK(e.hwnd);
            };

            TextBoxFocusTracker.TextBoxLostFocus += (sender, e) =>
            {
                if (isClassNameOSK(e.ClassName)) return;
                if (isHwndOsk(e.hwnd)) return;
                TryTerminate();
            };

            TextBoxFocusTracker.Start();
        }

        public static void Stop()
        {
            TextBoxFocusTracker.Stop();
        }

        private static void TryStartOSK()
        {
            try
            {
                if (OSK != null && !OSK.HasExited) return; //it is still visible
                OSK = System.Diagnostics.Process.Start(OKSFilename);
                OSK.WaitForInputIdle();
                System.Threading.Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        private static void TryTerminate()
        {
            try
            {
                if (OSK == null || OSK.HasExited) return; //it is already terminated visible
                OSK.Kill();
                OSK.WaitForExit();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        private static void PositionOSK(IntPtr Reference)
        {
            //we do not do this, because Touch monitors usually only have one monitor...
        }

        private static bool isClassNameOSK(string ClassName)
        {
            if (string.IsNullOrWhiteSpace(OSKWinClass)) return false;
            return ClassName.StartsWith(ClassName);
        }

        private static bool isHwndOsk(IntPtr hwnd)
        {
            if (string.IsNullOrWhiteSpace(OSKWinTitle)) return false;
            string WinTitle = GetText(hwnd);
            return WinTitle.StartsWith(OSKWinTitle);
        }

        #region NativeMethods
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        public static string GetText(IntPtr hWnd)
        {
            // Allocate correct string length first
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        const int MONITOR_DEFAULTTONULL = 0;
        const int MONITOR_DEFAULTTOPRIMARY = 1;
        const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
        #endregion
    }
}