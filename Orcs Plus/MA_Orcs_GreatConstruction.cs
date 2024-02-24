using Assets.Code;
using CommunityLib;
using DuloGames.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class MA_Orcs_GreatConstruction : MonsterAction
    {
        public SG_Orc orcSociety;

        public HolyOrder_Orcs orcCulture;

        public Pr_Orcs_GreatConstruction greatConstruction = null;

        public Set_OrcCamp target = null;

        public int specialism = 0;

        public double campUtilValue = 5;

        public double specialisedCampUtilValue = -15;

        public double milThreatUtilBound = 30;

        public double mageCampHabUtilValue = 2;

        public MA_Orcs_GreatConstruction(SG_Orc orcs)
            : base(orcs.map)
        {
            orcSociety = orcs;
            ModCore.Get().data.orcSGCultureMap.TryGetValue(orcs, out orcCulture);
        }

        public override string getName()
        {
            return "Begin Great Construction";
        }

        public override string getShortDesc()
        {
            return "Begin a great construction effort in an unspecialised orc camp.";
        }

        public override Sprite getIconFore()
        {
            return map.world.iconStore.construction;
        }

        public override int getTurnsRequired()
        {
            return 8;
        }

        public override double getUtility(List<ReasonMsg> reasons)
        {
            // validate the current target
            if (target != null && (target.location.settlement != target || target.specialism != 0 || target.location.soc != orcSociety))
            {
                clearTarget();
            }
            // validate the great construction property
            else if (greatConstruction != null && !greatConstruction.location.properties.Contains(greatConstruction))
            {
                clearTarget();
            }

            // validate the specialism
            if (target != null && specialism != 0)
            {
                if (specialism == 2)
                {
                    if (!ModCore.GetComLib().checkHasLocus(target.location))
                    {
                        clearTarget();
                    }
                }
                else if (specialism == 3)
                {
                    if (!target.location.properties.Any(pr => pr is PrWM_Manticore || pr is PrWM_CagedManticore))
                    {
                        clearTarget();
                    }
                }
                else if (specialism == 5)
                {
                    if (!target.subs.Any(sub => sub is Sub_Shipwreck wreck && !wreck.isReinforced()))
                    {
                        clearTarget();
                    }
                }
                else if (specialism == 6)
                {
                    if (target.location.hex.z != 1 && !target.location.properties.Any(pr => pr is Pr_TunnelsAbove) && !target.location.getNeighbours().Any(n => n.hex.z == 1))
                    {
                        clearTarget();
                    }
                }
            }

            double utility = 0.0;
            double val = -10000.0;
            if (orcCulture == null)
            {
                reasons?.Add(new ReasonMsg("ERROR: No Orc Culture Found", val));
                return val;
            }

            val = -100.0;
            if (greatConstruction != null)
            {
                reasons?.Add(new ReasonMsg("Ongoing great construction (" + greatConstruction.location.getName() + ")", val));
                return val;
            }

            val = -1000.0;
            if (orcSociety.actionUnderway != this && orcCulture.plunderValue <= 10.0)
            {
                reasons?.Add(new ReasonMsg("Insufficient plunder", val));
                utility += val;
            }

            if (target != null)
            {
                if (specialism == 1)
                {
                    utility += getFortressUtility(target, reasons);
                }
                else if (specialism == 2)
                {
                    utility += getMageCampUtility(target, reasons);
                }
                else if (specialism == 3)
                {
                    utility += getMenagerieUtility(target, reasons);
                }
                else if (specialism == 5)
                {
                    utility += getShipyardUtility(target, reasons);
                }
                else if (specialism == 6)
                {
                    utility += getMinesUtility(target, reasons);
                }

                utility += getNeighbourCampUtility(target, reasons);

                return utility;
            }
            else
            {
                double targetUtility = 0.0;
                Set_OrcCamp targetCamp = null;
                int targetSpecialism = 0;
                List<ReasonMsg> targetReasonMsgs = null;

                int testSpecialism = 0;
                double testUtility = 0.0;
                List<ReasonMsg> testReasonMsgs = null;

                if (reasons != null)
                {
                    targetReasonMsgs = new List<ReasonMsg>();
                    testReasonMsgs = new List<ReasonMsg>();
                }

                int targetFortressCount = (int)Math.Ceiling((orcCulture.camps.Count - orcCulture.specializedCamps.Count) / 4.0);
                int fortressCount = orcCulture.specializedCamps.FindAll(c => c.specialism == 1).Count;
                bool needFortress = fortressCount < targetFortressCount;

                foreach (Set_OrcCamp camp in orcCulture.camps)
                {
                    if (camp.specialism != 0)
                    {
                        continue;
                    }

                    if (needFortress)
                    {
                        testSpecialism = 1;
                        testUtility = getFortressUtility(camp, testReasonMsgs);
                        testUtility += getNeighbourCampUtility(camp, testReasonMsgs);

                        if (testUtility > targetUtility)
                        {
                            targetCamp = camp;
                            targetSpecialism = testSpecialism;
                            targetReasonMsgs = testReasonMsgs;
                            targetUtility = testUtility;
                        }

                        testSpecialism = 0;
                        testUtility = 0.0;
                        testReasonMsgs = new List<ReasonMsg>();
                    }

                    if (ModCore.GetComLib().checkHasLocus(camp.location))
                    {
                        testSpecialism = 2;
                        testUtility = getMageCampUtility(camp, testReasonMsgs);
                        testUtility += getNeighbourCampUtility(camp, testReasonMsgs);

                        if (testUtility > targetUtility)
                        {
                            targetCamp = camp;
                            targetSpecialism = testSpecialism;
                            targetReasonMsgs = testReasonMsgs;
                            targetUtility = testUtility;
                        }

                        testSpecialism = 0;
                        testUtility = 0.0;
                        testReasonMsgs = new List<ReasonMsg>();
                    }

                    if (camp.location.properties.Any(pr => pr is PrWM_Manticore))
                    {
                        testSpecialism = 3;
                        testUtility = getMenagerieUtility(camp, testReasonMsgs);
                        testUtility += getNeighbourCampUtility(camp, targetReasonMsgs);

                        if (testUtility > targetUtility)
                        {
                            targetCamp = camp;
                            targetSpecialism = testSpecialism;
                            targetReasonMsgs = testReasonMsgs;
                            targetUtility = testUtility;
                        }

                        testSpecialism = 0;
                        testUtility = 0.0;
                        testReasonMsgs = new List<ReasonMsg>();
                    }

                    Sub_Shipwreck wreck = (Sub_Shipwreck)camp.subs.FirstOrDefault(sub => sub is Sub_Shipwreck);
                    if (camp.location.isCoastal && camp.location.getNeighbours().Any(l => l.isOcean) && wreck != null && !wreck.isReinforced())
                    {
                        double baseCost = 2 * map.param.ch_orc_buildFortressCostPerNeighbour;
                        int specilisedNeighbourCount = camp.location.getNeighbours().FindAll(l => l.settlement is Set_OrcCamp camp2 && camp2.specialism > 0).Count;
                        double cost = baseCost + (specilisedNeighbourCount * map.param.ch_orc_buildFortressCostPerNeighbour) - 10;

                        if (wreck.integrity > cost / 5.0)
                        {
                            testSpecialism = 5;
                            testUtility = getShipyardUtility(camp, testReasonMsgs);
                            testUtility += getNeighbourCampUtility(camp, testReasonMsgs);

                            if (testUtility > targetUtility)
                            {
                                targetCamp = camp;
                                targetSpecialism = testSpecialism;
                                targetReasonMsgs = testReasonMsgs;
                                targetUtility = testUtility;
                            }

                            testSpecialism = 0;
                            testUtility = 0.0;
                            testReasonMsgs = new List<ReasonMsg>();
                        }
                    }

                    if (orcSociety.canGoUnderground())
                    {
                        Pr_TunnelsAbove tunnelsAbove = (Pr_TunnelsAbove)camp.location.properties.FirstOrDefault(pr => pr is Pr_TunnelsAbove);
                        if (camp.location.hex.z == 1 || tunnelsAbove != null || camp.location.getNeighbours().Any(n => n.hex.z == 1))
                        {
                            testSpecialism = 6;
                            testUtility = getMinesUtility(camp, testReasonMsgs);
                            testUtility += getNeighbourCampUtility(camp, testReasonMsgs);

                            if (testUtility > targetUtility)
                            {
                                targetCamp = camp;
                                targetSpecialism = testSpecialism;
                                targetReasonMsgs = testReasonMsgs;
                                targetUtility = testUtility;
                            }

                            testSpecialism = 0;
                            testUtility = 0.0;
                            testReasonMsgs = new List<ReasonMsg>();
                        }
                    }
                }

                if (targetCamp != null && targetSpecialism > 0)
                {
                    target = targetCamp;
                    specialism = targetSpecialism;
                    utility += targetUtility;
                    reasons?.AddRange(targetReasonMsgs);
                    return utility;
                }
            }

            val = -1000.0;
            reasons?.Add(new ReasonMsg("No desire to specialise camps", val));
            utility += val;
            return utility;
        }

        public void clearTarget()
        {
            if (greatConstruction != null)
            {
                greatConstruction.location.properties.Remove(greatConstruction);
                greatConstruction = null;
            }

            target = null;
            specialism = 0;

            if (orcSociety.actionUnderway == this)
            {
                orcSociety.actionUnderway = null;
            }
        }

        public double getNeighbourCampUtility(Set_OrcCamp camp, List<ReasonMsg> reasonMsgs)
        {
            double utility = 0.0;

            int neighbouringCamps = 0;
            int neighbouringSpecialisedCamps = 0;
            foreach (Location neighbour in camp.location.getNeighbours())
            {
                if (neighbour.soc == orcSociety && neighbour.settlement is Set_OrcCamp camp2)
                {
                    neighbouringCamps++;

                    if (camp2.specialism > 0)
                    {
                        neighbouringSpecialisedCamps++;
                    }
                }
            }

            double val = 0.0;
            if (neighbouringCamps > 0 && specialism != 6)
            {
                val = campUtilValue * neighbouringCamps;
                reasonMsgs?.Add(new ReasonMsg("Neighbouring orc camps", val));
                utility += val;
            }

            if (neighbouringSpecialisedCamps > 0)
            {
                val = specialisedCampUtilValue * neighbouringSpecialisedCamps;
                reasonMsgs?.Add(new ReasonMsg("Neighbouring specialised orc camps", val));
                utility += val;
            }

            return utility;
        }

        public double getFortressUtility(Set_OrcCamp camp, List<ReasonMsg> reasonMsgs)
        {
            double utility = 0.0;

            int unownedNeighbourCount = 0;
            HashSet<SocialGroup> groups = new HashSet<SocialGroup>();
            double milThreat = 0.0;

            foreach(Location neighbour in camp.location.getNeighbours())
            {
                if (neighbour.soc != orcSociety)
                {
                    unownedNeighbourCount++;

                    if (neighbour.soc != null && !groups.Contains(neighbour.soc))
                    {
                        groups.Add(neighbour.soc);

                        if (ModCore.Get().isHostileAlignment(orcSociety, neighbour))
                        {
                            double milStrengthFactor = orcSociety.currentMilitary / neighbour.soc.currentMilitary;

                            if (milStrengthFactor < 1.0)
                            {
                                milThreat += milThreatUtilBound - (milThreatUtilBound * milStrengthFactor);
                            }
                        }
                    }
                }
            }

            if (unownedNeighbourCount > 0)
            {
                double val = 2 * unownedNeighbourCount;
                reasonMsgs?.Add(new ReasonMsg("Boarder territory", val));
                utility += val;
            }

            if (milThreat > 0)
            {
                reasonMsgs?.Add(new ReasonMsg("Military threat", milThreat));
                utility += milThreat;
            }

            return utility;
        }

        public double getMageCampUtility(Set_OrcCamp camp, List<ReasonMsg> reasonMsgs)
        {
            double utility = 100.0;
            reasonMsgs?.Add(new ReasonMsg("Base", 100.0));

            int uninhabitableNeighbourCount = 0;

            foreach (Location neighbour in camp.location.getNeighbours())
            {
                if (neighbour.hex.getHabilitability() < map.param.orc_habRequirement * map.opt_orcHabMult && !neighbour.properties.Any(pr => pr is Pr_Vinerva_Life))
                {
                    uninhabitableNeighbourCount++;
                }
            }

            if (uninhabitableNeighbourCount > 0)
            {
                double val = (1 - orcCulture.tenet_expansionism.status) * uninhabitableNeighbourCount * mageCampHabUtilValue;
                reasonMsgs?.Add(new ReasonMsg("New lands become habitable", val));
            }

            return utility;
        }

        public double getMenagerieUtility(Set_OrcCamp camp, List<ReasonMsg> reasonMsgs)
        {
            double utility = 80.0;
            reasonMsgs?.Add(new ReasonMsg("Base", 80.0));

            return utility;
        }

        public double getShipyardUtility(Set_OrcCamp camp, List<ReasonMsg> reasonMsgs)
        {
            double utility = 90.0;
            reasonMsgs?.Add(new ReasonMsg("Base", 90.0));

            return utility;
        }

        public double getMinesUtility(Set_OrcCamp camp, List<ReasonMsg> reasonMsgs)
        {
            double utility = -1000.0;
            if (!orcSociety.canGoUnderground())
            {
                reasonMsgs?.Add(new ReasonMsg("Orcs are not aware of the underground", utility));
                return utility;
            }

            int mineCount = orcCulture.specializedCamps.FindAll(c => c.specialism == 6).Count;
            utility = 40.0 - (10.0 * mineCount);

            reasonMsgs?.Add(new ReasonMsg("Base", 80.0));
            reasonMsgs?.Add(new ReasonMsg("Existing mines", -10.0 * mineCount));

            return utility;
        }

        public override void complete()
        {
            if (orcCulture.plunderValue >= 10.0)
            {
                orcCulture.spendGold(10.0);
                greatConstruction = new Pr_Orcs_GreatConstruction(target, orcSociety, specialism);
                target.location.properties.Add(greatConstruction);

                map.addUnifiedMessage(orcSociety, target.location, "Great Construction", "The orcs of the " + orcSociety.getName() + " have begun a great construction project in " + target.getName(), "Great Construction");
            }
        }
    }
}
