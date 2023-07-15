using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;

namespace Orcs_Plus
{
    public class H_Orcs_InsectileSymbiosis : H_Orcs_GodTenet
    {
        public H_Orcs_InsectileSymbiosis(HolyOrder_Orcs orcs)
            : base(orcs)
        {

        }

        public override string getName()
        {
            return "Insectile Symbiosis";
        }

        public override string getDesc()
        {
            return "These orcs exist in a strange state of symbiosis with Cordyceps and the insects that it controls. When elder aligned, orc armies will gain sensitivity to the vespidic attack pheromone, as well as emit limited pheromones of their own. When fully elder aligned, Cordyceps-infected insects will be allowed to mature within their bodies, emerging upon the orcs' natural death.";
        }
    }
}
