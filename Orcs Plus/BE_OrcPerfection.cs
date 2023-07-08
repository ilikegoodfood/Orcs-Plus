using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class BE_OrcPerfection : BattleEffect
    {
        public double buff = 0.25;

        public BE_OrcPerfection(BattleArmy battle)
        : base(battle)
        {

        }

        public override Sprite getIconFore()
        {
            return map.world.iconStore.ophanimLight;
        }

        public override string getName()
        {
            return "Perfect Coordination";
        }

        public override string getShortDesc()
        {
            return "These orcs are guided by Ophanim's perfect will, which boosts the performance of armies on this side by " + ((int)(100.0 * buff)).ToString() + "%";
        }

        public override double getLethalityDelta()
        {
            return buff;
        }
    }
}
