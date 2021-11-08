using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OnScreenKeyboardManager
{
    class Program
    {

        static void Main(string[] args)
        {
            //Ensue Single instance
            System.Threading.Mutex mut =  new System.Threading.Mutex(false, Application.ProductName);
            bool running = !mut.WaitOne(0, false);
            if (running)
            {
                Application.ExitThread();
                return;
            }

            //Startup manager
            OnScreenKeyboardHandler.Start();
            try
            {
                //Show main Window
                var Form = new Main();
                Form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //Unregister hooks
            OnScreenKeyboardHandler.Stop();
        }
    }
}
