using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_Perfection : H_Orcs_GodTenet
    {
        public double thresholdMinor = 150.0;

        public double thresholdMajor = 300.0;

        public H_Orcs_Perfection(HolyOrder_Orcs orcCulture)
            : base(orcCulture)
        {

        }

        public override string getName()
        {
            return "Perfection";
        }

        public override string getDesc()
        {
            return "Ophanim's will drives these dissorganized orcs to seek a state of harmonic perfection. They work, build and train with a mindless zeal that makes them a force to be recond with. With enough devotion, Ophanim will be able to convert their armies into perfect hordes.";
        }
    }
}
