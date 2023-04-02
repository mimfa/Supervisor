using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MiMFa_Supervisor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Supervisor());
            }
            catch(Exception ex) { if (MessageBox.Show(ex.Message, "Apllication is Closing [0]", MessageBoxButtons.RetryCancel, MessageBoxIcon.Stop) == DialogResult.Retry) Main(); }
        }
    }
}
