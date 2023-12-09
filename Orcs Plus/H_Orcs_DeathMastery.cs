using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class H_Orcs_DeathMastery : H_Orcs_GodTenet
    {
        public UAEN_OrcShaman shaman = null;

        public H_Orcs_DeathMastery(HolyOrder_Orcs orcCulture)
        : base(orcCulture)
        {

        }

        public override string getName()
        {
            return "Death Mastery";
        }

        public override string getDesc()
        {
            return "These orcs have been taught death magic to help them threaten the realms of good, accelerating the search for the grail and thus spreading Ixthus' influence. Perhaps, with time, they too may master immortality.";
        }

        public override void turnTickTemple(Sub_Temple temple)
        {
            if (status < 0 && order is HolyOrder_Orcs orcCulture && temple is Sub_OrcCultureCapital)
            {
                if (shaman == null || shaman.isDead || shaman.hp <= 0)
                {
                    shaman = null;

                    if (Eleven.random.Next(10) < 3)
                    {
                        shaman = new UAEN_OrcShaman(order.map.locations[orcCulture.capital], orcCulture.orcSociety, new Person(orcCulture.map.soc_neutral, orcCulture.map.soc_neutral.houseOrc));
                        order.map.locations[orcCulture.capital].units.Add(shaman);
                        orcCulture.map.units.Add(shaman);
                        orcCulture.agents.Add(shaman);
                        shaman.person.levelUp();
                    }
                }
            }
        }
    }
}
