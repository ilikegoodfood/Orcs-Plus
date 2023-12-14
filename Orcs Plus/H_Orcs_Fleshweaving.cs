using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_Fleshweaving : H_Orcs_GodTenet
    {
        public H_Orcs_Fleshweaving(HolyOrder_Orcs orcCUlture)
            : base(orcCUlture)
        {

        }

        public override string getName()
        {
            return "Fleshweaving";
        }

        public override string getDesc()
        {
            return "The flesh of the orcs is hardy and alien, resiting all efforts at manipulation. Despite this, the orcs could be taught to manipulate flesh, and by blending it with their own magics, they may yet join Escamrak.";
        }
    }
}
