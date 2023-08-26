using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class UM_PerfectHorde : UM_OrcArmy
    {
        public double strengthFactor = 1.4;

        public double goldGainFactor = 2.0;

        public UM_PerfectHorde(Location location, SocialGroup sg, Set_OrcCamp camp)
            : base(location, sg, camp)
        {

        }

        public override Sprite getPortraitForeground()
        {
            return EventManager.getImg("OrcsPlus.Icon_PerfectHorde.png");
        }

        public override string getName()
        {
            return "Perfect Horde";
        }

        public override void turnTickInner(Map map)
        {
            updateMaxHP();

            if (task is Task_RazeLocation && location.settlement is SettlementHuman settlementHuman && society is SG_Orc orcSociety && orcSociety.capital != -1 && map.locations[orcSociety.capital].soc == orcSociety && map.locations[orcSociety.capital].settlement is Set_OrcCamp capitalCamp)
            {
                Pr_OrcPlunder pr_OrcPlunder = capitalCamp.location.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();

                if (pr_OrcPlunder == null)
                {
                    pr_OrcPlunder = new Pr_OrcPlunder(capitalCamp.location);
                    capitalCamp.location.properties.Add(pr_OrcPlunder);
                }

                int gold = (int)(map.param.ch_orcRazeGoldGainArmy * settlementHuman.prosperity * settlementHuman.population * goldGainFactor);

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

        new void updateMaxHP()
        {
            if (homeLocation != -1 && map.locations[homeLocation].soc is SG_Orc)
            {
                Pr_OrcishIndustry industry = map.locations[homeLocation].properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();
                double totalIndustry = 0;

                if (industry != null)
                {
                    totalIndustry += industry.charge;
                }

                foreach (Location neighbour in map.locations[this.homeLocation].getNeighbours())
                {
                    if (neighbour.soc == society)
                    {
                        industry = neighbour.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();

                        if (industry != null)
                        {
                            totalIndustry += industry.charge;
                        }
                    }
                }

                Pr_Ophanim_Perfection perfection = map.locations[this.homeLocation].properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
                double perfectionMult = 1.0;
                if (perfection != null)
                {
                    perfectionMult += (perfection.charge / 1200);
                }

                maxHp = (int)(totalIndustry * perfectionMult * map.param.minor_orcMilitaryScaling * map.difficultyMult_shrinkWithDifficulty * map.opt_orcStrMult * strengthFactor);

                if (maxHp < 10 * strengthFactor)
                {
                    maxHp = (int)Math.Ceiling(10 * strengthFactor);
                }
            }
            else
            {
                maxHp = (int)Math.Ceiling(10 * strengthFactor);
            }
        }

        public override void inBattle(BattleArmy battleArmy)
        {
            if (battleArmy.attackers.Contains(this))
            {
                (battleArmy.attEffect = new BE_OrcPerfection(battleArmy)).begin();
            }
            else
            {
                (battleArmy.defEffect = new BE_OrcPerfection(battleArmy)).begin();
            }
        }
    }
}
