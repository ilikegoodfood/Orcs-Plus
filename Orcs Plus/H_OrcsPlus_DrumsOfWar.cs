using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_OrcsPlus_DrumsOfWar : HolyTenet
    {
        public H_OrcsPlus_DrumsOfWar(HolyOrder_OrcsPlus_Orcs ord)
            : base(ord)
        {

        }

        public override string getName()
        {
            return "Drums of War";
        }

        public override string getDesc()
        {
            return "Members of this orc culture will attack all outsiders who enter their domain, and will consider all peoples valid targets for raiding and war. When under human influence, they will tolerate humans, both within their lands, and as neighbours. When under elder inlfuence, they will tolerate its agents and the dark empire.";
        }

        public override int getMaxPositiveInfluence()
        {
            return 2;
        }

        public override int getMaxNegativeInfluence()
        {
            return -2;
        }

        public override bool structuralTenet()
        {
            return true;
        }
    }
}
