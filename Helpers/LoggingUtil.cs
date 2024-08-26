using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPTBrainAnalyzer.Helpers
{
    public static class LoggingUtil
    {
        public static BepInEx.Logging.ManualLogSource Logger { get; set; } = null;
        public static string ModPathRelative { get; } = "/BepInEx/plugins/DanW-SPTBrainAnalyzer";

        public static void LogInfo(string message)
        {
            if (!SPTBrainAnalyzerPlugin.ShowDebugMessages.Value)
            {
                return;
            }

            Logger.LogInfo(message);
        }

        public static void LogWarning(string message)
        {
            Logger.LogWarning(message);
        }

        public static void LogError(string message)
        {
            Logger.LogError(message);
        }

        public static string GetLoggingPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory + ModPathRelative + "/log/";
        }

        public static void CreateLogFile(string filename, string content)
        {
            try
            {
                if (!Directory.Exists(GetLoggingPath()))
                {
                    Directory.CreateDirectory(GetLoggingPath());
                }

                File.WriteAllText(GetLoggingPath() + filename, content);

                LogInfo("Writing " + filename + "...done.");
            }
            catch (Exception e)
            {
                e.Data.Add("Filename", filename);
                LogError("Writing " + filename + "...failed!");
                LogError(e.ToString());
            }
        }
    }
}