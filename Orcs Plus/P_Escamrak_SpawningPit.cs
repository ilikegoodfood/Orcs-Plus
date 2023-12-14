using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class P_Escamrak_SpawningPit : Power
    {
        public P_Escamrak_SpawningPit(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Gift of Flesh";
        }

        public override string getDesc()
        {
            return "Creates a Spawning Pit at the target orc camp. While the orcs are at war, they will release a Maddened Spawn army from the spawning pit every 20 turns.";
        }

        public override string getFlavour()
        {
            return "A sample of Escamrak's everliving flesh, granted to his loyal servents, so that they may foster and feed its unending growth.";
        }

        public override string getRestrictionText()
        {
            return "Requires a specialised orc camp of a culture that has the Fleshweaving Tenet fully elder aligned. The camp must not already contain a Spawning Pit.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Icon_Escamrak_SpawningPit.png");
        }

        public override int getCost()
        {
            return 2;
        }

        public override bool validTarget(Location loc)
        {
            if (loc.settlement is Set_OrcCamp camp && camp.specialism > 0)
            {
                if (loc.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture.tenet_god is H_Orcs_Fleshweaving fleshweaving && fleshweaving.status < -1)
                {
                    if (!camp.subs.Any(sub => sub is Sub_Orcs_SpawningPit))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void cast(Location loc)
        {
            if (loc.settlement != null)
            {
                loc.settlement.subs.Add(new Sub_Orcs_SpawningPit(loc.settlement));
            }
        }
    }
}
