using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OnScreenKeyboardManager
{
    public partial class Debug : Form
    {
        TraceListener tl;

        public Debug()
        {
            InitializeComponent();
        }

        private void Debug_Shown(object sender, EventArgs e)
        {
            tl = new TraceListener(textBox1);
            System.Diagnostics.Trace.Listeners.Add(tl);
        }

        private void Debug_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Trace.Listeners.Remove(tl);
        }

        private class TraceListener : System.Diagnostics.TraceListener
        {
            private TextBox _tb;

            public TraceListener(TextBox tb)
            {
                _tb = tb;
            }

            public override void Write(string message)
            {
                _tb.Text += message;
            }

            public override void WriteLine(string message)
            {
                _tb.Text += message + Environment.NewLine;

            }
        }
    }
}
