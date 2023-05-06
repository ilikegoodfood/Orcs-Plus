using Assets.Code;


namespace Orcs_Plus
{
    public class Task_GatherAtLocation : Assets.Code.Task
    {
        public Location target;

        public int steps = 0;

        public int dur = 0;

        public Task_GatherAtLocation(Location loc, int duration)
        {
            target = loc;
            dur = duration;
        }

        public override string getShort()
        {
            return "Gathering at " + target.getName();
        }

        public override string getLong()
        {
            return getShort();
        }

        public override void turnTick(Unit unit)
        {
            dur -= 1;
            if (dur <= 0)
            {
                unit.map.addUnifiedMessage(unit, null, "Gathering Cancelled", unit.getName() + " has cancelled gathering at " + unit.location.getName() + " because it took too long to reach the gathering site", UnifiedMessage.messageType.TASK_CANCELLED);
                unit.task = null;
                return;
            }
            else if (unit.society.isAtWar())
            {
                unit.map.addUnifiedMessage(unit, null, "Gathering Cancelled", unit.getName() + " has cancelled gathering at " + unit.location.getName() + " due to the outbreak of war", UnifiedMessage.messageType.TASK_CANCELLED);
                unit.task = null;
                return;
            }

            if (unit.location == target)
            {
                if (unit.isCommandable())
                {
                    unit.map.addUnifiedMessage(unit, null, "Unit Arrives", unit.getName() + " has reached " + unit.location.getName() + ". It will now wait for " + dur.ToString() + " turns.", UnifiedMessage.messageType.UNIT_ARRIVES);
                }

                if (dur <= 0)
                {
                    unit.task = null;
                }
                return;
            }

            while (unit.movesTaken < unit.getMaxMoves())
            {
                Location[] pathTo = unit.location.map.getPathTo(unit.location, target, unit, !unit.society.isAtWar());
                if (pathTo == null)
                {
                    pathTo = unit.location.map.getPathTo(unit.location, target, unit);
                }

                if (pathTo == null || pathTo.Length < 2)
                {
                    unit.task = null;
                    return;
                }

                unit.location.map.adjacentMoveTo(unit, pathTo[1]);
                unit.movesTaken++;
                steps++;
                if (unit.isCommandable())
                {
                    EventManager.onEnthralledUnitMove(unit.location.map, unit);
                    foreach (Property property in unit.location.properties)
                    {
                        if (property is Pr_DeepOneCult)
                        {
                            unit.map.hintSystem.popHint(HintSystem.hintType.DEEP_ONES);
                        }

                        if (property is Pr_ArcaneSecret)
                        {
                            unit.map.hintSystem.popHint(HintSystem.hintType.MAGIC);
                        }
                    }
                }

                if (unit.location == target)
                {
                    if (steps > 1 && unit.isCommandable())
                    {
                        unit.map.addUnifiedMessage(unit, null, "Unit Arrives", unit.getName() + " has reached " + unit.location.getName() + ". It will now wait for " + dur.ToString() + " turns.", UnifiedMessage.messageType.UNIT_ARRIVES);
                    }

                    if (dur <= 0)
                    {
                        unit.task = null;
                    }
                    return;
                }
            }

            if (unit.location == target)
            {
                if (steps > 1 && unit.isCommandable())
                {
                    unit.map.addUnifiedMessage(unit, null, "Unit Arrives", unit.getName() + " has reached " + unit.location.getName() + ". It will now wait for " + dur.ToString() + " turns.", UnifiedMessage.messageType.UNIT_ARRIVES);
                }

                if (dur <= 0)
                {
                    unit.task = null;
                }
            }
        }
    }
}
