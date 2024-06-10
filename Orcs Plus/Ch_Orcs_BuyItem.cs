using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Ch_Orcs_BuyItem : Ch_BuyItem
    {
        public Ch_Orcs_BuyItem(Location location)
            :base(location)
        {
            restock();
        }

        public override string getName()
        {
            return "Buy item from Horde";
        }

        public override string getDesc()
        {
            return "Buys " + onSale.getName() + ", at the cost of " + getCost() + " gold. The merchants of the orc hall will restock with another item, with an increased chance to be rare based on orcish industry.\n" + onSale.getShortDesc();
        }

        public override string getRestriction()
        {
            return "Costs " + getCost() + " <b>gold</b>";
        }

        public override bool validFor(UA ua)
        {
            return ua.person.gold >= getCost();
        }

        public override void complete(UA u)
        {
            u.person.gold -= getCost();
            if (u.person.gold < 0)
            {
                u.person.gold = 0;
            }

            u.person.gainItem(onSale, false);
            string oldItemName = onSale.getName();
            restock();
            msgString = "As soon as the " + oldItemName + " had been sold, the merchants begin offering ";
            if ("aeiouAEIOU".IndexOf(onSale.getName().First()) >= 0)
            {
                msgString += "an " + onSale.getName();
            }
            else
            {
                msgString += "a " + onSale.getName();
            }
        }

        new public void restock()
        {
            if (location.settlement is Set_OrcCamp camp && location.soc is SG_Orc orcSociety)
            {
                Sub_Temple orcTemple = (Sub_Temple)camp.subs.FirstOrDefault(sub => sub is Sub_OrcTemple || sub is Sub_OrcCultureCapital);
                if (orcTemple != null && orcTemple is Sub_OrcCultureCapital seat)
                {
                    bool hasBanner = false;
                    foreach (Ch_Orcs_BuyItem purchase in seat.buyChallenges)
                    {
                        if (purchase == this)
                        {
                            continue;
                        }

                        if (purchase.onSale is I_HordeBanner)
                        {
                            hasBanner = true;
                            break;
                        }
                    }

                    if (!hasBanner)
                    {
                        onSale = new I_HordeBanner(map, orcSociety, location);
                        return;
                    }
                }
            }

            Pr_OrcishIndustry industry = (Pr_OrcishIndustry)location.properties.FirstOrDefault(pr => pr is Pr_OrcishIndustry);
            double industryValue = 0;
            if (industry != null)
            {
                industryValue = industry.charge / 100;
            }

            double roll = Eleven.random.NextDouble();
            double result = industryValue - roll;
            if (result > 0.8)
            {
                onSale = Items.getOrcItemFromPool3(map, location.soc as SG_Orc, location);
            }
            else if (result > 0.5)
            {
                onSale = Items.getOrcItemFromPool2(map, location.soc as SG_Orc, location);
            }
            else
            {
                roll = Math.Min(Eleven.random.Next(10), Eleven.random.Next(10));
                if (roll > 7)
                {
                    onSale = Items.getOrcItemFromPool3(map, location.soc as SG_Orc, location);
                }
                else if (roll > 5)
                {
                    onSale = Items.getOrcItemFromPool2(map, location.soc as SG_Orc, location);
                }
                else
                {
                    onSale = Item.getItemFromPool1(map, -1);
                }
            }
        }

        public int getCost()
        {
            int cost = onSale.getLevel() * 35;

            if (onSale is I_HordeBanner)
            {
                cost = 140;
            }

            return cost;
        }
    }
}
