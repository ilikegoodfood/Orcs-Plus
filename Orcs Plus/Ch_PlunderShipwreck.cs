using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SortedDictionaryProvider;

namespace Orcs_Plus
{
    public class Ch_PlunderShipwreck : Challenge
    {
        public Pr_Shipwreck wreck;

        public double chargeCost = 15.0;

        public Ch_PlunderShipwreck(Location location, Pr_Shipwreck wreck)
            : base(location)
        {
            this.wreck = wreck;
        }

        public override string getName()
        {
            return "Plunder Shipwreck";
        }

        public override string getDesc()
        {
            return "Plunder the wreck in search of coins and other precious items. Performing this action degrades the wreck, reducing its charge by " + chargeCost +".";
        }

        public override string getCastFlavour()
        {
            return "Lost to stormy seas, a failure of navigation, or sunk during a siege of a city. There are many reasons for a ship to find itself at the bottom of the ocean it once proudly rode across, or beached against a granite-grey clifface, and equally many treasures it could have been carrying when it was.";
        }

        public override double getProfile()
        {
            return wreck.profile;
        }

        public override double getMenace()
        {
            return 0.0;
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Shipwreck.png");
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override bool validFor(UM ua)
        {
            return false;
        }

        public override double getComplexity()
        {
            return 7;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 1.0;
            msgs?.Add(new ReasonMsg("Base", result));

            return result;
        }

        public override int getDanger()
        {
            return 4;
        }

        public override void complete(UA u)
        {
            if (u.person != null)
            {
                int gold = Eleven.random.Next(26) + Eleven.random.Next(26);
                u.person.addGold(gold);
                msgString = u.getName() + " found loot worth " + gold + " gold while plundering the " + wreck.getName();

                wreck.charge -= chargeCost;
                if (wreck.charge <= 0.0)
                {
                    location.properties.Remove(wreck);
                }

                int rarityRoll = Eleven.random.Next(100);
                if (rarityRoll < 10)
                {
                    if (rarityRoll < map.param.ch_exploreruins_parameterValue2 * 2)
                    {
                        msgString += ", and a trincket.";
                        u.person.gainItem(Item.getItemFromPool1(map, -1), false);
                    }
                    else if (rarityRoll < map.param.ch_exploreruins_parameterValue3 * 2)
                    {
                        msgString += ", and an item.";
                        u.person.gainItem(Item.getItemFromPool2(map, -1), false);
                    }
                    else if (rarityRoll < 20)
                    {
                        bool gotAbyssalItem = false;

                        if (Eleven.random.Next(2) == 1)
                        {
                            if (ModCore.core.data.tryGetModAssembly("DeepOnesPlus", out ModData.ModIntegrationData intDataDOPlus) && intDataDOPlus.assembly != null && intDataDOPlus.typeDict.TryGetValue("Kernel", out Type kernelType) && kernelType != null)
                            {
                                //Console.WriteLine("OrcsPlus: Deep Ones Plus Enabled");

                                ModKernel kernel = u.map.mods.FirstOrDefault(k => k.GetType() == kernelType);

                                if (kernel != null && intDataDOPlus.methodInfoDict.TryGetValue("getAbyssalItem", out MethodInfo MI_getAbyssalItem) && MI_getAbyssalItem != null)
                                {
                                    gotAbyssalItem = true;
                                    msgString += ", and an artifact from the abyssal depths.";
                                    u.person.gainItem((Item)MI_getAbyssalItem.Invoke(kernel, new object[] { u.map, u }));
                                }
                            }
                        }

                        if (!gotAbyssalItem)
                        {
                            msgString += ", and an artifact.";
                            u.person.gainItem(Item.getItemFromPool3(map, -1), false);
                        }
                    }
                }
                else
                {
                    msgString += ".";
                }
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.GOLD
            };
        }
    }
}
