using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D365CodeReviewer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Set up unhandled exception handling
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}\n\nDetails: {e.Exception.StackTrace}",
                            "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            // Log the error
            try
            {
                string logFile = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "D365CodeReviewer",
                    "error_log.txt");
                
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logFile));
                
                System.IO.File.AppendAllText(logFile, 
                    $"[{DateTime.Now}] Thread Exception:\n{e.Exception}\n\n");
            }
            catch
            {
                // Could not log the error
            }
        }
        
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            
            MessageBox.Show($"A fatal error occurred: {(ex != null ? ex.Message : "Unknown error")}\n\nThe application needs to close.",
                            "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            // Log the error
            try
            {
                string logFile = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "D365CodeReviewer",
                    "error_log.txt");
                
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logFile));
                
                System.IO.File.AppendAllText(logFile, 
                    $"[{DateTime.Now}] Unhandled Exception:\n{(ex != null ? ex.ToString() : "Unknown error")}\n\n");
            }
            catch
            {
                // Could not log the error
            }
        }
    }
}