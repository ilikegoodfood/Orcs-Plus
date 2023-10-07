using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class P_Vinerva_Health : Power
    {
        public P_Vinerva_Health(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Gourd of Blood";
        }

        public override string getDesc()
        {
            return "Orc Elders will share the Gourd's vile contents among warriors of the horde, granting them the ability to rapoidly recover from even the most gruesome of wounds. This effect of this gift lasts for 10 turns, and can be extended by giving the gift multiple times.";
        }

        public override string getFlavour()
        {
            return "Filled with blood of an uncertain origin, the huge red gourd sits amidst a tangle of thrashing, thorn-covered vines. Few dare approach it, and fewer still dare consume it's all to meaty flesh.";
        }

        public override string getRestrictionText()
        {
            return "Must be cast on an orc camp, who's culture's Life Mother tenet is elder aligned.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Foreground_BloodGourd.png");
        }

        public override Sprite getIconBack()
        {
            return map.world.iconStore.standardBack;
        }

        public override bool validTarget(Location loc)
        {
            Pr_Vinerva_Health giftHealth = (Pr_Vinerva_Health)loc.properties.FirstOrDefault(pr => pr is Pr_Vinerva_Health);
            return loc.settlement is Set_OrcCamp && loc.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_LifeMother life && life.status < 0 && (giftHealth == null || giftHealth.charge <= 250.0);
        }

        public override int getCost()
        {
            return 2;
        }

        public override void cast(Location loc)
        {
            base.cast(loc);

            Pr_Vinerva_Health giftHealth = (Pr_Vinerva_Health)loc.properties.FirstOrDefault(pr => pr is Pr_Vinerva_Health);

            if (giftHealth == null)
            {
                loc.properties.Add(new Pr_Vinerva_Health(loc));
            }
            else
            {
                giftHealth.charge += 50.0;
            }
        }
    }
}
