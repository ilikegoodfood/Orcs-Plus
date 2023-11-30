using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_Curseweaving : H_Orcs_GodTenet
    {
        public int usedCount = 0;

        public H_Orcs_Curseweaving(HolyOrder_Orcs orcSociety)
            : base (orcSociety)
        {

        }

        public override string getName()
        {
            return "Curse Weaving";
        }

        public override string getDesc()
        {
            return "These orcs have been taught to instil fear into the very souls of their enemies. They will use this knowledge to condemn the houses of their enemies to an eternity of weakness and disgrace.";
        }
    }
}
