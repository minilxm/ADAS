using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace AgingSystem
{
    static class Program
    {
        static AgingSystem.App app = new AgingSystem.App();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            //AgingSystem.App app = new AgingSystem.App();
            app.InitializeComponent();
            app.Run();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (app.MainWindow != null)
            {
                IntPtr hwnd = new WindowInteropHelper(app.MainWindow).Handle;
                SendMessage(hwnd, 0x1001, 0, 0);
            }
            MessageBox.Show("错误，CurrentDomain未捕捉到的异常!");
            LogUnhandledException(e.ExceptionObject);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show("错误，Application未捕捉到的异常!");
            LogUnhandledException(e.Exception);
        }

        static void LogUnhandledException(object exceptionobj)
        {
            //Log the exception here or report it to developer  
            Exception e = ((Exception)exceptionobj);
            MessageBox.Show(e.Message + "\nStackTrace=>" + e.StackTrace);

        }

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
    }
}
