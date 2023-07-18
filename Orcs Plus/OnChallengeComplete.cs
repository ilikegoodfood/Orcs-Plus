using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public static class OnChallengeComplete
    {
        public static void Ch_Orcs_BuildFortress(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        public static void Ch_Orcs_BuildMages(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        public static void Ch_Orcs_BuildMenagerie(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildMenagerie]), true);
            }
        }

        public static void Ch_Orcs_BuildShipyard(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildShipyard]), true);
            }
        }

        public static void Rt_Orcs_ClaimTerritory(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.location.settlement is Set_OrcCamp camp && camp.subs.Count > 0)
            {
                List<Sub_OrcWaystation> waystations = new List<Sub_OrcWaystation>();
                foreach (Subsettlement sub in camp.subs)
                {
                    if (sub is Sub_OrcWaystation waystation)
                    {
                        waystations.Add(waystation);
                    }
                }

                if (waystations.Count > 0)
                {
                    foreach (Sub_OrcWaystation waystation in waystations)
                    {
                        camp.subs.Remove(waystation);
                    }
                }
            }

            if (ua.location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (ua.isCommandable())
                {
                    ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Expand]), true);
                }

                if (orcSociety.cachedGone)
                {
                    orcSociety.cachedGone = false;
                    orcCulture.cachedGone = false;
                }
            }
        }

        public static void Ch_Orcs_DevastateOrcishIndustry(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (ua.isCommandable())
                {
                    ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.DevastateIndustry]), true);
                }
                else
                {
                    ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.DevastateIndustry]));
                }
            }
        }

        public static void Ch_Orcs_Expand(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            HolyOrder_Orcs orcCulture = null;
            if (ua.location.soc is SG_Orc orcSociety)
            {
                ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }
            else
            {
                if (ua.location.settlement != null && ua.location.settlement.subs.Count > 0)
                {
                    Sub_OrcWaystation waystation = ua.location.settlement.subs.OfType<Sub_OrcWaystation>().FirstOrDefault();
                    if (waystation != null && waystation.orcSociety != null)
                    {
                        orcSociety = waystation.orcSociety;
                        ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);

                        List<Location> targets = new List<Location>();
                        Location target = null;

                        foreach (Location neighbour in ua.location.getNeighbours())
                        {
                            if (orcSociety.canSettle(neighbour))
                            {
                                targets.Add(neighbour);
                            }
                        }

                        if (targets.Count == 1)
                        {
                            target = targets[0];
                        }
                        else if (targets.Count > 1)
                        {
                            target = targets[Eleven.random.Next(targets.Count)];
                        }

                        if (target != null)
                        {
                            Settlement oldSettlement = target.settlement;
                            target.soc = orcSociety;
                            target.settlement = new Set_OrcCamp(target);
                            if (oldSettlement != null && oldSettlement.subs.Count > 0)
                            {
                                foreach (Subsettlement sub in oldSettlement.subs)
                                {
                                    if (!(sub is Sub_OrcWaystation))
                                    {
                                        target.settlement.subs.Add(sub);
                                    }
                                }
                            }

                            if (ua.isCommandable())
                            {
                                target.settlement.isInfiltrated = true;
                            }

                            orcSociety.expandTarget = -1;
                        }
                    }
                }
            }

            if (ua.isCommandable() && orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Expand]), true);
            }
        }

        public static void Ch_Subjugate_Orcs(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (ua.location.settlement is Set_OrcCamp camp)
                {
                    if (camp.specialism == 0)
                    {
                        ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Subjugate]), true);
                    }
                    else
                    {
                        ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Subjugate] * 2), true);
                    }
                }
            }
        }

        public static void Mg_EnslaveTheDead(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua is UAEN_OrcElder && !ua.isCommandable())
            {
                foreach (Unit unit in ua.location.units)
                {
                    if (unit is UM_UntamedDead dead && dead.master == ua)
                    {
                        dead.master = null;
                    }
                }
            }
        }

        public static void Ch_RaidPeriphery(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc != null)
            {
                HashSet<SG_Orc> influencedOrcSocieties = new HashSet<SG_Orc>();
                List<SG_Orc> influencedOrcSocieties_Warring = new List<SG_Orc>();
                List<SG_Orc> influencedOrcSocieties_Regional = new List<SG_Orc>();

                foreach (SocialGroup sg in ua.map.socialGroups)
                {
                    if (sg is SG_Orc orcSociety && sg.getRel(ua.location.soc).state == DipRel.dipState.war)
                    {
                        influencedOrcSocieties_Warring.Add(orcSociety);
                        influencedOrcSocieties.Add(orcSociety);
                    }
                }

                foreach (Location neighbour in ua.location.getNeighbours())
                {
                    if (neighbour.soc != null && neighbour.soc != ua.location.soc && neighbour.soc is SG_Orc orcSociety && !influencedOrcSocieties.Contains(orcSociety))
                    {
                        influencedOrcSocieties_Regional.Add(orcSociety);
                        influencedOrcSocieties.Add(orcSociety);
                    }
                }

                foreach (SG_Orc orcSociety in influencedOrcSocieties_Warring)
                {
                    ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Raided enemy settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                }

                foreach (SG_Orc orcSociety in influencedOrcSocieties_Regional)
                {
                    ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Raided encroaching settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                }
            }
        }

        public static void Rt_RaidPort(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc != null)
            {
                HashSet<SG_Orc> influencedOrcSocieties = new HashSet<SG_Orc>();
                List<SG_Orc> influencedOrcSocieties_Warring = new List<SG_Orc>();
                List<SG_Orc> influencedOrcSocieties_Regional = new List<SG_Orc>();

                foreach (SocialGroup sg in ua.map.socialGroups)
                {
                    if (sg is SG_Orc orcSociety && sg.getRel(ua.location.soc).state == DipRel.dipState.war)
                    {
                        influencedOrcSocieties_Warring.Add(orcSociety);
                        influencedOrcSocieties.Add(orcSociety);
                    }
                }

                foreach (Location neighbour in ua.location.getNeighbours())
                {
                    if (neighbour.soc != null && neighbour.soc != ua.location.soc && neighbour.soc is SG_Orc orcSociety && !influencedOrcSocieties.Contains(orcSociety))
                    {
                        influencedOrcSocieties_Regional.Add(orcSociety);
                        influencedOrcSocieties.Add(orcSociety);
                    }
                }

                foreach (SG_Orc orcSociety in influencedOrcSocieties_Warring)
                {
                    ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Raided enemy settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                }

                foreach (SG_Orc orcSociety in influencedOrcSocieties_Regional)
                {
                    ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Raided encroaching settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                }
            }

            if (ua.location.settlement.subs.OfType<Sub_Docks>().FirstOrDefault() != null)
            {
                int wreckRoll = Eleven.random.Next(10);

                if (wreckRoll < 1)
                {
                    Pr_Shipwreck wreck = ua.location.properties.OfType<Pr_Shipwreck>().FirstOrDefault();

                    if (wreck == null)
                    {
                        ua.location.properties.Add(new Pr_Shipwreck(ua.location));
                    }
                    else
                    {
                        wreck.charge += 25.0;
                    }
                }
            }
        }

        public static void Rt_RaidShipping(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            foreach (TradeRoute route in ua.map.tradeManager.routes)
            {
                if (route.raidingCooldown == ua.map.param.ch_raidShippingCooldown && route.path.Contains(ua.location))
                {
                    int wreckRoll = Eleven.random.Next(10);

                    if (wreckRoll < 1)
                    {
                        List<Location> oceanLocations = route.path.FindAll(l => l.isOcean).ToList();

                        if (oceanLocations.Count > 0)
                        {
                            Location wreckLocation = oceanLocations[Eleven.random.Next(oceanLocations.Count)];
                            Pr_Shipwreck wreck = wreckLocation.properties.OfType<Pr_Shipwreck>().FirstOrDefault();

                            if (wreck == null)
                            {
                                wreckLocation.properties.Add(new Pr_Shipwreck(wreckLocation));
                            }
                            else
                            {
                                wreck.charge += 25.0;
                            }
                        }
                    }
                }
            }
        }

        // Template Item
        public static void Ch_(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {

        }
    }
}
