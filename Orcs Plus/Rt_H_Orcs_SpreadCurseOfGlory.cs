using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CommunityLib.AgentAI;

namespace Orcs_Plus
{
    public class Rt_H_Orcs_SpreadCurseOfGlory : Ritual
    {
        public Rt_H_Orcs_SpreadCurseOfGlory(Location location)
            : base(location)
        {

        }

        public override string getName()
        {
            return "Spread Curse of Glory";
        }

        public override string getDesc()
        {
            return "Spreads the curse of glory to a human bloodline.";
        }

        public override string getCastFlavour()
        {
            return "Their blood has been stirred into a frenzy by the curse. Perhaps it can be passed off to someone else...";
        }

        public override string getRestriction()
        {
            return "May only be used once per sleep cycyle.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Icon_CurseOfGlory.png");
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 0.0;

            double val = unit.getStatLore();

            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", val));
                result += val;
            }
            else
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                result = 1.0;
            }

            return result;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;

            if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_GlorySeeker glory && !glory.cursed && glory.status < -1)
            {
                double val = 100.0;
                msgs?.Add(new ReasonMsg("Base", val));
                utility += val;

                if (!elder.minions.Any(m => m is M_OrcChampion))
                {
                    val = -60.0;
                    msgs?.Add(new ReasonMsg("Unguarded by Champion", val));
                    utility += val;
                }
            }

            return utility;
        }

        public override bool valid()
        {
            return map.overmind.god is God_Eternity;
        }

        public override bool validFor(UA ua)
        {
            bool result = false;

            if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_GlorySeeker glory && glory.status < -1 && !glory.cursed)
            {
                if (!orcCulture.acolytes.Any(a => a != ua && ((a.task is Task_PerformChallenge performChallenge && performChallenge.challenge is Rt_H_Orcs_SpreadCurseOfGlory) || (a.task is Task_GoToPerformChallenge goPerformChallenge && goPerformChallenge.challenge is Rt_H_Orcs_SpreadCurseOfGlory))))
                {
                    if (ua.location.soc != null && orcCulture.getRel(ua.location.soc).state == DipRel.dipState.war)
                    {
                        if (ua.location.settlement is SettlementHuman humanSettlement && humanSettlement.ruler != null && !humanSettlement.ruler.house.curses.Any(curse => curse is Curse_EGlory))
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        public override int getCompletionMenace()
        {
            return 12;
        }

        public override int getCompletionProfile()
        {
            return 9;
        }

        public override bool isChannelled()
        {
            return true;
        }

        public override double getComplexity()
        {
            return 20.0;
        }

        public override void complete(UA u)
        {
            if (u is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_GlorySeeker glory && !glory.cursed && glory.status < -1 && elder.location.settlement is SettlementHuman humanSettlement && humanSettlement.ruler != null && !humanSettlement.ruler.house.curses.Any(curse => curse is Curse_EGlory))
            {
                humanSettlement.ruler.house.curses.Add(new Curse_EGlory());
                glory.cursed = true;
            }
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override int[] buildPositiveTags()
        {
            return new int[] {
                Tags.COMBAT,
                Tags.ORC
            };
        }
    }
}
