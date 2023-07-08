using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_H_Orcs_MadnessFestival : ChallengeHoly
    {
        public double madnessRate = 0.35;

        public double madnessInflicted = 0.0;

        public Ch_H_Orcs_MadnessFestival(Location location)
            : base(location)
        {

        }

        public override string getName()
        {
            return "Holy: Festival of Madness";
        }

        public override string getDesc()
        {
            return "Gradually drives nearby human and elf rulers and agents mad. The Elder gains profile (2) and menace (4) each turn while performing this challenge.";
        }

        public override string getCastFlavour()
        {
            return "An elder leads this camp in a celebration of primal chaos, occasionally recreating fragments of the insane knowledge contained within Iastur's book. Few minds can withstand the power of these recreations.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by a member of an Orc Culture within their own borders. The Orc Culture's Harbringers of Madness tenet status must be elder aligned (-1 or lower).";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double val = unit.getStatLore();

            if (val < 1)
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                val = 1.0;
            }
            else
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", val));
            }

            return val;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 20.0;
            msgs?.Add(new ReasonMsg("Base", 20.0));

            int rulerCount = 0;
            foreach(Location neighbour in location.getNeighbours())
            {
                if(neighbour.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
                {
                    T_Insane insanity = settlementHuman.ruler.traits.OfType<T_Insane>().FirstOrDefault();
                    if (insanity == null || insanity.level < insanity.getMaxLevel())
                    {
                        rulerCount++;
                    }
                }
            }

            if (rulerCount > 0)
            {
                double val = rulerCount * 10;
                msgs?.Add(new ReasonMsg("Potential Insanity", val));
                utility += val;
            }

            return utility;
        }

        public override double getComplexity()
        {
            return 20;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.madness;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            if (location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_HarbringersMadness harbringer && harbringer.status < 0)
            {
                foreach (Location neighbour in location.getNeighbours())
                {
                    if (neighbour.settlement is SettlementHuman)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool validFor(UA ua)
        {
            return ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety == location.soc;
        }

        public override void onBegin(Unit unit)
        {
            madnessInflicted = 0.0;
        }

        public override void turnTick(UA ua)
        {
            ua.addProfile(2);
            ua.addMenace(4);

            madnessInflicted += madnessRate * getProgressPerTurnInner(ua, null);

            if (madnessInflicted >= 1.0)
            {
                foreach (Unit unit in location.units)
                {
                    if (!unit.isCommandable() && unit is UA agent && agent != map.awarenessManager.getChosenOne() && (agent.person.species is Species_Human || agent.person.species is Species_Elf))
                    {
                        agent.person.sanity -= Math.Floor(madnessInflicted);
                    }
                }

                foreach (Location neighbour in location.getNeighbours())
                {
                    if (neighbour.settlement is SettlementHuman settlementHuman)
                    {
                        Property.addToProperty(getName(), Property.standardProperties.MADNESS, 1.0, neighbour);

                        if (settlementHuman.ruler != null)
                        {
                            settlementHuman.ruler.sanity -= Math.Floor(madnessInflicted);

                            if (settlementHuman.ruler.sanity < 1.0)
                            {
                                settlementHuman.ruler.goInsane(-1);
                            }
                        }
                    }

                    foreach (Unit unit in neighbour.units)
                    {
                        if (!unit.isCommandable() && unit is UA agent && agent != map.awarenessManager.getChosenOne() && (agent.person.species is Species_Human || agent.person.species is Species_Elf))
                        {
                            agent.person.sanity -= Math.Floor(madnessInflicted);

                            if (agent.person.sanity < 1.0)
                            {
                                agent.person.goInsane(-1);
                            }
                        }
                    }
                }

                madnessInflicted -= Math.Floor(madnessInflicted);
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.MADNESS
            };
        }
    }
}
