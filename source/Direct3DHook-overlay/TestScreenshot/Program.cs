using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TestScreenshot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomainAssemblyResolve);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        /*
        private static System.Reflection.Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return System.Reflection.Assembly.LoadFrom(".\\SlimDX-x32\\EasyHook.dll");
        }*/
    }
}
