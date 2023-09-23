using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_HarbingersMadness : H_Orcs_GodTenet
    {
        public H_Orcs_HarbingersMadness(HolyOrder_Orcs orcCulture)
            : base(orcCulture)
        {

        }

        public override string getName()
        {
            return "Harbingers of Madness";
        }

        public override string getDesc()
        {
            return "These orcs have been granted a glimpse within Iasturs book, and the knowledge that they gained foretells insanity. Not only will these orcs spread that knowledge to others, driving them mad, but they will, eventually, drive those they interact with insane as well.";
        }
    }
}
