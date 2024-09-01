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
    [BepInPlugin("com.DanW.BrainAnalyzer", "DanW-BrainAnalyzer", "1.0.0")]
    public class SPTBrainAnalyzerPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> Enabled;
        public static ConfigEntry<bool> CreateCSVFile;
        public static ConfigEntry<bool> ShowDebugMessages;

        private void Awake()
        {
            Logger.LogInfo("Loading BrainAnalyzer...");

            LoggingUtil.Logger = Logger;

            new Patches.BotOwnerBrainActivatePatch().Enable();

            Enabled = Config.Bind("Main", "Enabled", true, "Enable all features of this mod");
            CreateCSVFile = Config.Bind("Main", "Create CSV File", true, "Create a CSV file of all EFT brain types and brain layers when the first bot is generated");
            ShowDebugMessages = Config.Bind("Main", "Show debug messages", false, "Show additional debugging information");

            Logger.LogInfo("Loading BrainAnalyzer...done.");

            disableLogicsTest();
        }

        private void disableLogicsTest()
        {
            EFT.WildSpawnType.assault.ToggleLogic("AssaultHaveEnemy", false);
            EFT.WildSpawnType.assault.ToggleLogic("Simple Target", false);
            //EFT.WildSpawnType.assault.ToggleLogic("Help", false);
        }
    }
}
