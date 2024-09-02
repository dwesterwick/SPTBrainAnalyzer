using EFT;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SPTBrainAnalyzer.Models
{
    public class LogicSettings : IEquatable<LogicSettings>
    {
        public Type LogicType { get; private set; } = null;
        public string LogicName { get; private set; }
        public WildSpawnType Role { get; private set; }
        public EPlayerSideMask SideMask { get; private set; }

        public bool HasLogicType => LogicType != null;

        public LogicSettings([NotNull] string _logicName, WildSpawnType _role, EPlayerSideMask _sideMask)
        {
            LogicName = _logicName;
            Role = _role;
            SideMask = _sideMask;
        }

        public void RegisterLogicType(Type logicType)
        {
            if (!typeof(AICoreLayerClass<BotLogicDecision>).IsAssignableFrom(logicType))
            {
                throw new InvalidOperationException("Cannot register a type (" + logicType.Name + ") that is not a brain layer");
            }

            LogicType = logicType;
        }

        public bool Matches(BotOwner bot)
        {
            return Matches(bot.Profile.Info.Settings.Role, bot.Profile.Side);
        }

        public bool Matches(WildSpawnType role, EPlayerSide side)
        {
            if (Role != role)
            {
                return false;
            }

            if (!SideMask.CheckSide(side))
            {
                return false;
            }

            return true;
        }

        public bool Matches(WildSpawnType role, EPlayerSide side, Type logicType)
        {
            if (LogicType != logicType)
            {
                return false;
            }

            return Matches(role, side);
        }

        public bool Matches(WildSpawnType role, EPlayerSide side, string logicName)
        {
            if (LogicName != logicName)
            {
                return false;
            }

            return Matches(role, side);
        }

        public bool Matches(WildSpawnType role, EPlayerSideMask sideMask)
        {
            if (Role != role)
            {
                return false;
            }

            if (SideMask != sideMask)
            {
                return false;
            }

            return true;
        }

        public bool Matches(WildSpawnType role, EPlayerSideMask sideMask, Type logicType)
        {
            if (LogicType != logicType)
            {
                return false;
            }

            return Matches(role, sideMask);
        }

        public bool Matches(WildSpawnType role, EPlayerSideMask sideMask, string logicName)
        {
            if (LogicName != logicName)
            {
                return false;
            }

            return Matches(role, sideMask);
        }

        public bool Equals(LogicSettings other)
        {
            if ((LogicName != other.LogicName) && (LogicType != other.LogicType))
            {
                return false;
            }

            return Matches(other.Role, other.SideMask);
        }
    }
}
