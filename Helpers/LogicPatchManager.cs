using EFT;
using SPTBrainAnalyzer.Models;
using SPTBrainAnalyzer.Patches;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPTBrainAnalyzer.Helpers
{
    public static class LogicPatchManager
    {
        private static Dictionary<string, Type> logicNames = new Dictionary<string, Type>();
        private static Dictionary<Type, object> logicPatches = new Dictionary<Type, object>();
        private static Dictionary<LogicSettings, bool> isLogicDisabled = new Dictionary<LogicSettings, bool>();
        private static Dictionary<LogicSettings, bool> pendingLogicToToggle = new Dictionary<LogicSettings, bool>();

        public static bool IsLogicDisabled(this WildSpawnType role, Type logicType, EPlayerSide side)
        {
            LogicSettings settings = findLogicSettings(logicType, role, side);
            if (settings != null)
            {
                return isLogicDisabled[settings];
            }

            return false;
        }

        public static void UpdateActiveBrainLayers(this BotOwner bot)
        {
            bot.Brain.BaseBrain.readAllBrainLayers();
            updateAllPendingLogicTypes();

            foreach (LogicSettings settings in pendingLogicToToggle.Keys.ToArray())
            {
                if (!settings.HasLogicType)
                {
                    continue;
                }

                if (!settings.Matches(bot))
                {
                    continue;
                }

                if (isLogicDisabled.ContainsKey(settings))
                {
                    isLogicDisabled[settings] = pendingLogicToToggle[settings];
                }
                else
                {
                    isLogicDisabled.Add(settings, pendingLogicToToggle[settings]);
                }

                pendingLogicToToggle.Remove(settings);

                createShallUseNowPatch(settings.LogicType);
            }
        }

        public static void ToggleLogic(this WildSpawnType role, string logicName, bool value, EPlayerSideMask sideMask = EPlayerSideMask.All)
        {
            LogicSettings settings = findLogicSettings(logicName, role, sideMask);
            if (settings != null)
            {
                isLogicDisabled[settings] = !value;
                return;
            }

            settings = findPendingLogicSettings(logicName, role, sideMask);
            if (settings != null)
            {
                pendingLogicToToggle[settings] = !value;
                return;
            }

            pendingLogicToToggle.Add(new LogicSettings(logicName, role, sideMask), !value);
        }

        private static object createShallUseNowPatch(Type logicType)
        {
            if (logicPatches.ContainsKey(logicType))
            {
                return logicPatches[logicType];
            }

            object patch = Activator.CreateInstance(typeof(ShallUseNowPatch<>).MakeGenericType(logicType), new object[0]);
            if (patch == null)
            {
                throw new InvalidOperationException("Could not create ShallUseNowPatch for " + logicType.Name);
            }

            MethodInfo enableMethodInfo = typeof(ShallUseNowPatch<>).GetMethod("Enable");
            enableMethodInfo.Invoke(patch, null);

            LoggingUtil.LogInfo("Created ShallUseNowPatch for " + logicType.Name);

            return patch;
        }

        private static LogicSettings findLogicSettings(Type logicType, WildSpawnType role, EPlayerSide side)
        {
            foreach (LogicSettings settings in isLogicDisabled.Keys)
            {
                if (settings.Matches(role, side, logicType))
                {
                    return settings;
                }
            }

            return null;
        }

        private static LogicSettings findLogicSettings(string logicName, WildSpawnType role, EPlayerSideMask sideMask = EPlayerSideMask.All)
        {
            foreach (LogicSettings settings in isLogicDisabled.Keys)
            {
                if (settings.Matches(role, sideMask, logicName))
                {
                    return settings;
                }
            }

            return null;
        }

        private static LogicSettings findPendingLogicSettings(string logicName, WildSpawnType role, EPlayerSideMask sideMask = EPlayerSideMask.All)
        {
            foreach (LogicSettings settings in pendingLogicToToggle.Keys)
            {
                if ((settings.LogicName == logicName) && (settings.Role == role) && (settings.SideMask == sideMask))
                {
                    return settings;
                }
            }

            return null;
        }

        private static void readAllBrainLayers(this BaseBrain brain)
        {
            Dictionary<int, AICoreLayerClass<BotLogicDecision>> botBrainLayerDictionary = brain.GetBrainLayerDictionary();
            foreach (int index in botBrainLayerDictionary.Keys)
            {
                string logicName = botBrainLayerDictionary[index].Name();

                if (logicNames.ContainsKey(logicName))
                {
                    continue;
                }

                Type logicType = botBrainLayerDictionary[index].GetType();
                logicNames.Add(logicName, logicType);
            }
        }

        private static void updateAllPendingLogicTypes()
        {
            foreach (LogicSettings settings in pendingLogicToToggle.Keys)
            {
                if (settings.HasLogicType)
                {
                    continue;
                }

                foreach (string name in logicNames.Keys)
                {
                    if (settings.LogicName == name)
                    {
                        settings.RegisterLogicType(logicNames[name]);
                    }
                }
            }
        }
    }
}
