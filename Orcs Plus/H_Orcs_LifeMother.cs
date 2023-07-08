using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_LifeMother : H_Orcs_GodTenet
    {
        public H_Orcs_LifeMother(HolyOrder_Orcs orcCulture)
            : base(orcCulture)
        {

        }

        public override string getName()
        {
            return "Life Mother";
        }

        public override string getDesc()
        {
            return "Vinerva is the mother of all natural things, including orcs. She will be worshipped, protected, nurtured, and Vinerva will grant unique gifts to her orc children, which they will accept with reverence.";
        }
    }
}
