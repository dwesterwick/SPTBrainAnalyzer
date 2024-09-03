using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTBrainAnalyzer.Models
{
    public class DisabledLogicSettings : IEquatable<DisabledLogicSettings>
    {
        public WildSpawnType Role { get; private set; }
        public EPlayerSide Side { get; private set; }
        public AICoreLayerClass<BotLogicDecision> Layer { get; private set; }
        public int Index { get; private set; }

        public DisabledLogicSettings(WildSpawnType _role, EPlayerSide _side, AICoreLayerClass<BotLogicDecision> _layer, int _index)
        {
            Role = _role;
            Side = _side;
            Layer = _layer;
            Index = _index;
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

            if (Side != side)
            {
                return false;
            }

            return true;
        }

        public bool Matches(WildSpawnType role, EPlayerSide side, Type logicType)
        {
            if (Layer.GetType() != logicType)
            {
                return false;
            }

            return Matches(role, side);
        }

        public bool Matches(WildSpawnType role, EPlayerSide side, AICoreLayerClass<BotLogicDecision> layer)
        {
            return Matches(role, side, layer.GetType());
        }

        public bool Equals(DisabledLogicSettings other)
        {
            return Matches(other.Role, other.Side, other.Layer);
        }
    }
}
