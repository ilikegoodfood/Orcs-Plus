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
    public class Rt_Orcs_ReclaimHordeBanner : Ritual
    {
        public Rt_Orcs_ReclaimHordeBanner(Location loc)
            : base(loc)
        {

        }

        public override string getName()
        {
            return "Reclaim Horde Banner";
        }

        public override string getDesc()
        {
            return "This upstart has lost his horde banner and is returning home to get a new one.";
        }

        public override string getCastFlavour()
        {
            return "The loss of an upstart's Horde Banner is a great dishonour. To remedy it, they must retur home and proove themselves once again.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.MIGHT;
        }

        public override double getComplexity()
        {
            return 20.0;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 0.0;

            double val = unit.getStatMight();
            if (val >= 1.0)
            {
                msgs?.Add(new ReasonMsg("Stat: Might", val));
                result += val;
            }

            val = unit.getStatCommand();
            if (val >= 1.0)
            {
                msgs?.Add(new ReasonMsg("Stat: Command", val));
                result += val;
            }

            if (result < 1.0)
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                result = 1.0;
            }

            return result;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.i_orcishBanner;
        }

        public override int getInherentDanger()
        {
            return 8;
        }

        public override bool validFor(UA ua)
        {
            if (ua.society is SG_Orc orcSociety && !ua.person.items.Any(i => i is I_HordeBanner banner && banner.orcs == orcSociety))
            {
                if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                {
                    if (ua.location.soc == orcSociety && ua.location.settlement is Set_OrcCamp camp && camp.specialism > 0 && camp.subs.Any(sub => sub is Sub_OrcCultureCapital || sub is Sub_OrcTemple))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;

            if (ua.society is SG_Orc orcSociety)
            {
                if (!ua.person.items.Any(i => i is I_HordeBanner banner && banner.orcs == orcSociety))
                {
                    msgs?.Add(new ReasonMsg("Lost horde banner", 500.0));
                    utility += 500.0;
                }
            }

            return utility;
        }

        public override void complete(UA u)
        {
            if (u.society is SG_Orc orcSociety)
            {
                I_HordeBanner banner = new I_HordeBanner(map, orcSociety, u.location);

                if (u.isCommandable())
                {
                    u.person.gainItem(banner);
                }
                else
                {
                    Item[] items = u.person.items;
                    int itemIndex = 0;
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i] == null)
                        {
                            itemIndex = i;
                            break;
                        }
                    }
                    u.person.items[itemIndex] = banner;
                }
            }
        }
    }
}
