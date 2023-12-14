using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Rt_H_Orcs_SpreadCurseBrokenSpirit : Ritual
    {
        public Rt_H_Orcs_SpreadCurseBrokenSpirit(Location location)
            : base(location)
        {

        }
            public override string getName()
        {
            return "Curseweaving: Break Spirit";
        }

        public override string getDesc()
        {
            return "Inflicts the house of the target ruler with a terrible curse, filling them, and their family, with an unceasing and cripling fear.";
        }

        public override string getCastFlavour()
        {
            return "They jump at the sound of a door closing in the night, leave lanterns lit in the small hours, and have guards preceed them into every room. The fear that gnaws at their innermost thoughts is indescribable...";
        }

        public override string getRestriction()
        {
            return "May only be used once for each level of elder alignment that the Curseweaving tenet has.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Icon_BrokenSpirit.png");
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

            if (ua.location.soc is Society society && ua.location.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
            {
                if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_Curseweaving curse && curse.status < 0 && curse.usedCount < Math.Abs(curse.status))
                {
                    double val = 100.0;
                    msgs?.Add(new ReasonMsg("Base", val));
                    utility += val;

                    if (ua.location == society.getCapital())
                    {
                        val = 30;
                        msgs?.Add(new ReasonMsg("Target is Soverign", val));
                        utility += val;
                    }

                    if (!elder.minions.Any(m => m is M_OrcChampion))
                    {
                        val = -200.0;
                        msgs?.Add(new ReasonMsg("Unguarded by Champion", val));
                        utility += val;
                    }
                }
            }

            return utility;
        }

        public override bool validFor(UA ua)
        {
            bool result = false;

            if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_Curseweaving curse && curse.status < 0 && curse.usedCount < Math.Abs(curse.status))
            {
                if (!orcCulture.acolytes.Any(a => a != ua && ((a.task is Task_PerformChallenge performChallenge && performChallenge.challenge is Rt_H_Orcs_SpreadCurseBrokenSpirit) || (a.task is Task_GoToPerformChallenge goPerformChallenge && goPerformChallenge.challenge is Rt_H_Orcs_SpreadCurseBrokenSpirit))))
                {
                    if (ua.location.soc != null && orcCulture.getRel(ua.location.soc).state == DipRel.dipState.war)
                    {
                        if (ua.location.settlement is SettlementHuman humanSettlement && humanSettlement.ruler != null && !humanSettlement.ruler.house.curses.Any(curse2 => curse2 is Curse_BrokenSpirit))
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
            if (u is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_Curseweaving curse && curse.usedCount < Math.Abs(curse.getMaxNegativeInfluence()) && elder.location.settlement is SettlementHuman humanSettlement && humanSettlement.ruler != null && !humanSettlement.ruler.house.curses.Any(curse2 => curse2 is Curse_BrokenSpirit))
            {
                humanSettlement.ruler.house.curses.Add(new Curse_BrokenSpirit());
                curse.usedCount++;
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
