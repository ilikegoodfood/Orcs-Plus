using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orcs_Plus
{
    internal class UAA_OrcElder : UAA
    {
        public UAA_OrcElder(Location loc, HolyOrder sg)
            : base(loc, sg)
        {
            // Raises stats granted by base: Might 1-3, Intrigue 1-3, Lore 2-3, Command 2-3
            // New Ranges: Might 2-3, Intrigue 1-2, Lore 2-3, Command 3-5
            person.stat_might = 2 + Eleven.random.Next(2);
            person.stat_intrigue = 1 + Eleven.random.Next(2);
            person.stat_command = 3 + Eleven.random.Next(3);
            person.hasSoul = false;
        }

        public UAA_OrcElder(Location loc, HolyOrder sg, Person p)
            : base(loc, sg, p)
        {
            // Raises stats granted by base: Might 1-3, Intrigue 1-3, Lore 2-3, Command 2-3
            // New Ranges: Might 2-3, Intrigue 1-2, Lore 2-3, Command 3-5
            person.stat_might = 2 + Eleven.random.Next(2);
            person.stat_intrigue = 1 + Eleven.random.Next(2);
            person.stat_command = 3 + Eleven.random.Next(3);
            person.hasSoul = false;
        }

        public override void die(Map map, string v, Person killer = null)
        {
            base.die(map, v, killer);
            bool flag = isCommandable() || this == map.awarenessManager.getChosenOne();
            if (flag)
            {
                map.stats.keyEvent = this.getName() + " died";
            }
        }

        public new List<Unit> getVisibleUnits()
        {
            List<Unit> result = new List<Unit>();

            // Do Stuff

            return result;
        }

        public override double getDisruptUtility(Unit c, List<ReasonMsg> reasons)
        {
            double val = -10000.0;
            reasons?.Add(new ReasonMsg("Does not disrupt agents", val));
            return -10000.0;
        }

        public override double getAttackUtility(Unit c, List<ReasonMsg> reasons, bool includeDangerousFoe = true)
        {
            double val = -10000.0;
            reasons?.Add(new ReasonMsg("Does not attack agents", val));
            return -10000.0;
        }

        public override double getBodyguardUtility(Unit c, List<ReasonMsg> reasons)
        {
            double val = -10000.0;
            reasons?.Add(new ReasonMsg("Does not guard agents", val));
            return -10000.0;
        }

        public override void turnTickAI()
        {
            ModCore.comLibAI.turnTickAI(this);
        }

        public override bool definesName()
        {
            return true;
        }

        public override string getName()
        {
            return "Orc Elder";
        }

        public override Sprite getPortraitBackground()
        {
            return map.world.iconStore.standardBack;
        }

        public override Sprite getPortraitForeground()
        {
            return EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
        }

        public override void spendSkillPoint()
        {
            person.skillPoints--;
            switch(Eleven.random.Next(3))
            {
                case 0:
                    person.stat_might++;
                    break;
                case 1:
                    person.stat_lore++;
                    break;
                case 2:
                    person.stat_command++;
                    break;
                default:
                    break;
            }
        }

        public override string getEventID_combatVAR()
        {
            return "anw.combatOrcVictoryAttackingRetreat";
        }

        public override string getEventID_combatVDR()
        {
            return "anw.combatOrcVictoryDefendingRetreat";
        }

        public override string getEventID_combatVAL()
        {
            return "anw.combatOrcVictoryAttackingLethal";
        }

        public override string getEventID_combatVDL()
        {
            return "anw.combatOrcVictoryDefendingLethal";
        }

        public override string getEventID_combatDAR()
        {
            return "anw.combatOrcDefeatAttackingRetreat";
        }

        public override string getEventID_combatDDR()
        {
            return "anw.combatOrcDefeatDefendingRetreat";
        }

        public override string getEventID_combatDAL()
        {
            return "anw.combatOrcDefeatAttackingLethal";
        }

        public override string getEventID_combatDDL()
        {
            return "anw.combatOrcDefeatDefendingLethal";
        }

        public override int[] getPositiveTags()
        {
            int[] tags = new int[2] { Tags.ORC, Tags.RELIGION };
            int[] pTags = person.getTags();

            int[] result = new int[pTags.Length + tags.Length];

            for(int i = 0; i < tags.Length; i++)
            {
                result[i] = tags[i];
            }
            for(int i = 0; i < pTags.Length; i++)
            {
                result[tags.Length + i] = pTags[i];
            }

            return result;
        }
    }
}
