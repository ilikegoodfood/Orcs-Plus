using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class PowersFromTenets
    {
        public int lowestStatus = 0;

        public int highestStatus = 0;

        public int maxPower = 1;

        public Dictionary<Power, Func<Map, Power, HolyTenet, bool>> godPowers = new Dictionary<Power, Func<Map, Power, HolyTenet, bool>>();

        public PowersFromTenets(Map map)
        {
            if (ModCore.Get().data.godTenetTypes.TryGetValue(map.overmind.god.GetType(), out Type tenetType) && tenetType != null)
            {
                switch (tenetType.Name)
                {
                    case nameof(H_Orcs_LifeMother):
                        godPowers.Add(new P_Vinerva_Life(map), elderAligned1);
                        godPowers.Add(new P_Vinerva_Health(map), elderAligned1);
                        godPowers.Add(new P_Vinerva_Thorns(map), elderAligned2);
                        break;
                    case nameof(H_Orcs_Perfection):
                        godPowers.Add(new P_Ophanim_PerfectHorde(map), elderAligned2);
                        break;
                    default:
                        break;
                }
            }
        }

        public void updateOrcPowers(Map map)
        {
            if (godPowers.Count == 0)
            {
                return;
            }

            int lowest = 4;
            HolyTenet lowestTenet = null;
            int highest = -4;
            HolyTenet highestTenet = null;

            foreach(HolyOrder_Orcs orcCulture in ModCore.Get().data.orcSGCultureMap.Values)
            {
                if (orcCulture.tenet_god == null)
                {
                    continue;
                }

                if (orcCulture.tenet_god.status < lowest)
                {
                    lowest = orcCulture.tenet_god.status;
                    lowestTenet = orcCulture.tenet_god;
                }

                if (orcCulture.tenet_god.status > highest)
                {
                    highest = orcCulture.tenet_god.status;
                    highestTenet = orcCulture.tenet_god;
                }
            }

            if (lowest == lowestStatus && highest == highestStatus && maxPower == map.overmind.getMaxPower())
            {
                return;
            }
            maxPower = map.overmind.getMaxPower();

            List<Power> currentPowers = map.overmind.god.powers.FindAll(p => godPowers.Keys.Contains(p));
            List<Power> validPowers = new List<Power>();
            foreach (KeyValuePair<Power, Func<Map, Power, HolyTenet, bool>> pair in godPowers)
            {
                if (pair.Value(map, pair.Key, lowestTenet) || pair.Value(map, pair.Key, highestTenet))
                {
                    validPowers.Add(pair.Key);
                }
            }

            List<Power> gainedPowers = validPowers.FindAll(p => !currentPowers.Contains(p));
            List<Power> lostPowers = currentPowers.FindAll(p => !validPowers.Contains(p));

            string gainedPowerNames = "";
            string lostPowerNames = "";

            foreach (Power p in gainedPowers)
            {
                if (gainedPowerNames != "")
                {
                    gainedPowerNames += ", ";
                }

                gainedPowerNames += p.getName();
            }

            foreach (Power p in lostPowers)
            {
                if (lostPowerNames != "")
                {
                    lostPowerNames += ", ";
                }

                lostPowerNames += p.getName();
            }

            if (lostPowerNames != "")
            {
                foreach (Power p in lostPowers)
                {
                    int index = map.overmind.god.powers.FindIndex(pow => pow == p);
                    if (index != -1)
                    {
                        map.overmind.god.powers.RemoveAt(index);
                        map.overmind.god.powerLevelReqs.RemoveAt(index);
                    }
                }

                map.addUnifiedMessage(lowestTenet != null ? lowestTenet.order : (object)map.locations[0], null, "Lost Access To Powers", "Changes in the tenets of an Orc Culture, or your maximum power, has resulted in you losing access to the following powers:\n" + lostPowerNames, "Lost Powers");
            }

            if (gainedPowerNames != "")
            {
                foreach(Power p in gainedPowers)
                {
                    map.overmind.god.powers.Add(p);
                    map.overmind.god.powerLevelReqs.Add(0);
                }

                map.addUnifiedMessage(lowestTenet != null ? lowestTenet.order : (object)map.locations[0], null, "Gained Access To Powers", "Changes in the tenets of an Orc Culture, or your maximum power, has resulted in you gaining access to the following powers:\n" + gainedPowerNames, "Gained Powers");
            }
        }

        private bool elderAligned1(Map map, Power p, HolyTenet tenet)
        {
            if (p.getCost() <= map.overmind.getMaxPower())
            {
                if (tenet != null && tenet.status < 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool elderAligned2(Map map, Power p, HolyTenet tenet)
        {
            if (p.getCost() <= map.overmind.getMaxPower())
            {
                if (tenet != null && tenet.status < -1)
                {
                    return true;
                }
            }

            return false;
        }

        private bool elderALigned3(Map map, Power p, HolyTenet tenet)
        {
            if (p.getCost() <= map.overmind.getMaxPower())
            {
                if (tenet != null && tenet.status < -2)
                {
                    return true;
                }
            }

            return false;
        }

        private bool elderAlignedMax(Map map, Power p, HolyTenet tenet)
        {
            if (p.getCost() <= map.overmind.getMaxPower())
            {
                if (tenet != null && tenet.status == tenet.getMaxNegativeInfluence())
                {
                    return true;
                }
            }

            return false;
        }

        private bool elderAwakeMaxElder(Map map, Power p, HolyTenet tenet)
        {
            if (p.getCost() <= map.overmind.getMaxPower())
            {
                if (map.overmind.god.awake && tenet != null && tenet.status == tenet.getMaxNegativeInfluence())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
