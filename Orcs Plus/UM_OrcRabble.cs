using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class UM_OrcRabble : UM_OrcArmy
    {
        public UM_OrcRabble (Location loc, SocialGroup sg, Set_OrcCamp camp, int hp)
            : base (loc, sg, camp)
        {
            this.hp = hp;
            maxHp = hp;
        }

        public override string getName()
        {
            return "Orc Rabble";
        }

        public override void turnTickInner(Map map)
        {
            hp -= 2;
            maxHp = hp;

            if (maxHp <= 0)
            {
                disband(map, "Army Dissipated");
            }

            if (task is Task_RazeLocation && location.settlement is SettlementHuman settlementHuman && society is SG_Orc orcSociety && orcSociety.capital != -1 && map.locations[orcSociety.capital].soc == orcSociety && map.locations[orcSociety.capital].settlement is Set_OrcCamp capitalCamp)
            {
                Pr_OrcPlunder pr_OrcPlunder = capitalCamp.location.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();

                if (pr_OrcPlunder == null)
                {
                    pr_OrcPlunder = new Pr_OrcPlunder(capitalCamp.location);
                    capitalCamp.location.properties.Add(pr_OrcPlunder);
                }

                int gold = (int)(map.param.ch_orcRazeGoldGainArmy * settlementHuman.prosperity * settlementHuman.population);

                if (gold > 0)
                {
                    pr_OrcPlunder.gold += gold;
                    map.addMessage(getName() + " plunders " + gold + " gold", 0.2, positive: true, base.location.hex);
                    if (map.burnInComplete)
                    {
                        if (gold >= 10)
                        {
                            map.addUnifiedMessage(this, base.location, "Plunder", getName() + " plunders " + gold + " gold from " + base.location.getName() + " while razing the settlement, which will be gathered in the orc main fortress's plunder, which your agents can access", UnifiedMessage.messageType.ORC_PLUNDER, force: true);
                        }

                        map.hintSystem.popHint(HintSystem.hintType.ORC_PLUNDER);
                    }
                }
            }
        }

        public override bool checkForDisband(Map map)
        {
            if (society.isGone() || !map.socialGroups.Contains(society))
            {
                task = null;
                location.units.Remove(this);
                map.units.Remove(this);
                isDead = true;
                return true;
            }

            return false;
        }

        public override void inBattle(BattleArmy battleArmy)
        {
            return;
        }

        public override Sprite getPortraitForeground()
        {
            return map.world.textureStore.unit_orc;
        }
    }
}
