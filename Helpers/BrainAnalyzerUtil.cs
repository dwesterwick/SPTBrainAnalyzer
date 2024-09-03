using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT;
using HarmonyLib;
using SPTBrainAnalyzer.Helpers;

namespace SPTBrainAnalyzer
{
    public static class BrainAnalyzerUtil
    {
        private static readonly string CSVFilename = "BrainAnalysis.csv";
        private static readonly string CSVHeaderRow = "WildSpawnType,Brain Type,Brain Type Class,Layer Type,Layer Type Class,Layer Priority,Layer Index";

        private static FieldInfo brainDictionaryField = null;

        public static void AnalyzeBrainsOfAllWildSpawnTypes(BotOwner donorOwner)
        {
            if (!SPTBrainAnalyzerPlugin.Enabled.Value || !SPTBrainAnalyzerPlugin.CreateCSVFile.Value)
            {
                return;
            }

            LoggingUtil.LogWarning("Analyzing brains using " + donorOwner.name + "...");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(CSVHeaderRow);

            WildSpawnType currentWildSpawnType = donorOwner.Profile.Info.Settings.Role;

            foreach (string wildSpawnTypeName in Enum.GetNames(typeof(WildSpawnType)))
            {
                try
                {
                    WildSpawnType wildSpawnType = (WildSpawnType)Enum.Parse(typeof(WildSpawnType), wildSpawnTypeName);

                    donorOwner.Profile.Info.Settings.Role = wildSpawnType;
                    donorOwner.Brain.Activate();

                    List<string> CSVLines = donorOwner.Brain.BaseBrain.getCSVLinesForBaseBrain(wildSpawnType);
                    foreach (string CSVLine in CSVLines)
                    {
                        sb.AppendLine(CSVLine);
                    }
                }
                catch (Exception e)
                {
                    LoggingUtil.LogError("Could not analyze brain for " + wildSpawnTypeName + ": " + e.Message);
                    LoggingUtil.LogError(e.StackTrace);
                }
            }

            LoggingUtil.CreateLogFile(CSVFilename, sb.ToString());

            donorOwner.Brain.BaseBrain.erase();

            donorOwner.Profile.Info.Settings.Role = currentWildSpawnType;
            donorOwner.Brain.Activate();

            LoggingUtil.LogWarning("Analyzing brains...done. " + donorOwner.name + " is now broken!");

            string message = "CSV brain dump complete. Please exit the raid!";
            NotificationManagerClass.DisplayMessageNotification(message, EFT.Communications.ENotificationDurationType.Long, EFT.Communications.ENotificationIconType.Alert, UnityEngine.Color.red);
        }

        public static Dictionary<int, AICoreLayerClass<BotLogicDecision>> GetBrainLayerDictionary(this BaseBrain brain)
        {
            if (brainDictionaryField == null)
            {
                brainDictionaryField = AccessTools.Field(typeof(AICoreStrategyAbstractClass<BotLogicDecision>), "dictionary_0");
            }

            return brainDictionaryField.GetValue(brain) as Dictionary<int, AICoreLayerClass<BotLogicDecision>>;
        }

        private static List<string> getCSVLinesForBaseBrain(this BaseBrain brain, WildSpawnType wildSpawnType)
        {
            List<string> CSVLines = new List<string>();
            Type brainType = brain.GetType();

            Dictionary<int, AICoreLayerClass<BotLogicDecision>> brainDictionary = brain.GetBrainLayerDictionary();

            LoggingUtil.LogInfo($"{brain.ShortName()} ({brainType.Name}):");
            foreach (int layerIndex in brainDictionary.Keys)
            {
                AICoreLayerClass<BotLogicDecision> layer = brainDictionary[layerIndex];
                LoggingUtil.LogInfo($"{layerIndex}: {layer.Name()} ({layer.GetType().Name}): {layer.Priority}");
                CSVLines.Add($"{wildSpawnType},{brain.ShortName()},{brainType.Name},{layer.Name()},{layer.GetType().Name},{layer.Priority},{layerIndex}");
            }

            return CSVLines;
        }

        private static void erase(this BaseBrain brain)
        {
            Dictionary<int, AICoreLayerClass<BotLogicDecision>> brainDictionary = brain.GetBrainLayerDictionary();

            foreach (int layerIndex in brainDictionary.Keys.ToArray())
            {
                brain.method_3(layerIndex);
            }
        }
    }
}
