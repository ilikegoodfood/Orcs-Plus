using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class ItemFromOrcPlunder : ItemTradeInterface
    {
        Map map;

        Pr_OrcPlunder plunder;

        Person other;

        int delta;

        public ItemFromOrcPlunder(Map map, Pr_OrcPlunder plunder, Person trader)
        {
            this.map = map;
            this.plunder = plunder;
            this.other = trader;
            delta = plunder.gold;
        }

        public void addItemToSet(Item item)
        {
            for (int i = 0; i < plunder.items.Length; i++)
            {
                if (plunder.items[i] == null)
                {
                    plunder.items[i] = item;
                    break;
                }
            }
        }

        public void addGold(int delta)
        {
            plunder.gold += delta;

            if (other != null && other.unit != null && other.unit.society != null && plunder.location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture))
            {
                if (other.unit.isCommandable())
                {
                    ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg("Gifted Gold", delta / 2), true);
                }
                else if (!other.unit.society.isDark())
                {
                    ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg("Gifted Gold", delta / 2));
                }
            }
        }

        public double getGold()
        {
            return plunder.gold;
        }

        public Sprite getIconBack()
        {
            return map.world.iconStore.standardBack;
        }

        public Sprite getIconFore()
        {
            return map.world.iconStore.basicPrayer;
        }

        public Item[] getItems()
        {
            return plunder.items;
        }

        public string getName()
        {
            return "Donate To Orc Culture";
        }

        public void nullTopItem()
        {
            plunder.items[0] = null;
        }

        public void rotateItems()
        {
            Item item = plunder.items[0];
            for (int i = 0; i < plunder.items.Length - 1; i++)
            {
                plunder.items[i] = plunder.items[i + 1];
            }
            plunder.items[plunder.items.Length - 1] = item;
        }

        public void rotateItemsReversed()
        {
            Item item = plunder.items[plunder.items.Length - 1];
            for (int i = 0; i < plunder.items.Length - 1; i++)
            {
                plunder.items[plunder.items.Length - 1 - i] = plunder.items[plunder.items.Length - 2 - i];
            }
            plunder.items[0] = item;
        }

        public void setTopItem(Item item)
        {
            plunder.items[0] = item;
        }

        public void endTrading()
        {
            delta -= plunder.gold;
        }
    }
}
