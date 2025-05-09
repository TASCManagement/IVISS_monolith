using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace IVISS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            string appGuid =
       ((GuidAttribute)Assembly.GetExecutingAssembly().
           GetCustomAttributes(typeof(GuidAttribute), false).
               GetValue(0)).Value.ToString();

            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("IVISS executable already running");
                    return;
                }

                Application.Run(new MainV1());
            }


           
        }
    }
}
