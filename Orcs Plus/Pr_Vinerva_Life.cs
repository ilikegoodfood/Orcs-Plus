using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;
using UnityEngine;

namespace Orcs_Plus
{
    public class Pr_Vinerva_Life : Property
    {
        public List<Challenge> challenges = new List<Challenge>();

        public Challenge nurture;

        public Pr_Vinerva_Life(Location location)
            : base(location)
        {
            nurture = new Ch_H_Orcs_NurtureOrchard(location);
            challenges.Add(nurture);
        }

        public override string getName()
        {
            return "Sapling of Life";
        }

        public override string getDesc()
        {
            return "The sapling of life thrives in harsh conditions, and will die if local conditions imrpove too much. If nurtured, it will grow into a vast orchard, which provides a stable environment, food, water, and shelter, to any who dare live between the gnarled, wind-warped trees.";
        }

        public override List<Challenge> getChallenges()
        {
            return challenges;
        }

        public override Sprite getSprite(World world)
        {
            return EventManager.getImg("OrcsPlus.Foreground_SaplingOak.png");
        }

        public override void turnTick()
        {
            if (charge <= 0.0)
            {
                location.properties.Remove(this);
                return;
            }

            bool valid = location.hex.getHabilitability() < map.opt_orcHabMult * map.param.orc_habRequirement;

            if (valid)
            {
                if (location.isOcean || location.soc != null || location.hex.getHabilitability() >= location.map.opt_orcHabMult * location.map.param.orc_habRequirement)
                {
                    valid = false;
                }
                else if (location.settlement != null)
                {
                    if (ModCore.comLib.tryGetSettlementTypeForOrcExpansion(location.settlement.GetType(), out List<Type> subsettlementBlacklist))
                    {

                        foreach (Subsettlement sub in location.settlement.subs)
                        {
                            if (subsettlementBlacklist.Contains(sub.GetType()))
                            {
                                valid = false;
                            }
                        }
                    }
                    else
                    {
                        valid = false;
                    }
                }
            }

            if (valid)
            {
                valid = false;

                foreach (Location neighbour in location.getNeighbours())
                {
                    if (neighbour.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_LifeMother life && life.status < 0)
                    {
                        valid = true;
                        break;
                    }

                    if (neighbour.soc is HolyOrder_Orcs orcCulture2 && orcCulture2.tenet_god is H_Orcs_LifeMother life2 && life2.status < 0)
                    {
                        valid = true;
                        break;
                    }

                    Sub_OrcWaystation waystation = neighbour.settlement?.subs.OfType<Sub_OrcWaystation>().FirstOrDefault();
                    if (waystation != null && ModCore.Get().data.orcSGCultureMap.TryGetValue(waystation.orcSociety, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null && orcCulture3.tenet_god is H_Orcs_LifeMother life3 && life3.status < 0)
                    {
                        valid = true;
                        break;
                    }
                }
            }

            if (!valid)
            {
                location.properties.Remove(this);
                return;
            }

            if (charge >= 300.0)
            {
                SG_Orc orcSociety = null;

                foreach (Unit u in location.units)
                {
                    if (u is UA ua && ua.task is Task_PerformChallenge challenge && challenge.challenge == nurture)
                    {
                        orcSociety = ua.society as SG_Orc;

                        if (orcSociety == null)
                        {
                            if (ua.society is HolyOrder_Orcs orcCulture)
                            {
                                orcSociety = orcCulture.orcSociety;
                            }
                        }

                        if (orcSociety != null)
                        {
                            crises(orcSociety);
                            return;
                        }
                    }
                }

                if (orcSociety == null)
                {
                    List<SG_Orc> orcSocieties = new List<SG_Orc>();

                    foreach (Location neighbour in location.getNeighbours())
                    {
                        if (neighbour.soc is SG_Orc orcSociety2 && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null && orcCulture2.tenet_god is H_Orcs_LifeMother life && life.status < 0)
                        {
                            orcSocieties.Add(orcSociety2);
                            continue;
                        }

                        if (neighbour.soc is HolyOrder_Orcs orcCulture3 && orcCulture3.tenet_god is H_Orcs_LifeMother life3 && life3.status < 0)
                        {
                            orcSocieties.Add(orcCulture3.orcSociety);
                            continue;
                        }

                        Sub_OrcWaystation waystation = neighbour.settlement?.subs.OfType<Sub_OrcWaystation>().FirstOrDefault();
                        if (waystation != null && waystation.orcSociety != null && ModCore.Get().data.orcSGCultureMap.TryGetValue(waystation.orcSociety, out HolyOrder_Orcs orcCulture4) && orcCulture4 != null && orcCulture4.tenet_god is H_Orcs_LifeMother life4 && life4.status < 0)
                        {
                            orcSocieties.Add(waystation.orcSociety);
                            continue;
                        }
                    }

                    if (orcSocieties.Count == 0)
                    {
                        location.properties.Remove(this);
                        return;
                    }
                    else
                    {
                        orcSociety = orcSocieties[0];
                        if (orcSocieties.Count > 1)
                        {
                            orcSociety = orcSocieties[Eleven.random.Next(orcSocieties.Count)];
                        }
                    }
                }

                if (orcSociety != null)
                {
                    crises(orcSociety);
                    return;
                }
            }
        }

        public void crises(SG_Orc orcSociety)
        {
            location.soc = orcSociety;

            Settlement set = location.settlement;
            location.settlement = new Set_OrcCamp(location);
            if (set != null)
            {
                foreach (Subsettlement sub in set.subs)
                {
                    if (!(sub is Sub_OrcWaystation))
                    {
                        location.settlement.subs.Add(sub);
                    }
                }
            }
            location.settlement.isInfiltrated = true;

            location.properties.Add(new Pr_Vinerva_LifeBoon(location));
            location.properties.Remove(this);

            ModCore.Get().TryAddInfluenceGain(orcSociety, new ReasonMsg("Nurtured Orchard of Life to Maturity", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RecieveGift]), true);
        }

        public override bool canTriggerCrisis()
        {
            return true;
        }

        public override string getCrisis()
        {
            return "If this modifier hits 300% an Orchard of Life will have gorwn here, and an orc camp will immediately be built.";
        }
    }
}
