using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_BloodOffering : H_Orcs_GodTenet
    {
        public H_Orcs_BloodOffering(HolyOrder_Orcs orcCulture)
            : base (orcCulture)
        {

        }

        public override string getName()
        {
            return "Blood Offerings";
        }

        public override string getDesc()
        {
            return "The violence of orcs can easily be manipulated as a means to gain bloodstains. Orc agents will cause others to gain, and recieve, progressively more bloodstains as this tenet becomes more elder aligned.";
        }
    }
}
