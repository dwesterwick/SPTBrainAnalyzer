using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT;
using SPTBrainAnalyzer.Models;
using SPTBrainAnalyzer.Patches;

namespace SPTBrainAnalyzer.Helpers
{
    public enum LogicDisablingMethod
    {
        ShallUseNowPatch,
        RemoveFromDictionary,
        BigBrain
    }

    public static class LogicPatchManager
    {
        public static LogicDisablingMethod Method { get; set; } = LogicDisablingMethod.BigBrain;

        private static Dictionary<string, Type> logicNames = new Dictionary<string, Type>();
        private static Dictionary<Type, object> logicPatches = new Dictionary<Type, object>();
        private static Dictionary<LogicSettings, bool> isLogicDisabled = new Dictionary<LogicSettings, bool>();
        private static Dictionary<LogicSettings, bool> pendingLogicToToggle = new Dictionary<LogicSettings, bool>();
        private static Dictionary<BotOwner, List<DisabledLogicSettings>> disabledLogics = new Dictionary<BotOwner, List<DisabledLogicSettings>>();
        private static Dictionary<LogicSettings, bool> logicSettingsToApply = new Dictionary<LogicSettings, bool>();

        public static bool IsLogicDisabled(this WildSpawnType role, Type logicType, EPlayerSide side)
        {
            LogicSettings settings = findLogicSettings(logicType, role, side);
            if (settings != null)
            {
                return isLogicDisabled[settings];
            }

            return false;
        }

        public static void ApplyChanges()
        {
            foreach (BotOwner bot in disabledLogics.Keys.ToArray())
            {
                bot.UpdateActiveBrainLayers();
            }
        }

        public static void UpdateActiveBrainLayers(this BotOwner bot)
        {
            switch (Method)
            {
                case LogicDisablingMethod.ShallUseNowPatch: bot.updateActiveBrainLayers_ShallUseNowPatch(); break;
                case LogicDisablingMethod.RemoveFromDictionary: bot.updateActiveBrainLayers_removeFromDictionary(); break;
            }
        }

        public static void ToggleLogic(this WildSpawnType role, string logicName, bool value, EPlayerSideMask sideMask = EPlayerSideMask.All)
        {
            switch (Method)
            {
                case LogicDisablingMethod.ShallUseNowPatch: role.toggleLogic_shallUseNowPatch(logicName, value, sideMask); break;
                case LogicDisablingMethod.RemoveFromDictionary: role.toggleLogic_removeFromDictionary(logicName, value, sideMask); break;
            }
        }

        private static void toggleLogic_removeFromDictionary(this WildSpawnType role, string logicName, bool value, EPlayerSideMask sideMask = EPlayerSideMask.All)
        {
            LogicSettings settings = findLogicSettingsToApply(logicName, role, sideMask);
            if (settings != null)
            {
                logicSettingsToApply[settings] = value;
            }
            else
            {
                logicSettingsToApply.Add(new LogicSettings(logicName, role, sideMask), value);
            }
        }

        private static void toggleLogic_shallUseNowPatch(this WildSpawnType role, string logicName, bool value, EPlayerSideMask sideMask = EPlayerSideMask.All)
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

        private static void updateActiveBrainLayers_removeFromDictionary(this BotOwner bot)
        {
            if ((bot == null) || bot.IsDead)
            {
                if (disabledLogics.ContainsKey(bot))
                {
                    disabledLogics.Remove(bot);
                }

                return;
            }

            if (!disabledLogics.ContainsKey(bot))
            {
                disabledLogics.Add(bot, new List<DisabledLogicSettings>());
            }

            if (!SPTBrainAnalyzerPlugin.Enabled.Value)
            {
                return;
            }

            foreach (LogicSettings settings in logicSettingsToApply.Keys)
            {
                if (logicSettingsToApply[settings])
                {
                    resumeBrainLayer(bot, settings);
                }
                else
                {
                    suppressBrainLayer(bot, settings);
                }
            }
        }

        private static void updateActiveBrainLayers_ShallUseNowPatch(this BotOwner bot)
        {
            if (!SPTBrainAnalyzerPlugin.Enabled.Value)
            {
                return;
            }

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
                if (settings.Matches(role, sideMask, logicName))
                {
                    return settings;
                }
            }

            return null;
        }

        private static LogicSettings findLogicSettingsToApply(string logicName, WildSpawnType role, EPlayerSideMask sideMask = EPlayerSideMask.All)
        {
            foreach (LogicSettings settings in logicSettingsToApply.Keys)
            {
                if (settings.Matches(role, sideMask, logicName))
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

        private static void suppressBrainLayer(this BotOwner bot, LogicSettings settings)
        {
            if (!settings.Matches(bot))
            {
                return;
            }

            Dictionary<int, AICoreLayerClass<BotLogicDecision>> botBrainLayerDictionary = bot.Brain.BaseBrain.GetBrainLayerDictionary();
            foreach (int index in botBrainLayerDictionary.Keys.ToArray())
            {
                if (botBrainLayerDictionary[index].Name() != settings.LogicName)
                {
                    continue;
                }

                disabledLogics[bot].Add(new DisabledLogicSettings(bot.Profile.Info.Settings.Role, bot.Profile.Side, botBrainLayerDictionary[index], index));
                
                bot.Brain.BaseBrain.method_3(index);

                if (!bot.Brain.BaseBrain.method_2(botBrainLayerDictionary[index]))
                {
                    botBrainLayerDictionary.Remove(index);

                    LoggingUtil.LogInfo("Removed " + settings.LogicName + " from " + bot.name + "'s brain");
                }
                else
                {
                    LoggingUtil.LogError("Could not remove " + settings.LogicName + " from " + bot.name + "'s brain");
                }

                break;
            }
        }

        private static void resumeBrainLayer(this BotOwner bot, LogicSettings settings)
        {
            foreach (DisabledLogicSettings disabledSettings in disabledLogics[bot].ToArray())
            {
                if (!settings.Matches(bot))
                {
                    continue;
                }

                Dictionary<int, AICoreLayerClass<BotLogicDecision>> botBrainLayerDictionary = bot.Brain.BaseBrain.GetBrainLayerDictionary();

                if (botBrainLayerDictionary.ContainsKey(disabledSettings.Index))
                {
                    LoggingUtil.LogWarning("Could not restore " + settings.LogicName + " to " + bot.name + "'s brain because index " + disabledSettings.Index + " already exists");
                    break;
                }

                //botBrainLayerDictionary.Add(disabledSettings.Index, disabledSettings.Layer);

                if (bot.Brain.BaseBrain.method_0(disabledSettings.Index, disabledSettings.Layer, true))
                {
                    disabledLogics[bot].Remove(disabledSettings);

                    LoggingUtil.LogInfo("Restored " + settings.LogicName + " to " + bot.name + "'s brain");
                }
                else
                {
                    LoggingUtil.LogError("Could not restore " + settings.LogicName + " to " + bot.name + "'s brain");
                }

                break;
            }
        }
    }
}
