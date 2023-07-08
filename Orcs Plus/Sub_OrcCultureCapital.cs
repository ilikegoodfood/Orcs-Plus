using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Sub_OrcCultureCapital : Sub_HolyOrderCapital
    {
        public Sub_OrcCultureCapital(Settlement set, HolyOrder_Orcs order)
            : base(set, order)
        {
            // Remove unwanted challenges added by parent types.
            challenges.Clear();

            this.order = order;

            // Add new challenges
            challenges.Add(new Ch_H_Orcs_ReprimandUpstart(this, settlement.location));

            if (ModCore.core.data.godTenetTypes.TryGetValue(order.map.overmind.god.GetType(), out Type tenetType) && tenetType != null && tenetType == typeof(H_Orcs_Perfection))
            {
                challenges.Add(new Ch_H_Orcs_PerfectionFestival(settlement.location));
            }
        }

        public override string getName()
        {
            return "Seat of the Elders";
        }

        public override string getHoverOverText()
        {
            return "The seat of power of the elders of the \"" + order.getName() + "\" culture, from where all its decisions are made.";
        }

        public override bool canBeInfiltrated()
        {
            return false;
        }

        public override int getSecurityBoost()
        {
            return 0;
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("OrcsPlus.Icon_GreatHall.png");
        }
    }
}
