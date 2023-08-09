using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class P_Ophanim_PerfectHorde : Power
    {
        public P_Ophanim_PerfectHorde (Map map)
            : base (map)
        {

        }

        public override string getName()
        {
            return "Perfect Horde";
        }

        public override string getDesc()
        {
            return "Perfect the target orc army.";
        }

        public override string getFlavour()
        {
            return "The orcs in this horde have surrendered thier will to Ophanim, becoming perfect in doing so. They do not waste time on internal squabbling, aimless posturing, or progressing their own interests within the horde. Instead, their flesh is remade in crystal, and they do naught but hone their skills, so that they might better fight the enemies of The Inescapable Truth.";
        }

        public override string getRestrictionText()
        {
            return "Must be cast on an orc army whose home location is at 300% perfection, and that is from an orc culture whose Perfection tenet is at maximum elder alignement (-2).";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Foreground_PerfectHorde.png");
        }

        public override bool validTarget(Unit unit)
        {
            bool valid = false;

            if (unit is UM_OrcArmy orcArmy && !(orcArmy is UM_PerfectHorde) && orcArmy.homeLocation != -1 && orcArmy.society is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_Perfection perfection && perfection.status < -1)
            {
                Pr_Ophanim_Perfection perfectionLocal = orcArmy.map.locations[orcArmy.homeLocation].properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
                if (perfectionLocal != null && perfectionLocal.charge >= 300.0)
                {
                    valid = true;
                }
            }

            return valid;
        }

        public override int getCost()
        {
            return 4;
        }

        public override void cast(Unit unit)
        {
            UM_OrcArmy orcArmy = unit as UM_OrcArmy;
            SG_Orc orcSociety = unit.society as SG_Orc;

            if (orcArmy != null && orcSociety != null)
            {
                base.cast(unit);

                UM_PerfectHorde perfectHorde = new UM_PerfectHorde(map.locations[orcArmy.homeLocation], orcSociety, orcArmy.parent);
                orcArmy.location.units.Add(perfectHorde);
                orcArmy.map.units.Add(perfectHorde);
                perfectHorde.location = orcArmy.location;
                orcArmy.parent.army = perfectHorde;

                double hpPercentage = (double)orcArmy.hp / (double)orcArmy.maxHp;
                perfectHorde.updateMaxHP();
                perfectHorde.hp = (int)Math.Ceiling(perfectHorde.maxHp * hpPercentage);

                ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Made Perfect", ModCore.core.data.influenceGain[ModData.influenceGainAction.RecieveGift]), true);

                orcArmy.disband(unit.map, "Remade into Perfect Horde");
            }
        }
    }
}
