using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using SPTBrainAnalyzer.Helpers;

namespace SPTBrainAnalyzer
{
    [BepInDependency("xyz.drakia.bigbrain", "1.3.2")]
    [BepInPlugin("com.DanW.BrainAnalyzer", "DanW-BrainAnalyzer", "1.2.0")]
    public class SPTBrainAnalyzerPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> Enabled;
        public static ConfigEntry<bool> AnalyzeCurrentBrainFirst;
        public static ConfigEntry<bool> ShowDebugMessages;

        public static ConfigEntry<bool> DisableTestLogics;

        protected void Awake()
        {
            Logger.LogInfo("Loading BrainAnalyzer...");

            LoggingUtil.Logger = Logger;

            new Patches.BotOwnerBrainActivatePatch().Enable();
            new Patches.SPTPMCBrainChangerDisablePatch().Enable();
            new Patches.SPTPScavBrainChangerDisablePatch().Enable();
            new Patches.SPTScavBrainChangerDisablePatch().Enable();

            Enabled = Config.Bind("Main", "Enabled", true, "Create a CSV file of all EFT brain types and brain layers when the first bot is generated");
            AnalyzeCurrentBrainFirst = Config.Bind("Main", "Analyze Current Brain First", false, "Analyze the bot's existing brain before all other brain combinations");
            ShowDebugMessages = Config.Bind("Main", "Show debug messages", false, "Show additional debugging information");

            BrainAnalyzerUtil.Init();

            Logger.LogInfo("Loading BrainAnalyzer...done.");
        }
    }
}
