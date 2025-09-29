using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace OnScreenKeyboardManager
{
    /// <summary>
    /// Handles showing and hiding of the On screen keyboard whenever an "text box like" control gets or loses input focus
    /// </summary>
    class OnScreenKeyboardHandler
    {
        static string OKSFilename = "OSK";

        static string OSKWinClass = "OSKMainClass"; 
        static string OSKWinTitle = "";

        /// <summary>4f
        /// Set up handlers and start listening for focus event
        /// </summary>
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
                System.Diagnostics.Process.Start(OKSFilename);
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
                var OSK = Process.GetProcessesByName(OKSFilename).FirstOrDefault();
                if (OSK == null) return; //it is already terminated visible
                OSK.Kill();
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

        /// <summary>
        /// Checks if the given Window class name belongs to the On-Screen keyboard, 
        /// meaning the OSK got the focus
        /// </summary>
        /// <returns></returns>
        private static bool isClassNameOSK(string ClassName)
        {
            if (string.IsNullOrWhiteSpace(OSKWinClass)) return false;
            return ClassName.StartsWith(OSKWinClass);
        }

        /// <summary>
        /// Checks if the given Window Handle belongs to the On-Screen keyboard, 
        /// meaning the OSK got the focus
        /// </summary>
        /// <returns></returns>
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