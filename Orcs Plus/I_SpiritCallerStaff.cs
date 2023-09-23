using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class I_SpiritCallerStaff : Item
    {
        public Person holder;

        public I_SpiritCallerStaff(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Orc Spirit Caller's Staff";
        }

        public override string getShortDesc()
        {
            return "A thick wooden staff, topped with an orc skull. It is used by orc spirit callers to communicate with the dead. Grants +1 <b>lore</b>, and +1 attack and defence in locations with at least 50 death.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Foreground_SpiritCallerStaff.png");
        }

        public override Sprite getIconBack()
        {
            return map.world.iconStore.standardBack;
        }

        public Person getHolder(Person owner = null)
        {
            if (owner != null && owner != holder)
            {
                holder = owner;
            }

            if (holder != null && !holder.items.Contains(this))
            {
                holder = null;
            }

            if (holder == null)
            {
                foreach (Person person in map.persons)
                {
                    if (person.items.Contains(this))
                    {
                        holder = person;
                        break;
                    }
                }
            }

            return holder;
        }

        public override int getLoreBonus()
        {
            return 1;
        }

        public override int getAttackBonus()
        {
            int result = 0;

            getHolder();

            if (holder?.unit?.location != null)
            {
                Pr_Death death = (Pr_Death)holder.unit.location.properties.FirstOrDefault(pr => pr is Pr_Death);
                if (death != null && death.charge >= 50.0)
                {
                    result = 1;
                }
            }

            return result;
        }

        public override int getDefenceBonus()
        {
            int result = 0;

            getHolder();

            if (holder?.unit?.location != null)
            {
                Pr_Death death = (Pr_Death)holder.unit.location.properties.FirstOrDefault(pr => pr is Pr_Death);
                if (death != null && death.charge >= 50.0)
                {
                    result = 1;
                }
            }

            return result;
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
