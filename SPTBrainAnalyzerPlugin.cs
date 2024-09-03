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

        public static ConfigEntry<bool> DisableTestLogics;

        private void Awake()
        {
            Logger.LogInfo("Loading BrainAnalyzer...");

            LoggingUtil.Logger = Logger;

            new Patches.BotOwnerBrainActivatePatch().Enable();

            Enabled = Config.Bind("Main", "Enabled", true, "Enable all features of this mod");
            CreateCSVFile = Config.Bind("Main", "Create CSV File", false, "Create a CSV file of all EFT brain types and brain layers when the first bot is generated");
            ShowDebugMessages = Config.Bind("Main", "Show debug messages", false, "Show additional debugging information");

            DisableTestLogics = Config.Bind("Test", "Disable Test Logics", true, "[For testing] Disable comabat layers for normal Scavs");

            createConfigEvents();

            Logger.LogInfo("Loading BrainAnalyzer...done.");

            toggleTestLogics(!DisableTestLogics.Value);
        }

        private void createConfigEvents()
        {
            Enabled.SettingChanged += (object sender, EventArgs e) =>
            {
                LogicPatchManager.ApplyChanges();
            };

            DisableTestLogics.SettingChanged += (object sender, EventArgs e) =>
            {
                toggleTestLogics(!DisableTestLogics.Value);
            };
        }

        private void toggleTestLogics(bool value)
        {
            EFT.WildSpawnType.assault.ToggleLogic("AssaultHaveEnemy", value);
            EFT.WildSpawnType.assault.ToggleLogic("Simple Target", value);
            EFT.WildSpawnType.assault.ToggleLogic("Help", value);
            EFT.WildSpawnType.assault.ToggleLogic("Pursuit", value);

            EFT.WildSpawnType.assaultGroup.ToggleLogic("AssaultHaveEnemy", value);
            EFT.WildSpawnType.assaultGroup.ToggleLogic("AdvAssaultTarget", value);
            EFT.WildSpawnType.assaultGroup.ToggleLogic("Pmc", value);

            LogicPatchManager.ApplyChanges();
        }
    }
}
