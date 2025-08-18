using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static DuloGames.UI.UITooltip;

namespace Orcs_Plus
{
    public class I_ShamanStaff : Item
    {
        public static HashSet<Type> geomancyTypes = new HashSet<Type> {
                typeof(Mg_Tremor),
                typeof(Mg_Volcano),
                typeof(Mg_BringTheSnows),
                typeof(Mg_DeathOfTheSun),
                typeof(Mg_G_Nurture),
                typeof(Mg_G_BountifulHarvest),
                typeof(Mg_N_GeomanticSupport),
                typeof(Mg_AraneFortress),
                typeof(Mg_AssaultChanneller)
            };

        public bool doublesHeld = false;

        public I_ShamanStaff(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Orc Shaman's Staff";
        }

        public override string getShortDesc()
        {
            return "A thick wooden staff, topped with a large gem. It is used by orc shamans to enhance and focus their connection to geomantic loci. Grants +1 <b>lore</b>, and " + map.param.trait_challengeBooster + " progress per turn when performing challenges relating to geomancy (max of only one will have challenge effect per agent)." + (doublesHeld ? "[DISABLED]" : "");
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Foreground_ShamanStaff.png");
        }

        public override Sprite getIconBack()
        {
            return map.world.iconStore.standardBack;
        }

        public override int getLoreBonus()
        {
            return 1;
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

                if (item is I_ShamanStaff)
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

                if (item is I_ShamanStaff)
                {
                    doublesHeld = true;
                }
            }

            if (!doublesHeld && geomancyTypes.Contains(challenge.GetType()))
            {
                bonus = map.param.trait_challengeBooster;
            }

            return bonus;
        }

        public override int getLevel()
        {
            return LEVEL_ARTEFACT;
        }

        public override int getMorality()
        {
            return MORALITY_NEUTRAL;
        }
    }
}
