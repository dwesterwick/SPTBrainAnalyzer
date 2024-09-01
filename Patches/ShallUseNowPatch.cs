using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT;
using SPT.Reflection.Patching;
using SPTBrainAnalyzer.Helpers;

namespace SPTBrainAnalyzer.Patches
{
    public class ShallUseNowPatch<T> : ModulePatch where T: AICoreLayerClass<BotLogicDecision>
    {
        private static Dictionary<BotOwner, Stopwatch> updateTimers = new Dictionary<BotOwner, Stopwatch>();
        private static Dictionary<BotOwner, bool> isLogicDisabled = new Dictionary<BotOwner, bool>();
        private static int updateDelay = 200;

        protected override MethodBase GetTargetMethod()
        {
            LoggingUtil.LogInfo("Created ShallUseNow patch for " + typeof(T).Name);

            return typeof(T).GetMethod("ShallUseNow", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref bool __result, T __instance, BotOwner ___botOwner_0)
        {
            if (!SPTBrainAnalyzerPlugin.Enabled.Value)
            {
                return true;
            }

            addKeysIfNeeded(___botOwner_0);

            if (updateTimers[___botOwner_0].ElapsedMilliseconds < updateDelay)
            {
                return !isLogicDisabled[___botOwner_0];
            }

            updateTimers[___botOwner_0].Restart();

            isLogicDisabled[___botOwner_0] = ___botOwner_0.Profile.Info.Settings.Role.IsLogicDisabled(typeof(T), ___botOwner_0.Profile.Side);
            if (isLogicDisabled[___botOwner_0])
            {
                LoggingUtil.LogInfo("Suppressing ShallUseNow for logic " + __instance.Name() + " for " + ___botOwner_0.name);
            }

            __result = !isLogicDisabled[___botOwner_0];
            return !isLogicDisabled[___botOwner_0];
        }

        private static void addKeysIfNeeded(BotOwner ___botOwner_0)
        {
            if (!updateTimers.ContainsKey(___botOwner_0))
            {
                updateTimers.Add(___botOwner_0, Stopwatch.StartNew());
            }

            if (!isLogicDisabled.ContainsKey(___botOwner_0))
            {
                isLogicDisabled.Add(___botOwner_0, false);
            }
        }
    }
}
