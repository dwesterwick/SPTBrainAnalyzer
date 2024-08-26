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
        
        public static void AnalyzeBrainsOfAllWildSpawnTypes(BotOwner donorOwner)
        {
            LoggingUtil.LogWarning("Analyzing brains...");

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

            donorOwner.Profile.Info.Settings.Role = currentWildSpawnType;
            donorOwner.Brain.Activate();

            LoggingUtil.LogWarning("Analyzing brains...done.");
        }

        private static List<string> getCSVLinesForBaseBrain(this BaseBrain brain, WildSpawnType wildSpawnType)
        {
            List<string> CSVLines = new List<string>();
            Type brainType = brain.GetType();

            FieldInfo brainDictionaryField = AccessTools.Field(typeof(AICoreStrategyAbstractClass<BotLogicDecision>), "dictionary_0");
            var brainDictionary = (Dictionary<int, AICoreLayerClass<BotLogicDecision>>)brainDictionaryField.GetValue(brain);

            LoggingUtil.LogInfo($"{brain.ShortName()} ({brainType.Name}):");
            foreach (int layerIndex in brainDictionary.Keys)
            {
                AICoreLayerClass<BotLogicDecision> layer = brainDictionary[layerIndex];
                LoggingUtil.LogInfo($"{layerIndex}: {layer.Name()} ({layer.GetType().Name}): {layer.Priority}");
                CSVLines.Add($"{wildSpawnType},{brain.ShortName()},{brainType.Name},{layer.Name()},{layer.GetType().Name},{layer.Priority},{layerIndex}");
            }

            return CSVLines;
        }
    }
}
