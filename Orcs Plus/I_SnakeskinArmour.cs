using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class I_SnakeskinArmour : Item
    {
        public bool doublesHeld = false;
        public I_SnakeskinArmour(Map map)
            : base (map)
        {

        }

        public override string getName()
        {
            return "Snakeskin Armour";
        }

        public override string getShortDesc()
        {
            return "A wooden cuirass coating in hardened Snakeskin. This armour is often favoured by elite orc warriors, who claim that it is embued with an ancient protective power, which will reduce the damage the wearer suffers from attacks by 1, minimum 1. (max of only one will have effect per agent)." + (doublesHeld ? "[DISABLED]" : "");
        }

        public override Sprite getIconBack()
        {
            return map.world.iconStore.standardBack;
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Foreground_SnakeskinArmour.png");
        }

        public override void turnTick(Person owner)
        {
            doublesHeld = false;
            foreach (Item item in owner.items)
            {
                if (item == this)
                {
                    break;
                }

                if (item is I_SnakeskinArmour)
                {
                    doublesHeld = true;
                }
            }
        }

        public override int getLevel()
        {
            return LEVEL_RARE;
        }

        public override int getMorality()
        {
            return MORALITY_NEUTRAL;
        }
    }
}
