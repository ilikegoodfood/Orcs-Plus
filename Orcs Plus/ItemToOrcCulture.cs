using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class ItemToOrcCulture : ItemTradeInterface
    {
        Map map;

        Item[] items;

        HolyOrder_Orcs orcCulture;

        int gold = 0;

        int delta;

        Person other;

        public ItemToOrcCulture(Map map, HolyOrder_Orcs orcCulture, Person trader)
        {
            this.map = map;
            items = new Item[3];
            this.orcCulture = orcCulture;
            delta = trader.gold;
            other = trader;
        }

        public void addItemToSet(Item item)
        {
            for (int i = 0; i < this.items.Length; i++)
            {
                if (items[i] == null)
                {
                    this.items[i] = item;
                    break;
                }
            }
        }

        public void addGold(int delta)
        {
            gold += delta;
        }

        public double getGold()
        {
            return gold;
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
            return items;
        }

        public string getName()
        {
            return "Donate To Orc Culture";
        }

        public void nullTopItem()
        {
            items[0] = null;
        }

        public void rotateItems()
        {
            Item item = items[0];
            for (int i = 0; i < items.Length - 1; i++)
            {
                items[i] = items[i + 1];
            }
            items[items.Length - 1] = item;
        }

        public void rotateItemsReversed()
        {
            Item item = items[items.Length - 1];
            for (int i = 0; i < items.Length - 1; i++)
            {
                items[this.items.Length - 1 - i] = items[items.Length - 2 - i];
            }
            items[0] = item;
        }

        public void setTopItem(Item item)
        {
            items[0] = item;
        }

        public void endTrading()
        {
            delta -= other.gold;
            orcCulture.receiveFunding(other, delta);
        }
    }
}
