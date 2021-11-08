using System;
using System.Windows;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

namespace OnScreenKeyboardManager
{
    class TextBoxFocusTracker
    {
        #region NativeMethods
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        const uint EVENT_OBJECT_NAMECHANGE = 0x800C;
        const uint EVENT_OBJECT_FOCUS = 0x8005;
        const uint WINEVENT_OUTOFCONTEXT = 0;

        // Need to ensure delegate is not collected while we're using it,
        // storing it in a class field is simplest way to do this.
        static WinEventDelegate procDelegate = new WinEventDelegate(WinEventProc);
        #endregion

        static IntPtr hhook;

        public static event EventHandler<TextBoxFocusEventArgs> TextBoxGetFocus;
        public static event EventHandler<TextBoxFocusEventArgs> TextBoxLostFocus;

        static public void Start()
        {
            // Listen for name change changes across all processes/threads on current desktop...
            hhook = SetWinEventHook(EVENT_OBJECT_FOCUS, EVENT_OBJECT_FOCUS, IntPtr.Zero, procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        static public void Stop()
        {
            UnhookWinEvent(hhook);
        }

        static System.Collections.Specialized.StringCollection TextBoxes = SetUpTextBoxLikeControls();

        static private System.Collections.Specialized.StringCollection SetUpTextBoxLikeControls()
        {
            var TextBoxes = new System.Collections.Specialized.StringCollection();
            TextBoxes.Add("Edit"); //Classic Win32 Textbox
            TextBoxes.Add("Windows.UI.Input.InputSite.WindowClass"); //Windows Terminal/Powershell
            //TextBoxes.Add("ConsoleWindowClass"); //Good old Cmd; Has issues
            TextBoxes.Add("WindowsForms8.EDIT"); //Winforms
            TextBoxes.Add("WindowsForms9.EDIT"); //Winforms
            TextBoxes.Add("WindowsForms10.EDIT"); //Winforms
            TextBoxes.Add("WindowsForms11.EDIT"); //Winforms
            TextBoxes.Add("WindowsForms12.EDIT"); //Winforms
            TextBoxes.Add("Scintilla"); //Winforms

            return TextBoxes;
        }

        private static bool isTextBox(string ClassName)
        {
            foreach (var tb in TextBoxes)
            {
                if (ClassName.ToString().StartsWith(tb)) return true;
            }
            return false;
        }

        static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            //Retrieve Class Name
            int nRet;
            StringBuilder ClassName = new StringBuilder(256);
            nRet = GetClassName(hwnd, ClassName, ClassName.Capacity);
            if (nRet == 0) //No chars returned
            {
                TextBoxLostFocus.Invoke(null, new TextBoxFocusEventArgs() { ClassName = "", hwnd = hwnd });
                return;
            }

            //Check if we are interested
            System.Diagnostics.Trace.WriteLine("Receive event from: " + ClassName.ToString());
            if (isTextBox(ClassName.ToString()))
            {
                TextBoxGetFocus.Invoke(null, new TextBoxFocusEventArgs() { ClassName = ClassName.ToString(), hwnd = hwnd });
            }
            else
            {
                TextBoxLostFocus.Invoke(null, new TextBoxFocusEventArgs() { ClassName = ClassName.ToString(), hwnd = hwnd });
            }
        }
    }
}