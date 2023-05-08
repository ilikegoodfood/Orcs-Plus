﻿using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_H_Orcs_CleansingFestival : Challenge
    {
        public double shadowPull = 0.007;

        public Ch_H_Orcs_CleansingFestival(Location loc)
            : base(loc)
        {
            
        }

        public override string getName()
        {
            return "Holy: Cleansing Festival";
        }

        public override string getDesc()
        {
            return "Draws shadow from nearby locations and units to this location, and simultaneously cleanses shadow at this location. Reduces society menace each turn.";
        }

        public override string getCastFlavour()
        {
            return "An elder leads the camp in a festival of light and purity. Warriors and children dance in the twilight, inhaling the purifying smoke from the great bonfire that stands at the very heart of the camp, driving the shadows away.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by a member of an Orc Culture within their own borders. The Orc Culture's Shadow Weaving tenet status must be greater than zero.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double val = unit.getStatLore();

            if (val < 1)
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                val = 1.0;
            }
            else
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", val));
            }

            return val;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;
            double shadow = location.getShadow();
            double pullShadow = 0.0;

            foreach (Location loc in location.getNeighbours())
            {
                pullShadow += Math.Min(loc.getShadow(), shadowPull * getComplexity());
            }

            double val = Math.Min(1, shadow + pullShadow) * 100;
            val *= 1 - ua.person.shadow;
            msgs?.Add(new ReasonMsg("Potential Shadow Reduction (reduced by shadow)", val));
            utility += val;

            val = ua.person.shadow * ua.map.param.ch_cleanseOwnSoul_Menace;
            val *= 1 - ua.person.shadow;
            msgs?.Add(new ReasonMsg("Own Shadow (reduced by shadow)", val));
            utility += val;

            return utility;
        }

        public override double getComplexity()
        {
            return 15;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.festival;
        }

        public override int isGoodTernary()
        {
            return 1;
        }

        public override bool valid()
        {
            bool result = false;

            if (location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenets.OfType<H_Orcs_ShadowWeaving>().FirstOrDefault()?.status > 0)
            {
                double shadow = location.getShadow();

                if (shadow > 0.05)
                {
                    result = true;
                }
            }

            return result;
        }

        public override bool validFor(UA ua)
        {
            return ua is UAA_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety == location.soc;
        }

        public override void turnTick(UA ua)
        {
            ua.addProfile(1);
            ua.addMenace(-0.5 * getProgressPerTurnInner(ua, null));

            foreach (Location neighbour in location.getNeighbours())
            {
                float deltaShadow = (float)Math.Min(1.0 - location.getShadow(), Math.Min(neighbour.getShadow(), shadowPull * getProgressPerTurnInner(ua, null)));

                if (neighbour.settlement != null)
                {
                    neighbour.settlement.shadow -= deltaShadow;
                }
                else
                {
                    neighbour.hex.purity += (float)(deltaShadow);
                }

                location.settlement.shadow += deltaShadow;

                foreach (Unit unit in location.units)
                {
                    if (unit is UA agent)
                    {
                        agent.person.shadow -= shadowPull * getProgressPerTurnInner(ua, null);
                    }
                }

                if (neighbour.soc is SG_Orc orcs)
                {
                    if (orcs.upstart != null && orcs.upstart.homeLocation == neighbour.index)
                    {
                        orcs.upstart.person.shadow -= shadowPull * getProgressPerTurnInner(ua, null);
                    }
                }
            }

            location.settlement.shadow -= 0.035 * getProgressPerTurnInner(ua, null);

            foreach (Unit unit in location.units)
            {
                if (unit is UA agent)
                {
                    agent.person.shadow -= shadowPull * getProgressPerTurnInner(ua, null);
                }
            }

            if (location.soc is SG_Orc orcSociety)
            {
                if (orcSociety.upstart != null && orcSociety.upstart.homeLocation == location.index)
                {
                    orcSociety.upstart.person.shadow -= shadowPull * getProgressPerTurnInner(ua, null);
                }

                orcSociety.menace -= 0.35 * getProgressPerTurnInner(ua, null);

                if (orcSociety.menace < 0)
                {
                    orcSociety.menace = 0;
                }
            }

            ua.addMenace(-0.5 * getProgressPerTurnInner(ua, null));
        }

        public override int[] buildNegativeTags()
        {
            return new int[]
            {
                Tags.SHADOW
            };
        }
    }
}