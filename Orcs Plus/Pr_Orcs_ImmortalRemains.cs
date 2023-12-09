using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Pr_Orcs_ImmortalRemains : Property
    {
        public UA ua = null;

        public Person person = null;

        public double profile = 0.0;

        List<Challenge> challenges = new List<Challenge>();

        public Pr_Orcs_ImmortalRemains(Location location, UA ua)
            : base(location)
        {
            this.ua = ua;
            person = ua.person;

            challenges.Add(new Ch_Orcs_LootRemains(location, this));
            challenges.Add(new Mg_Orcs_ReviveImmortal(location, this));
        }

        public override string getName()
        {
            return Eleven.capFirst(person.species.name()) + "'s Immortal Body";
        }

        public override string getDesc()
        {
            return "These are the immortal remains of " + (ua != null ? ua.getName() : person.getFullName()) + ". If a sufficiently powerful necromancer wishes, they can revive this person, but they must do so before the remains degrade into nothing.";
        }

        public override Sprite getSprite(World world)
        {
            return ua != null ? ua.getPortraitForeground() : world.textureStore.evil_orcUpstart;
        }

        public override void turnTick()
        {
            if (charge <= 0.0)
            {
                location.properties.Remove(this);
            }

            influences.Add(new ReasonMsg("Remains degrading", -1.0));
            addToProperty("Remains Degrading", standardProperties.DEATH, 1.0, location);

            if (profile < 100.0)
            {
                profile += 2;
            }
        }

        public override bool survivesRuin()
        {
            return true;
        }

        public override List<Challenge> getChallenges()
        {
            return challenges;
        }

        public override standardProperties getPropType()
        {
            return standardProperties.OTHER;
        }
    }
}
