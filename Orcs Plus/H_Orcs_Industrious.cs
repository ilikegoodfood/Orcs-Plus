using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_Industrious :HolyTenet
    {
        public HolyOrder_Orcs orcCulture;

        public H_Orcs_Industrious(HolyOrder_Orcs orcCulture)
            : base (orcCulture)
        {
            this.orcCulture = orcCulture;
        }

        public override string getName()
        {
            return "Industrious";
        }

        public override string getDesc()
        {
            return "The native smallholdings and household industries of the orc hordes offer little for the fielding of armies. With a little nudge, their elders can be tought how to organise industries that are more conducive to warfare.";
        }

        public override int getMaxNegativeInfluence()
        {
            return -2;
        }

        public override int getMaxPositiveInfluence()
        {
            return 0;
        }
    }
}
