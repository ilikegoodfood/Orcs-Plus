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
    public class P_Vinerva_Thorns : Power
    {
        public P_Vinerva_Thorns (Map map)
            : base (map)
        {

        }

        public override string getName()
        {
            return "Wall of Thorns";
        }

        public override string getDesc()
        {
            return "A wall of thorns grows around the orc camp, raising its defence and dealing a small amount of damage to armies that attempt to raze the camp.";
        }

        public override string getFlavour()
        {
            return "A thick hedgerow of viscous thorn bushes spring up around the outer perimeter of an orc camp. Almost impenetrably thick, these bushes shift unnaturally, allowing orcs to come and go as they please, but grasping and biting at any who attempt to force their way through.";
        }

        public override string getRestrictionText()
        {
            return "Must be cast on an orc camp who's culture's Life Mother tenet is at maximum elder alignment.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Icon_ThornWall.png");
        }

        public override bool validTarget(Location loc)
        {
            Pr_Vinerva_Thorns giftThorns = (Pr_Vinerva_Thorns)loc.properties.FirstOrDefault(pr => pr is Pr_Vinerva_Thorns);
            if (giftThorns != null && giftThorns.charge > 250.0)
            {
                return false;
            }

            // Checks if arget is within range of vinerva heart
            bool valid = false;
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

            if (valid)
            {
                if (loc.settlement is Set_OrcCamp && loc.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_LifeMother life && life.status < -1)
                {
                    return true;
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

            Pr_Vinerva_Thorns giftThorns = loc.properties.OfType<Pr_Vinerva_Thorns>().FirstOrDefault();

            if (giftThorns == null)
            {
                giftThorns = new Pr_Vinerva_Thorns(loc);
                loc.properties.Add(giftThorns);
            }
            else
            {
                giftThorns.charge += 50.0;
            }

            if (loc.settlement != null)
            {
                loc.settlement.defences += 25;
            }

            SG_Orc orcSociety = loc.soc as SG_Orc;

            if (orcSociety != null)
            {
                ModCore.Get().TryAddInfluenceGain(orcSociety, new ReasonMsg("Granted Vinerva's Protection", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RecieveGift]), true);
            }
        }
    }
}