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
        public static void AnalyzeBrains(BotOwner donorOwner)
        {
            LoggingUtil.LogWarning("Analyzing brains...");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Brain Type,Brain Type Class,Layer Type,Layer Type Class,Layer Priority,Layer Index");

            foreach (Type brainType in GetAllBaseBrainTypes())
            {
                foreach (string CSVLine in brainType.analyzeBaseBrain(donorOwner))
                {
                    sb.AppendLine(CSVLine);
                }
            }

            LoggingUtil.CreateLogFile("BrainAnalysis.csv", sb.ToString());

            LoggingUtil.LogWarning("Analyzing brains...done.");
        }

        public static IEnumerable<Type> GetAllBaseBrainTypes()
        {
            return SPT.Reflection.Utils.PatchConstants.EftTypes
                .Where(t => typeof(BaseBrain).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract);
        }

        private static List<string> analyzeBaseBrain(this Type brainType, BotOwner botOwner)
        {
            List<string> CSVLines = new List<string>();

            try
            {
                object[] brainConstructorArgs = brainType.getDefaultConstructorArgumentsForBaseBrainType(botOwner);
                var brain = Activator.CreateInstance(brainType, brainConstructorArgs);

                FieldInfo brainDictionaryField = AccessTools.Field(typeof(AICoreStrategyAbstractClass<BotLogicDecision>), "dictionary_0");
                MethodInfo shortNameMethod = AccessTools.Method(typeof(BaseBrain), "ShortName");

                Dictionary<int, AICoreLayerClass<BotLogicDecision>> brainDictionary = (Dictionary<int, AICoreLayerClass<BotLogicDecision>>)brainDictionaryField.GetValue(brain);
                string brainName = (string)shortNameMethod.Invoke(brain, new object[0]);

                LoggingUtil.LogInfo(brainName + " (" + brainType.Name + "):");
                foreach (int layerIndex in brainDictionary.Keys)
                {
                    AICoreLayerClass<BotLogicDecision> layer = brainDictionary[layerIndex];
                    LoggingUtil.LogInfo(layerIndex + ": " + layer.Name() + " (" + layer.GetType().Name + "): " + layer.Priority);

                    CSVLines.Add($"{brainName},{brainType.Name},{layer.Name()},{layer.GetType().Name},{layer.Priority},{layerIndex}");
                }
            }
            catch (Exception e)
            {
                LoggingUtil.LogError("Could not analyze " + brainType.Name + ": " + e.Message);
                LoggingUtil.LogError(e.StackTrace);

                return new List<string>();
            }

            return CSVLines;
        }

        private static object[] getDefaultConstructorArgumentsForBaseBrainType(this Type brainType, BotOwner botOwner)
        {
            List<object> arguments = new List<object>() { botOwner };
            ConstructorInfo[] constructors = brainType.GetConstructors();
            ParameterInfo[] parameters = constructors[0].GetParameters();

            for (int p = 1; p < parameters.Length; p++)
            {
                Type parameterType = parameters[p].ParameterType;
                object defaultParamterValue = Activator.CreateInstance(parameterType);

                arguments.Add(defaultParamterValue);
            }

            return arguments.ToArray();
        }
    }
}
