using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Curse_EGlory : CurseE
    {
        public override string getName()
        {
            return "Maker's Cursed: Glory (" + count + ")";
        }

        public override string getDesc()
        {
            return "Broken Maker's Curse: Every member of this House who is killed in combat or assassinated increases the curse.";
        }

        public override void onPersonKilled(Person person, object killer)
        {
            if (killer is Person || (killer is Unit u && u.person != null))
            {
                count++;
                person.map.addMessage("Curse of Glory increased", 1.0, true, person.getLocation().hex);
            }
        }
    }
}
