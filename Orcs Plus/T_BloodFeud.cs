using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class T_BloodFeud : Trait
    {
        public SG_Orc orcSociety;

        public T_BloodFeud(SG_Orc orcSociety)
            : base()
        {
            this.orcSociety = orcSociety;
        }

        public override string getName()
        {
            return (orcSociety?.getName() ?? "NULL") + " Blood Fued";
        }

        public override string getDesc()
        {
            return "This unit is the target of a blood fued by the " + (orcSociety?.getName() ?? "NULL") + " as a result of killing one of their Elders. Their armies and upstarts will always view this unit as an enemy.";
        }

        public override int getMaxLevel()
        {
            return 1;
        }

        public override void onAcquire(Person person)
        {
            person.unit?.rituals.Add(new Rt_Orcs_EndBloodFeud(person.getLocation()));
        }

        public override int[] getTags()
        {
            return new int[] { 
                Tags.COMBAT,
                Tags.DANGER,
                Tags.ORC
            };
        }
    }
}
