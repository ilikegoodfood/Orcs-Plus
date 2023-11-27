using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class MA_Orc_HireMercenaries : MonsterAction
    {
        public SG_Orc orcSociety;

        public SocialGroup target;

        public int costA = 250;

        public int costB = 150;

        public double armySizeA = 75.0;

        public double armySizeB = 125.0;

        public MA_Orc_HireMercenaries(SG_Orc orcSociety, SocialGroup target)
            : base (orcSociety.map)
        {
            this.orcSociety = orcSociety;
            this.target = target;
        }

        public override string getName()
        {
            return "Hire Mercinaries from " + target.getName();
        }

        public override string getShortDesc()
        {
            return "Hires a mercinary army from " + target.getName();
        }

        public override Sprite getIconFore()
        {
            return map.world.iconStore.muster;
        }

        public override int getTurnsRequired()
        {
            return 10;
        }

        public override double getUtility(List<ReasonMsg> reasons)
        {
            double utility = -50.0;
            reasons?.Add(new ReasonMsg("Base Reluctance", -50.0));

            if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_MammonClient client && client.status < 0)
            {
                int cost = costA;
                if (client.status < -1)
                {
                    cost = costB;
                }

                if (cost > orcCulture.plunderValue)
                {
                    double val = orcCulture.plunderValue - cost;
                    reasons?.Add(new ReasonMsg("Too Expensive", val));
                    utility += val;
                }
                else
                {
                    if (orcSociety.isAtWar())
                    {
                        double val = 50.0;
                        reasons?.Add(new ReasonMsg("At War", val));
                        utility += val;

                        double orcMight = orcSociety.currentMilitary + orcCulture.currentMilitary;
                        double enemyMight = 0.0;
                        int warCount = 0;

                        foreach(KeyValuePair<SocialGroup, DipRel> pair in orcSociety.relations)
                        {
                            if (pair.Value.state == DipRel.dipState.war)
                            {
                                warCount++;
                                enemyMight += pair.Key.currentMilitary;
                            }
                        }

                        if (warCount > 1)
                        {
                            val = 10 * (warCount - 1);
                            reasons?.Add(new ReasonMsg("Multiple wars", val));
                            utility += val;
                        }

                        if (orcMight - 10 > enemyMight)
                        {
                            val = enemyMight - (orcMight - 10);
                            reasons?.Add(new ReasonMsg("Superior Military", val));
                            utility += val;
                        }
                        else if (orcMight - 10 < enemyMight)
                        {
                            val = 2 * (enemyMight - (orcMight - 10));
                            reasons?.Add(new ReasonMsg("Inferior Military", val));
                            utility += val;
                        }

                        if (orcCulture.plunderValue >= 3 * cost)
                        {
                            val = ((orcCulture.plunderValue - (cost * 2)) / cost) * 20;
                            reasons?.Add(new ReasonMsg("Excess Gold", val));
                            utility += val;
                        }
                    }

                    if (target is Society society)
                    {
                        if (society.capital != -1 && map.locations[society.capital].settlement is SettlementHuman settlementHuman)
                        {
                            double val = 100 * (settlementHuman.prosperity - 1.0);
                            if (val > 0.0)
                            {
                                reasons?.Add(new ReasonMsg("Prosperous Capital", val));
                                utility += val;
                            }
                            else if (val < 0.0)
                            {
                                val /= 3;
                                reasons?.Add(new ReasonMsg("Struggling Capital", val));
                                utility += val;
                            }

                            if (orcCulture.tenet_alignment.status < 0)
                            {
                                if (society.isDarkEmpire || society.isOphanimControlled)
                                {
                                    val = 20.0;
                                    reasons?.Add(new ReasonMsg("Elder's Support", val));
                                    utility += val;
                                }
                            }
                            else if (orcCulture.tenet_alignment.status > 0)
                            {
                                if (society.isDarkEmpire || society.isOphanimControlled)
                                {
                                    val = -50.0;
                                    reasons?.Add(new ReasonMsg("Elder will not support an orc culture that is human aligned", val));
                                    utility += val;
                                }
                            }

                            Person nationRuler = settlementHuman.ruler;

                            if (nationRuler != null)
                            {
                                if (nationRuler.hates.Contains(Tags.GOLD) || nationRuler.extremeHates.Contains(Tags.GOLD) || nationRuler.hates.Contains(Tags.ORC) || nationRuler.extremeHates.Contains(Tags.ORC))
                                {
                                    val = -20.0;
                                    reasons?.Add(new ReasonMsg("Ruler dislikes gold", val));
                                    utility += val;
                                }
                                else
                                {
                                    if (nationRuler.likes.Contains(Tags.GOLD))
                                    {
                                        val = -5.0;
                                        reasons?.Add(new ReasonMsg("Ruler is greedy", val));
                                        utility += val;
                                    }
                                    else if (nationRuler.extremeLikes.Contains(Tags.GOLD))
                                    {
                                        val = -10.0;
                                        reasons?.Add(new ReasonMsg("Ruler is greedy", val));
                                        utility += val;
                                    }
                                }

                                if (nationRuler.hates.Contains(Tags.GOLD) || nationRuler.extremeHates.Contains(Tags.GOLD) || nationRuler.hates.Contains(Tags.ORC) || nationRuler.extremeHates.Contains(Tags.ORC))
                                {
                                    val = -20.0;
                                    reasons?.Add(new ReasonMsg("Ruler dislikes orcs", val));
                                    utility += val;
                                }
                                else
                                {
                                    if (nationRuler.likes.Contains(Tags.ORC))
                                    {
                                        val = 10.0;
                                        reasons?.Add(new ReasonMsg("Ruler likes orcs", val));
                                        utility += val;
                                    }
                                    else if (nationRuler.extremeLikes.Contains(Tags.ORC))
                                    {
                                        val = 20.0;
                                        reasons?.Add(new ReasonMsg("Ruler likes orcs", val));
                                        utility += val;
                                    }
                                }
                            }
                        }
                    }
                    else if (target is SG_Orc orcSociety2)
                    {
                        if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
                        {
                            if (orcCulture.tenet_alignment.status < 0 && orcCulture2.tenet_alignment.status > 0)
                            {
                                double val = -50.0;
                                reasons?.Add(new ReasonMsg("Elder aligned orc culture will not support an orc culture that is human aligned", val));
                                utility += val;
                            }

                            if (orcCulture.tenet_alignment.status > 0 && orcCulture2.tenet_alignment.status < 0)
                            {
                                double val = -50.0;
                                reasons?.Add(new ReasonMsg("Human aligned orc culture will not support an orc culture that is elder aligned", val));
                                utility += val;
                            }

                            if (orcCulture2.ophanim_PerfectSociety)
                            {
                                double val = 20.0;
                                reasons?.Add(new ReasonMsg("Perfect Horde", val));
                                utility += val;
                            }
                        }
                    }
                }
            }
            else
            {
                reasons?.Add(new ReasonMsg("Orc Culture is not a client of Mammon", -10000.0));
                utility -= 10000.0;
            }

            return utility;
        }

        public override void complete()
        {
            if (orcSociety.getRel(target).state == DipRel.dipState.war || orcSociety.capital == -1)
            {
                return;
            }

            if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_MammonClient client && client.status < 0)
            {
                int cost = costA;
                double armySize = armySizeA;
                int duration = 25;
                if (client.status < -1)
                {
                    cost = costB;
                    armySize = armySizeB;
                }

                if (orcCulture.spendGold(cost))
                {
                    if (target is Society society && society.capital != -1 && map.locations[society.capital].settlement is SettlementHuman settlementHuman)
                    {
                        Person nationRuler = settlementHuman.ruler;

                        if (nationRuler != null)
                        {
                            if (nationRuler.hates.Contains(Tags.GOLD) || nationRuler.extremeHates.Contains(Tags.GOLD) || nationRuler.hates.Contains(Tags.ORC) || nationRuler.extremeHates.Contains(Tags.ORC))
                            {
                                armySize -= 25.0;
                            }
                            else
                            {
                                if (nationRuler.likes.Contains(Tags.GOLD))
                                {
                                    armySize -= 5.0;
                                }
                                else if (nationRuler.extremeLikes.Contains(Tags.GOLD))
                                {
                                    armySize -= 10.0;
                                }

                                if (nationRuler.likes.Contains(Tags.ORC))
                                {
                                    armySize += 25.0;
                                    duration += 5;
                                }
                                else if (nationRuler.extremeLikes.Contains(Tags.ORC))
                                {
                                    armySize += 50.0;
                                    duration += 10;
                                }
                            }
                        }

                        if (orcCulture.tenet_alignment.status < 1)
                        {
                            if (society.isDarkEmpire || society.isOphanimControlled)
                            {
                                armySize += 25;
                                duration += 5;
                            }
                        }
                        else
                        {
                            if (society.isDarkEmpire || society.isOphanimControlled)
                            {
                                armySize -= 25;
                            }
                        }

                        armySize *= settlementHuman.prosperity;
                    }
                    else if (target is SG_Orc orcSociety2 && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
                    {
                        if (orcCulture.tenet_alignment.status < 0 && orcCulture2.tenet_alignment.status > 0)
                        {
                            armySize -= 25;
                        }
                        else if (orcCulture.tenet_alignment.status > 0 && orcCulture2.tenet_alignment.status < 0)
                        {
                            armySize -= 25;
                        }

                        if (orcCulture2.ophanim_PerfectSociety)
                        {
                            armySize += 25;
                            duration += 5;
                        }
                    }

                    Location spawnLocation = target.getCapitalHex().location;
                    UM_Mercenary mercs = new UM_Mercenary(spawnLocation, target, orcSociety, Math.Max((int)Math.Floor(armySize), 10), duration);
                    map.units.Add(mercs);
                    spawnLocation.units.Add(mercs);
                }
            }
        }
    }
}
