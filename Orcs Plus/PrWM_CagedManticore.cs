﻿using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class PrWM_CagedManticore : PrWM_Manticore
    {
        public PrWM_CagedManticore(Location loc)
            : base (loc)
        {
            challenges.Remove (challenge);
            challenge = null;
        }

        public override string getName()
        {
            return "Caged Manticore";
        }

        public override string getDesc()
        {
            return "A wondering manticore has been captured and placed in a cage in the centre of this orc camp. The orcs are guarding it jellously. They clearly have plans for its future.";
        }

        public override void turnTick()
        {
            if (location.settlement is Set_OrcCamp camp && camp.specialism == 0)
            {
                if (location.soc is SG_Orc orcs && ((orcs.actionUnderway is MA_Orcs_GreatConstruction gConstruction && gConstruction.target == location.settlement) || location.properties.Any(pr => pr is Pr_Orcs_GreatConstruction gConstruct && gConstruct.orcSociety == location.soc)))
                {
                    return;
                }
            }

            location.properties.Remove(this);
            location.properties.Add(new PrWM_Manticore(location));
            return;
        }
    }
}
