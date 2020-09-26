using System;
using System.IO;
using System.Windows;

namespace AdoptionAlacrityDashboard
{
    public class Logger
    {
        private Logger()
        {
            try
            {
                writer = new StreamWriter(logfilename, true);
                writer.AutoFlush = true;
            }
            catch (Exception exp)
            {
                MessageBox.Show($"Error opening log file {logfilename}: {exp.Message}, closing application");
                Environment.Exit(1);
            }
        }

        private string logfilename = "AdoptionAlacrity.log";
        private StreamWriter writer;
        public static Logger Log = new Logger();

        public void WriteLog(string text, params object[] arg)
        {
            writer.Write(DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff    "));
            writer.WriteLine(text, arg);
            writer.Flush();
        }
    }
}