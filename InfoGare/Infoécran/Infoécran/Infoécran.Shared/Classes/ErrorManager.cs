using System;  
using Windows.Storage;

namespace Infoécran.Classes
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
        }

    }
}
