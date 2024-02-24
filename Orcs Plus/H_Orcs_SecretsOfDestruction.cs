using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_SecretsOfDestruction : H_Orcs_GodTenet
    {
        public H_Orcs_SecretsOfDestruction(HolyOrder_Orcs orcCulture)
            :base(orcCulture)
        {

        }

        public override string getName()
        {
            return "Secrets of Destruction";
        }

        public override string getDesc()
        {
            return "The Evil Beneath demands destruction, and offers up knowledge of explosives to those willing to fulfil its demand.";
        }
    }
}
