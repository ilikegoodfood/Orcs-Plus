using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;

namespace Orcs_Plus
{
    public class H_Orcs_SerpentWarriors : H_Orcs_ShadowWarriors
    {
        public H_Orcs_SerpentWarriors(HolyOrder_Orcs orcCulture)
            : base(orcCulture)
        {

        }

        public override string getName()
        {
            return "Serpent Warriors";
        }

        public override string getDesc()
        {
            return "The warriors of this orc culture are decorated in snakeskin, both painted and real, in honor of She Who Feast. These elite serpent warriors fight harder, dealing more damage to their enimies, and eventually learn to shrug of harm.";
        }
    }
}
