﻿using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orcs_Plus
{
    public class ModData
    {
        public bool isClean = true;

        public Dictionary<SG_Orc, HolyOrder_Orcs> orcSGCultureMap;

        public bool isPlayerTurn = false;

        public Dictionary<HolyOrder_Orcs, List<ReasonMsg>> influenceGainElder;

        public Dictionary<HolyOrder_Orcs, List<ReasonMsg>> influenceGainHuman;

        public Dictionary<Settlement, Pair<double, double>> orcFestivalShadowGain_Dark;
        public Dictionary<Settlement, Pair<double, double>> orcFestivalShadowGain_Light;

        private Dictionary<string, ModIntegrationData> modIntegrationData;

        public Dictionary<Type, Type> godTenetTypes;

        private Dictionary<Type, HashSet<Type>> settlementTypesForWaystations;

        public enum influenceGainAction
        {
            AgentKill,
            ArmyKill,
            BuildFortress,
            BuildMages,
            BuildMenagerie,
            BuildShipyard,
            CommandeerShips,
            DevastateIndustry,
            Expand,
            Raiding,
            RazeLocation,
            RazingLocation,
            Subjugate,
            RecieveGift,
            BuildWaystation,
            BloodMoney,
            BuildTemple,
            BuildMines,
            WarlordLeave,
            WarlordJoin
        }

        public Dictionary<influenceGainAction, int> influenceGain;

        public enum menaceGainAction
        {
            Retreat
        }

        public Dictionary<menaceGainAction, int> menaceGain;

        public Dictionary<int, float> orcGeoMageHabitabilityBonus;

        public Dictionary<string, string> perfectHordeNameDict;

        public List<AIChallenge> forbiddenChallenges;

        public ModData()
        {
            modIntegrationData = new Dictionary<string, ModIntegrationData>();
            orcGeoMageHabitabilityBonus = new Dictionary<int, float>();
            perfectHordeNameDict = new Dictionary<string, string>();
            forbiddenChallenges = new List<AIChallenge>();

            influenceGain = new Dictionary<influenceGainAction, int>
            {
                { influenceGainAction.AgentKill, 20 },
                { influenceGainAction.ArmyKill, 20 },
                { influenceGainAction.BuildFortress, 40 },
                { influenceGainAction.BuildMages, 50 },
                { influenceGainAction.BuildMenagerie, 50 },
                { influenceGainAction.BuildShipyard, 40 },
                { influenceGainAction.BuildMines, 40 },
                { influenceGainAction.CommandeerShips, 20 },
                { influenceGainAction.DevastateIndustry, 20 },
                { influenceGainAction.Expand, 15 },
                { influenceGainAction.Raiding, 10 },
                { influenceGainAction.RazeLocation, 20 },
                { influenceGainAction.RazingLocation, 4 },
                { influenceGainAction.Subjugate, 15 },
                { influenceGainAction.RecieveGift, 20 },
                { influenceGainAction.BuildWaystation, 30 },
                { influenceGainAction.BloodMoney, 30 },
                { influenceGainAction.BuildTemple, 40 },
                { influenceGainAction.WarlordLeave, -60 },
                { influenceGainAction.WarlordJoin, 40 }
            };

            menaceGain = new Dictionary<menaceGainAction, int>
            {
                { menaceGainAction.Retreat, -2 }
            };

            orcSGCultureMap = new Dictionary<SG_Orc, HolyOrder_Orcs>();

            influenceGainElder = new Dictionary<HolyOrder_Orcs, List<ReasonMsg>>();
            influenceGainHuman = new Dictionary<HolyOrder_Orcs, List<ReasonMsg>>();

            orcFestivalShadowGain_Dark = new Dictionary<Settlement, Pair<double, double>>();
            orcFestivalShadowGain_Light = new Dictionary<Settlement, Pair<double, double>>();

            initialiseGodSpecificTenets();
            initialiseSettlementTypesForWaystations();
        }

        private void initialiseGodSpecificTenets()
        {
            if (godTenetTypes == null)
            {
                godTenetTypes = new Dictionary<Type, Type> {
                    { typeof(God_Snake), typeof(H_Orcs_SerpentWarriors) },
                    { typeof(God_LaughingKing), typeof(H_Orcs_HarbingersMadness) },
                    { typeof(God_Vinerva), typeof(H_Orcs_LifeMother) },
                    { typeof(God_Ophanim), typeof(H_Orcs_Perfection) },
                    { typeof(God_Mammon), typeof(H_Orcs_MammonClient) },
                    { typeof(God_Cards), typeof(H_Orcs_Lucky) },
                    { typeof(God_Eternity), typeof(H_Orcs_GlorySeeker) },
                    { typeof(God_Underground), typeof(H_Orcs_SecretsOfDestruction) }
                };
            }
        }

        private void initialiseSettlementTypesForWaystations()
        {
            if (settlementTypesForWaystations == null)
            {
                settlementTypesForWaystations = new Dictionary<Type, HashSet<Type>>
                {
                    { typeof(Set_CityRuins), new HashSet<Type>() },
                    { typeof(Set_MinorOther), new HashSet < Type >() },
                    { typeof(Set_MinorVinerva), new HashSet < Type >() },
                    { typeof(Set_VinervaManifestation), new HashSet < Type >() },
                    { typeof(Set_TombOfGods), new HashSet < Type >() },
                    { typeof(Set_DeepOneSanctum), new HashSet < Type >() },
                    { typeof(Set_Shipwreck), new HashSet < Type >() }
                };
            }
        }

        public void clean()
        {
            if (isClean)
            {
                return;
            }

            modIntegrationData.Clear();
            orcGeoMageHabitabilityBonus.Clear();
            perfectHordeNameDict.Clear();
            forbiddenChallenges.Clear();
            orcSGCultureMap.Clear();
            influenceGainElder.Clear();
            influenceGainHuman.Clear();
            orcFestivalShadowGain_Dark.Clear();
            orcFestivalShadowGain_Light.Clear();

            initialiseGodSpecificTenets();
            initialiseSettlementTypesForWaystations();

            isPlayerTurn = false;

            isClean = true;
        }

        public void afterLoading()
        {
            modIntegrationData = new Dictionary<string, ModIntegrationData>();

            initialiseSettlementTypesForWaystations();

            isPlayerTurn = true;
        }

        public void onTurnStart(Map map)
        {
            updateOrcSGCultureMap(map);
        }

        public void updateOrcSGCultureMap(Map map)
        {
            //Console.WriteLine("OrcsPlus: updating orcSGCultureMap");
            orcSGCultureMap.Clear();

            foreach (SocialGroup sg in map.socialGroups)
            {
                if (sg is HolyOrder_Orcs orcCulture)
                {
                    orcSGCultureMap.Add(orcCulture.orcSociety, orcCulture);
                }
            }

            //Console.WriteLine("OrcsPlus: orcSGCultureMap updated");
        }

        public void onTurnEnd(Map map)
        {
            //Console.WriteLine("OrcsPlus: running data.onTurnEnd");
            isPlayerTurn = false;

            influenceGainElder.Clear();
            influenceGainHuman.Clear();

            manageGeoMages(map);

            List<Settlement> festivalKeysToRemove = new List<Settlement>();
            foreach (Settlement key in orcFestivalShadowGain_Dark.Keys)
            {
                if (key.location.settlement != key)
                {
                    festivalKeysToRemove.Add(key);
                }
            }
            foreach (Settlement key in festivalKeysToRemove)
            {
                orcFestivalShadowGain_Dark.Remove(key);
            }

            festivalKeysToRemove.Clear();
            foreach (Settlement key in orcFestivalShadowGain_Light.Keys)
            {
                if (key.location.settlement != key)
                {
                    festivalKeysToRemove.Add(key);
                }
            }
            foreach (Settlement key in festivalKeysToRemove)
            {
                orcFestivalShadowGain_Light.Remove(key);
            }
        }

        public void manageGeoMages(Map map)
        {
            //Console.WriteLine("OrcsPlus: managing geo mages");
            float geoMageIncrement = 0.01f;
            float geoMageMax = (float)(map.param.orc_habRequirement * map.opt_orcHabMult);

            HashSet<int> affectedLocationIndexSet = new HashSet<int>();
            Dictionary<Location, int> affectedLocations = new Dictionary<Location, int>();

            //Console.WriteLine("OrcsPlus: Trimming dictionary");
            List<int> mutableKeys = new List<int>();
            mutableKeys.AddRange(orcGeoMageHabitabilityBonus.Keys);
            foreach (int index in mutableKeys)
            {
                if (index < 0 || index >= map.locations.Count)
                {
                    orcGeoMageHabitabilityBonus.Remove(index);
                    continue;
                }

                if (!isAffectedByGeoMage(map.locations[index]))
                {
                    orcGeoMageHabitabilityBonus[index] -= geoMageIncrement;

                    if (orcGeoMageHabitabilityBonus[index] <= 0f)
                    {
                        orcGeoMageHabitabilityBonus.Remove(index);
                        continue;
                    }
                }
                else
                {
                    affectedLocationIndexSet.Add(index);
                    affectedLocations.Add(map.locations[index], 0);
                }
            }

            //Console.WriteLine("OrcsPlus: iterating map locations");
            foreach (Location location in map.locations)
            {
                if (location.settlement is Set_OrcCamp camp && camp.specialism == 2)
                {
                    if (ModCore.GetComLib().checkHasLocus(location))
                    {
                        //Console.WriteLine("OrcsPlus: found geo mage at " + location.getName());
                        if (!affectedLocationIndexSet.Contains(location.index))
                        {
                            //Console.WriteLine("OrcsPlus: " + location.getName() + " is not already documented");
                            affectedLocationIndexSet.Add(location.index);
                            affectedLocations.Add(location, 0);

                            if (!orcGeoMageHabitabilityBonus.ContainsKey(location.index))
                            {
                                orcGeoMageHabitabilityBonus.Add(location.index, 0f);
                            }
                        }
                        affectedLocations[location]++;

                        //Console.WriteLine("OrcsPlus: processing geo mage neighbours");
                        foreach (Location neighbour in location.getNeighbours())
                        {
                            if (!affectedLocationIndexSet.Contains(neighbour.index))
                            {
                                //Console.WriteLine("OrcsPlus: " + neighbour.getName() + " is not already documented");
                                affectedLocationIndexSet.Add(neighbour.index);
                                affectedLocations.Add(neighbour, 0);

                                if (!orcGeoMageHabitabilityBonus.ContainsKey(neighbour.index))
                                {
                                    orcGeoMageHabitabilityBonus.Add(neighbour.index, 0f);
                                }
                            }

                            affectedLocations[neighbour]++;
                        }
                    }
                }
            }

            foreach (KeyValuePair<Location, int> pair in affectedLocations)
            {
                if (orcGeoMageHabitabilityBonus[pair.Key.index] < geoMageMax * pair.Value)
                {
                    orcGeoMageHabitabilityBonus[pair.Key.index] += geoMageIncrement * pair.Value;

                    if (orcGeoMageHabitabilityBonus[pair.Key.index] > geoMageMax * pair.Value)
                    {
                        orcGeoMageHabitabilityBonus[pair.Key.index] = geoMageMax * pair.Value;
                    }
                }
                else if (orcGeoMageHabitabilityBonus[pair.Key.index] > geoMageMax * pair.Value)
                {
                    orcGeoMageHabitabilityBonus[pair.Key.index] -= geoMageIncrement;

                    if (orcGeoMageHabitabilityBonus[pair.Key.index] < geoMageMax * pair.Value)
                    {
                        orcGeoMageHabitabilityBonus[pair.Key.index] = geoMageMax * pair.Value;
                    }
                }
            }
        }

        public bool isAffectedByGeoMage(Location loc)
        {
            if (loc.settlement is Set_OrcCamp camp && camp.specialism == 2 && ModCore.GetComLib().checkHasLocus(loc))
            {
                return true;
            }

            foreach (Location neighbour in loc.getNeighbours())
            {
                if (neighbour.settlement is Set_OrcCamp camp2 && camp2.specialism == 2 && ModCore.GetComLib().checkHasLocus(neighbour))
                {
                    return true;
                }
            }

            return false;
        }

        public void getBattleArmyEnemies(BattleArmy battle, Unit u, out List<UM> enemies, out List<UA> enemyComs)
        {
            if (u is UM)
            {
                if (battle.attackers.Contains(u))
                {
                    enemies = battle.defenders;
                    enemyComs = battle.defComs;
                }
                else
                {
                    enemies = battle.attackers;
                    enemyComs = battle.attComs;
                }
            }
            else
            {
                if (battle.attComs.Contains(u))
                {
                    enemies = battle.defenders;
                    enemyComs = battle.defComs;
                }
                else
                {
                    enemies = battle.attackers;
                    enemyComs = battle.attComs;
                }
            }
        }

        public void getBattleArmyEnemies(CommunityLib.ArmyBattleData battle, Unit u, out List<UM> enemies, out List<UA> enemyComs)
        {
            if (u is UM)
            {
                if (battle.attackers.Contains(u))
                {
                    enemies = battle.defenders;
                    enemyComs = battle.defComs;
                }
                else
                {
                    enemies = battle.attackers;
                    enemyComs = battle.attComs;
                }
            }
            else
            {
                if (battle.attComs.Contains(u))
                {
                    enemies = battle.defenders;
                    enemyComs = battle.defComs;
                }
                else
                {
                    enemies = battle.attackers;
                    enemyComs = battle.attComs;
                }
            }
        }

        public List<SG_Orc> getOrcSocieties(Map map, bool includeGone = false)
        {
            List<SG_Orc> result = new List<SG_Orc>();

            foreach (SocialGroup sg in map.socialGroups)
            {
                SG_Orc orcSociety = sg as SG_Orc;

                if (orcSociety != null && (includeGone || !orcSociety.isGone()))
                {
                    result.Add(orcSociety);
                }
            }
            return result;
        }

        public List<HolyOrder_Orcs> getOrcCultures(Map map, bool includeGone = false)
        {
            List<HolyOrder_Orcs> result = new List<HolyOrder_Orcs>();
            foreach (SocialGroup sg in map.socialGroups)
            {
                if (sg is HolyOrder_Orcs orcCulture && (includeGone || !orcCulture.isGone()))
                {
                    result.Add(orcCulture);
                }
            }
            return result;
        }

        public void getOrcSocietiesAndCultures(Map map, out List<SG_Orc> orcSocieties, out List<HolyOrder_Orcs> orcCultures, bool includeGone = false)
        {
            orcSocieties = new List<SG_Orc>();
            orcCultures = new List<HolyOrder_Orcs>();
            foreach (SocialGroup sg in map.socialGroups)
            {
                SG_Orc orcSociety = sg as SG_Orc;
                HolyOrder_Orcs orcCulture = sg as HolyOrder_Orcs;

                if (orcSociety != null && (includeGone || ! orcSociety.isGone()))
                {
                    orcSocieties.Add(orcSociety);
                }
                else if (orcCulture != null && (includeGone || !orcCulture.isGone()))
                {
                    orcCultures.Add(orcCulture);
                }
            }
            return;
        }

        public void getOrcCamps(Map map, SG_Orc orcSociety, out List<Set_OrcCamp> orcCamps, out List<Set_OrcCamp> specializedOrcCamps)
        {
            orcCamps = new List<Set_OrcCamp>();
            specializedOrcCamps = new List<Set_OrcCamp>();

            foreach (Location loc in map.locations)
            {
                if (loc.settlement != null && loc.soc == orcSociety)
                {
                    Set_OrcCamp camp = loc.settlement as Set_OrcCamp;
                    if (camp != null)
                    {
                        if (camp.specialism == 0)
                        {
                            orcCamps.Add(camp);
                        }
                        else
                        {
                            specializedOrcCamps.Add(camp);
                        }
                    }
                }
            }
            return;
        }

        public void getOrcCamps(Map map, HolyOrder_Orcs orcCulture, out List<Set_OrcCamp> orcCamps, out List<Set_OrcCamp> specializedOrcCamps)
        {
            SG_Orc orcSociety = orcCulture.orcSociety;
            orcCamps = new List<Set_OrcCamp>();
            specializedOrcCamps = new List<Set_OrcCamp>();

            foreach (Location loc in map.locations)
            {
                if (loc.soc == orcSociety && loc.settlement is Set_OrcCamp camp )
                {
                    if (camp.specialism == 0)
                    {
                        orcCamps.Add(camp);
                    }
                    else
                    {
                        specializedOrcCamps.Add(camp);
                    }
                }
            }
            return;
        }

        public List<UM_OrcArmy> getOrcArmies(Map map, SG_Orc orcSociety)
        {
            List<UM_OrcArmy> result = new List<UM_OrcArmy>();

            foreach (Unit unit in map.units)
            {
                if (unit is UM_OrcArmy army && (orcSociety == null || army.society == orcSociety))
                {
                    result.Add(army);
                }
            }

            return result;
        }

        public List<UM_OrcArmy> getOrcArmies(Map map, HolyOrder_Orcs orcCulture)
        {
            SG_Orc orcSociety = orcCulture?.orcSociety;
            List<UM_OrcArmy> result = new List<UM_OrcArmy>();

            foreach (Unit unit in map.units)
            {
                if (unit is UM_OrcArmy army && (orcSociety == null || army.society == orcSociety))
                {
                    result.Add(army);
                }
            }

            return result;
        }

        internal void addModIntegrationData(string key, ModIntegrationData intData)
        {
            if (key == "" || intData == null || intData.assembly == null)
            {
                return;
            }

            if (modIntegrationData.TryGetValue(key, out ModIntegrationData data) && (data == null || data.assembly == null))
            {
                modIntegrationData[key] = intData;
            }
            else
            {
                modIntegrationData.Add(key, intData);
            }
        }

        internal bool tryGetModIntegrationData(string key, out ModIntegrationData intData)
        {
            return modIntegrationData.TryGetValue(key, out intData);
        }

        internal bool tryAddGodTenetType(Type godType, Type tenetType)
        {
            if (godType.IsSubclassOf(typeof(God)) && tenetType.IsSubclassOf(typeof(HolyTenet)))
            {
                initialiseGodSpecificTenets();

                if (!godTenetTypes.ContainsKey(godType))
                {
                    godTenetTypes.Add(godType, tenetType);
                    return true;
                }

                if (godTenetTypes[godType] == null)
                {
                    godTenetTypes[godType] = tenetType;
                    return true;
                }
            }

            return false;
        }

        internal void tryAddSettlementTypeForWaystation(Type t, HashSet<Type> subsettlementBlacklist = null)
        {
            if (!t.IsSubclassOf(typeof(Settlement)))
            {
                return;
            }

            initialiseSettlementTypesForWaystations();

            if (settlementTypesForWaystations.TryGetValue(t, out HashSet<Type> blacklist))
            {
                if (subsettlementBlacklist != null)
                {
                    blacklist.UnionWith(subsettlementBlacklist);
                }
            }
            else
            {
                if (subsettlementBlacklist == null)
                {
                    subsettlementBlacklist = new HashSet<Type>();
                }

                settlementTypesForWaystations.Add(t, subsettlementBlacklist);
            }

            return;
        }

        internal Dictionary<Type, HashSet<Type>> getSettlementTypesForWaystation() => settlementTypesForWaystations;
    }
}
