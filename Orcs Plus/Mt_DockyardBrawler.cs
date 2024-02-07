using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Mt_DockyardBrawler : MinionTrait
    {
        public int timer = 5;

        public override string getName()
        {
            return "Dockyard Brawler";
        }

        public override string getDesc()
        {
            return "Every 5 turns if in a city with a dock, this minion will start a fight in the docks, briefly disrupting a random agent that is performing a challenge in the city. This action will generate 5 menace, but will not increase your minimum menace.";
        }

        public override void turnTick(UA ua, Minion m)
        {
            timer--;

            if (timer <= 0)
            {
                if (ua.location != null && ua.location.settlement is SettlementHuman && ua.location.settlement.subs.Any(sub => sub is Sub_Docks))
                {
                    UA target = null;
                    List<UA> targets = new List<UA>();
                    foreach (Unit unit in ua.location.units)
                    {
                        if (unit != ua && unit is UA targetUA && targetUA.task is Task_PerformChallenge tPerformChallenge && !(tPerformChallenge.challenge is Ch_LayLow) && !(tPerformChallenge.challenge is Ch_LayLowWilderness) && !(tPerformChallenge.challenge is Ch_FleeBeneathTheWaves))
                        {
                            if (ua.isCommandable() || (ua.society != null && ua.society.isDark()))
                            {
                                if (!targetUA.isCommandable())
                                {
                                    targets.Add(targetUA);
                                }
                            }
                            else if (!ua.isCommandable() && (ua.society == null || !ua.society.isDark()))
                            {
                                if (targetUA.isCommandable() || targetUA.society == null || targetUA.society.isDark() || (ua.society != null && targetUA.society != null && ua.society != targetUA.society && (ua.society.getRel(targetUA.society).state == DipRel.dipState.hostile || ua.society.getRel(targetUA.society).state == DipRel.dipState.war)))
                                {
                                    targets.Add(targetUA);
                                }
                            }
                        }
                    }

                    if (targets.Count > 0)
                    {
                        target = targets[0];

                        if (targets.Count > 1)
                        {
                            target = targets[Eleven.random.Next(targets.Count)];
                        }
                    }

                    if (target != null)
                    {
                        ua.map.addUnifiedMessage(ua, target, "Dockyard Brawl", "An orc corsair in the retinue of " + ua.getName() + " has started a brawl in the docks in " + ua.location.settlement.getName() + ". " + target.getName() + " was forced to abandon their efforts to " + ((target.task as Task_PerformChallenge)?.challenge.getName() ?? "ERROR: Invalid Task") + " in an attempt to break up the fight.", "Dockyard Brawl");
                        target.task = new Task_Disrupted(1);
                        ua.inner_menace += 5;
                    }
                }

                timer = 5;
            }
        }
    }
}
