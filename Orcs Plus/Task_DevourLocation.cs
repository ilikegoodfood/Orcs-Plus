using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Task_DevourLocation : Assets.Code.Task
    {
        public bool ignorePeace = false;

        public double deathGainFactor;
        public double healthGainFactor;

        public Task_DevourLocation(double deathGainFactor = 2, double healthGainFactor = 2)
        {
            this.deathGainFactor = deathGainFactor;
            this.healthGainFactor = healthGainFactor;
        }

        public override string getShort()
        {
            return "Devouring population";
        }

        public override string getLong()
        {
            return "This army is devouring the population of this location.";
        }

        public override void turnTick(Unit unit)
        {
            if (!ignorePeace)
            {
                if (unit.location.soc != null && unit.society.getRel(unit.location.soc).state != DipRel.dipState.war)
                {
                    unit.task = null;
                    return;
                }
            }

            UM um = unit as UM;
            if (um != null)
            {
                if (um.location.settlement != null)
                {
                    unit.map.world.prefabStore.particleCombat(unit.location.hex, unit.location.hex);
                    Property.addToProperty("Military Action", Property.standardProperties.DEVASTATION, 12.0, um.location);
                    um.location.settlement.defences -= (double)(um.hp / 10 + 1);

                    SettlementHuman settlementHuman = unit.location.settlement as SettlementHuman;
                    if (settlementHuman != null)
                    {
                        int kills = (int)Math.Max(settlementHuman.population * 0.1, 2);
                        settlementHuman.population -= kills;
                        Property.addToProperty("Population Devoured by " + um.getName(), Property.standardProperties.DEATH, kills * deathGainFactor, um.location);
                        unit.hp += (int)Math.Ceiling(kills * healthGainFactor);
                    }

                    if (um.location.settlement.defences <= 0.0 || um.society == unit.location.soc || (settlementHuman != null && settlementHuman.population <= 0))
                    {
                        um.location.settlement.defences = 0.0;

                        bool attackerIsElder = !(um.society is Society);
                        if (!attackerIsElder)
                        {
                            if (um.society == um.map.soc_dark)
                            {
                                attackerIsElder = true;
                            }
                            else if (um.society is Society society && (society.isDarkEmpire || society.isOphanimControlled))
                            {
                                attackerIsElder = true;
                            }
                        }

                        if (attackerIsElder)
                        {
                            if (um.location.settlement is SettlementHuman && um.map.burnInComplete)
                            {
                                um.map.hintSystem.popHint(HintSystem.hintType.SOCIALGROUP_MENACE);
                                um.society.menace += um.map.param.society_menaceGainFromRaze * um.map.difficultyMult_growWithDifficulty;
                            }

                            um.addMenace(um.map.param.um_menaceGainFromRaze * um.map.difficultyMult_growWithDifficulty);
                        }

                        bool victimIsElder = um.location.soc != null && !(um.location.soc is Society);
                        if (!victimIsElder)
                        {
                            if (unit.location.soc == um.map.soc_dark)
                            {
                                victimIsElder = true;
                            }
                            else if (um.location.soc is Society society && (society.isDarkEmpire || society.isOphanimControlled))
                            {
                                victimIsElder = true;
                            }
                        }

                        if (victimIsElder && um.location.soc != null)
                        {
                            um.location.soc.menace -= (double)um.map.param.um_menaceLostFromRaze;
                            if (um.location.soc.menace < 0.0)
                            {
                                um.location.soc.menace = 0.0;
                            }
                        }

                        if (settlementHuman != null && um.map.opt_allowRefugees)
                        {
                            int pop = settlementHuman.population / 2;
                            if (pop > 0 && um.location.soc is Society society)
                            {
                                Property.addToProperty("Refugees Devoured by " + um.getName(), Property.standardProperties.DEATH, pop * deathGainFactor, um.location);
                                unit.hp += (int)Math.Ceiling(pop * healthGainFactor);
                            }
                        }

                        um.location.soc = null;
                        um.task = null;
                        um.location.settlement.fallIntoRuin("Devoured by " + um.getName(), um);
                    }
                }
                else
                {
                    um.task = null;
                }
            }
            else
            {
                unit.task = null;
            }
        }
    }
}
