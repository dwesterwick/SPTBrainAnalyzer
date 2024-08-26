using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT;

namespace SPTBrainAnalyzer.Patches
{
    public class BotOwnerBrainActivatePatch : ModulePatch
    {
        private static bool ranAnalysis = false;

        protected override MethodBase GetTargetMethod()
        {
            return typeof(StandartBotBrain).GetMethod("Activate", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(StandartBotBrain __instance, BotOwner ___botOwner_0)
        {
            if (ranAnalysis || !SPTBrainAnalyzerPlugin.Enabled.Value)
            {
                return;
            }

            ranAnalysis = true;
            BrainAnalyzerUtil.AnalyzeBrainsOfAllWildSpawnTypes(___botOwner_0);
        }
    }
}
