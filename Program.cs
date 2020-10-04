using System;
using System.Windows.Forms;

namespace AdoptionAlacrityDashboard
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
            Application.Run(new DashboardForm());
        }

        static internal void Exit()
        {
            Application.Exit();
        }
    }
}
