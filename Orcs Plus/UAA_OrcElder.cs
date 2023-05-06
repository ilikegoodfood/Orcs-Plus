using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orcs_Plus
{
    internal class UAA_OrcElder : UAA
    {
        public Rt_H_Orcs_GiftGold giftGold;

        public UAA_OrcElder(Location loc, HolyOrder sg)
            : base(loc, sg)
        {
            // Raises stats granted by base: Might 1-3, Intrigue 1-3, Lore 2-3, Command 2-3
            // New Ranges: Might 2-3, Intrigue 1-2, Lore 2-3, Command 3-5
            person.stat_might = 2 + Eleven.random.Next(2);
            person.stat_intrigue = 1 + Eleven.random.Next(2);
            person.stat_command = 3 + Eleven.random.Next(3);
            person.hasSoul = false;
            person.traits.Add(new T_ReveredElder());
            person.species = map.species_orc;

            giftGold = new Rt_H_Orcs_GiftGold(loc);
            rituals.Add(giftGold);
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
            person.traits.Add(new T_ReveredElder());
            person.species = map.species_orc;

            giftGold = new Rt_H_Orcs_GiftGold(loc);
            rituals.Add(giftGold);
        }

        public override void turnTick(Map map)
        {
            base.turnTick(map);
            person.XP += map.param.socialGroup_orc_upstartXPPerTurn;
            if (person.skillPoints > 0)
            {
                spendSkillPoint();
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
            ModCore.core.comLibAI.turnTickAI(this);
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
            Sprite result;
            if (order.genderExclusive == -1)
            {
                result = EventManager.getImg("OrcsPlus.Foreground_OrcElder_F.png");
            }
            else if (order.genderExclusive == 1)
            {
                result = EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
            }
            else
            {
                if (person.index % 2 == 0)
                {
                    result = EventManager.getImg("OrcsPlus.Foreground_OrcElder_F.png");
                }
                else
                {
                    result = EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
                }
            }
            return result;
        }

        public override Sprite getPortraitForegroundAlt()
        {
            Sprite result;
            if (order.genderExclusive == -1)
            {
                result = EventManager.getImg("OrcsPlus.Foreground_OrcElder_F.png");
            }
            else if (order.genderExclusive == 1)
            {
                result = EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
            }
            else
            {
                if (person.index % 2 == 0)
                {
                    result = EventManager.getImg("OrcsPlus.Foreground_OrcElder_F.png");
                }
                else
                {
                    result = EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
                }
            }
            return result;
        }

        public override void spendSkillPoint()
        {
            if (aiGrowthTargetTags.ContainsKey(Tags.INTRIGUE))
            {
                aiGrowthTargetTags[Tags.INTRIGUE] -= 20;
            }

            base.spendSkillPoint();
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
