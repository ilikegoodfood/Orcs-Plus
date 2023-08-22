using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Pr_Vinerva_Health : Property
    {
        public List<Challenge> challenges = new List<Challenge>();

        public Pr_Vinerva_Health(Location location)
            : base (location)
        {
            challenges.Add(new Ch_H_Orcs_HarvestGourd(location));
        }

        public override string getName()
        {
            return "Gourd of Blood";
        }

        public override string getDesc()
        {
            return "The gourd of blood is a huge red gourd, surrounded by thrashing, thorn covered vines. Its flesh is closer to meat than plant matter and it is filled to bursting with blood of uncertain origins. Those who manage to harvest it, and partake of its interior, can recover from even the most gruesome wounds, at least for a while.";
        }

        public override List<Challenge> getChallenges()
        {
            return challenges;
        }

        public override Sprite getSprite(World world)
        {
            return EventManager.getImg("OrcsPlus.Foreground_BloodGourd.png");
        }

        public override void turnTick()
        {
            if (charge > 300.0)
            {
                charge = 300.0;
            }

            Pr_Death death = location.properties.OfType<Pr_Death>().FirstOrDefault();

            if (death != null && charge < 300.0)
            {
                Property.addToProperty("Fertilizer for Gourd of Blood", Property.standardProperties.DEATH, -5.0, location);
                influences.Add(new ReasonMsg("Feeding on Death", Math.Min(4.0, 300.0 - charge)));

                
            }
        }
    }
}
