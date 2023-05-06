using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class M_OrcChampion : Minion
    {
        public M_OrcChampion(Map map)
            :base(map)
        {

        }

        public override string getName()
        {
            return "Orc Champion";
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("OrcsPlus.Foreground_OrcChampion.png");
        }

        public override Sprite getIconBack()
        {
            return map.world.textureStore.clear;
        }

        public override int getCommandCost()
        {
            return 3;
        }

        public override int getAttack()
        {
            return 6;
        }

        public override int getMaxDefence()
        {
            return 5;
        }

        public override int getMaxHP()
        {
            return 5;
        }

        public override int getGoldCost()
        {
            return map.param.minion_orcWarriorCost * 3;
        }

        public override int[] getTags()
        {
            return new int[]
            {
                Tags.ORC
            };
        }

        public override Minion getClone()
        {
            return new M_OrcChampion(map);
        }
    }
}
