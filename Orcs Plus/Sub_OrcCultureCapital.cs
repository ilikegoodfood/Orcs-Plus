using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Sub_OrcCultureCapital : Sub_HolyOrderCapital
    {
        public Ch_Orcs_BuyItem[] buyChallenges;

        public List<Challenge> allChallenges;

        public int restockTimer = 0;

        public Sub_OrcCultureCapital(Settlement set, HolyOrder_Orcs order)
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

            if (ModCore.core.data.godTenetTypes.TryGetValue(order.map.overmind.god.GetType(), out Type tenetType) && tenetType != null && tenetType == typeof(H_Orcs_Perfection))
            {
                challenges.Add(new Ch_H_Orcs_PerfectionFestival(settlement.location));
            }

            for (int i = 0; i < buyChallenges.Length; i++)
            {
                buyChallenges[i] = new Ch_Orcs_BuyItem(set.location);
                restockTimer = 25;
            }
            buyChallenges[0].onSale = new I_HordeBanner(order.map, order.orcSociety, settlement.location);
        }

        public override string getName()
        {
            return "Seat of the Elders";
        }

        public override string getHoverOverText()
        {
            return "The seat of power of the elders of the \"" + order.getName() + "\" culture, from where all its decisions are made. It stands as a great hall near the centre of the camp. It is well stocked with grott, a favoured beverage of the ord hordes, at all times, and is surrounded by orc hagglers and traders, plying their wears.";
        }

        public override void turnTick()
        {
            base.turnTick();

            if (settlement.isInfiltrated)
            {
                ModCore.core.TryAddInfluenceGain(order as HolyOrder_Orcs, new ReasonMsg("Infiltrated Seat of the Elders (Per Turn)", 2.0), true);
            }

            restockTimer--;

            if (restockTimer <= 0)
            {
                List<Ch_Orcs_BuyItem> buy = new List<Ch_Orcs_BuyItem>();
                buy.AddRange(buyChallenges);

                foreach (Unit unit in settlement.location.units)
                {
                    if (unit.task is Task_PerformChallenge task && task.challenge is Ch_Orcs_BuyItem buyI)
                    {
                        buy.Remove(buyI);
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

        public override Sprite getIcon()
        {
            return EventManager.getImg("OrcsPlus.Icon_GreatHall.png");
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
