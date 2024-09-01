using EFT;
using SPTBrainAnalyzer.Models;
using SPTBrainAnalyzer.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPTBrainAnalyzer.Helpers
{
    public static class LogicPatchManager
    {
        private static Type[] allBaseBrainTypes = null;
        private static Type[] allBaseLogicTypes = null;
        private static Dictionary<string, Type> logicNames = new Dictionary<string, Type>();
        private static Dictionary<Type, object> logicPatches = new Dictionary<Type, object>();
        private static Dictionary<LogicSettings, bool> isLogicDisabled = new Dictionary<LogicSettings, bool>();

        public static bool IsLogicDisabled(this WildSpawnType role, Type logicType, EPlayerSide side)
        {
            LogicSettings settings = findLogicSettings(logicType, role, side);
            if (settings != null)
            {
                return isLogicDisabled[settings];
            }

            return false;
        }

        public static void ToggleLogic(this WildSpawnType role, Type logicType, bool value, EPlayerSideMask sideMask = EPlayerSideMask.All)
        {
            if (!logicPatches.ContainsKey(logicType))
            {
                logicPatches.Add(logicType, createShallUseNowPatch(logicType));
            }

            LogicSettings settings = findLogicSettings(logicType, role, sideMask);
            if (settings != null)
            {
                isLogicDisabled[settings] = !value;
                return;
            }

            isLogicDisabled.Add(new LogicSettings(logicType, role, sideMask), !value);
        }

        public static void ToggleLogic(this WildSpawnType role, string logicName, bool value, EPlayerSideMask sideMask = EPlayerSideMask.All)
        {
            role.ToggleLogic(FindBaseLogicLayerSimpleAbstractClass(logicName), value, sideMask);
        }

        public static IEnumerable<Type> GetAllBaseBrainTypes()
        {
            if (allBaseBrainTypes != null)
            {
                return allBaseBrainTypes;
            }

            allBaseBrainTypes = SPT.Reflection.Utils.PatchConstants.EftTypes
                .Where(t => typeof(BaseBrain).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .ToArray();

            return allBaseBrainTypes;
        }

        public static IEnumerable<Type> GetAllBaseLogicTypes()
        {
            if (allBaseLogicTypes != null)
            {
                return allBaseLogicTypes;
            }

            allBaseLogicTypes = SPT.Reflection.Utils.PatchConstants.EftTypes
                .Where(t => typeof(BaseLogicLayerSimpleAbstractClass).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .ToArray();

            return allBaseLogicTypes;
        }

        public static Type FindBaseLogicLayerSimpleAbstractClass(string desiredLogicName)
        {
            if (logicNames.ContainsKey(desiredLogicName))
            {
                return logicNames[desiredLogicName];
            }

            foreach (Type logicType in GetAllBaseLogicTypes())
            {
                var logic = Activator.CreateInstance(logicType, logicType.getDefaultConstructorArgumentsForBaseLogicType());

                MethodInfo nameMethodInfo = logicType.GetMethod("Name");
                string logicName = (string)nameMethodInfo.Invoke(logic, null);

                if (logicName == desiredLogicName)
                {
                    logicNames.Add(logicName, logicType);

                    return logicType;
                }
            }

            LoggingUtil.LogError("Cannot find type for " + desiredLogicName);

            return null;
        }

        private static object createShallUseNowPatch(string logicName)
        {
            Type logicType = FindBaseLogicLayerSimpleAbstractClass(logicName);
            if (logicType == null)
            {
                throw new InvalidOperationException("Cannot find logic class for " + logicName);
            }

            return createShallUseNowPatch(logicType);
        }

        private static object createShallUseNowPatch(Type logicType)
        {
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

        private static LogicSettings findLogicSettings(string logicName, WildSpawnType role, EPlayerSideMask sideMask = EPlayerSideMask.All)
        {
            return findLogicSettings(FindBaseLogicLayerSimpleAbstractClass(logicName), role, sideMask);
        }

        private static LogicSettings findLogicSettings(Type logicType, WildSpawnType role, EPlayerSideMask sideMask = EPlayerSideMask.All)
        {
            foreach (LogicSettings settings in isLogicDisabled.Keys)
            {
                if ((settings.LogicType == logicType) && (settings.Role == role) && (settings.SideMask == sideMask))
                {
                    return settings;
                }
            }

            return null;
        }

        private static LogicSettings findLogicSettings(string logicName, WildSpawnType role, EPlayerSide side)
        {
            return findLogicSettings(FindBaseLogicLayerSimpleAbstractClass(logicName), role, side);
        }

        private static LogicSettings findLogicSettings(Type logicType, WildSpawnType role, EPlayerSide side)
        {
            foreach (LogicSettings settings in isLogicDisabled.Keys)
            {
                if ((settings.LogicType == logicType) && (settings.Role == role) && settings.SideMask.CheckSide(side))
                {
                    return settings;
                }
            }

            return null;
        }

        private static object[] getDefaultConstructorArgumentsForBaseBrainType(this Type brainType, BotOwner botOwner = null)
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

        private static object[] getDefaultConstructorArgumentsForBaseLogicType(this Type brainType, BotOwner botOwner = null, int priority = -1)
        {
            List<object> arguments = new List<object>() { botOwner, priority };
            ConstructorInfo[] constructors = brainType.GetConstructors();
            ParameterInfo[] parameters = constructors[0].GetParameters();

            for (int p = 2; p < parameters.Length; p++)
            {
                Type parameterType = parameters[p].ParameterType;
                object defaultParamterValue = Activator.CreateInstance(parameterType);

                arguments.Add(defaultParamterValue);
            }

            return arguments.ToArray();
        }
    }
}
