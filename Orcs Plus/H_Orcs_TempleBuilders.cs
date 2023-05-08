using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_TempleBuilders : H_TempleBuilders
    {
        public HolyOrder_Orcs orcCulture;

        public H_Orcs_TempleBuilders (HolyOrder_Orcs orcCulture)
            : base (orcCulture)
        {
            this.orcCulture = orcCulture;
        }

        public override double addUtility(Challenge c, UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;

            if (c is Ch_H_Orcs_BuildTemple)
            {
                double val = 0.0;
                foreach (UAA acolyte in orcCulture.acolytes)
                {
                    if (acolyte.task is CommunityLib.Task_GoToPerformChallengeAtLocation perfomrChallenge && perfomrChallenge.challenge is Ch_H_Orcs_BuildTemple)
                    {
                        val = -50.0;
                        msgs?.Add(new ReasonMsg("Already Being Done", val));
                        utility += val;
                    }
                }

                val = status * 20;

                if (val > 0)
                {
                    msgs?.Add(new ReasonMsg("Tenet: " + getName(), val));
                    utility += val;
                }
            }

            return utility;
        }
    }
}
