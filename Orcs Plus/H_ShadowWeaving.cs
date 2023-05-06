using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_ShadowWeaving : HolyTenet
    {
        public H_ShadowWeaving(HolyOrder_Orcs ord)
            : base(ord)
        {

        }

        public override string getName()
        {
            return "Shadow Weaving";
        }

        public override string getDesc()
        {
            return "If under human influence, orcs will purge shadow from their lands. As elder infleunce increases, they will allow shadow to spread through their camps. At maximum, orcs will push shadow to surrounding settlements.";
        }

        public override int getMaxPositiveInfluence()
        {
            return 1;
        }

        public override int getMaxNegativeInfluence()
        {
            return -2;
        }
    }
}
