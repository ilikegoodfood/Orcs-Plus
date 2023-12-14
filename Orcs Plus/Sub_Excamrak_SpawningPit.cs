using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Sub_Excamrak_SpawningPit : Subsettlement
    {
        public int fleshGrowth = 0;

        public int growthRate = 5;

        public int maxFlesh = 25;

        public List<Challenge> challenges;

        public Sub_Excamrak_SpawningPit(Settlement set)
            : base(set)
        {
            challenges = new List<Challenge>();
        }

        public override string getName()
        {
            return "Flesh Pit";
        }

        public override string getHoverOverText()
        {
            return "A vast pit dug into the ground just outside of " + settlement.getName() + ". It's walls writhe with flesh, ever growing, every shifting, and ever hungry. When the horde is threatened, the orcs lower a ramp into the pit, allowing the living flesh to escape, and roam free.";
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("OrcsPlus.Icon_Escamrak_SpawningPit.png");
        }

        public override void turnTick()
        {
            if (fleshGrowth < maxFlesh)
            {
                fleshGrowth += growthRate;

                if (fleshGrowth > maxFlesh)
                {
                    fleshGrowth = maxFlesh;
                }
            }
        }

        public void spawnArmy()
        {
            if (fleshGrowth > 0)
            {
                if (ModCore.Get().data.tryGetModIntegrationData("Escamrak", out ModData.ModIntegrationData intDataEscam))
                {
                    if (intDataEscam.constructorInfoDict.TryGetValue("SpawningGroundArmy", out ConstructorInfo ci))
                    {
                        UM army = (UM)ci.Invoke(new object[] { settlement.location, settlement.map.soc_dark });
                        army.maxHp = fleshGrowth;
                        army.hp = fleshGrowth;
                        army.location = settlement.location;

                        settlement.map.units.Add(army);
                        settlement.location.units.Add(army);

                        fleshGrowth = 0;
                    }
                }
            }
        }

        public override bool canBeInfiltrated()
        {
            return false;
        }

        public override int getSecurityBoost()
        {
            return 0;
        }

        public override List<Challenge> getChallenges()
        {
            return challenges;
        }
    }
}
