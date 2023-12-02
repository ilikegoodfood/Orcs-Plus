using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;

namespace Orcs_Plus
{
    public class H_Orcs_ShadowWarriors : H_Orcs_GodTenet
    {
        public H_Orcs_ShadowWarriors(HolyOrder_Orcs orcCulture)
            : base(orcCulture)
        {

        }

        public override string getName()
        {
            return "Shadow Warriors";
        }

        public override string getDesc()
        {
            return "Directed and honed by the influence of an elder god, these orc warriors are most deadly when their camp is fully enshadowed, dealing more damage to their enemies, and, at maximum elder alignment, shrugging off harm.";
        }
    }
}
