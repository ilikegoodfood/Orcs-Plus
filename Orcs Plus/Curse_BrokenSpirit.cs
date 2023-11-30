using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Curse_BrokenSpirit : Curse
    {
        public override string getName()
        {
            return "Broken Spirit";
        }

        public override string getDesc()
        {
            return "This is a family haunted by fear. A deep-seated, festering, worrysome beast that gnaws, everhungry, at their mind and spirit. They have a deep aversion to risk and conflict that no mortal remedy can cure them of. So terrible is this foe, that it infects all those in service of this house.";
        }

        public override void turnTick(Person p)
        {
            if (p.unit is UAEN || !(p.unit is UAE))
            {
                foreach (Trait trait in p.traits)
                {
                    if (trait is T_ChosenOne || trait is T_BrokenSpirit)
                    {
                        return;
                    }
                }
                p.receiveTrait(new T_BrokenSpirit());
            }
        }
    }
}
