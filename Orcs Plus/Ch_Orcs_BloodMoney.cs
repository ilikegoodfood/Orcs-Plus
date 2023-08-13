using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_BloodMoney : Challenge
    {
        public int cost = 60;

        public Ch_Orcs_BloodMoney(Location location)
            : base(location)
        {

        }

        public override string getName()
        {
            return "Blood Money";
        }

        public override string getDesc()
        {
            return "Pay " + cost + " gold to orcs in this camp to convince them to form a new army.";
        }

        public override string getCastFlavour()
        {
            return "During times of war, orc elder's are sometimes known to pay gold to those who would normally not fight, in exchange for them doing so. This money is a payment not to the those who join the fighting, but to those they, in turn, leave behind. The price of a life's blood, soon to be spilled on the fields of battle.";
        }

        public override string getRestriction()
        {
            return "Can be performed in an infiltrated specialised camp, or by an Orc Elder in a specialised camp belonging to their culture. The specialised camp's army must not currently exist.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Blood_Gold.png");
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 20.0;
            msgs?.Add(new ReasonMsg("Base", utility));

            HolyOrder_Orcs orcCulture = ua.society as HolyOrder_Orcs;
            SG_Orc orcSociety = ua.society as SG_Orc;

            if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
            }

            int warCount = 0;
            double orcMight = orcSociety.currentMilitary + orcCulture.currentMilitary;
            double enemyMight = 0.0;

            foreach(KeyValuePair<SocialGroup, DipRel> pair in orcSociety.relations)
            {
                if (pair.Value.state == DipRel.dipState.war)
                {
                    warCount++;
                    enemyMight += pair.Key.currentMilitary;
                }
            }

            if (warCount > 0)
            {
                double val = 50.0;
                msgs?.Add(new ReasonMsg("At War", val));
                utility += val;

                if (warCount > 1)
                {
                    val = 10 * (warCount - 1);
                    msgs?.Add(new ReasonMsg("In multiple wars", val));
                    utility += val;
                }

                if (orcMight - 10 > enemyMight)
                {
                    val = enemyMight - (orcMight - 10);
                    msgs?.Add(new ReasonMsg("Superior Military", val));
                    utility += val;
                }
                else if (orcMight - 10 < enemyMight)
                {
                    val = 2 * (enemyMight - (orcMight - 10));
                    msgs?.Add(new ReasonMsg("Inferior Military", val));
                    utility += val;
                }
            }

            return utility;
        }

        public override double getComplexity()
        {
            return 20;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 0.0;

            int val = unit.getStatCommand();
            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Stat: Command", val));
                result += val;
            }

            val = unit.getStatLore();
            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", val));
                result += val;
            }

            if (result < 1.0)
            {
                msgs?.Add(new ReasonMsg("Base", val));
                result = 1.0;
            }

            return result;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            return location.settlement is Set_OrcCamp camp && camp.specialism != 0 && camp.specialism != 4 && camp.army == null;
        }

        public override bool validFor(UA ua)
        {
            if (ua.isCommandable() && location.settlement.isInfiltrated && ua.person.getGold() >= cost)
            {
                return true;
            }
            
            if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety == location.soc && elder.person.getGold() >= cost)
            {
                return true;
            }

            return false;
        }

        public override void complete(UA u)
        {
            Set_OrcCamp camp = location.settlement as Set_OrcCamp;

            if (!(location.soc is SG_Orc) || camp == null || camp.army != null || camp.specialism == 0 || camp.specialism == 4)
            {
                return;
            }

            UM_OrcArmy army = null;
            if (camp.specialism == 1 || camp.specialism == 2)
            {
                army = new UM_OrcArmy(location, location.soc, camp);
            }
            else if (camp.specialism == 3)
            {
                army = new UM_OrcBeastArmy(location, location.soc, camp);
            }
            else if (camp.specialism == 5)
            {
                army = new UM_OrcCorsair(location, location.soc, camp);
            }

            if (army != null)
            {
                camp.army = army;
                map.units.Add(army);
                location.units.Add(army);
                army.updateMaxHP();
            }

            u.person.gold -= cost;
        }

        public override int getCompletionMenace()
        {
            return 4;
        }

        public override int getCompletionProfile()
        {
            return 8;
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.COMBAT,
                Tags.ORC
            };
        }

        public override int[] buildNegativeTags()
        {
            return new int[]
                { 
                    Tags.GOLD
                };
        }
    }
}
