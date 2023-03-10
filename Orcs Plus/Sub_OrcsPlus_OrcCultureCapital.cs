using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    internal class Sub_OrcsPlus_OrcCultureCapital : Sub_HolyOrderCapital
    {
        public Sub_OrcsPlus_OrcCultureCapital(Settlement set, HolyOrder_OrcsPlus_Orcs order)
            : base(set, order)
        {
            // Remove unwanted challenges added by parent types.
            challenges.Clear();

            this.order = order;

            // Notify the holy order that this temple exists.
            order.newTempleCreated(this);

            // Add new challenges
        }

        public override string getName()
        {
            return "Seat of the Elders";
        }

        public override string getHoverOverText()
        {
            return "The seat of power of the elders of the \"" + order.getName() + "\" culture, from where all its decisions are made. Here you may influence the tenets of their doctrine, changing how the culture sees their duty towards the world, and so how they act.";
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
            return settlement.map.world.iconStore.orcDefences;
        }

        public override void turnTick()
        {
            foreach (HolyTenet tenet in order.tenets)
            {
                tenet.turnTickTemple(this);
            }
        }
    }
}
