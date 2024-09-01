using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTBrainAnalyzer.Models
{
    public class LogicSettings
    {
        public Type LogicType { get; private set; }
        public WildSpawnType Role { get; private set; }
        public EPlayerSideMask SideMask { get; private set; }

        public LogicSettings(Type _logicType, WildSpawnType _role, EPlayerSideMask _sideMask)
        {
            LogicType = _logicType;
            Role = _role;
            SideMask = _sideMask;
        }
    }
}
