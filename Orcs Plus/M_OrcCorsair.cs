using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class M_OrcCorsair : Minion
    {
        UA holder;

        public M_OrcCorsair(Map map)
            : base(map)
        {
            traits.Add(new Mt_DockyardBrawler());

            foreach(Unit unit in map.units)
            {
                if (unit is UA ua && ua.minions.Contains(this))
                {
                    holder = ua;
                    break;
                }
            }
        }

        public override string getName()
        {
            return "Orc Corsair";
        }

        public override string getShortDesc()
        {
            return base.getShortDesc() + "\nGains +2 attack and defence when at sea.";
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("OrcsPlus.Foreground_OrcCorsair.png");
        }

        public override Sprite getIconBack()
        {
            return map.world.textureStore.clear;
        }

        public override int getCommandCost()
        {
            return 2;
        }

        public override int getAttack()
        {
            int result = 4;

            if (holder != null && !holder.minions.Contains(this))
            {
                holder = null;
            }

            if (holder == null)
            {
                foreach (Unit unit in map.units)
                {
                    if (unit is UA ua && ua.minions.Contains(this))
                    {
                        holder = ua;
                        break;
                    }
                }
            }

            if (holder != null && holder.location != null && holder.location.isOcean)
            {
                result += 2;
            }

            return result;
        }

        public override int getMaxDefence()
        {
            int result = 2;

            if (holder != null && !holder.minions.Contains(this))
            {
                holder = null;
            }

            if (holder == null)
            {
                foreach (Unit unit in map.units)
                {
                    if (unit is UA ua && ua.minions.Contains(this))
                    {
                        holder = ua;
                        break;
                    }
                }
            }

            if (holder != null && holder.location != null && holder.location.isOcean)
            {
                result += 2;
            }

            return result;
        }

        public override int getMaxHP()
        {
            return 4;
        }

        public override int getGoldCost()
        {
            return map.param.minion_orcWarriorCost;
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
            return new M_OrcCorsair(map);
        }
    }
}
