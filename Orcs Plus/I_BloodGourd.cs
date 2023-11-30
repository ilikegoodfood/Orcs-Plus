using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class I_BloodGourd : Item
    {
        public int timer = 3;

        public double deathNeed = 100.0;

        public I_BloodGourd(Map map)
            : base(map)
        {
            challenges.Add(new Rti_EatBloodGourd(map.locations[0], this));
        }

        public override string getName()
        {
            return "Gourd of Blood";
        }

        public override string getShortDesc()
        {
            return "The gourd of blood is a small red gourd that can only be found in the depths of the wilderness. Its flesh is closer to meat than plant matter and it is filled to bursting with blood of uncertain origins. It is occasionally harvested by orc tribes, a process that is extremely dangerous. The holder regains 1 hp every three turns, or they can consume the gourd to fully refill their health.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("OrcsPlus.Foreground_BloodGourd.png");
        }

        public override List<Ritual> getRituals(UA ua)
        {
            return challenges;
        }

        public override Sprite getIconBack()
        {
            return map.world.iconStore.standardBack;
        }

        public override void turnTick(Person owner)
        {
            timer--;

            if (owner.unit is UA ua)
            {
                if (timer <= 0)
                {
                    if (owner.unit.hp < owner.unit.maxHp)
                    {
                        owner.unit.hp++;
                    }
                    else
                    {
                        foreach (Minion minion in ua.minions)
                        {
                            if (minion != null && minion.hp < minion.getMaxHP())
                            {
                                minion.hp++;
                                break;
                            }
                        }
                    }
                }

                Pr_Death death = (Pr_Death)owner.unit.location.properties.FirstOrDefault(pr => pr is Pr_Death);
                if (death != null)
                {
                    double delta = Math.Min(5.0, death.charge);
                    Property.addToProperty("Fertilizer for Gourd of Blood", Property.standardProperties.DEATH, -delta, owner.unit.location);
                    deathNeed -= delta;
                }

                if (deathNeed <= 0.0)
                {
                    bool duplicated = false;

                    for (int i = 0; i < owner.items.Length; i++)
                    {
                        if (owner.items[i] == null)
                        {
                            owner.items[i] = new I_BloodGourd(map);
                            duplicated = true;
                            break;
                        }
                    }

                    if (duplicated)
                    {
                        owner.map.addUnifiedMessage(owner, null, "Gourd of Blood Propagated", owner.unit.getName() + "'s Gourd of Blood has, fattened by the presence of death, grown a second gourd. " + owner.unit.getName() + " now holds both gourds.", "Gourd Propagated");
                    }
                    else
                    {
                        string msgBody = owner.unit.getName() + "'s Gourd of Blood has, fattened by the presence of death, grown a second gourd. " + owner.unit.getName() + ", being unable to carry the second gourd, ";
                        
                        if (owner.unit.hp < owner.unit.maxHp)
                        {
                            msgBody += "ate it, regaining " + (owner.unit.maxHp - owner.unit.hp) + " hp.";
                            owner.unit.hp = owner.unit.maxHp;
                        }
                        else
                        {
                            msgBody += "discarded it.";
                        }

                        owner.map.addUnifiedMessage(owner, null, "Gourd of Blood Propagated", msgBody, "Gourd Propagated");
                    }
                }
            }

            if (deathNeed <= 0.0)
            {
                deathNeed = 100.0;
            }

            if (timer <= 0)
            {
                timer = 3;
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
