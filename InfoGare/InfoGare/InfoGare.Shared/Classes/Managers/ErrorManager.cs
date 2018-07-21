using System;
using System.Net.Http;
using Windows.Storage;

namespace Infogare.Classes
{
    public static class ErrorManager
    {
        public static void Log(Exception e)
        {
            var fullLog = @"EXCEPTION RAISED : FROM APP " + @" \n";
            fullLog += e.Message + "    From : " + e.Source + @"\n";
            fullLog += e.StackTrace;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("StacktraceError"))
            {
                ApplicationData.Current.LocalSettings.Values["StacktraceError"] += @"\n" + fullLog;
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values.Add("StacktraceError", fullLog);
            }
            TrySendReport();
        }

        private static async void TrySendReport()
        {
            try
            {
                var htclient = new HttpClient();
                await htclient.PostAsync("http://paulfasola.fr/WinApps/Infogare/ErrorCatcher.php", new StringContent(ApplicationData.Current.LocalSettings.Values["StacktraceError"].ToString()));
                ApplicationData.Current.LocalSettings.Values["StacktraceError"] = null;
            }
            catch (Exception)
            {
            }
        }
    }
}
