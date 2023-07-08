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
            return EventManager.getImg("OrcsPlus.Sapling_Oak.png");
        }

        public override Sprite getIconBack()
        {
            return map.world.iconStore.standardBack;
        }

        public override bool validTarget(Location loc)
        {
            if (loc.properties.OfType<Pr_Vinerva_Life>().FirstOrDefault() != null)
            {
                return false;
            }

            bool result = loc.hex.getHabilitability() < map.opt_orcHabMult * map.param.orc_habRequirement;

            if (result)
            {
                if (loc.isOcean || loc.soc != null)
                {
                    result = false;
                }
                else if (loc.settlement != null)
                {
                    if (ModCore.comLib.tryGetSettlementTypeForOrcExpansion(loc.settlement.GetType(), out List<Type> subsettlementBlacklist))
                    {

                        foreach (Subsettlement sub in loc.settlement.subs)
                        {
                            if (subsettlementBlacklist.Contains(sub.GetType()))
                            {
                                result = false;
                            }
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
            }

            if (result)
            {
                result = false;

                foreach (Location neighbour in loc.getNeighbours())
                {
                    if (neighbour.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_LifeMother life && life.status < 0)
                    {
                        result = true;
                        break;
                    }

                    if (neighbour.soc is HolyOrder_Orcs orcCulture2 && orcCulture2.tenet_god is H_Orcs_LifeMother life2 && life2.status < 0)
                    {
                        result = true;
                        break;
                    }

                    Sub_OrcWaystation waystation = neighbour.settlement?.subs.OfType<Sub_OrcWaystation>().FirstOrDefault();
                    if (waystation != null && ModCore.core.data.orcSGCultureMap.TryGetValue(waystation.orcSociety, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null && orcCulture3.tenet_god is H_Orcs_LifeMother life3 && life3.status < 0)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
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
