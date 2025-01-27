using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_BuildTemple : Challenge
    {
        public int cost = 60;

        public Minion exemplar = null;

        public Ch_Orcs_BuildTemple(Location location)
            : base(location)
        {
            exemplar = new M_OrcChampion(map);
        }

        public override challengeStat getChallengeType()
        {
            return Challenge.challengeStat.COMMAND;
        }

        public override string getName()
        {
            return "Build Great Hall";
        }

        public override string getDesc()
        {
            return $"Builds a great hall in this orc camp, which acts as a cultural hub. An Orc Champion (5 HP, 5 Defence, 6 Attack, 3 Cost) will pledge themselves to your service if you build the great hall. You will also gain {ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildTemple]} influence over the orc culture.";
        }

        public override string getRestriction()
        {
            return "Can only be performed in an infiltrated specialised orc camp. Requires " + cost + " gold.";
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double val = unit.getStatCommand();

            if (val < 1)
            {
                val = 1.0;
                msgs?.Add(new ReasonMsg("Base", val));
            }
            else
            {
                msgs?.Add(new ReasonMsg("Stat: Command", val));
            }

            return val;
        }

        public override double getComplexity()
        {
            return map.param.ch_h_buildtemple_complexity;
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Icon_GreatHall.png");
        }

        public override bool validFor(UA ua)
        {
            return ua.isCommandable() && ua.person.gold >= cost;
        }

        public override bool valid()
        {
            return location.soc is SG_Orc orcSociety && location.settlement is Set_OrcCamp camp && camp.infiltration == 1.0 && camp.specialism != 0 && !camp.subs.Any(sub => sub is Sub_Temple);
        }

        public override void complete(UA u)
        {
            SG_Orc orcSociety = location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = location.soc as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcCulture != null && location.settlement is Set_OrcCamp camp && camp.specialism != 0 && !camp.subs.Any(sub => sub is Sub_Temple))
            {
                Sub_OrcTemple hall = new Sub_OrcTemple(location.settlement, orcCulture);
                location.settlement.subs.Add(hall);

                u.person.gold -= cost;

                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildTemple]), true);

                int commandCost = exemplar.getCommandCost();
                int commandLimit = u.getStatCommandLimit();
                int usedCommand = u.getCurrentlyUsedCommand();
                int minionCount = 0;

                if (commandCost <= commandLimit)
                {
                    foreach (Minion minion in u.minions)
                    {
                        if (minion != null)
                        {
                            minionCount++;
                        }
                    }

                    if (minionCount > 2 || commandCost + u.getCurrentlyUsedCommand() > u.getStatCommandLimit())
                    {
                        if (u.isCommandable() && map.burnInComplete && map.world.displayMessages)
                        {
                            map.world.prefabStore.popMinionDismiss(u, exemplar.getClone());
                            return;
                        }
                    }

                    bool[] dismissals = dismissalPlan(u, commandCost);
                    for (int i = 0; i < dismissals.Length; i++)
                    {
                        if (dismissals[i] && u.minions[i] != null)
                        {
                            u.minions[i].disband("Dismissed from service of " + u.getName());
                            u.minions[i] = null;
                        }
                    }

                    for (int i = 0; i < u.minions.Length; i++)
                    {
                        if (u.minions[i] == null)
                        {
                            u.minions[i] = exemplar.getClone();
                            break;
                        }
                    }
                }
            }
        }

        public bool[] dismissalPlan(UA ua, int commandCost)
        {
            bool[] dismissalPlan = new bool[ua.minions.Length];
            int commandLimit = ua.getStatCommandLimit();
            int usedCommand = ua.getCurrentlyUsedCommand();

            if (commandCost > commandLimit)
            {
                throw new Exception("Was unable to fire enough people to reach desired command cost");
            }
            else if (commandCost + usedCommand > commandLimit)
            {
                for (int i = 0; i < ua.minions.Length; i++)
                {
                    if (ua.minions[i] != null)
                    {
                        dismissalPlan[i] = true;
                        usedCommand -= ua.minions[i].getCommandCost();

                        if (commandCost + usedCommand <= commandLimit)
                        {
                            return dismissalPlan;
                        }
                    }
                }

                if (commandCost + usedCommand > commandLimit)
                {
                    throw new Exception ("Was unable to fire enough people to reach desired command cost");
                }
            }

            return dismissalPlan;
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.RELIGION,
                Tags.COOPERATION,
                Tags.ORC
            };
        }
    }
}
