using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT;
using SPTBrainAnalyzer.Helpers;

namespace SPTBrainAnalyzer.Patches
{
    public class BotOwnerBrainActivatePatch : ModulePatch
    {
        private static bool ranAnalysis = false;

        protected override MethodBase GetTargetMethod()
        {
            return typeof(StandartBotBrain).GetMethod(nameof(StandartBotBrain.Activate), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(StandartBotBrain __instance, BotOwner ___botOwner_0)
        {
            if (ranAnalysis || !SPTBrainAnalyzerPlugin.Enabled.Value)
            {
                return;
            }

            ranAnalysis = true;

            try
            {
                BrainAnalyzerUtil.AnalyzeBrainsOfAllWildSpawnTypes(___botOwner_0);
            }
            catch (Exception e)
            {
                LoggingUtil.LogError("Cannot run brain-layer analysis on " + ___botOwner_0.name + ": " + e.Message);
                LoggingUtil.LogError(e.StackTrace);
            }
        }
    }
}
