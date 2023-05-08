using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_Intolerance : HolyTenet
    {
        public H_Orcs_Intolerance(HolyOrder_Orcs ord)
            : base(ord)
        {

        }

        public override string getName()
        {
            return "Intolerance";
        }

        public override string getDesc()
        {
            return "Orcs or notoriously intolerant of outsiders, opportunistically attacking anyone who comes too close. With some influence, they can be discouraged from attacking others.";
        }

        public override int getMaxPositiveInfluence()
        {
            return 2;
        }

        public override int getMaxNegativeInfluence()
        {
            return -2;
        }
    }
}
