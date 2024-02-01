using Assets.Code;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class I_OrcHeaddress : Item
    {
        public Person holder = null;

        public bool doublesHeld = false;

        public double xpModifier = 0.2;

        public I_OrcHeaddress(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Orc Headdress";
        }

        public override string getShortDesc()
        {
            return "A heavy wood and cloth headdress carved to resemble an orc, but still identifiably different to any familiar with orc faces. You gain +" + (int)(xpModifier * 100) + "% XP when completing challenges relating to Orcs, and, while in an orc camp, your profile is always at its minimum (max of only one will have effect per agent)." + (doublesHeld ? " [DISABLED]" : "");
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Foreground_OrcHeaddress.png");
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

        public override void turnTick(Person owner)
        {
            getHolder(owner);

            doublesHeld = false;
            foreach (Item item in owner.items)
            {
                if (item == this)
                {
                    break;
                }

                if (item is I_OrcHeaddress)
                {
                    doublesHeld = true;
                }
            }
        }

        public override double getProfileChange(double current)
        {
            getHolder();

            if (!doublesHeld && holder != null)
            {
                if (holder.unit?.location?.settlement is Set_OrcCamp)
                {
                    return -(current - holder.unit.inner_profileMin);
                }
            }

            return 0;
        }

        public override int getXPModifier(Person person, int amount)
        {
            Challenge challenge = OnChallengeComplete.lastChallengeCompleted;

            if (challenge != null)
            {
                bool orcTag = false;

                if (challenge.getPositiveTags().Contains(Tags.ORC))
                {
                    orcTag = true;
                }
                if (challenge.getNegativeTags().Contains(Tags.ORC))
                {
                    orcTag = true;
                }

                if (orcTag)
                {
                    return (int)Math.Ceiling(amount * 0.2);
                }
            }

            return 0;
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
