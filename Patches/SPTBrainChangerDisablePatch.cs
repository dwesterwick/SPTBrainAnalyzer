using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;
using SPTBrainAnalyzer.Helpers;

namespace SPTBrainAnalyzer.Patches
{
    public class SPTPMCBrainChangerDisablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(AIBrainSpawnWeightAdjustment).GetMethod(nameof(AIBrainSpawnWeightAdjustment.GetPmcWildSpawnType), BindingFlags.Public | BindingFlags.Instance);
        
        [PatchPrefix]
        protected static bool PatchPrefix(ref WildSpawnType __result, BotOwner botOwner_0)
        {
            __result = botOwner_0.Profile.Info.Settings.Role;
            return !SPTBrainChangerDisablePatchHelpers.ShouldDisablePatch(__result);
        }
    }

    public class SPTPScavBrainChangerDisablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(AIBrainSpawnWeightAdjustment).GetMethod(nameof(AIBrainSpawnWeightAdjustment.GetRandomisedPlayerScavType), BindingFlags.Public | BindingFlags.Instance);

        [PatchPrefix]
        protected static bool PatchPrefix(ref WildSpawnType __result, BotOwner botOwner)
        {
            __result = botOwner.Profile.Info.Settings.Role;
            return !SPTBrainChangerDisablePatchHelpers.ShouldDisablePatch(__result);
        }
    }

    public class SPTScavBrainChangerDisablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(AIBrainSpawnWeightAdjustment).GetMethod(nameof(AIBrainSpawnWeightAdjustment.GetAssaultScavWildSpawnType), BindingFlags.Public | BindingFlags.Instance);

        [PatchPrefix]
        protected static bool PatchPrefix(ref WildSpawnType __result, BotOwner botOwner)
        {
            __result = botOwner.Profile.Info.Settings.Role;
            return !SPTBrainChangerDisablePatchHelpers.ShouldDisablePatch(__result);
        }
    }

    public static class SPTBrainChangerDisablePatchHelpers
    {
        public static bool ShouldDisablePatch(WildSpawnType role)
        {
            if (BrainAnalyzerUtil.IsRunningAnalysis)
            {
                LoggingUtil.LogWarning("Preventing SPT from changing the brain type for " + role + " bots");
                return true;
            }

            return false;
        }
    }
}
