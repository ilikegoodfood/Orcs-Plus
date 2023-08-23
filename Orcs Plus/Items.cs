using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public static class Items
    {
        public static Item getOrcItemFromPool2(Map map, SG_Orc orcSociety, Location location)
        {
            int index = Eleven.random.Next(10);

            if (index == 0)
            {
                return new I_DrinkingHorn(map, location);
            }
            else if (index == 1)
            {
                return new I_SnakeskinArmour(map);
            }
            else
            {
                return new I_SnakeskinArmour(map);
            }
        }

        public static Item getOrcItemFromPool3(Map map, SG_Orc orcSociety, Location location)
        {
            int index = Eleven.random.Next(10);

            if (index == 0)
            {
                return new I_SacrificialDagger(map);
            }
            else
            {
                return new I_HordeBanner(map, orcSociety, location);
            }
        }
    }
}
