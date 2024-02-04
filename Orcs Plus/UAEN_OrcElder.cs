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
    public class UAEN_OrcElder : UAEN
    {
        public Rt_H_Orcs_GiftGold rt_giftGold = null;

        public Rt_H_Orcs_SpreadCurseOfGlory rt_spreadCurseGlory = null;

        public Rt_H_Orcs_SpreadCurseBrokenSpirit rt_spreadCurseBroken = null;

        public Sprite foreground = null;

        public Sprite foregroundAlt = null;

        public UAEN_OrcElder(Location loc, HolyOrder sg)
            : base(loc, sg)
        {
            // Raises stats granted by base: Might 1-3, Intrigue 1-3, Lore 2-3, Command 2-3
            // New Ranges: Might 2-3, Intrigue 1-2, Lore 2-3, Command 3-5
            person.stat_might = 2 + Eleven.random.Next(2);
            person.stat_intrigue = 1 + Eleven.random.Next(2);
            person.stat_command = 3 + Eleven.random.Next(3);
            person.hasSoul = false;
            person.traits.Add(new T_ReveredElder());
            person.species = map.species_orc;

            rt_giftGold = new Rt_H_Orcs_GiftGold(loc);
            rituals.Add(rt_giftGold);

            if (sg is HolyOrder_Orcs orcCulture)
            {
                if (orcCulture.tenet_god is H_Orcs_GlorySeeker)
                {
                    rt_spreadCurseGlory = new Rt_H_Orcs_SpreadCurseOfGlory(loc);
                    rituals.Add(rt_spreadCurseGlory);
                }
                else if (orcCulture.tenet_god is H_Orcs_Curseweaving)
                {
                    rt_spreadCurseBroken = new Rt_H_Orcs_SpreadCurseBrokenSpirit(loc);
                    rituals.Add(rt_spreadCurseBroken);
                }
            }

            setPortraitForeground();
        }

        public UAEN_OrcElder(Location loc, HolyOrder sg, Person p)
            : base(loc, sg, p)
        {
            // Raises stats granted by base: Might 1-3, Intrigue 1-3, Lore 2-3, Command 2-3
            // New Ranges: Might 2-3, Intrigue 1-2, Lore 2-3, Command 3-5
            person.stat_might = 2 + Eleven.random.Next(2);
            person.stat_intrigue = 1 + Eleven.random.Next(2);
            person.stat_command = 3 + Eleven.random.Next(3);
            person.hasSoul = false;
            person.traits.Add(new T_ReveredElder());
            person.species = map.species_orc;

            rt_giftGold = new Rt_H_Orcs_GiftGold(loc);
            rituals.Add(rt_giftGold);

            if (sg is HolyOrder_Orcs orcCulture)
            {
                if (orcCulture.tenet_god is H_Orcs_GlorySeeker)
                {
                    rt_spreadCurseGlory = new Rt_H_Orcs_SpreadCurseOfGlory(loc);
                    rituals.Add(rt_spreadCurseGlory);
                }
                else if (orcCulture.tenet_god is H_Orcs_Curseweaving)
                {
                    rt_spreadCurseBroken = new Rt_H_Orcs_SpreadCurseBrokenSpirit(loc);
                    rituals.Add(rt_spreadCurseBroken);
                }
            }

            setPortraitForeground();
        }

        public void setPortraitForeground()
        {
            HolyOrder_Orcs orcCulture = society as HolyOrder_Orcs;
            if (orcCulture == null)
            {
                foreground = EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
                foregroundAlt = EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
            }

            if (orcCulture.genderExclusive == -1)
            {
                foreground = EventManager.getImg("OrcsPlus.Foreground_OrcElder_F.png");
                foregroundAlt = EventManager.getImg("OrcsPlus.Foreground_OrcElder_F.png");
            }
            else if (orcCulture.genderExclusive == 1)
            {
                foreground = EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
                foregroundAlt = EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
            }
            else
            {
                if (person.index % 2 == 0)
                {
                    foreground = EventManager.getImg("OrcsPlus.Foreground_OrcElder_F.png");
                    foregroundAlt = EventManager.getImg("OrcsPlus.Foreground_OrcElder_F.png");
                }
                else
                {
                    foreground = EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
                    foregroundAlt = EventManager.getImg("OrcsPlus.Foreground_OrcElder.png");
                }
            }
        }

        public override void turnTick(Map map)
        {
            base.turnTick(map);

            if (society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_Fleshweaving fleshweaving)
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
                                FI_BonusType.SetValue(fleshStatBonusTrait, "Command");
                            }

                            fleshStatBonusTrait.level = -fleshweaving.status;
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

            SG_Orc orcSociety = null;
            HolyOrder_Orcs orcCulture = society as HolyOrder_Orcs;

            if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
            }

            if (orcSociety != null && !orcSociety.isGone() && orcCulture != null)
            {
                Type dominionBanner = null;
                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
                {
                    intDataCCC.typeDict.TryGetValue("Banner", out dominionBanner);
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
                        else if (dominionBanner != null && agent.person.items.Any(i => i != null && (i.GetType() == dominionBanner || i.GetType().IsSubclassOf(dominionBanner))))
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

            SG_Orc orcSociety = null;
            HolyOrder_Orcs orcCulture = society as HolyOrder_Orcs;

            if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
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
                            Type dominionBanner = null;
                            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
                            {
                                intDataCCC.typeDict.TryGetValue("Banner", out dominionBanner);
                            }

                            foreach (Item item in target.person.items)
                            {
                                if (item != null)
                                {
                                    if (dominionBanner != null && (item.GetType() == dominionBanner || item.GetType().IsSubclassOf(dominionBanner)))
                                    {
                                        reasons?.Add(new ReasonMsg("Orcs will not disrupt banner bearer", -10000.0));
                                        utility -= 10000.0;
                                        otherIsTarget = false;
                                        break;
                                    }
                                    else
                                    {
                                        I_HordeBanner banner = item as I_HordeBanner;
                                        if (banner?.orcs == orcSociety && (target.society == null || society.getRel(target.society).state != DipRel.dipState.war) && (feud == null))
                                        {
                                            reasons?.Add(new ReasonMsg("Orcs will not disrupt banner bearer", -10000.0));
                                            utility -= 10000.0;
                                            otherIsTarget = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (otherIsTarget)
                        {
                            double val = map.turn - person.disruptionTurnLast;
                            if (val < map.param.utility_ua_disruptSoftTimeoutTimer)
                            {
                                val = map.param.utility_ua_disruptSoftTimeoutTimer - val;
                                val *= map.param.utility_ua_disruptSoftTimeoutMult;
                                val *= -30.0;

                                reasons?.Add(new ReasonMsg("Have recently disrupted", val));
                                utility += val;
                            }

                            if (disruptionExhaustion > 0)
                            {
                                val = -disruptionExhaustion;
                                reasons?.Add(new ReasonMsg("Disrupted Recently", val));
                                utility += val;
                            }

                            val = -map.getStepDist(location, target.location);
                            reasons?.Add(new ReasonMsg("Distance", val));
                            utility += val;

                            int disruptCount = 0;
                            foreach (UAEN_OrcElder elder in orcCulture.acolytes)
                            {
                                if (elder.task is Task_DisruptUA tDisrupt && tDisrupt.other == target)
                                {
                                    disruptCount++;
                                }
                            }

                            if (disruptCount > 0)
                            {
                                val = disruptCount * -50.0;
                                reasons?.Add(new ReasonMsg("Fellow elders are already disrupting", val));
                                utility += val;
                            }

                            if (target.task is Task_PerformChallenge tPerformChallenge && (target.location.soc == orcSociety || target.location.soc == orcCulture) && !(tPerformChallenge.challenge is Ch_LayLow || tPerformChallenge.challenge is Ch_LayLowWilderness))
                            {
                                val = -50.0;
                                foreach (Unit unit in target.location.units)
                                {
                                    if (!(unit is UAEN_OrcElder elder) || elder.society != orcCulture)
                                    {
                                        if (unit.task is Task_DisruptUA tDisrupt && tDisrupt.other == target)
                                        {
                                            reasons?.Add(new ReasonMsg(unit.getName() + " already disrupting", val));
                                            utility += val;
                                        }
                                    }
                                }

                                val = 20;
                                reasons?.Add(new ReasonMsg("Agent is interfereing with " + orcSociety.getName() + "'s territory", val));
                                utility += val;

                                if (tPerformChallenge.challenge is Ch_Orcs_StealPlunder)
                                {
                                    val = 40;
                                    reasons?.Add(new ReasonMsg("Agent stealing gold from " + orcSociety.getName() + "'s plunder", val));
                                    utility += val;
                                }
                            }
                            else if (target.task is Task_GoToPerformChallenge tGoPerformChallenge
                                && ((tGoPerformChallenge is Task_GoToPerformChallengeAtLocation tGoPerformChallengeAtLocation && (tGoPerformChallengeAtLocation.target.soc == orcSociety || tGoPerformChallengeAtLocation.target.soc == orcCulture)) || (!(tGoPerformChallenge.challenge is Ritual) && (tGoPerformChallenge.challenge.location.soc == orcSociety || tGoPerformChallenge.challenge.location.soc == orcCulture)))
                                && !(tGoPerformChallenge.challenge is Ch_LayLow || tGoPerformChallenge.challenge is Ch_LayLowWilderness))
                            {
                                val = 20;
                                reasons?.Add(new ReasonMsg("Agent is travelling to interfere with " + orcSociety.getName() + "'s territory", val));
                                utility += val;

                                if (tGoPerformChallenge.challenge is Ch_Orcs_StealPlunder)
                                {
                                    val = 40;
                                    reasons?.Add(new ReasonMsg("Agent is travelling to steal gold from " + orcSociety.getName() + "'s plunder", val));
                                    utility += val;
                                }
                            }
                            else
                            {
                                reasons?.Add(new ReasonMsg("Agent is not travelling to interfere with " + orcSociety.getName() + "'s territory", -10000.0));
                                utility -= 10000.0;
                                return utility;
                            }

                            if (target.location.soc != orcSociety && target.location.soc != orcCulture)
                            {
                                val = -25;
                                reasons?.Add(new ReasonMsg("Target is outside of " + orcSociety.getName() + "'s territory", val));
                                utility += val;
                            }

                            utility += person.getTagUtility(new int[]
                            {
                                Tags.COMBAT,
                                Tags.CRUEL,
                                Tags.DANGER
                            }, new int[0], reasons);
                            utility += person.getTagUtility(target.getPositiveTags(), target.getNegativeTags(), reasons);

                            if (target.person != null && (person.hates.Contains(target.person.index + 10000) || person.extremeHates.Contains(target.person.index + 10000)))
                            {
                                val = map.param.utility_ua_disruptDislikedPerson;
                                reasons?.Add(new ReasonMsg("Person I dislike is doing something", val));
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

                            foreach (ModKernel modKernel in map.mods)
                            {
                                utility = modKernel.unitAgentAIDisrupt(map, target, reasons, utility);
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

            SG_Orc orcSociety = null;
            HolyOrder_Orcs orcCulture = society as HolyOrder_Orcs;

            if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
            }

            if (orcSociety != null && !orcSociety.isGone() && orcCulture != null)
            {
                if ((orcCulture.tenet_god is H_Orcs_GlorySeeker glory && glory.status < 0) || (orcCulture.tenet_god is H_Orcs_BloodOffering blood && blood.status < 0 && (c.person?.hasSoul ?? false)))
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
                                Type dominionBanner = null;
                                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
                                {
                                    intDataCCC.typeDict.TryGetValue("Banner", out dominionBanner);
                                }

                                foreach (Item item in target.person.items)
                                {
                                    if (item != null)
                                    {
                                        if (dominionBanner != null && (item.GetType() == dominionBanner || item.GetType().IsSubclassOf(dominionBanner)))
                                        {
                                            reasons?.Add(new ReasonMsg("Orcs will not attack banner bearer", -10000.0));
                                            utility -= 10000.0;
                                            return utility;
                                        }
                                        else
                                        {
                                            I_HordeBanner banner = item as I_HordeBanner;
                                            if (banner?.orcs == orcSociety && (target.society == null || society.getRel(target.society).state != DipRel.dipState.war) && (feud == null))
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
            return "Orc Elder";
        }

        public override Sprite getPortraitBackground()
        {
            return map.world.iconStore.standardBack;
        }

        public override Sprite getPortraitForeground()
        {
            return foreground;
        }

        public override Sprite getPortraitForegroundAlt()
        {
            return foregroundAlt;
        }

        public override void spendSkillPoint()
        {
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
            int[] tags = new int[2] { Tags.ORC, Tags.RELIGION };
            int[] pTags = person.getTags();

            int[] result = new int[pTags.Length + tags.Length];

            for(int i = 0; i < tags.Length; i++)
            {
                result[i] = tags[i];
            }
            for(int i = 0; i < pTags.Length; i++)
            {
                result[tags.Length + i] = pTags[i];
            }

            return result;
        }
    }
}
