using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Task_AvengeBloodFeud : Assets.Code.Task
    {
        public Set_OrcCamp target;

        public UA other;

        public UA us;

        public Task_AvengeBloodFeud(UA us, UA them)
        {
            this.us = us;
            other = them;

            getTargetCamp();
        }

        public void getTargetCamp()
        {
            target = null;

            SG_Orc orcSociety = us.society as SG_Orc;
            HolyOrder_Orcs orcCulture = us.society as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }
            else if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
            }

            if (orcSociety != null && orcCulture != null)
            {
                int distance = -1;
                List<Set_OrcCamp> targetCamps = new List<Set_OrcCamp> ();
                foreach (Set_OrcCamp camp in orcCulture.specializedCamps)
                {
                    int stepDist = ModCore.GetComLib().getTravelTimeTo(us, camp.location);
                    if (distance == -1 || stepDist <= distance)
                    {
                        if (stepDist < distance)
                        {
                            targetCamps.Clear();
                        }

                        distance = stepDist;
                        targetCamps.Add(camp);
                    }
                }

                if (targetCamps.Count > 0)
                {
                    target = targetCamps[0];

                    if (targetCamps.Count > 1)
                    {
                        target = targetCamps[Eleven.random.Next(targetCamps.Count)];
                    }
                }
                else if (orcCulture.camps.Count > 0)
                {
                    target = orcCulture.camps[0];

                    if (orcCulture.camps.Count > 1)
                    {
                        target = orcCulture.camps[Eleven.random.Next(orcCulture.camps.Count)];
                    }
                }
            }
        }

        public override string getShort()
        {
            return "Gathering Avenging Warriors at " + (target != null ? target.getName() : "ERROR: No Target");
        }

        public override string getLong()
        {
            return getShort();
        }

        public override void turnTick(Unit unit)
        {
            if (target == null || target.location == null || target.location.settlement != target)
            {
                getTargetCamp();
            }

            if (target == null)
            {
                unit.task = null;
                return;
            }

            if (other == null || other.isDead || other.location == null || !other.location.units.Contains(other))
            {
                unit.task = null;
                return;
            }

            if (unit is UA ua)
            {
                if (unit.location == target.location)
                {
                    UM escort = new UM_VengenceHorde(unit.location, unit.location.soc, target, ua);
                    unit.map.units.Add(escort);
                    unit.location.units.Add(escort);
                    escort.location = unit.location;

                    escort.maxHp = 7;
                    escort.hp = escort.maxHp;
                    escort.task = new Task_EscortUA(ua);

                    ua.task = new CommunityLib.Task_AttackUnitWithCustomEscort(ua, other, escort);
                }
                else
                {
                    while (unit.movesTaken < unit.getMaxMoves())
                    {
                        Location[] pathTo = CommunityLib.Pathfinding.getPathTo(unit.location, target.location, unit, !unit.society.isAtWar());
                        if (pathTo == null)
                        {
                            pathTo = CommunityLib.Pathfinding.getPathTo(unit.location, target.location, unit);
                        }

                        if (pathTo == null || pathTo.Length < 2)
                        {
                            unit.task = null;
                            return;
                        }

                        unit.map.adjacentMoveTo(unit, pathTo[1]);
                        unit.movesTaken++;

                        if (unit.location == target.location)
                        {
                            unit.task = null;
                            return;
                        }
                    }

                    if (unit.location == target.location)
                    {
                        unit.task = null;
                        return;
                    }
                }
            }
        }

        public override Location getLocation()
        {
            return target.location;
        }
    }
}
