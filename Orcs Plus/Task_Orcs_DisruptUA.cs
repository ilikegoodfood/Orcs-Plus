using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Task_Orcs_DisruptUA : Task_DisruptUA
    {
        public SG_Orc orcSociety;

        public HolyOrder_Orcs orcCulture;

        public Task_Orcs_DisruptUA(UA us, UA them)
            : base(us, them)
        {
            orcSociety = us.society as SG_Orc;
            orcCulture = us.society as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }
            else if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
            }
        }

        public override void turnTick(Unit unit)
        {
            if (orcSociety == null || orcCulture == null || other.isDead || !(unit is UA ua))
            {
                unit.task = null;
                return;
            }
            
            if ((other.task is Task_PerformChallenge tPerformChallenge && (other.location.soc == orcSociety || other.location.soc == orcCulture) && !(tPerformChallenge.challenge is Ch_LayLow || tPerformChallenge.challenge is Ch_LayLowWilderness))
                || (other.task is Task_GoToPerformChallenge tGoPerformChallenge && (tGoPerformChallenge is Task_GoToPerformChallengeAtLocation tGoPerformChallengeAtLocation && (tGoPerformChallengeAtLocation.target.soc == orcSociety || tGoPerformChallengeAtLocation.target.soc == orcCulture) || (!(tGoPerformChallenge.challenge is Ritual) && (tGoPerformChallenge.challenge.location.soc == orcSociety || tGoPerformChallenge.challenge.location.soc == orcCulture))))
                && !(tGoPerformChallenge.challenge is Ch_LayLow || tGoPerformChallenge.challenge is Ch_LayLowWilderness))
            {
                base.turnTick(unit);
            }
            else
            {
                unit.task = null;
            }
        }
    }
}
