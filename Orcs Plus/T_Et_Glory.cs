using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class T_Et_Glory : Trait
    {
        public override string getName()
        {
            return "Blessing of Glory (" + level + ")";
        }

        public override string getDesc()
        {
            return "Grants one might each time this agent kills an agent with higher might than themselves (including all modifiers).";
        }

        public override int getMaxLevel()
        {
            return 100;
        }

        public override int[] getTags()
        {
            return new int[]
            {
                Tags.COMBAT
            };
        }

        public override int getMightChange()
        {
            return level;
        }

        public override void onKill(Person victim)
        {
            Person cursed = assignedTo;
            if (cursed != null && victim.unit is UA)
            {
                if (victim.getStatMight() > cursed.getStatMight() && level < getMaxLevel())
                {
                    level++;
                }
            }
        }
    }
}
