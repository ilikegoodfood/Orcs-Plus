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
    public class Sub_Orcs_SpawningPit : Subsettlement
    {
        public double fleshStore = 0;

        public double growthRate = 1.25;

        public double maxFlesh = 25;

        public double menaceGain = 10.0;

        public List<Challenge> challenges;

        public Sub_Orcs_SpawningPit(Settlement set)
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
            if (fleshStore < maxFlesh)
            {
                fleshStore += growthRate;

                if (fleshStore > maxFlesh)
                {
                    fleshStore = maxFlesh;
                }
            }

            if (!(settlement.location.soc is SG_Orc orcSociety) || (!ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) || !(orcCulture.tenet_god is H_Orcs_Fleshweaving fleshweaving && fleshweaving.status < -1)))
            {
                settlement.subs.Remove(this);
                return;
            }

            if (fleshStore >= maxFlesh && settlement.location.soc.isAtWar())
            {
                spawnArmy();
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg("Gift of flesh grew into army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RecieveGift] / 4), true);

                menace += 10;
            }
        }

        public void spawnArmy()
        {
            if (fleshStore > 0)
            {
                if (ModCore.Get().data.tryGetModIntegrationData("Escamrak", out ModData.ModIntegrationData intDataEscam))
                {
                    if (intDataEscam.constructorInfoDict.TryGetValue("SpawningGroundArmy", out ConstructorInfo ci))
                    {
                        if (intDataEscam.kernel != null && intDataEscam.fieldInfoDict.TryGetValue("FleshSociety", out FieldInfo FI_fleshSociety))
                        {
                            SocialGroup targetSocialGroup = (SocialGroup)(FI_fleshSociety.GetValue(intDataEscam.kernel) ?? settlement.map.soc_dark);
                            UM army = (UM)ci.Invoke(new object[] { settlement.location, targetSocialGroup });
                            army.maxHp = (int)fleshStore;
                            army.hp = (int)fleshStore;
                            army.location = settlement.location;

                            settlement.map.units.Add(army);
                            settlement.location.units.Add(army);

                            fleshStore = 0;
                        }
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
