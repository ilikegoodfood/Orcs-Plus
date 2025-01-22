using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Rti_ChangeHorde : Ritual
    {
        public I_HordeBanner banner;

        public UA holder;

        public Rti_ChangeHorde(Location location, I_HordeBanner banner)
            : base(location)
        {
            this.banner = banner;
        }

        public override string getName()
        {
            return $"Join the {banner.orcs.getName()}";
        }

        public override string getDesc()
        {
            return $"Sever all ties with you current Orc Horde and become a member of the {banner.orcs.getName()}. You current horde is unlikley to appreciate this action.";
        }

        public override string getCastFlavour()
        {
            return "Your people have failed you. They have failed to meet your expectations, and they have failed to meet the demands of your dark leige. It is time to find a hrode that will serve you better.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by an Orc Warlord. Requires an orc warbanner of an orc horde that has not been exterminated.";
        }

        public override double getProfile()
        {
            return 0.0;
        }

        public override double getMenace()
        {
            return 50.0;
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            int num = 0;
            num += unit.getStatMight();
            msgs?.Add(new ReasonMsg("Stat: Might", unit.getStatMight()));
            num += unit.getStatCommand();
            msgs?.Add(new ReasonMsg("Stat: Command", unit.getStatCommand()));
            if (num < 1)
            {
                num++;
                msgs?.Add(new ReasonMsg("Base", 1.0));
            }

            return num;
        }

        public override double getComplexity()
        {
            return 100.0;
        }

        public override int getCompletionMenace()
        {
            return 0;
        }

        public override int getCompletionProfile()
        {
            return 10;
        }

        public override bool validFor(UA ua)
        {
            if (holder != ua)
            {
                holder = ua;
            }

            if (banner == null)
            {
                return false;
            }

            if (!(ua is UAE_Warlord warlord) || !warlord.isCommandable())
            {
                return false;
            }

            if (warlord.society != banner.orcs && !banner.orcs.isGone())
            {
                return true;
            }

            return false;
        }

        public override bool valid()
        {
            return true;
        }

        public override Sprite getSprite()
        {
            if (banner != null)
            {
                return banner.map.world.textureStore.flagSigils[banner.orcs.flagSigil];
            }

            return map.world.iconStore.i_orcishBanner;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override void complete(UA u)
        {
            if (banner == null)
            {
                return;
            }

            if (holder != u)
            {
                holder = u;
            }

            SocialGroup oldSocialGroup = u.society;
            SG_Orc abandonnedOrcSociety = u.society as SG_Orc;
            HolyOrder_Orcs abandonnedOrcCulture = u.society as HolyOrder_Orcs;

            if (abandonnedOrcSociety != null || abandonnedOrcCulture != null)
            {
                if (abandonnedOrcCulture == null)
                {
                    ModCore.Get().data.orcSGCultureMap.TryGetValue(abandonnedOrcSociety, out abandonnedOrcCulture);
                }
                else
                {
                    abandonnedOrcSociety = abandonnedOrcCulture.orcSociety;
                }
            }
            

            SG_Orc newOrcSociety = banner.orcs;
            ModCore.Get().data.orcSGCultureMap.TryGetValue(newOrcSociety, out HolyOrder_Orcs newOrcCultue);

            if (!oldSocialGroup.isGone() && oldSocialGroup is Society soc && !soc.isDarkEmpire && !soc.isOphanimControlled && !soc.isDark())
            {
                Person sovereign = soc.getSovreign();
                if (sovereign != null && !sovereign.isDead)
                {
                    sovereign.decreasePreference(u.personID + 10000);
                }
            }

            u.society = newOrcSociety;
            ModCore.Get().TryAddInfluenceGain(newOrcCultue, new ReasonMsg($"{u.getName()} has come to lead us", ModCore.Get().data.influenceGain[ModData.influenceGainAction.WarlordJoin]));

            msgString = $"{u.getName()} has severed all ties with {oldSocialGroup.getName()}, angering them in the process. They now lead the {newOrcSociety.getName()} against the interests of humanity.";

            if (abandonnedOrcCulture != null && !abandonnedOrcSociety.isGone())
            {
                ModCore.Get().TryAddInfluenceGain(abandonnedOrcCulture, new ReasonMsg($"Betrayed by {u.getName()}", ModCore.Get().data.influenceGain[ModData.influenceGainAction.WarlordLeave]));
                if (!u.person.traits.Any(t => t is T_BloodFeud fued && fued.orcSociety == abandonnedOrcSociety))
                {
                    u.person.traits.Add(new T_BloodFeud(abandonnedOrcSociety));
                    map.addUnifiedMessage(u, abandonnedOrcSociety.getCapitalHex().location, "Blood Feud", $"The {abandonnedOrcSociety.getName()} feel betrayed by {u.getName()}'s abandonment. In response to ther actions, the {abandonnedOrcSociety.getName()} have declared a blood feud against them. They must now spend the rest of their days looking over their shoulder for the sudden appearance of an avenging orc upstart.", "Orc Blood Feud");
                }
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[] {
                Tags.ORC,
                Tags.AMBITION
            };
        }

        public override int[] buildNegativeTags()
        {
            return new int[] {
                Tags.COOPERATION
            };
        }
    }
}
