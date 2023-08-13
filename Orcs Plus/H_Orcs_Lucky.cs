using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_Lucky : H_Orcs_GodTenet
    {
        public H_Orcs_Lucky(HolyOrder_Orcs orcCulture)
            : base(orcCulture)
        {

        }

        public override string getName()
        {
            return "Orcs' Gambit";
        }

        public override string getDesc()
        {
            return "These orcs have been granted inhuman luck. Cards played on orc units have a high chance of not being consumed.";
        }
    }
}
