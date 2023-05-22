using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Pr_Orcs_SacrificialSite : Property
    {
        public List<Challenge> challenges = new List<Challenge>();

        public Ch_Orcs_DeathFestival deathFestival;

        public Pr_Orcs_SacrificialSite(Location location)
            : base(location)
        {
            deathFestival = new Ch_Orcs_DeathFestival(location);
            challenges.Add(deathFestival);
        }

        public override string getName()
        {
            return "Orc Sacrificial Site";
        }

        public override string getDesc()
        {
            return "A sacrificial alter hidden here by an orc shaman. It's surface is stained red with blood, and pitted. Human remains have been carefully placed in the surrounding area.";
        }

        public override void turnTick()
        {
            influences.Add(new ReasonMsg("Lack of maintenance", -2.0));
        }

        public override bool canTriggerCrisis()
        {
            return false;
        }

        public override List<Challenge> getChallenges()
        {
            return challenges;
        }

        public override standardProperties getPropType()
        {
            return standardProperties.OTHER;
        }

        public override Sprite getSprite(World world)
        {
            return world.iconStore.skull;
        }
    }
}
