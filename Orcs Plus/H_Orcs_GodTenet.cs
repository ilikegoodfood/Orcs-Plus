using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_GodTenet : HolyTenet
    {
        public H_Orcs_GodTenet(HolyOrder_Orcs orcCulture)
            : base(orcCulture)
        {

        }

        public override int getMaxPositiveInfluence()
        {
            return 0;
        }

        public override int getMaxNegativeInfluence()
        {
            return -2;
        }
    }
}
