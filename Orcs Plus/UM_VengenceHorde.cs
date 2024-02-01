using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class UM_VengenceHorde : UM_OrcArmy
    {
        public UA escortee;

        public UM_VengenceHorde(Location loc, SocialGroup sg, Set_OrcCamp camp, UA escortTarget)
            : base(loc, sg, camp)
        {
            escortee = escortTarget;
        }

        public override string getName()
        {
            return "Avenging Warriors";
        }

        public override Sprite getPortraitForeground()
        {
            return map.world.textureStore.minion_orcWarrior;
        }

        public override void turnTickInner(Map map)
        {
            if (escortee.isDead || (!(escortee.task is Task_AttackUnitWithEscort) && escortee.engaging == null && escortee.engagedBy == null))
            {
                disband(map, "Escort Complete");
            }
        }

        public override void turnTickAI()
        {
            if (escortee.isDead || (!(escortee.task is Task_AttackUnitWithEscort) && escortee.engaging == null && escortee.engagedBy == null))
            {
                disband(map, "Escort Complete");
            }
            else
            {
                task = new Task_EscortUA(escortee);
            }
        }

        public override bool checkForDisband(Map map)
        {
            return escortee.isDead || (!(escortee.task is Task_AttackUnitWithEscort) && escortee.engaging == null && escortee.engagedBy == null);
        }
    }
}
