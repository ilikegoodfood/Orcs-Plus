using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_GlorySeeker : H_Orcs_GodTenet
    {
        public bool cursed = false;

        public H_Orcs_GlorySeeker(HolyOrder_Orcs orcCulture)
            :base (orcCulture)
        {

        }

        public override string getName()
        {
            return "Glory Seekers";
        }

        public override string getDesc()
        {
            return "These orcs have been cursed with a burning passion for glory. Elders and Spirit Callers may engage in combat of their own volition, and upstarts are more likely to pick fights, even if the odds are stacked against them. As this passions increases, they gain the ability to spread the curse, once per sleep cycle, to a human bloodline.";
        }
    }
}
