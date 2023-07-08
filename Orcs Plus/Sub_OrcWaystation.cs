using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Sub_OrcWaystation : Subsettlement
    {
        public List<Challenge> challenges = new List<Challenge>();

        public SG_Orc orcSociety;

        public Sub_OrcWaystation(Settlement settlement, SG_Orc orcSociety)
            :base(settlement)
        {
            this.orcSociety = orcSociety;
            settlement.location.soc = orcSociety;
            challenges.Add(new Ch_Orcs_Expand(settlement, orcSociety));
            challenges.Add(new Ch_OrcRaiding(settlement));
            challenges.Add(new Ch_Orcs_RaidOutpost(settlement.location, orcSociety));
            challenges.Add(new Ch_Rest_InOrcCamp(settlement.location));
            challenges.Add(new Ch_Orcs_FundWaystation(settlement.location));
        }

        public override string getName()
        {
            return (orcSociety?.getName() ?? "NULL") + " Waystation";
        }

        public override string getInvariantName()
        {
            return "Orc Waystation";
        }

        public override string getHoverOverText()
        {
            return "A hidden waystation built by the " + (orcSociety?.getName() ?? "NULL") + ". It allows orcs to safely pass through the area to conduct raids or expand into new territories.";
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("OrcsPlus.Waystation.png");
        }

        public override List<Challenge> getChallenges()
        {
            return challenges;
        }

        public override bool canBeInfiltrated()
        {
            return false;
        }

        public override void turnTick()
        {
            if (settlement.location.settlement != settlement)
            {
                settlement = settlement.location.settlement;
            }

            if (settlement == null)
            {
                return;
            }

            if (orcSociety == null || orcSociety.isGone())
            {
                settlement.subs.Remove(this);
                return;
            }

            if (!ModCore.core.data.getSettlementTypesForWaystation().Contains(settlement.GetType()))
            {
                settlement.subs.Remove(this);
                return;
            }

            bool neighbouring = false;
            if (settlement.location.soc == orcSociety)
            {
                neighbouring = true;
            }
            else
            {
                foreach (Location neighbour in settlement.location.getNeighbours())
                {
                    if (neighbour.soc == orcSociety)
                    {
                        neighbouring = true;
                        break;
                    }
                }
            }

            if (!neighbouring)
            {
                settlement.subs.Remove(this);
                return;
            }
        }
    }
}
