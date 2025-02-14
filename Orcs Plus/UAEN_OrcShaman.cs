using Assets.Code;
using Assets.Code.Modding;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class UAEN_OrcShaman : UAEN
    {
        public Rt_Orcs_SacrificialSite sacrificalSite;

        public UAEN_OrcShaman(Location loc, SocialGroup sg, Person p)
            : base(loc, sg, p)
        {
            p.stat_might = 2 + Eleven.random.Next(2);
            p.stat_intrigue = 2;
            p.stat_lore = 3 + Eleven.random.Next(3);
            p.stat_command = 3;
            p.hasSoul = false;
            p.species = map.species_orc;

            T_ArcaneKnowledge knowledge = new T_ArcaneKnowledge();
            knowledge.level = 0;
            p.receiveTrait(knowledge);

            T_MasteryDeath deathMastery = new T_MasteryDeath();
            deathMastery.level = 2;
            p.receiveTrait(deathMastery);

            sacrificalSite = new Rt_Orcs_SacrificialSite(loc, this);
            rituals.Add(sacrificalSite);
        }

        public override void turnTick(Map map)
        {
            base.turnTick(map);

            if (society is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture))
            {
                if (orcCulture.tenet_god is H_Orcs_Fleshweaving fleshweaving)
                {
                    if (ModCore.Get().data.tryGetModIntegrationData("Escamrak", out ModIntegrationData intDataEscam))
                    {
                        if (intDataEscam.typeDict.TryGetValue("FleshStatBonusTrait", out Type fleshStatBonusType) && intDataEscam.constructorInfoDict.TryGetValue("FleshStatBonusTrait", out ConstructorInfo ci) && intDataEscam.fieldInfoDict.TryGetValue("FleshStatBonusTrait_BonusType", out FieldInfo FI_BonusType))
                        {
                            Trait fleshStatBonusTrait = person.traits.FirstOrDefault(t => t.GetType() == fleshStatBonusType || t.GetType().IsSubclassOf(fleshStatBonusType));
                            if (fleshweaving.status >= 0 || isCommandable())
                            {
                                if (fleshStatBonusTrait != null)
                                {
                                    person.traits.Remove(fleshStatBonusTrait);
                                }
                            }
                            else
                            {
                                if (fleshStatBonusTrait == null)
                                {
                                    fleshStatBonusTrait = (Trait)ci.Invoke(new object[0]);
                                    person.receiveTrait(fleshStatBonusTrait);
                                    FI_BonusType.SetValue(fleshStatBonusTrait, "Lore");
                                }

                                fleshStatBonusTrait.level = -fleshweaving.status;
                            }
                        }
                    }
                }
            }

            person.XP += map.param.socialGroup_orc_upstartXPPerTurn;
            if (person.skillPoints > 0)
            {
                spendSkillPoint();
            }
        }

        public new List<Unit> getVisibleUnits()
        {
            List<Unit> units = new List<Unit>();

            SG_Orc orcSociety = society as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcSociety != null && !orcSociety.isGone() && orcCulture != null)
            {
                List<Type> dominionBannerTypes = new List<Type>();
                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.typeDict.TryGetValue("Banner", out Type dominionBanner))
                {
                    dominionBannerTypes.Add(dominionBanner);
                }
                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCCR) && intDataCCCR.typeDict.TryGetValue("Banner", out Type dominionBanner2))
                {
                    dominionBannerTypes.Add(dominionBanner2);
                }

                foreach (Unit unit in map.units)
                {
                    if (unit == this)
                    {
                        continue;
                    }

                    if (unit is UA agent)
                    {
                        if (agent.society != null && (agent.society == orcSociety || (orcCulture != null && agent.society == orcCulture)))
                        {
                            units.Add(unit);
                        }
                        else if (agent.person.traits.Any(t => t is T_BloodFeud f && f.orcSociety == orcSociety))
                        {
                            units.Add(unit);
                        }
                        else if (agent.person.items.Any(i => i is I_HordeBanner banner && banner.orcs == orcSociety))
                        {
                            units.Add(unit);
                        }
                        else if (dominionBannerTypes.Count > 0 && agent.person.items.Any(i => i != null && dominionBannerTypes.Any(t => i.GetType() == t || i.GetType().IsSubclassOf(t))))
                        {
                            units.Add(unit);
                        }
                        else if (agent.homeLocation != -1 && (map.locations[agent.homeLocation].soc == orcSociety || (orcCulture != null && map.locations[agent.homeLocation].soc == orcCulture)) && !(agent.task is Task_InHiding))
                        {
                            units.Add(unit);
                        }
                        else if (agent.location.soc != null && (agent.location.soc == orcSociety || (orcCulture != null && agent.location.soc == orcCulture)) && !(agent.task is Task_InHiding))
                        {
                            units.Add(unit);
                        }
                        else if (agent.society != null && (agent.society.getRel(orcSociety).state == DipRel.dipState.war || (orcCulture != null && agent.society.getRel(orcCulture).state == DipRel.dipState.war)) && !(agent.task is Task_InHiding))
                        {
                            units.Add(unit);
                        }
                        else if (agent.task is Task_PerformChallenge performChallenge && performChallenge.challenge.isChannelled() && map.getStepDist(location, agent.location) <= 10)
                        {
                            units.Add(unit);
                        }
                        else if (agent.task is Task_GoToPerformChallenge goPerformChallenge && !(goPerformChallenge.challenge is Ritual) && (goPerformChallenge.challenge.location.soc == orcSociety || goPerformChallenge.challenge.location.soc == orcCulture))
                        {
                            units.Add(unit);
                        }
                        else if (agent.task is Task_AttackUnit attack && attack.target is UA && (attack.target.society == orcSociety || attack.target.society == orcCulture))
                        {
                            units.Add(unit);
                        }
                        else if (orcCulture.tenet_intolerance.status < 0 && agent.task is Task_AttackUnit attack2 && attack2.target is UA && attack2.target.isCommandable())
                        {
                            units.Add(unit);
                        }
                    }
                }
            }

            return units;
        }

        public override double getDisruptUtility(Unit c, List<ReasonMsg> reasons)
        {
            double utility = 0.0;

            SG_Orc orcSociety = society as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcSociety != null && !orcSociety.isGone() && orcCulture != null)
            {
                if (c is UA target)
                {
                    H_Orcs_Intolerance intolerance = orcCulture.tenet_intolerance;
                    if (intolerance != null)
                    {
                        bool otherIsTarget = false;

                        T_BloodFeud feud = (T_BloodFeud)target.person.traits.FirstOrDefault(t => t is T_BloodFeud f && f.orcSociety == orcSociety);
                        if (feud != null)
                        {
                            otherIsTarget = true;
                        }
                        else if (target.society != null)
                        {
                            if (target.society != orcSociety && target.society != orcCulture)
                            {
                                if (target.homeLocation == -1 || (map.locations[target.homeLocation].soc != orcSociety && map.locations[target.homeLocation].soc != orcCulture))
                                {
                                    if (intolerance.status == 0)
                                    {
                                        otherIsTarget = true;
                                    }
                                    else if (intolerance.status > 0)
                                    {
                                        if (orcSociety.getRel(target.society).state == DipRel.dipState.war || target.society.isDark() || target.isCommandable())
                                        {
                                            otherIsTarget = true;
                                        }
                                        else
                                        {
                                            reasons?.Add(new ReasonMsg("Orcs of a human-tolerant culture will not disrupt agents of good societies", -10000.0));
                                            utility -= 10000.0;
                                        }
                                    }
                                    else if (intolerance.status < 0)
                                    {
                                        if (!target.isCommandable() && (orcSociety.getRel(target.society).state == DipRel.dipState.war || target.society is SG_Orc || target.society is HolyOrder_Orcs || !target.society.isDark()))
                                        {
                                            otherIsTarget = true;
                                        }
                                        else
                                        {
                                            reasons?.Add(new ReasonMsg("Orcs of an elder-tolerant culture will not disrupt the elder's agents or societies", -10000.0));
                                            utility -= 10000.0;
                                        }
                                    }
                                    else
                                    {
                                        reasons?.Add(new ReasonMsg("ERROR: Invalid Intolerance Tenet Status", -10000.0));
                                        utility -= 10000.0;
                                    }
                                }
                                else
                                {
                                    reasons?.Add(new ReasonMsg("Orcs will only disrupt outsiders", -10000.0));
                                    utility -= 10000.0;
                                }
                            }
                            else
                            {
                                reasons?.Add(new ReasonMsg("Target is of own culture", -10000.0));
                                utility -= 10000.0;
                            }
                        }
                        else
                        {
                            if (!(target.isCommandable() && intolerance.status < 0))
                            {
                                otherIsTarget = true;
                            }
                            else
                            {
                                reasons?.Add(new ReasonMsg("Orcs of an elder-tolerant culture will not disrupt the elder's agents", -10000.0));
                                utility -= 10000.0;
                            }
                        }

                        if (otherIsTarget)
                        {
                            List<Type> dominionBannerTypes = new List<Type>();
                            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.typeDict.TryGetValue("Banner", out Type dominionBanner))
                            {
                                dominionBannerTypes.Add(dominionBanner);
                            }
                            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCCR) && intDataCCCR.typeDict.TryGetValue("Banner", out Type dominionBanner2))
                            {
                                dominionBannerTypes.Add(dominionBanner2);
                            }

                            foreach (Item item in target.person.items)
                            {
                                if (item != null)
                                {
                                    I_HordeBanner banner = item as I_HordeBanner;
                                    if (banner?.orcs == orcSociety && (target.society == null || society.getRel(target.society).state != DipRel.dipState.war) && (feud == null))
                                    {
                                        reasons?.Add(new ReasonMsg("Orcs will not disrupt banner bearer", -10000.0));
                                        utility -= 10000.0;
                                        return utility;
                                    }

                                    foreach (Type dominionBannerType in dominionBannerTypes)
                                    {
                                        if (item.GetType() == dominionBannerType || item.GetType().IsSubclassOf(dominionBannerType))
                                        {
                                            reasons?.Add(new ReasonMsg("Orcs will not disrupt banner bearer", -10000.0));
                                            utility -= 10000.0;
                                            return utility;
                                        }
                                    }
                                }
                            }

                            if (target.location.soc == orcSociety || (((target.society != null && orcSociety.getRel(target.society).state == DipRel.dipState.war) || feud != null) && (target.location.soc == null || orcSociety.getRel(target.location.soc).state == DipRel.dipState.war)))
                            {
                                double val;
                                if (target.location.soc != orcSociety)
                                {
                                    val = -25;
                                    reasons?.Add(new ReasonMsg("Target is outside of " + orcSociety.getName() + "'s territory", val));
                                    utility += val;
                                }
                                else if (target.task is Task_PerformChallenge task)
                                {
                                    val = 20;
                                    reasons?.Add(new ReasonMsg("Agent is interfereing with " + orcSociety.getName() + "'s territory", val));
                                    utility += val;

                                    if (task.challenge is Ch_Orcs_StealPlunder)
                                    {
                                        val = 40;
                                        reasons?.Add(new ReasonMsg("Agent stealing gold from " + orcSociety.getName() + "'s plunder", val));
                                        utility += val;
                                    }
                                }
                                else if (target.task is Task_GoToPerformChallenge task2 && !(task2.challenge is Ritual) && (task2.challenge.location.soc == orcSociety || task2.challenge.location.soc == orcCulture))
                                {
                                    val = 20;
                                    reasons?.Add(new ReasonMsg("Agent is travelling to interfere with " + orcSociety.getName() + "'s territory", val));
                                    utility += val;

                                    if (task2.challenge is Ch_Orcs_StealPlunder)
                                    {
                                        val = 40;
                                        reasons?.Add(new ReasonMsg("Agent is travelling to steal gold from " + orcSociety.getName() + "'s plunder", val));
                                        utility += val;
                                    }
                                }

                                utility += person.getTagUtility(new int[]
                                {
                                    Tags.COMBAT,
                                    Tags.CRUEL,
                                    Tags.DANGER
                                }, new int[0], reasons);
                                utility += person.getTagUtility(target.getPositiveTags(), target.getNegativeTags(), reasons);

                                if (target.task is Task_PerformChallenge tChallenge2 && tChallenge2.challenge.isChannelled())
                                {
                                    val = 20;
                                    reasons?.Add(new ReasonMsg("Channeler", val));
                                    utility += val;
                                }

                                if (ModCore.GetComLib().checkIsVampire(target))
                                {
                                    val = -35;
                                    reasons?.Add(new ReasonMsg("Fear of Vampires", val));
                                    utility += val;
                                }

                                if (ModCore.Get().data.tryGetModIntegrationData("LivingWilds", out ModIntegrationData intDataLW) && intDataLW.typeDict.TryGetValue("NatureCritter", out Type natureCritterType))
                                {
                                    if (target.GetType().IsSubclassOf(natureCritterType))
                                    {
                                        val = -35;
                                        reasons?.Add(new ReasonMsg("Nature", val));
                                        utility += val;
                                    }
                                }

                                if (feud != null)
                                {
                                    val = 20;
                                    reasons?.Add(new ReasonMsg("Blood Feud", val));
                                    utility += val;
                                }

                                if (target.society != null && orcSociety.getRel(target.society).state == DipRel.dipState.war)
                                {
                                    val = 20.0;
                                    reasons?.Add(new ReasonMsg("At War", val));
                                    utility += val;
                                }

                                foreach (HolyTenet holyTenet in orcCulture.tenets)
                                {
                                    utility += holyTenet.addUtilityAttack(this, target, reasons);
                                }
                                foreach (ModKernel modKernel in map.mods)
                                {
                                    utility = modKernel.unitAgentAIAttack(map, this, target, reasons, utility);
                                }
                            }
                            else
                            {
                                if (target.society != null && orcSociety.getRel(target.society).state == DipRel.dipState.war)
                                {
                                    reasons?.Add(new ReasonMsg("Tresspassing in territory of " + target.location.soc.getName(), -10000.0));
                                    utility -= 10000.0;
                                }
                                else
                                {
                                    reasons?.Add(new ReasonMsg("Target is outside of " + orcSociety.getName() + "'s territory", -10000.0));
                                    utility -= 10000.0;
                                }
                            }
                        }
                        else
                        {
                            reasons?.Add(new ReasonMsg("Invalid Target", -10000.0));
                            utility -= 10000.0;
                        }
                    }
                    else
                    {
                        reasons?.Add(new ReasonMsg("ERROR: Failed to find Intolerence Tenet", -10000.0));
                        utility -= 10000.0;
                    }
                }
                else
                {
                    reasons?.Add(new ReasonMsg("Target is not Agent", -10000.0));
                    utility -= 10000.0;
                }
            }
            else
            {
                if (orcSociety == null)
                {
                    reasons?.Add(new ReasonMsg("ERROR: Agent is not of Orc Social Group", -10000.0));
                    utility -= 10000.0;
                }
                else if (orcCulture == null)
                {
                    reasons?.Add(new ReasonMsg("ERROR: Failed to find orc culture", -10000.0));
                    utility -= 10000.0;
                }
            }

            return utility;
        }

        public override double getAttackUtility(Unit c, List<ReasonMsg> reasons, bool includeDangerousFoe = true)
        {
            double utility = 0.0;

            SG_Orc orcSociety = society as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcSociety != null && !orcSociety.isGone() && orcCulture != null)
            {
                if ((orcCulture.tenet_god is H_Orcs_GlorySeeker glory && glory.status < 0) || (c.task is Task_PerformChallenge tChallenge && tChallenge.challenge.isChannelled()) || (orcCulture.tenet_god is H_Orcs_BloodOffering blood && blood.status < 0 && (c.person?.hasSoul ?? false)))
                {
                    if (c is UA target)
                    {
                        H_Orcs_Intolerance intolerance = orcCulture.tenet_intolerance;
                        if (intolerance != null)
                        {
                            bool otherIsTarget = false;

                            T_BloodFeud feud = (T_BloodFeud)target.person.traits.FirstOrDefault(t => t is T_BloodFeud f && f.orcSociety == orcSociety);
                            if (feud != null)
                            {
                                otherIsTarget = true;
                            }
                            else if (target.society != null)
                            {
                                if (target.society != orcSociety && target.society != orcCulture)
                                {
                                    if (target.homeLocation == -1 || (map.locations[target.homeLocation].soc != orcSociety && map.locations[target.homeLocation].soc != orcCulture))
                                    {
                                        if (intolerance.status == 0)
                                        {
                                            otherIsTarget = true;
                                        }
                                        else if (intolerance.status > 0)
                                        {
                                            if (orcSociety.getRel(target.society).state == DipRel.dipState.war || target.society.isDark() || target.isCommandable())
                                            {
                                                otherIsTarget = true;
                                            }
                                            else
                                            {
                                                reasons?.Add(new ReasonMsg("Orcs of a human-tolerant culture will not attack agents of good societies", -10000.0));
                                                utility -= 10000.0;
                                            }
                                        }
                                        else if (intolerance.status < 0)
                                        {
                                            if (!target.isCommandable() && (orcSociety.getRel(target.society).state == DipRel.dipState.war || target.society is SG_Orc || target.society is HolyOrder_Orcs || !target.society.isDark()))
                                            {
                                                otherIsTarget = true;
                                            }
                                            else
                                            {
                                                reasons?.Add(new ReasonMsg("Orcs of an elder-tolerant culture will not attack the elder's agents or societies", -10000.0));
                                                utility -= 10000.0;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        reasons?.Add(new ReasonMsg("Orcs will only attack outsiders", -10000.0));
                                        utility -= 10000.0;
                                    }
                                }
                                else
                                {
                                    reasons?.Add(new ReasonMsg("Target is of own culture", -10000.0));
                                    utility -= 10000.0;
                                }
                            }
                            else
                            {
                                if (!(target.isCommandable() && intolerance.status < 0))
                                {
                                    otherIsTarget = true;
                                }
                                else
                                {
                                    reasons?.Add(new ReasonMsg("Orcs of an elder-tolerant culture will not attack the elder's agents", -10000.0));
                                    utility -= 10000.0;
                                }
                            }

                            if (otherIsTarget)
                            {
                                List<Type> dominionBannerTypes = new List<Type>();
                                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.typeDict.TryGetValue("Banner", out Type dominionBanner))
                                {
                                    dominionBannerTypes.Add(dominionBanner);
                                }
                                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCCR) && intDataCCCR.typeDict.TryGetValue("Banner", out Type dominionBanner2))
                                {
                                    dominionBannerTypes.Add(dominionBanner2);
                                }

                                foreach (Item item in target.person.items)
                                {
                                    if (item != null)
                                    {
                                        I_HordeBanner banner = item as I_HordeBanner;
                                        if (banner?.orcs == orcSociety && (target.society == null || society.getRel(target.society).state != DipRel.dipState.war) && (feud == null))
                                        {
                                            reasons?.Add(new ReasonMsg("Orcs will not attack banner bearer", -10000.0));
                                            utility -= 10000.0;
                                            return utility;
                                        }

                                        foreach (Type dominionBannerType in dominionBannerTypes)
                                        {
                                            if (item.GetType() == dominionBannerType || item.GetType().IsSubclassOf(dominionBannerType))
                                            {
                                                reasons?.Add(new ReasonMsg("Orcs will not attack banner bearer", -10000.0));
                                                utility -= 10000.0;
                                                return utility;
                                            }
                                        }
                                    }
                                }

                                if (target.location.soc == orcSociety || (((target.society != null && orcSociety.getRel(target.society).state == DipRel.dipState.war) || feud != null) && (target.location.soc == null || orcSociety.getRel(target.location.soc).state == DipRel.dipState.war)))
                                {
                                    double val = 20;

                                    if (orcCulture.tenet_god is H_Orcs_GlorySeeker glory2 && glory2.status < 0)
                                    {
                                        reasons?.Add(new ReasonMsg("Glory Seeker", val));
                                        utility += val;
                                    }
                                    else if (target.person.hasSoul && orcCulture.tenet_god is H_Orcs_BloodOffering blood2 && blood2.status < 0)
                                    {
                                        reasons?.Add(new ReasonMsg("Blood for the Blood God", val));
                                        utility += val;
                                    }

                                    if (target.location.soc != orcSociety)
                                    {
                                        val = -25;
                                        reasons?.Add(new ReasonMsg("Target is outside of " + orcSociety.getName() + "'s territory", val));
                                        utility += val;
                                    }
                                    else if (target.task is Task_PerformChallenge task)
                                    {
                                        val = 20;
                                        reasons?.Add(new ReasonMsg("Agent is interfereing with " + orcSociety.getName() + "'s territory", val));
                                        utility += val;

                                        if (task.challenge is Ch_Orcs_StealPlunder)
                                        {
                                            val = 20;
                                            reasons?.Add(new ReasonMsg("Agent stealing gold from " + orcSociety.getName() + "'s plunder", val));
                                            utility += val;
                                        }
                                    }
                                    else if (target.task is Task_GoToPerformChallenge task2 && !(task2.challenge is Ritual) && (task2.challenge.location.soc == orcSociety || task2.challenge.location.soc == orcCulture))
                                    {
                                        val = 20;
                                        reasons?.Add(new ReasonMsg("Agent is travelling to interfere with " + orcSociety.getName() + "'s territory", val));
                                        utility += val;

                                        if (task2.challenge is Ch_Orcs_StealPlunder)
                                        {
                                            val = 20;
                                            reasons?.Add(new ReasonMsg("Agent is travelling to steal gold from " + orcSociety.getName() + "'s plunder", val));
                                            utility += val;
                                        }
                                    }

                                    utility += person.getTagUtility(new int[]
                                    {
                                        Tags.COMBAT,
                                        Tags.CRUEL,
                                        Tags.DANGER
                                    }, new int[0], reasons);
                                    utility += person.getTagUtility(target.getPositiveTags(), target.getNegativeTags(), reasons);

                                    if (target.task is Task_PerformChallenge tChallenge2 && tChallenge2.challenge.isChannelled())
                                    {
                                        val = 20;
                                        reasons?.Add(new ReasonMsg("Channeler", val));
                                        utility += val;
                                    }

                                    if (ModCore.GetComLib().checkIsVampire(target))
                                    {
                                        val = -35;
                                        reasons?.Add(new ReasonMsg("Fear of Vampires", val));
                                        utility += val;
                                    }

                                    if (ModCore.Get().data.tryGetModIntegrationData("LivingWilds", out ModIntegrationData intDataLW) && intDataLW.typeDict.TryGetValue("NatureCritter", out Type natureCritterType))
                                    {
                                        if (target.GetType().IsSubclassOf(natureCritterType))
                                        {
                                            val = -35;
                                            reasons?.Add(new ReasonMsg("Nature", val));
                                            utility += val;
                                        }
                                    }

                                    if (includeDangerousFoe)
                                    {
                                        val = 0.0;
                                        double dangerUtility = target.getDangerEstimate() - getDangerEstimate();
                                        if (dangerUtility > 0.0)
                                        {
                                            val -= map.param.utility_ua_attackDangerReluctanceOffset;
                                        }
                                        dangerUtility += 2;
                                        if (dangerUtility > 0.0)
                                        {
                                            val += map.param.utility_ua_attackDangerReluctanceMultiplier;
                                        }
                                        reasons?.Add(new ReasonMsg("Dangerous Foe", val));
                                        utility += val;
                                    }

                                    if (feud != null)
                                    {
                                        val = 50;
                                        reasons?.Add(new ReasonMsg("Blood Feud", val));
                                        utility += val;
                                    }

                                    if (target.society != null && orcSociety.getRel(target.society).state == DipRel.dipState.war)
                                    {
                                        val = 50.0;
                                        reasons?.Add(new ReasonMsg("At War", val));
                                        utility += val;
                                    }

                                    foreach (HolyTenet holyTenet in orcCulture.tenets)
                                    {
                                        utility += holyTenet.addUtilityAttack(this, target, reasons);
                                    }
                                    foreach (ModKernel modKernel in map.mods)
                                    {
                                        utility = modKernel.unitAgentAIAttack(map, this, target, reasons, utility);
                                    }
                                }
                                else
                                {
                                    if (target.society != null && orcSociety.getRel(target.society).state == DipRel.dipState.war)
                                    {
                                        reasons?.Add(new ReasonMsg("Tresspassing in territory of " + target.location.soc.getName(), -10000.0));
                                        utility -= 10000.0;
                                    }
                                    else
                                    {
                                        reasons?.Add(new ReasonMsg("Target is outside of " + orcSociety.getName() + "'s territory", -10000.0));
                                        utility -= 10000.0;
                                    }
                                }
                            }
                            else
                            {
                                reasons?.Add(new ReasonMsg("Invalid Target", -10000.0));
                                utility -= 10000.0;
                            }
                        }
                        else
                        {
                            reasons?.Add(new ReasonMsg("ERROR: Failed to find Intolerence Tenet", -10000.0));
                            utility -= 10000.0;
                        }
                    }
                    else
                    {
                        reasons?.Add(new ReasonMsg("Target is not Agent", -10000.0));
                        utility -= 10000.0;
                    }
                }
                else
                {
                    if (orcCulture.tenet_god is H_Orcs_GlorySeeker)
                    {
                        reasons?.Add(new ReasonMsg("Glory Seeker Tenet is not elder aligned", -10000.0));
                        utility -= 10000.0;
                    }
                    else
                    {
                        reasons?.Add(new ReasonMsg("Culture does not have Glory Seeker god specific tenet", -10000.0));
                        utility -= 10000.0;
                    }
                }
            }
            else
            {
                if (orcSociety == null)
                {
                    reasons?.Add(new ReasonMsg("ERROR: Agent is not of Orc Social Group", -10000.0));
                    utility -= 10000.0;
                }
                else if (orcCulture == null)
                {
                    reasons?.Add(new ReasonMsg("ERROR: Failed to find orc culture", -10000.0));
                    utility -= 10000.0;
                }
            }

            return utility;
        }

        public override double getBodyguardUtility(Unit c, List<ReasonMsg> reasons)
        {
            double val = -10000.0;
            reasons?.Add(new ReasonMsg("Does not guard agents", val));
            return -10000.0;
        }

        public override void turnTickAI()
        {
            ModCore.Get().comLibAI.turnTickAI(this);
        }

        public override bool definesName()
        {
            return true;
        }

        public override string getName()
        {
            return "Orc Spirit Caller";
        }

        public override Sprite getPortraitBackground()
        {
            return map.world.iconStore.standardBack;
        }

        public override Sprite getPortraitForeground()
        {
            return EventManager.getImg("OrcsPlus.Foreground_OrcShaman.png");
        }

        public override Sprite getPortraitForegroundAlt()
        {
            return EventManager.getImg("OrcsPlus.Foreground_OrcShaman.png");
        }

        public override void spendSkillPoint()
        {
            aiGrowthTargetTags.Remove(Tags.INTRIGUE);
            if (aiGrowthTargetTags.ContainsKey(Tags.LORE))
            {
                aiGrowthTargetTags[Tags.LORE] += 20;
                if (map.automatic && map.overmind.autoAI.currentMode == Overmind_Automatic.aiMode.MAGIC)
                {
                    aiGrowthTargetTags[Tags.LORE] += 200;
                }
            }

            if (aiGrowthTargetTags.ContainsKey(Tags.INTRIGUE))
            {
                aiGrowthTargetTags[Tags.INTRIGUE] -= 20;
            }

            base.spendSkillPoint();
            person.skillPoints--;
        }

        public override bool isCommandable()
        {
            bool result = corrupted;

            if (!result)
            {
                foreach (Trait trait in person.traits)
                {
                    if (trait.grantsCommand())
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public override int[] getPositiveTags()
        {
            int[] tags = new int[2] { Tags.ORC, Tags.UNDEAD };
            int[] pTags = person.getTags();

            int[] result = new int[pTags.Length + tags.Length];

            for (int i = 0; i < tags.Length; i++)
            {
                result[i] = tags[i];
            }
            for (int i = 0; i < pTags.Length; i++)
            {
                result[tags.Length + i] = pTags[i];
            }

            return result;
        }
    }
}
