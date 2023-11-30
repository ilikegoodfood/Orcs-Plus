using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class T_BrokenSpirit : Trait
    {
        public override string getName()
        {
            return "Broken Spirit";
        }

        public override string getDesc()
        {
            return "This person is haunted by fear. A deep-seated, festering, worrysome beast that gnaws, everhungry, at their mind and spirit. They have a deep aversion to risk and conflict that no mortal remedy can cure them of.";
        }

        public override int getMaxLevel()
        {
            return 1;
        }

        public override int getSecurityChangeFromRuler(SettlementHuman settlementHuman, Person ruler)
        {
            return 1;
        }

        public override double getUtilityChanges(Challenge c, UA uA, List<ReasonMsg> reasons)
        {
            double utility = 0.0;
            int count = 0;

            foreach (int tag in c.getPositiveTags())
            {
                if (tag == Tags.AMBITION || tag == Tags.COMBAT || tag == Tags.DANGER)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                utility = count * -20.0;
                reasons?.Add(new ReasonMsg("Broken Spirit", utility));
            }

            return utility;
        }

        public override double getActionUtility(Person person, SettlementHuman hum, Assets.Code.Action act, List<ReasonMsg> reasons)
        {
            if (act is Act_FundArmy || act is Act_Muster || act is Act_RaiseArmy)
            {
                return 0.0;
            }

            double utility = 0.0;
            int count = 0;

            foreach (int tag in act.getPositiveTags())
            {
                if (tag == Tags.AMBITION || tag == Tags.COMBAT || tag == Tags.DANGER)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                utility = count * -20.0;
                reasons?.Add(new ReasonMsg("Broken Spirit", utility));
            }

            return utility;
        }
    }
}
