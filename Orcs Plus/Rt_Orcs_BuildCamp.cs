using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Orcs_Plus
{
    public class Rt_Orcs_BuildCamp : Ritual
    {
        public int buildCost = 5;
        public double boost = 25.0;

        public Rt_Orcs_BuildCamp(Location location)
            :base (location)
        {

        }

        public override string getName()
        {
            return "Build Orc Camp";
        }

        public override string getCastFlavour()
        {
            return "A band of orcish raiders creates many small and temporary camps as they travel and raid beyond their native lands. With some effort, and a significant cost in labour, these temporary camps can leveraged to create permanent orcish camps.";
        }

        public override string getDesc()
        {
            return "Command a group of orcish raiders to create a permanent outpost at a location boardering their territory. Some of the raiders will remain at the new outpost to maintain it, and keep it secure. This will reduce the number of raiders in your group by 10, and establish <b>orcish industry</b> and <b>orc defences</b> with " + boost + "% charge.";
        }

        public override string getRestriction()
        {
            return "Requires a group of orcish raiders with more than " + buildCost + " hp, at empty location (can include city ruins or ancient ruins) with habilitability > 5%. If not adjacent to your existing horde requires the location to be coastal and you to have an orcish shipyard (with ships), OR for your horde to currently occupy no locations.";
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.orcDefences;
        }

        public override Challenge.challengeStat getChallengeType()
        {
            return Challenge.challengeStat.COMMAND;
        }

        public override bool validFor(UM um)
        {
            bool result = false;

            if (um.hp <= buildCost)
            {
                return false;
            }

            SG_Orc orcSociety = um.society as SG_Orc;

            if (orcSociety == null)
            {
                return result;
            }

            if (um.location.isOcean || um.location.hex.getHabilitability() < um.location.map.opt_orcHabMult * um.location.map.param.orc_habRequirement)
            {
                return result;
            }

            result = true;

            if (um.location.settlement != null)
            {
                if (ModCore.comLib.tryGetSettlementTypeForOrcExpansion(um.location.settlement.GetType(), out List<Type> subsettlementBlacklist))
                {
                    if (subsettlementBlacklist?.Count > 0)
                    {
                        foreach (Subsettlement sub in um.location.settlement.subs)
                        {
                            if (subsettlementBlacklist.Contains(sub.GetType()))
                            {
                                result = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    result = false;
                }
            }

            if (result && orcSociety.lastTurnLocs.Count == 0)
            {
                return result;
            }

            if (result && um.location.getNeighbours().FirstOrDefault(l => l.soc == orcSociety) != null)
            {
                return result;
            }
            else if (result && um.location.isCoastal)
            {
                foreach (Location location in um.map.locations)
                {
                    if (location.soc == orcSociety && location.settlement is Set_OrcCamp camp && camp.specialism == 5)
                    {
                        return result;
                    }
                }
            }

            return false;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override int getCompletionMenace()
        {
            return 5;
        }

        public override int getCompletionProfile()
        {
            return 10;
        }

        public override double getProgressPerTurnInner(UM unit, List<ReasonMsg> msgs)
        {
            int result = 0;
            
            if (unit.person == null)
            {
                int val = unit.hp / buildCost;
                if (val > 0)
                {
                    msgs?.Add(new ReasonMsg("Army Size", val));
                    result = val;
                }
                else
                {
                    msgs?.Add(new ReasonMsg("Base", 1));
                    result = 1;
                }
            }
            else
            {
                int val = unit.person.getStatCommand();
                if (val > 0)
                {
                    msgs?.Add(new ReasonMsg("Stat: Command", val));
                    result += val;
                }

                val = unit.hp / buildCost;
                if (val > 0)
                {
                    msgs?.Add(new ReasonMsg("Army Size", val));
                    result += val;
                }

                if (result < 1)
                {
                    msgs?.Add(new ReasonMsg("Base", 1));
                    result = 1;
                }
            }

            return result;
        }

        public override double getComplexity()
        {
            return 10;
        }

        public override void complete(UM u)
        {
            Location location = u.location;
            location.soc = u.society;
            Settlement settlement = location.settlement;
            location.settlement = new Set_OrcCamp(u.location);
            if (settlement != null)
            {
                foreach (Subsettlement sub in settlement.subs)
                {
                    location.settlement.subs.Add(sub);
                }
            }
            location.settlement.isInfiltrated = true;

            Property.addToPropertySingleShot("Founded by Raiders", Property.standardProperties.ORCISH_INDUSTRY, boost - 5, location);
            Pr_OrcDefences defenses = location.properties.OfType<Pr_OrcDefences>().FirstOrDefault();
            if (defenses == null)
            {
                defenses = new Pr_OrcDefences(location);
                location.properties.Add(defenses);
                
            }
            defenses.charge = boost;
            u.hp -= buildCost;

            if (u.society.isGone())
            {
                u.society.cachedGone = false;

                HolyOrder_Orcs orcCulture = u.map.socialGroups.FirstOrDefault(sg => sg is HolyOrder_Orcs culture && culture.orcSociety == u.society) as HolyOrder_Orcs;
                if (orcCulture != null)
                {
                    orcCulture.cachedGone = false;
                }
            }

            if (u.isCommandable())
            {
                HolyOrder_Orcs orcCulture = null;

                if ( ModCore.core.data.orcSGCultureMap.ContainsKey(u.society as SG_Orc) && ModCore.core.data.orcSGCultureMap[u.society as SG_Orc] != null)
                {
                    orcCulture = ModCore.core.data.orcSGCultureMap[u.society as SG_Orc];
                }

                if (orcCulture != null)
                {
                    ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Expand]), true);
                }
            }
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
