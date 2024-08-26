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
            return typeof(BotOwner).GetMethod("method_10", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(BotOwner __instance)
        {
            if (ranAnalysis || !SPTBrainAnalyzerPlugin.Enabled.Value)
            {
                return;
            }

            BrainAnalyzerUtil.AnalyzeBrains(__instance);
            ranAnalysis = true;
        }
    }
}
