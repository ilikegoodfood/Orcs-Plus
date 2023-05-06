using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class D_Blank : EntityDesire
    {
        public D_Blank(Map map, DivineEntity parent)
            : base(map, parent)
        {
            
        }

        public override string getName()
        {
            return "Without Desire";
        }

        public override void turnTick()
        {
            entity.mood = 0.0;
        }
    }
}
