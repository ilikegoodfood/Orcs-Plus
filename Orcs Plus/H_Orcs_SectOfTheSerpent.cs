using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_SectOfTheSerpent : H_SectOfTheSerpent
    {
        public HolyOrder_Orcs orcCulture;

        public H_Orcs_SectOfTheSerpent(HolyOrder_Orcs orcCulture)
            : base(orcCulture)
        {
            this.orcCulture = orcCulture;
        }

        public override string getName()
        {
            return "Sect of the Serpent";
        }

        public override string getDesc()
        {
            return "Your agents with <b>menace</b> above their minimum will transfer it slowly to elders of this orc culture.";
        }

        public override int getMaxPositiveInfluence()
        {
            return 0;
        }

        public override int getMaxNegativeInfluence()
        {
            return -1;
        }

        public override void turnTick(UAA ua)
        {
            if (status < 0)
            {
                foreach (Unit unit in ua.map.units)
                {
                    if (unit.isCommandable() && unit is UA agent && agent.menace > agent.inner_menaceMin)
                    {
                        agent.addMenace(-0.1);
                        ua.inner_menace += 0.1;
                    }
                }
            }
        }

        public override void turnTickTemple(Sub_Temple temple)
        {
            return;
        }
    }
}
