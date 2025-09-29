using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OnScreenKeyboardManager
{
    /// <summary>
    /// Listens for OS level input focus events, and raise the TextBoxGetFocus and TextBoxLostFocus when an item 
    /// of our interest is involved. 
    /// We are looking for known Window class names, and only raise events on the ones we are interested in
    /// </summary>
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

        /// <summary>
        /// These are the events that we are actually interested in.
        /// </summary>
        /// <returns></returns>
        static private System.Collections.Specialized.StringCollection SetUpTextBoxLikeControls()
        {
            var TextBoxes = new System.Collections.Specialized.StringCollection();
            TextBoxes.Add("Edit"); //Classic Win32 Textbox, such as Notepad
            TextBoxes.Add("Windows.UI.Input.InputSite.WindowClass"); //Windows Terminal/Powershell
            TextBoxes.Add("ConsoleWindowClass"); //Good old Cmd; Has issues
            TextBoxes.Add("WindowsForms8.EDIT"); //WinForms
            TextBoxes.Add("WindowsForms9.EDIT"); //WinForms
            TextBoxes.Add("WindowsForms10.EDIT"); //WinForms
            TextBoxes.Add("WindowsForms11.EDIT"); //WinForms
            TextBoxes.Add("WindowsForms12.EDIT"); //WinForms
            TextBoxes.Add("Scintilla"); //The code editor "Scintilla"
            TextBoxes.Add("Windows.UI.Core.CoreComponentInputSource"); //Windows start menu
            TextBoxes.Add("Windows.UI.Input.InputSite.WindowClass");//Windows start menu
            TextBoxes.Add("Windows.UI.Core.CoreWindow");   //Most WinUI windows, we do not have access to the individual Edit control Class name, because it is handled by WinUI
            return TextBoxes;
        }

        /// <summary>
        /// Check if the Window class name is one that is in our list
        /// </summary>
        /// <param name="ClassName"></param>
        /// <returns></returns>
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
            System.Diagnostics.Trace.WriteLine(string.Format("Receive event type '{0}' from '{1}'", eventType, ClassName.ToString()));
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