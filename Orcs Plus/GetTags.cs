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
        public static int[] getPositiveTags(int[] baseTags, Challenge challenge)
        {
            switch (challenge)
            {
                case Ch_Orcs_BuildFortress _:
                    return appendOrcSGTag(baseTags, challenge);
                case Ch_Orcs_BuildMages _:
                    return appendOrcSGTag(baseTags, challenge);
                case Ch_Orcs_BuildMenagerie _:
                    return appendOrcSGTag(baseTags, challenge);
                case Ch_Orcs_BuildShipyard _:
                    return appendOrcSGTag(baseTags, challenge);
                case Ch_Orcs_Expand _:
                    return appendOrcSGTag(baseTags, challenge);
                case Ch_Orcs_OpportunisticEncroachment _:
                    return appendOrcSGTag(baseTags, challenge);
                case Ch_Orcs_OrganiseTheHorde _:
                    return appendOrcSGTag(baseTags, challenge);
                case Rti_Orc_AttackHere _:
                    return appendOrcSGTag_Warbanner(baseTags, challenge);
                case Rti_Orc_CeaseWar _:
                    return appendOrcSGTag_Warbanner(baseTags, challenge);
                case Rti_Orc_UniteTheHordes _:
                    return appendOrcSGTag_Warbanner(baseTags, challenge);
                default:
                    return baseTags;
            }
        }

        public static int[] appendOrcSGTag_Warbanner(int[] baseTags, Challenge challenge)
        {
            int[] output = baseTags;

            I_HordeBanner banner = null;
            if (challenge is Rti_Orc_AttackHere attack)
            {
                banner = attack.caster;
            }
            else if (challenge is Rti_Orc_CeaseWar endWar)
            {
                banner = endWar.caster;
            }
            else if (challenge is Rti_Orc_UniteTheHordes unite)
            {
                banner = unite.caster;
            }

            if (banner != null)
            {
                output = new int[baseTags.Length + 1];

                for (int i = 0; i < baseTags.Length; i++)
                {
                    output[i] = baseTags[i];
                }
                output[baseTags.Length] = banner.orcs.index + 20000;
            }

            return output;
        }

        public static int[] getNegativeTags(int[] baseTags, Challenge challenge)
        {
            switch (challenge)
            {
                case Ch_Orcs_AccessPlunder accessPlunder:
                    return appendOrcSGTag(baseTags, accessPlunder);
                case Ch_Orcs_StealPlunder _:
                    return appendOrcSGTag(baseTags, challenge);
                default:
                    return baseTags;
            }
        }

        public static int[] appendOrcSGTag(int[] baseTags, Challenge challenge)
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
    }
}
