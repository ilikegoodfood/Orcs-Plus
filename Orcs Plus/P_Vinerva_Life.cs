using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class P_Vinerva_Life : Power
    {
        public P_Vinerva_Life(Map map)
            : base (map)
        {

        }

        public override string getName()
        {
            return "Sapling of Life";
        }

        public override string getDesc()
        {
            return "Orc Elders will nurture the Sapling of Life until it grows into an Orchard of Life. The Orchard of Life grants shelter, food and water for an orc camp, ensuring habitability and immediate construction.";
        }

        public override string getFlavour()
        {
            return "The sapling of the orchard of life is remarkable simply by existing within the scorching hot deserts and frozen arctic soil. Visually similar to an oak sapling, it twists and writhes unnaturally, as if the limb of an animal rather than a plant. With a little nurturing, it will grow and multiply beyond what reason should allow.";
        }

        public override string getRestrictionText()
        {
            return "Must be cast on an uninhabitable, unsettled location adjacent to an orc culture who's Life Mother tenet is elder aligned.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Foreground_SaplingOak.png");
        }

        public override Sprite getIconBack()
        {
            return map.world.iconStore.standardBack;
        }

        public override bool validTarget(Location loc)
        {
            if (loc.properties.Any(pr => pr is Pr_Vinerva_Life || pr is Pr_Vinerva_LifeBoon))
            {
                return false;
            }

            // Location is only valid if habitability is too low for orcs to settle normally
            bool valid = loc.hex.getHabilitability() < map.opt_orcHabMult * map.param.orc_habRequirement;

            // Checks if arget is within range of vinerva heart
            if (valid)
            {
                valid = false;
                if (map.overmind.god is God_Vinerva vinerva)
                {
                    foreach (int heartLocIndex in vinerva.hearts)
                    {
                        if (map.getStepDist(loc, map.locations[heartLocIndex]) <= map.param.power_vinerva_growthMaxDist)
                        {
                            valid = true;
                            break;
                        }
                    }
                }
                else
                {
                    valid = true;
                }
            }

            // Checks if the location is valid for orcish expansion, ignoring habitability
            if (valid)
            {
                if (loc.isOcean || loc.soc != null)
                {
                    valid = false;
                }
                else if (loc.settlement != null)
                {
                    if (ModCore.GetComLib().tryGetSettlementTypeForOrcExpansion(loc.settlement.GetType(), out List<Type> subsettlementBlacklist))
                    {
                        foreach (Subsettlement sub in loc.settlement.subs)
                        {
                            if (subsettlementBlacklist.Contains(sub.GetType()))
                            {
                                valid = false;
                            }
                        }
                    }
                    else
                    {
                        valid = false;
                    }
                }
            }

            // If location is uninhabvitable to orcs, and is otherwise valid for orcish expansion, checks if location is neighbouring orc settlement of a culture with the tenet at the required alignment level
            if (valid)
            {
                foreach (Location neighbour in loc.getNeighbours())
                {
                    if (neighbour.settlement is Set_OrcCamp && neighbour.soc is SG_Orc orcSociety)
                    {
                        if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_LifeMother life && life.status < 0)
                        {
                            return true;
                        }
                    }
                    else if (neighbour.soc is HolyOrder_Orcs orcCulture2)
                    {
                        if (orcCulture2.tenet_god is H_Orcs_LifeMother life && life.status < 0)
                        {
                            return true;
                        }
                    }

                    // Regardless of neighbouring orc socities or cultures, checks for orc waystaions and validates against the culture of each waystation
                    if (neighbour.settlement != null && neighbour.settlement.subs.Count > 0)
                    {
                        List<Subsettlement> waystations = neighbour.settlement.subs.FindAll(sub => sub is Sub_OrcWaystation);
                        foreach (Subsettlement subsettlement in waystations)
                        {
                            if (subsettlement is Sub_OrcWaystation waystation)
                            {
                                if (ModCore.Get().data.orcSGCultureMap.TryGetValue(waystation.orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_LifeMother life && life.status < 0)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public override int getCost()
        {
            return 2;
        }

        public override void cast(Location loc)
        {
            base.cast(loc);
            Pr_Vinerva_Life giftLife = new Pr_Vinerva_Life(loc);
            loc.properties.Add(giftLife);
        }
    }
}
