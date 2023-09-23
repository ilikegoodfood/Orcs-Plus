using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class I_IdolOfMadness : Item
    {
        public bool doublesHeld = false;

        public I_IdolOfMadness(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Idol of Madness";
        }

        public override string getShortDesc()
        {
            return "A strange, brightly painted box carved with distorted human faces that look almost real. When an agent holding this idol attacks another agent, the victim looses one sanity. If the victim is killed, their close relatives each lose 5 sanity. Grants " + map.param.trait_challengeBooster + " progress per turn when performing challenges relating to madness (max of only one will have effect per agent)." + (doublesHeld ? "[DISABLED]" : "");
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Foreground_MadnessIdol.png");
        }

        public override Sprite getIconBack()
        {
            return map.world.iconStore.standardBack;
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

                if (item is I_IdolOfMadness)
                {
                    doublesHeld = true;
                }
            }
        }

        public override double getChallengeProgressChange(Challenge challenge, UA unit, List<ReasonMsg> msgs)
        {
            int bonus = 0;

            doublesHeld = false;
            foreach (Item item in unit.person.items)
            {
                if (item == this)
                {
                    break;
                }

                if (item is I_IdolOfMadness)
                {
                    doublesHeld = true;
                }
            }

            if (!doublesHeld && challenge.getPositiveTags().Contains(Tags.MADNESS))
            {
                bonus = 3;
            }

            return bonus;
        }

        public override void launchAttack(BattleAgents battle, UA me, UA them, int dealt, int defBefore)
        {
            doublesHeld = false;
            foreach (Item item in me.person.items)
            {
                if (item == this)
                {
                    break;
                }

                if (item is I_IdolOfMadness)
                {
                    doublesHeld = true;
                }
            }

            if (!doublesHeld && them.person != null)
            {
                them.person.sanity--;
                if (them.person.sanity <= 0)
                {
                    them.person.goInsane();
                }
            }
        }

        public override int getLevel()
        {
            return LEVEL_ARTEFACT;
        }

        public override int getMorality()
        {
            return MORALITY_EVIL;
        }
    }
}
