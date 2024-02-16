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
        public static Challenge lastChallengeCompleted;

        public static void processChallenge(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            lastChallengeCompleted = challenge;

            switch (task_PerformChallenge.challenge)
            {
                case Ch_Orcs_BuildFortress _:
                    Ch_Orcs_BuildFortress(challenge, ua, task_PerformChallenge);
                    break;
                case Ch_Orcs_BuildMages _:
                    Ch_Orcs_BuildMages(challenge, ua, task_PerformChallenge);
                    break;
                case Ch_Orcs_BuildMenagerie _:
                    Ch_Orcs_BuildMenagerie(challenge, ua, task_PerformChallenge);
                    break;
                case Ch_Orcs_BuildShipyard _:
                    Ch_Orcs_BuildShipyard(challenge, ua, task_PerformChallenge);
                    break;
                    case Ch_Orcs_BuildMines _:
                    Ch_Orcs_BuildMines(challenge, ua, task_PerformChallenge);
                    break;
                case Rt_Orcs_ClaimTerritory _:
                    Rt_Orcs_ClaimTerritory(challenge, ua, task_PerformChallenge);
                    break;
                case Ch_Orcs_DevastateOrcishIndustry _:
                    Ch_Orcs_DevastateOrcishIndustry(challenge, ua, task_PerformChallenge);
                    break;
                case Ch_Subjugate_Orcs _:
                    Ch_Subjugate_Orcs(challenge, ua, task_PerformChallenge);
                    break;
                case Mg_EnslaveTheDead _:
                    Mg_EnslaveTheDead(challenge, ua, task_PerformChallenge);
                    break;
                case Ch_RaidPeriphery _:
                    Ch_RaidPeriphery(challenge, ua, task_PerformChallenge);
                    break;
                case Rt_RaidPort _:
                    Rt_RaidPort(challenge, ua, task_PerformChallenge);
                    break;
                case Ch_Orcs_OpportunisticEncroachment _:
                    Ch_Orcs_OpportunisticEncroachment(challenge, ua, task_PerformChallenge);
                    break;
                case Ch_Orcs_OrganiseTheHorde _:
                    Ch_Orcs_OrganiseTheHorde(challenge, ua, task_PerformChallenge);
                    break;
                default:
                    break;
            }
        }

        public static void Ch_Orcs_BuildFortress(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        public static void Ch_Orcs_BuildMages(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        public static void Ch_Orcs_BuildMenagerie(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildMenagerie]), true);
            }
        }

        public static void Ch_Orcs_BuildShipyard(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildShipyard]), true);
            }
        }

        public static void Ch_Orcs_BuildMines(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildMines]), true);
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

            if (ua.society is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (ua.isCommandable())
                {
                    ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.Expand]), true);
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
            if (ua.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (ua.isCommandable())
                {
                    ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.DevastateIndustry]), true);
                }
                else
                {
                    ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.DevastateIndustry]));
                }
            }
        }

        public static void Ch_Subjugate_Orcs(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (ua.location.settlement is Set_OrcCamp camp)
                {
                    if (camp.specialism == 0)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.Subjugate]), true);
                    }
                    else
                    {
                        ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.Subjugate] * 2), true);
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
                    ModCore.Get().TryAddInfluenceGain(orcSociety, new ReasonMsg("Raided enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                }

                foreach (SG_Orc orcSociety in influencedOrcSocieties_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcSociety, new ReasonMsg("Raided encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
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
                    ModCore.Get().TryAddInfluenceGain(orcSociety, new ReasonMsg("Raided enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                }

                foreach (SG_Orc orcSociety in influencedOrcSocieties_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcSociety, new ReasonMsg("Raided encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                }
            }
        }

        public static void Ch_Orcs_OpportunisticEncroachment(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            SG_Orc orcSociety = challenge.location.soc as SG_Orc;
            if (challenge.location.settlement != null && challenge.location.settlement.subs.Count > 0)
            {
                Sub_OrcWaystation waystation = (Sub_OrcWaystation)challenge.location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(challenge));
                List<Location> targetLocations = new List<Location>();
                Location target = null;

                if (waystation != null)
                {
                    orcSociety = waystation.orcSociety;

                    foreach (Location neighbour in challenge.location.getNeighbours())
                    {
                        if (neighbour.settlement is SettlementHuman && !(neighbour.settlement is Set_City) && !(neighbour.settlement is Set_ElvenCity))
                        {
                            if (!neighbour.properties.Any(pr => pr is Pr_OrcEncroachment))
                            {
                                targetLocations.Add(neighbour);
                            }
                        }
                    }
                }

                if (targetLocations.Count > 0)
                {
                    target = targetLocations[0];
                    if (targetLocations.Count > 1)
                    {
                        target = targetLocations[Eleven.random.Next(targetLocations.Count)];
                    }
                }

                if (target != null)
                {
                    target.properties.Add(new Pr_OrcEncroachment(target, orcSociety));
                    challenge.msgString = "Orcs are beginning to encroach on human settlements in " + target.getName(true) + ", waiting for signs of weakness to make their land grab.";
                }
            }

            if (ua.isCommandable() && orcSociety != null)
            {
                if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                {
                    ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.Expand]), true);
                }
            }
        }

        public static void Ch_Orcs_OrganiseTheHorde(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (ua.isCommandable() && ua.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(task_PerformChallenge.challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.Raiding]), true);
            }
        }

        // Template Item
        public static void Ch_(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {

        }
    }
}
