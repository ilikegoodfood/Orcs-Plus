using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_Expansionism : HolyTenet
    {
        public HolyOrder_Orcs orcCulture;

        public H_Orcs_Expansionism(HolyOrder_Orcs orcCulture)
            : base(orcCulture)
        {
            this.orcCulture = orcCulture;
        }

        public override string getName()
        {
            return "Expansionism";
        }

        public override string getDesc()
        {
            return "Orc cultures are traditionally small, loosely organised affairs, that are incapable of mass expansion. When human aligned, the orc culture will avoid expanding to new lands. When elder aligned, they will be able to spread more before they stop expanding.";
        }

        public override int getMaxNegativeInfluence()
        {
            return -2;
        }

        public override int getMaxPositiveInfluence()
        {
            return 1;
        }
    }
}
