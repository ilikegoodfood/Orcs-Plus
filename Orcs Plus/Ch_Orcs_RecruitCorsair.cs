using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Ch_Orcs_RecruitCorsair : Ch_RecruitMinion
    {
        public Ch_Orcs_RecruitCorsair(Location loc, Minion example, int tyernaryGoodness, Subsettlement reqInf = null, Settlement reqInfSet = null)
            : base(loc, example, tyernaryGoodness, reqInf, reqInfSet)
        {

        }

        public override string getRestriction()
        {
            string text = "";

            if (reqInfSet != null)
            {
                text = " Requires " + reqInfSet.getName() + " to be an orc shipyard.";
            }

            return text + " " + base.getRestriction();
        }

        public override bool valid()
        {
            return location.settlement is Set_OrcCamp camp && camp.specialism == 5;
        }
    }
}
