using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class I_DrinkingHorn : Item
    {
        public bool full = false;

        public Rti_DrinkGrott rti_DrinkGrott;

        public I_DrinkingHorn(Map map, Location location)
            :base (map)
        {
            rti_DrinkGrott = new Rti_DrinkGrott(location, this);
            challenges.Add(rti_DrinkGrott);
        }

        public override string getName()
        {
            if (full)
            {
                return "Drinking Horn (Orc Grott)";
            }

            return "Drinking Horn (Empty)";
        }

        public override string getShortDesc()
        {
            return "A finely carved drinking horn with a tightly fitted ceramic stopper. It is traditionally used by orcs to transport and drink Grott when travelling outside of orc lands. When the contents are drunk, the drinker's might and command stats are temporarily raised by 1, bit if the drinker is not an orc, they suffer 2 damage.";
        }

        public override Sprite getIconBack()
        {
            return map.world.iconStore.standardBack;
        }

        public override Sprite getIconFore()
        {
            if (full)
            {
                return EventManager.getImg("OrcsPlus.Foreground_DrinkingHorn_Full.png");
            }

            return EventManager.getImg("OrcsPlus.Foreground_DrinkingHorn_Empty.png");
        }

        public override List<Ritual> getRituals(UA ua)
        {
            return challenges;
        }

        public override int getLevel()
        {
            return LEVEL_RARE;
        }

        public override int getMorality()
        {
            return MORALITY_EVIL;
        }
    }
}
