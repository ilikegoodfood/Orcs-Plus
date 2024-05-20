using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_LootRemains : Challenge
    {
        Pr_Orcs_ImmortalRemains immortalRemains;

        public Ch_Orcs_LootRemains(Location location, Pr_Orcs_ImmortalRemains remains)
            : base (location)
        {
            immortalRemains = remains;
        }

        public override string getName()
        {
            return "Loot " + immortalRemains.getName();
        }

        public override string getDesc()
        {
            return "Pillage any gold and items that can be found on the remains of " + immortalRemains.person.getFullName() +". Looting their remains will destroy them.";
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.take;
        }

        public override double getComplexity()
        {
            return 1;
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            msgs?.Add(new ReasonMsg("Base", 1.0));

            return 1.0;
        }

        public override double getProfile()
        {
            return immortalRemains.ua.inner_profileMin + immortalRemains.profile;
        }

        public override bool valid()
        {
            return immortalRemains.person.getGold() > 0 || immortalRemains.person.items.Any(i => i != null);
        }

        public override bool validFor(UA ua)
        {
            return true;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;

            if (ua.person == null)
            {
                return utility;
            }

            double val = 0.2 * immortalRemains.person.getGold();
            if (val > 0.0)
            {
                msgs?.Add(new ReasonMsg("Gold on remains", val));
                utility += val;
            }

            val = 0.0;
            for (int i = 0; i < immortalRemains.person.items.Length; i++)
            {
                if (immortalRemains.person.items[i] != null)
                {
                    val++;
                }
            }
            if (val > 0.0)
            {
                int freeItemSlots = 0;
                for (int i = 0; i < ua.person.items.Count(); i++)
                {
                    if (ua.person.items[i] == null)
                    {
                        freeItemSlots++;
                    }
                }

                val = Math.Min(val, freeItemSlots) * 25;
                msgs?.Add(new ReasonMsg("Items on remains", val));
                utility += val;
            }

            return utility;
        }

        public override int getCompletionProfile()
        {
            return 3;
        }

        public override int getCompletionMenace()
        {
            return 0;
        }

        public override void complete(UA u)
        {
            if (u.isCommandable() && !map.automatic)
            {
                map.world.prefabStore.popItemTrade(u.person, immortalRemains.person, "Loot Remains", -1, -1);
            }
            else
            {
                u.person.addGold((int)Math.Floor(immortalRemains.person.getGold()));
                immortalRemains.person.gold = 0;

                foreach (Item item in immortalRemains.person.items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    u.person.gainItem(item);
                }
                map.addUnifiedMessage(u, null, "Remians Looted", u.getName() + " has looted the remains of " + immortalRemains.person.getFullName() + ", destroying them in the process.", "Remains Looted", false);
            }

            location.properties.Remove(immortalRemains);
        }

        public override int[] buildPositiveTags()
        {
            return new int[] { Tags.GOLD };
        }

        public override int[] getNegativeTags()
        {
            return immortalRemains.person.getTags();
        }

        public override int isGoodTernary()
        {
            return 0;
        }
    }
}
