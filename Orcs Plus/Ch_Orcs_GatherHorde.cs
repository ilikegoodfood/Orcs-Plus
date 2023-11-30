using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orcs_Plus
{
    internal class Ch_Orcs_GatherHorde : Challenge
    {
        public Ch_Orcs_GatherHorde(Location loc) : base(loc)
        {
            
        }

        public override string getName()
        {
            return "Gather the Horde";
        }

        public override string getDesc()
        {
            return "Call all subjugated armies belonging to this orc society to this location. They will wait awhile for new orders.";
        }

        public override string getRestriction()
        {
            return "Requires an agent holding a war banner for this orc society, this orc camp to be subjugated, the orc society to be at peace, the society to have at least one military unit, and the military unit's home camp to be subjugated.";
        }

        public override void complete(UA u)
        {
            if (location.settlement == null || location.soc == null)
            {
                return;
            }

            SG_Orc orcs = location.soc as SG_Orc;

            if (orcs != null)
            {
                List<UM_OrcArmy> armies = ModCore.Get().data.getOrcArmies(map, orcs);
                List<UM_OrcArmy> affectedArmies = new List<UM_OrcArmy>();
                int travelDuration = 0;

                foreach (UM_OrcArmy army in armies)
                {
                    if (map.locations[army.homeLocation].settlement.isInfiltrated && !(army.task is Task_InBattle))
                    {
                        Location[] path = ModCore.GetComLib().pathfinding.getPathTo(army.location, location, army);
                        if (path != null)
                        {
                            int duration = ModCore.GetComLib().getTravelTimeTo(army, location);

                            if (duration > travelDuration)
                            {
                                travelDuration = duration;
                            }

                            affectedArmies.Add(army);
                        }
                    }
                }

                foreach (UM_OrcArmy army in affectedArmies)
                {
                    army.task = new Task_GatherAtLocation(location, travelDuration + 11);
                }
            }
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override double getComplexity()
        {
            return 10;
        }

        public override int getCompletionMenace()
        {
            return 30;
        }

        public override int getCompletionProfile()
        {
            return 15;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            //Messages are used to display values to the UI. Since not all passes gather data for the UI, messages can be null at some points, to save compute time while processing AI
            msgs?.Add(new ReasonMsg("Stat: Command", unit.getStatCommand()));
            return Math.Max(1, unit.getStatCommand());
        }

        public override Sprite getSprite()
        {
            return location.map.world.iconStore.i_orcishBanner;
        }

        public override int isGoodTernary()
        {
            //1 is good, 0 is neutral, -1 is evil
            return -1;
        }

        public override bool valid()
        {
            return location.settlement != null && location.settlement is Set_OrcCamp && location.soc != null && location.settlement.isInfiltrated && location.soc is SG_Orc orcSociety && !orcSociety.isAtWar() && orcSociety.currentMilitary > 0.0;
        }

        public override bool validFor(UA uA)
        {
            //Console.WriteLine("OrcsPlus: Running GatherHorde ValidFor");
            if (location.soc == null || !(location.soc is SG_Orc) || location.soc.isAtWar() || location.soc.currentMilitary == 0.0)
            {
                //Console.WriteLine("OrcsPlus: Invalid location, or no army found.");
                return false;
            }

            SG_Orc orcs = location.soc as SG_Orc;
            if (uA != null && uA.isCommandable())
            {
                //Console.WriteLine("OrcsPlus: Unit " + uA.getName() + " is commandable.");
                Item[] items = uA.person.items;
                bool hasInfiltratedArmyBase = false;
                foreach (Item item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    I_HordeBanner banner = item as I_HordeBanner;
                    if (banner != null && orcs == banner.orcs)
                    {
                        //Console.WriteLine("OrcsPlus: " + uA.getName() + " has orcish banner belonging to this orc social group.");
                        List<UM_OrcArmy> armies = ModCore.Get().data.getOrcArmies(map, orcs);

                        if (armies != null)
                        {
                            //Console.WriteLine("OrcsPlus: Orc armies found.");
                            foreach (UM_OrcArmy army in armies)
                            {
                                //Console.WriteLine("OrcsPlus: Iterating orc armies belonging to this social group.");
                                Settlement armyBase = map.locations[army.homeLocation].settlement;
                                if (armyBase != null && armyBase.isInfiltrated && !(armyBase.location == location && army.location == armyBase.location))
                                {
                                    //Console.WriteLine("OrcsPlus: Found army with infiltrated home location.");
                                    hasInfiltratedArmyBase = true;
                                    break;
                                }
                            }
                        }
                        return hasInfiltratedArmyBase;
                    }
                }
            }
            return false;
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
                {
                    Tags.ORC
                };
        }
    }
}
