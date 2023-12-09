using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Sub_OrcTemple : Sub_Temple
    {
        public Ch_Orcs_BuyItem[] buyChallenges;

        public List<Challenge> allChallenges;

        public int restockTimer = 0;

        public Sub_OrcTemple(Settlement set, HolyOrder order)
            : base(set, order)
        {
            // Remove unwanted challenges added by parent types.
            challenges.Clear();
            allChallenges = new List<Challenge>();
            buyChallenges = new Ch_Orcs_BuyItem[3];

            // Add new challenges
            challenges.Add(new Ch_H_Orcs_ReprimandUpstart(this, settlement.location));
            challenges.Add(new Ch_Orcs_DrinkGrott(settlement.location));
            challenges.Add(new Ch_Orcs_RefillDrinkingHorns(settlement.location));
            challenges.Add(new Ch_Orcs_ForceRestock(settlement.location, this));

            if (ModCore.Get().data.godTenetTypes.TryGetValue(order.map.overmind.god.GetType(), out Type tenetType) && tenetType != null && tenetType == typeof(H_Orcs_Perfection))
            {
                challenges.Add(new Ch_H_Orcs_PerfectionFestival(settlement.location));
            }

            for (int i = 0; i < buyChallenges.Length; i++)
            {
                buyChallenges[i] = new Ch_Orcs_BuyItem(set.location);
                restockTimer = 25;
            }
        }

        public override string getHoverOverText()
        {
            return "A great hall built by the \"" + order.getName() + "\" culture, within which its people can gather. It is well stocked with grott, a favoured beverage of the ord hordes, at all times, and is surrounded by orc hagglers and traders, plying their wears.";
        }

        public override void turnTick()
        {
            base.turnTick();

            if (settlement.infiltration == 1.0)
            {
                ModCore.Get().TryAddInfluenceGain(order as HolyOrder_Orcs, new ReasonMsg("Infiltrated Great Hall (Per Turn)", 1.0), true);
            }

            restockTimer--;

            if (restockTimer <= 0)
            {
                List<Ch_Orcs_BuyItem> buy = new List<Ch_Orcs_BuyItem>();
                buy.AddRange(buyChallenges);

                foreach (Unit unit in settlement.map.units)
                {
                    if (unit is UA ua)
                    {
                        if (ua.task is Task_PerformChallenge tChallenge && tChallenge.challenge is Ch_Orcs_BuyItem buyChallenge2)
                        {
                            buy.Remove(buyChallenge2);
                        }
                        else if (ua.task is Task_GoToPerformChallenge tGoChallenge && tGoChallenge.challenge is Ch_Orcs_BuyItem buyChallenge3)
                        {
                            buy.Remove(buyChallenge3);
                        }
                    }
                }

                if (buy.Count > 0)
                {
                    int index = Eleven.random.Next(buy.Count);

                    buy[index].restock();
                }

                restockTimer = 25;
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
            allChallenges.Clear();
            allChallenges.AddRange(challenges);
            allChallenges.AddRange(buyChallenges);

            return allChallenges;
        }
    }
}
