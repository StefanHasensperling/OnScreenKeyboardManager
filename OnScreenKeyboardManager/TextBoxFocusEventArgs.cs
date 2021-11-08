using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace OnScreenKeyboardManager
{
    class TextBoxFocusEventArgs : EventArgs
    {
        public string ClassName;
        public IntPtr hwnd;
    }
}