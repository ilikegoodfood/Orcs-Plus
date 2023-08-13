using Assets.Code;
using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public static class GetTags
    {
        public static int[] getTags_OrcsSpecific(int[] baseTags, Challenge challenge)
        {
            int[] output = baseTags;

            SG_Orc orcSociety = challenge.location.soc as SG_Orc;
            if (challenge.location.settlement != null && challenge.location.settlement.subs.Count > 0)
            {
                Sub_OrcWaystation waystation = (Sub_OrcWaystation)challenge.location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(challenge));
                if (waystation != null)
                {
                    orcSociety = waystation.orcSociety;
                }
            }

            if (orcSociety != null)
            {
                output = new int[baseTags.Length + 1];

                for (int i = 0; i < baseTags.Length; i++)
                {
                    output[i] = baseTags[i];
                }
                output[baseTags.Length] = orcSociety.index + 20000;
            }

            return output;
        }

        public static int[] getPositiveTags(int[] baseTags, Challenge challenge)
        {
            switch (challenge)
            {
                case Ch_Orcs_BuildFortress _:
                    return getPositiveTags_OrcsSpecialise(baseTags, challenge);
                case Ch_Orcs_BuildMages _:
                    return getPositiveTags_OrcsSpecialise(baseTags, challenge);
                case Ch_Orcs_BuildMenagerie _:
                    return getPositiveTags_OrcsSpecialise(baseTags, challenge);
                case Ch_Orcs_BuildShipyard _:
                    return getPositiveTags_OrcsSpecialise(baseTags, challenge);
                case Ch_Orcs_Expand _:
                    return getTags_OrcsSpecific(baseTags, challenge);
                case Ch_Orcs_OpportunisticEncroachment _:
                    return getTags_OrcsSpecific(baseTags, challenge);
                case Ch_Orcs_OrganiseTheHorde _:
                    return getTags_OrcsSpecific(baseTags, challenge);
                default:
                    return baseTags;
            }
        }

        public static int[] getPositiveTags_OrcsSpecialise(int[] baseTags, Challenge challenge)
        {
            int[] output = baseTags;

            if (challenge.location.soc is SG_Orc orcSociety)
            {
                output = new int[baseTags.Length + 1];

                for (int i = 0; i < baseTags.Length; i++)
                {
                    output[i] = baseTags[i];
                }
                output[baseTags.Length] = orcSociety.index + 20000;
            }

            return output;
        }

        public static int[] getNegativeTags(int[] baseTags, Challenge challenge)
        {
            switch (challenge)
            {
                case Ch_Orcs_AccessPlunder accessPlunder:
                    return getNegativeTags_AccessPlunder(baseTags, accessPlunder);
                case Ch_Orcs_StealPlunder _:
                    return getTags_OrcsSpecific(baseTags, challenge);
                default:
                    return baseTags;
            }
        }

        public static int[] getNegativeTags_AccessPlunder(int[] baseTags, Ch_Orcs_AccessPlunder challenge)
        {
            int[] output = baseTags;

            if (challenge.location.soc is SG_Orc orcSociety)
            {
                output = new int[baseTags.Length + 1];

                for (int i = 0; i < baseTags.Length; i++)
                {
                    output[i] = baseTags[i];
                }
                output[baseTags.Length] = orcSociety.index + 20000;
            }

            return output;
        }
    }
}
