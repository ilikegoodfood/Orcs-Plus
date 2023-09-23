using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    internal class T_Grott : Trait
    {
        public int duration = 0;

        public T_Grott(int duration)
        {
            this.duration = duration;
        }

        public override string getName()
        {
            return "Orc Grott (" + duration + ")";
        }

        public override string getDesc()
        {
            return "Increases might and command stats by 1 while the effect lasts. " + duration + " turns remaining. Obtained by drinking orc grott at a seat of the elders, a great hall, or from a drinking horn.";
        }

        public override int getMaxLevel()
        {
            return 5;
        }

        public override void turnTick(Person p)
        {
            duration--;
            if (duration <= 0)
            {
                if (p.unit is UA ua)
                {
                    I_DrinkingHorn horn = (I_DrinkingHorn)p.items.FirstOrDefault(i => i is I_DrinkingHorn h && h.full);

                    if (horn != null)
                    {
                        if (ua.isCommandable() && !p.map.automatic)
                        {
                            p.traits.Remove(this);

                            List<EventManager.ActiveEvent> activeEvents = EventManager.events.Values.Where(e => e.data.id.Contains("drinkGrott") && e.type == EventData.Type.INERT).ToList();
                            EventManager.ActiveEvent orcEvent = activeEvents.FirstOrDefault(e => e.data.id.Contains("drinkGrott_Orc"));
                            EventManager.ActiveEvent otherEvent = activeEvents.FirstOrDefault(e => !e.data.id.Contains("drinkGrott_Orc"));
                            EventContext ctx = EventContext.withUnit(p.map, ua);

                            if (activeEvents.Count == 0)
                            {
                                p.map.world.prefabStore.popMsg("UNABLE TO FIND DRINK GROTT EVENT", true, true);
                            }
                            else
                            {
                                if (p.species == p.map.species_orc && orcEvent != null)
                                {
                                    p.map.world.prefabStore.popEvent(orcEvent.data, ctx, null, false);
                                }
                                else if (p.species != p.map.species_orc && otherEvent != null)
                                {
                                    p.map.world.prefabStore.popEvent(otherEvent.data, ctx, null, false);
                                }
                                else
                                {
                                    p.map.world.prefabStore.popMsg("UNABLE TO FIND DRINK GROTT EVENT", true, true);
                                }
                            }
                        }
                        else if (ua.getChallengeUtility(horn.rti_DrinkGrott, null) > 0.0)
                        {
                            duration = p.map.param.ch_primalWatersDur;
                            horn.full = false;

                            if (p.species != p.map.species_orc)
                            {
                                ua.hp -= 2;

                                if (ua.hp <= 0)
                                {
                                    ua.die(p.map, "Killed by an event outcome", null);
                                    p.map.addUnifiedMessage(ua, null, ua.getName() + " dies", ua.getName() + " has been killed", UnifiedMessage.messageType.AGENT_DIES, true);
                                }
                            }
                        }
                        else
                        {
                            p.traits.Remove(this);

                            dismissExcessMinions(ua);
                        }
                    }
                    else
                    {
                        p.traits.Remove(this);

                        if (p.unit.isCommandable())
                        {
                            p.map.addUnifiedMessage(p, null, "Orc Grott Ends", p.unit.getName() + " is no longer under the influence of Orc Grott", UnifiedMessage.messageType.EFFECT_ENDS, false);
                        }
                        else
                        {
                            dismissExcessMinions(ua);
                        }
                    }
                }
            }
        }

        public void dismissExcessMinions(UA ua)
        {
            while (ua.getStatCommandLimit() < ua.getCurrentlyUsedCommand())
            {
                int count = 0;
                int cost = 0;
                int indexToDismiss = -1;

                for (int i = 0; i < ua.minions.Length; i++)
                {
                    if (ua.minions[i] != null)
                    {
                        count++;

                        if (ua.minions[i].getCommandCost() > 0 && (cost == 0 || ua.minions[i].getCommandCost() < cost))
                        {
                            indexToDismiss = i;
                            cost = ua.minions[i].getCommandCost();
                        }
                    }
                }

                if (count == 0 || indexToDismiss == -1)
                {
                    break;
                }

                ua.minions[indexToDismiss].disband("Dismissed from service of " + ua.getName());
                ua.minions[indexToDismiss] = null;
            }
        }

        public override int getMightChange()
        {
            return 1;
        }

        public override int getCommandChange()
        {
            return 1;
        }
    }
}
