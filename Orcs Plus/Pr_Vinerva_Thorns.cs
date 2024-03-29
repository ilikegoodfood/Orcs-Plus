﻿using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Pr_Vinerva_Thorns : Property
    {
        public Pr_Vinerva_Thorns(Location location)
            :base(location)
        {

        }

        public override string getName()
        {
            return "Wall of Thorns";
        }

        public override string getDesc()
        {
            return "A thick hedgerow of viscous thorn bushes protects the outer perimeter of the orc camp at this location. Almost impenetrably thick, these bushes shift unnaturally, allowing orcs to come and go as they please, but grasping and biting at any who attempt to force their way through.";
        }

        public override Sprite getSprite(World world)
        {
            return EventManager.getImg("OrcsPlus.Icon_ThornWall.png");
        }

        public override void turnTick()
        {
            if (charge <= 0.0)
            {
                location.properties.Remove(this);
            }

            if (charge > 300.0)
            {
                charge = 300.0;
            }

            if (location.properties.FirstOrDefault(pr => pr is Pr_Devastation)?.charge >= 100.0)
            {
                influences.Add(new ReasonMsg("Thorns Dying Back", Math.Max(-2.0, -charge)));
            }
            else if (charge < 300.0)
            {
                double chargeThreshold = Math.Ceiling(charge / 50.0) * 50.0;
                if (charge < chargeThreshold)
                {
                    influences.Add(new ReasonMsg("Thorns Regrowing", Math.Min(2.0, chargeThreshold - charge)));
                }
            }

            List<UM> deadUnits = new List<UM>();
            foreach (Unit unit in location.units)
            {
                if (unit is UM um && um.task is Task_RazeLocation)
                {
                    um.hp -= (int)Math.Ceiling(charge / 25.0);

                    if (um.hp <= 0)
                    {
                        deadUnits.Add(um);
                    }
                }
            }

            foreach (UM deadUnit in deadUnits)
            {
                deadUnit.die(map, "Destroyed in Seige");
                if (location.soc is SG_Orc orcSociety)
                {
                    ModCore.Get().TryAddInfluenceGain(orcSociety, new ReasonMsg("Vinerva's wall of thorns destroyed enemy army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                }
            }
        }
    }
}
