using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_MammonClient : H_Orcs_GodTenet
    {
        public H_Orcs_MammonClient(HolyOrder_Orcs orcCulture)
            : base(orcCulture)
        {

        }

        public override string getName()
        {
            return "Clients of Mammon";
        }

        public override string getDesc()
        {
            return "These orcs have been patronised by Mammon. As his influence over them grows, they will begin to produce value from their industries and hire mercenary armies to support them in conflicts.";

        }
    }
}
