using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class T_ReveredElder : Trait
    {
        int respawnInterval = 10;
        int respawnClock = 10;

        public override string getName()
        {
            return "Revered Elder";
        }

        public override string getDesc()
        {
            return String.Concat(new string[] {
                "Killing an Orc Elder will start a blood fued with that tribe, which only ends in the death of the target of the blood fued.",
                "\n",
                "Orc Elders periodically choose an `Orc Champion` to guard them. Once every ",
                respawnInterval.ToString(),
                " turns, if this unit does not have an `Orc Champion` minion, an empty minion slot is automatically filled with an `Orc Champion`, with 5 HP, 5 Defence, and 6 Attack. This minion will not spawn if the elder's command limit is below its 3 cost."
            });
        }

        public override int getMaxLevel()
        {
            return 1;
        }

        public override int[] getTags()
        {
            return new int[] { 
                Tags.ORC
            };
        }

        public override void turnTick(Person p)
        {
            UA ua = p.unit as UA;
            if (ua != null && ua.getStatCommandLimit() >= 3)
            {
                M_OrcChampion champion = ua.minions.OfType<M_OrcChampion>().FirstOrDefault();
                if (champion == null)
                {
                    if (respawnClock == 0)
                    {
                        respawnClock = respawnInterval;
                        if (ua.getStatCommandLimit() - ua.getCurrentlyUsedCommand() < 3)
                        {
                            bool[] dismissal = dismissalPlan(ua, 3);
                            for (int i = 0; i < dismissal.Length; i++)
                            {
                                if (dismissal[i] && ua.minions[i] != null)
                                {
                                    ua.minions[i].disband("Dismissed from service of " + ua.getName());
                                    ua.minions[i] = null;
                                }
                            }
                        }

                        for (int j = 0; j < ua.minions.Length; j++)
                        {
                            if (ua.minions[j] == null)
                            {
                                ua.minions[j] = new M_OrcChampion(ua.map);
                                break;
                            }
                        }
                    }
                    else
                    {
                        respawnClock--;
                    }
                }
            }
        }

        public bool[] dismissalPlan (UA ua, int newCost)
        {
            bool[] result = new bool[ua.minions.Length];
            int commandLimit = ua.getStatCommandLimit();
            int usedCommand = ua.getCurrentlyUsedCommand();
            int freedCommand = 0;

            if (ua.getStatCommandLimit() < newCost)
            {
                throw new Exception("Was unable to fire enough people to reach desired command cost");
            }

            if (commandLimit - usedCommand < newCost)
            {
                for (int i = 0; i < ua.minions.Length; i++)
                {
                    if (ua.minions[i] != null)
                    {
                        result[i] = true;
                        freedCommand += ua.minions[i].getCommandCost();

                        if (commandLimit - (usedCommand - freedCommand) >= newCost)
                        {
                            break;
                        }
                    }
                }

                if (commandLimit - (usedCommand - freedCommand) < newCost)
                {
                    throw new Exception("Was unable to fire enough people to reach desired command cost");
                }
            }

            return result;
        }
    }
}
