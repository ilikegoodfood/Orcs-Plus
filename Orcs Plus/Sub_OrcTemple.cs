using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Sub_OrcTemple : Sub_Temple
    {
        public Sub_OrcTemple(Settlement set, HolyOrder order)
            : base(set, order)
        {
            // Remove unwanted challenges added by parent types.
            challenges.Clear();

            // Add new challenges
            challenges.Add(new Ch_H_Orcs_ReprimandUpstart(this, settlement.location));

            if (ModCore.core.data.godTenetTypes.TryGetValue(order.map.overmind.god.GetType(), out Type tenetType) && tenetType != null && tenetType == typeof(H_Orcs_Perfection))
            {
                challenges.Add(new Ch_H_Orcs_PerfectionFestival(settlement.location));
            }
        }

        public override string getHoverOverText()
        {
            return "A great hall built by the \"" + order.getName() + "\" culture, within which its people can gather.";
        }

        public override bool canBeInfiltrated()
        {
            return false;
        }

        public override int getSecurityBoost()
        {
            return 0;
        }
    }
}
