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
            return typeof(StandartBotBrain).GetMethod("Activate", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(StandartBotBrain __instance, BotOwner ___botOwner_0)
        {
            if (!SPTBrainAnalyzerPlugin.Enabled.Value)
            {
                return;
            }

            try
            {
                LogicPatchManager.UpdateActiveBrainLayers(___botOwner_0);
            }
            catch (Exception e)
            {
                LoggingUtil.LogError("Cannot update active brain layers for " + ___botOwner_0.name + ": " + e.Message);
                LoggingUtil.LogError(e.StackTrace);
            }

            runAnalysis(___botOwner_0);
        }

        private static void runAnalysis(BotOwner ___botOwner_0)
        {
            if (ranAnalysis)
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
