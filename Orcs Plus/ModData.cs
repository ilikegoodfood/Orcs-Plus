using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orcs_Plus
{
    public class ModData
    {
        public Dictionary<SG_Orc, HolyOrder_Orcs> orcSGCultureMap;

        public bool isPlayerTurn = false;

        public Dictionary<HolyOrder_Orcs, List<ReasonMsg>> influenceGainElder;

        public Dictionary<HolyOrder_Orcs, List<ReasonMsg>> influenceGainHuman;

        private Dictionary<string, Assembly> modAssemblies;

        public List<Sub_OrcWaystation> waystationsToRemove;

        public Dictionary<Type, Type> godTenetTypes;

        private List<Type> settlementTypesForWaystations;

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
            Subjugate
        }

        public Dictionary<influenceGainAction, int> influenceGain;

        public enum menaceGainAction
        {
            Retreat
        }

        public Dictionary<menaceGainAction, int> menaceGain;

        public ModData()
        {
            modAssemblies= new Dictionary<string, Assembly>();

            influenceGain = new Dictionary<influenceGainAction, int>
            {
                { influenceGainAction.AgentKill, 16 },
                { influenceGainAction.ArmyKill, 16 },
                { influenceGainAction.BuildFortress, 35 },
                { influenceGainAction.BuildMages, 40 },
                { influenceGainAction.BuildMenagerie, 40 },
                { influenceGainAction.BuildShipyard, 35 },
                { influenceGainAction.CommandeerShips, 20 },
                { influenceGainAction.DevastateIndustry, 20 },
                { influenceGainAction.Expand, 12 },
                { influenceGainAction.Raiding, 10 },
                { influenceGainAction.RazeLocation, 16 },
                { influenceGainAction.RazingLocation, 4 },
                { influenceGainAction.Subjugate, 12}
            };

            menaceGain = new Dictionary<menaceGainAction, int>
            {
                { menaceGainAction.Retreat, -2 }
            };

            orcSGCultureMap = new Dictionary<SG_Orc, HolyOrder_Orcs>();

            influenceGainElder = new Dictionary<HolyOrder_Orcs, List<ReasonMsg>>();

            influenceGainHuman = new Dictionary<HolyOrder_Orcs, List<ReasonMsg>>();

            waystationsToRemove = new List<Sub_OrcWaystation>();

            godTenetTypes = new Dictionary<Type, Type> {
                { typeof(God_Snake), typeof(H_Orcs_SerpentWarriors) }
            };

            settlementTypesForWaystations = new List<Type>
            {
                typeof(Set_CityRuins),
                typeof(Set_MinorOther),
                typeof(Set_MinorVinerva),
                typeof(Set_VinervaManifestation),
                typeof(Set_TombOfGods)
            };
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
                HolyOrder_Orcs orcCulture = sg as HolyOrder_Orcs;

                if (orcCulture != null && (includeGone || !orcCulture.isGone()))
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
            SG_Orc orcSociety = orcCulture?.orcSociety;
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

        internal void addModAssembly(string key, Assembly asm)
        {
            if (key == "" || asm == null)
            {
                return;
            }

            if (modAssemblies.ContainsKey(key))
            {
                modAssemblies[key] = asm;
            }
            else
            {
                modAssemblies.Add(key, asm);
            }
        }

        internal bool tryGetModAssembly(string key, out Assembly asm)
        {
            bool result = modAssemblies.TryGetValue(key, out Assembly retASM);
            asm = retASM;

            return result;
        }

        internal bool tryAddSettlementTypeForWaystation(Type t)
        {
            if (!settlementTypesForWaystations.Contains(t))
            {
                settlementTypesForWaystations.Add(t);
                return true;
            }

            return false;
        }

        internal List<Type> getSettlementTypesForWaystation()
        {
            return settlementTypesForWaystations;
        }
    }
}
