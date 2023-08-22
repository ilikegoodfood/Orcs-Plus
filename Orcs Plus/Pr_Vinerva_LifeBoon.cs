using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;
using UnityEngine;

namespace Orcs_Plus
{
    public class Pr_Vinerva_LifeBoon : Property
    {
        public Pr_Vinerva_LifeBoon(Location location)
            : base(location)
        {

        }

        public override string getName()
        {
            return "Orchard of Life";
        }

        public override string getDesc()
        {
            return "The Orchard of Life maintains a stable environment in the region around it, offering food, water and shelter to any who dare live under the grarled, wind-warped trees. It is said that those who linger within the orchards can hear the angry whisperings of malaign spirits.";
        }

        public override bool hasBackgroundHexView()
        {
            return true;
        }

        public override Sprite getHexBackgroundSprite()
        {
            return EventManager.getImg("OrcsPlus.Hex_Orchard.png");
        }

        public override Sprite getSprite(World world)
        {
            return EventManager.getImg("OrcsPlus.Icon_Orchard.png");
        }

        public override void turnTick()
        {
            if (location.settlement != null && location.settlement.shadow < 1.0)
            {
                location.settlement.shadow += 0.02;

                if (location.settlement.shadow > 1.0)
                {
                    location.settlement.shadow = 1.0;
                }
            }
        }
    }
}
