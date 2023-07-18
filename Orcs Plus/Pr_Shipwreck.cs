using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Pr_Shipwreck : Property
    {
        public List<Challenge> challenges = new List<Challenge>();

        public Ch_PlunderShipwreck plunderWreck;

        public Ch_RecoverShipwreck recoverWreck;

        public double profile = 0;

        public Pr_Shipwreck(Location location)
            : base (location)
        {
            plunderWreck = new Ch_PlunderShipwreck(location, this);
            challenges.Add(plunderWreck);
            recoverWreck = new Ch_RecoverShipwreck(location, this);
            challenges.Add(recoverWreck);
        }

        public override string getName()
        {
            return "Wreck of " + location.name;
        }

        public override string getDesc()
        {
            return "A number of wrecked ships have been abandonned at this location, and may be recoverable before they are further damage by the relentless wind and salty seas.";
        }

        public override bool survivesRuin()
        {
            return true;
        }

        public override bool removedOnRuin()
        {
            return false;
        }

        public override void turnTick()
        {
            influences.Add(new ReasonMsg("Weathering", -0.5));

            if (profile < 100)
            {
                profile += 2;
            }
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
            return EventManager.getImg("OrcsPlus.Shipwreck.png");
        }
    }
}
