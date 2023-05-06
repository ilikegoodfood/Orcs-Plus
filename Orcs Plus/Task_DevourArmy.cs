using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Task_DevourArmy : Task_AttackArmy
    {
        double deathGainFactor;
        double healthGainFactor;

        public Task_DevourArmy(UM other, UM self, double deathGainFactor = 2, double healthGainFactor = 2)
            : base(other, self)
        {
            this.deathGainFactor = deathGainFactor;
            this.healthGainFactor = healthGainFactor;
        }

        public override string getShort()
        {
            if (other is UM_Refugees || other is UM_LuredCrowd)
            {
                return "Going to Devour " + other.getName() + " (" + turnsLeft + ")";
            }

            if (other.task is Task_InBattle)
            {
                return "Going to Ambush " + other.getName() + " (" + turnsLeft + ")";
            }

            return "Going to Attack " + other.getName() + " (" + turnsLeft + ")";
        }

        public override string getLong()
        {
            if (other is UM_Refugees || other is UM_LuredCrowd)
            {
                return "This army is off to Devour [" + other.getName() + "] at " + other.location.getName() + " (" + turnsLeft + " turns left )";
            }

            if (other.task is Task_InBattle)
            {
                return "This army is off to Ambush [" + other.getName() + "] at " + other.location.getName() + " (" + turnsLeft + " turns left )";
            }

            return "This army is off to Attack [" + other.getName() + "] at " + other.location.getName() + " (" + turnsLeft + " turns left )";
        }

        public override void turnTick(Unit unit)
        {
            if (other.isDead)
            {
                unit.task = null;
                return;
            }

            turnsLeft--;
            if (turnsLeft < 0)
            {
                unit.task = null;
                return;
            }

            if (other.location == unit.location)
            {
                if (unit is UM um)
                {
                    Task_InBattle battleTask = other.task as Task_InBattle;
                    BattleArmy battle;

                    if (battleTask == null)
                    {
                        battle = new BattleArmy(um, other);
                        um.task = new Task_InBattle(battle);
                        other.task = new Task_InBattle(battle);
                        joinBattle(um, battle);
                        return;
                    }
                    else
                    {
                        battle = battleTask.battle;
                    }

                    if (battle.attackers.Contains(um) || battle.defenders.Contains(um))
                    {
                        um.task = new Task_InBattle(battle);
                        joinBattle(um, battle);
                        return;
                    }

                    if (battle.attackers.Contains(other))
                    {
                        if (um.society is Society society && (society.isDarkEmpire || society.isOphanimControlled))
                        {
                            foreach(UM um2 in battle.attackers)
                            {
                                if (um2.society is Society society2 && (society2.isDarkEmpire || society2.isOphanimControlled))
                                {
                                    return;
                                }
                            }
                        }

                        battle.defenders.Add(um);
                        um.task = new Task_InBattle(battle);
                        joinBattle(um, battle);
                        return;
                    }

                    if (battle.defenders.Contains(other))
                    {
                        if (um.society is Society society && (society.isDarkEmpire || society.isOphanimControlled))
                        {
                            foreach (UM um2 in battle.defenders)
                            {
                                if (um2.society is Society society2 && (society2.isDarkEmpire || society2.isOphanimControlled))
                                {
                                    return;
                                }
                            }
                        }

                        battle.attackers.Add(um);
                        um.task = new Task_InBattle(battle);
                        joinBattle(um, battle);
                        return;
                    }
                }
                else
                {
                    unit.task = null;
                }
            }
            else
            {
                bool moved = unit.map.moveTowards(unit, other.location);
                if (!moved)
                {
                    World.log("Move Unsuccessful");
                    unit.task = null;
                    return;
                }

                if (other.location == unit.location)
                {
                    if (unit is UM um)
                    {
                        Task_InBattle battleTask = other.task as Task_InBattle;
                        BattleArmy battle;

                        if (battleTask == null)
                        {
                            battle = new BattleArmy(um, other);
                            um.task = new Task_InBattle(battle);
                            other.task = new Task_InBattle(battle);
                            joinBattle(um, battle);
                            return;
                        }
                        else
                        {
                            battle = battleTask.battle;
                        }

                        if (battle.attackers.Contains(um) || battle.defenders.Contains(um))
                        {
                            um.task = new Task_InBattle(battle);
                            joinBattle(um, battle);
                            return;
                        }

                        if (battle.attackers.Contains(other))
                        {
                            if (um.society is Society society && (society.isDarkEmpire || society.isOphanimControlled))
                            {
                                foreach (UM um2 in battle.attackers)
                                {
                                    if (um2.society is Society society2 && (society2.isDarkEmpire || society2.isOphanimControlled))
                                    {
                                        return;
                                    }
                                }
                            }

                            battle.defenders.Add(um);
                            um.task = new Task_InBattle(battle);
                            joinBattle(um, battle);
                            return;
                        }

                        if (battle.defenders.Contains(other))
                        {
                            if (um.society is Society society && (society.isDarkEmpire || society.isOphanimControlled))
                            {
                                foreach (UM um2 in battle.defenders)
                                {
                                    if (um2.society is Society society2 && (society2.isDarkEmpire || society2.isOphanimControlled))
                                    {
                                        return;
                                    }
                                }
                            }

                            battle.attackers.Add(um);
                            um.task = new Task_InBattle(battle);
                            joinBattle(um, battle);
                            return;
                        }
                    }
                    else
                    {
                        unit.task = null;
                    }
                }
            }
        }

        public void joinBattle(UM um, BattleArmy battle)
        {
            double advantage = battle.computeAdvantage();
            double damage = um.map.param.war_battleLethalityBaseline;

            if (battle.attackers.Contains(um))
            {
                if (advantage > 0.0)
                {
                    damage *= 1.0 + advantage;
                }

                if (battle.attEffect != null)
                {
                    damage *= 1.0 + battle.attEffect.getLethalityDelta();
                }

                foreach (UM enemy in battle.defenders)
                {
                    if (enemy is UM_Refugees || enemy is UM_LuredCrowd)
                    {
                        um.hp += (int)Math.Ceiling(enemy.hp * healthGainFactor);
                        Property.addToProperty("Civilians Devoured by " + um.getName(), Property.standardProperties.DEATH, enemy.hp * deathGainFactor, um.location);

                        battle.bufferedMessages.Add(string.Concat(new string[]
                        {
                                "<color=#aaaaaaff>",
                                um.getName(),
                                " devoured ",
                                enemy.getName(),
                                ", destroying them, and gaining ",
                                ((int)Math.Ceiling(enemy.hp * healthGainFactor)).ToString(),
                                "health </color>"
                        }));

                        enemy.hp = 0;
                        enemy.die(um.map, "Destroyed in battle", null);
                        continue;
                    }

                    double damageTarget = damage * um.getDamageMultiplier(enemy, battle);

                    int damageFinal = (int)Math.Ceiling(enemy.hp * damageTarget);

                    battle.bufferedMessages.Add(string.Concat(new string[]
                    {
                            "<color=#aaaaaaff>",
                            um.getName(),
                            " ambushes ",
                            enemy.getName(),
                            " for ",
                            damageFinal.ToString(),
                            "</color>"
                    }));

                    if (enemy.isCommandable())
                    {
                        um.map.addUnifiedMessage(um, um.location, "Battle", string.Concat(new string[]
                        {
                            enemy.getName(),
                            " was ambushed by ",
                            um.getName(),
                            " for ",
                            damageFinal.ToString()
                        }), UnifiedMessage.messageType.BATTLE, false);
                    }

                    enemy.hp -= damageFinal;
                    if (enemy is UM_HumanArmy || enemy is UM_OrcArmy)
                    {
                        Property.addToProperty("Battle", Property.standardProperties.DEATH, (double)(damageFinal / 2), enemy.location);
                    }

                    if (enemy.hp <= 0)
                    {
                        enemy.die(um.map, "Destroyed in battle", null);
                    }
                }

                if (um.isCommandable())
                {
                    um.map.addUnifiedMessage(um, um.location, "Battle", string.Concat(new string[]
                    {
                            um.getName(),
                            " ambushed defenders in battle at ",
                            um.location.getName()
                    }), UnifiedMessage.messageType.BATTLE, false);
                }
            }
            else if (battle.defenders.Contains(um))
            {
                if (battle.defEffect != null)
                {
                    damage *= 1.0 + battle.defEffect.getLethalityDelta();
                }

                foreach (UM enemy in battle.attackers)
                {
                    if (enemy is UM_Refugees || enemy is UM_LuredCrowd)
                    {
                        um.hp += (int)Math.Ceiling(enemy.hp * healthGainFactor);
                        Property.addToProperty("Civilians Devoured by " + um.getName(), Property.standardProperties.DEATH, enemy.hp * deathGainFactor, um.location);

                        battle.messages.Add(string.Concat(new string[]
                        {
                                "<color=#aaaaaaff>",
                                um.getName(),
                                " devoured ",
                                enemy.getName(),
                                ", destroying them, and gaining ",
                                ((int)Math.Ceiling(enemy.hp * healthGainFactor)).ToString(),
                                "health </color>"
                        }));

                        enemy.hp = 0;
                        enemy.die(um.map, "Destroyed in battle", null);
                        continue;
                    }

                    double damageTarget = damage * um.getDamageMultiplier(enemy, battle);

                    int damageFinal = (int)Math.Ceiling(enemy.hp * damageTarget);

                    battle.bufferedMessages.Add(string.Concat(new string[]
                    {
                            "<color=#aaaaaaff>",
                            um.getName(),
                            " ambushes ",
                            enemy.getName(),
                            " for ",
                            damageFinal.ToString(),
                            "</color>"
                    }));

                    if (enemy.isCommandable())
                    {
                        um.map.addUnifiedMessage(um, um.location, "Battle", string.Concat(new string[]
                        {
                            enemy.getName(),
                            " was ambushed by ",
                            um.getName(),
                            " for ",
                            damageFinal.ToString()
                        }), UnifiedMessage.messageType.BATTLE, false);
                    }

                    enemy.hp -= damageFinal;
                    if (enemy is UM_HumanArmy || enemy is UM_OrcArmy)
                    {
                        Property.addToProperty("Battle", Property.standardProperties.DEATH, (double)(damageFinal / 2), enemy.location);
                    }

                    if (enemy.hp <= 0)
                    {
                        enemy.die(um.map, "Destroyed in battle", null);
                    }
                }

                if (um.isCommandable())
                {
                    um.map.addUnifiedMessage(um, um.location, "Battle", string.Concat(new string[]
                    {
                            um.getName(),
                            " ambushed defenders in battle at ",
                            um.location.getName()
                    }), UnifiedMessage.messageType.BATTLE, false);
                }
            }
        }
    }
}
