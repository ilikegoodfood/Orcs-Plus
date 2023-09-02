using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class UM_PerfectRaiders : UM_OrcRaiders
    {
        public double strengthFactor = 2.0;

        public double goldGainFactor = 2.0;

        public UM_PerfectRaiders(Location location, SocialGroup orcSociety)
            : base(location, orcSociety)
        {

        }

        public override string getName()
        {
            return person.getName() + "'s Perfect Raiders";
        }

        public override Sprite getPortraitForeground()
        {
            return EventManager.getImg("OrcsPlus.Icon_PerfectHorde.png");
        }

        new public void assignMaxHP()
        {
            if (person != null)
            {
                base.assignMaxHP();
                maxHp = (int)Math.Floor(maxHp * strengthFactor);
            }
        }


    }
}
