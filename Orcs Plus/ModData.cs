using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class ModData
    {
        public Dictionary<SG_Orc, HolyOrder_OrcsPlus_Orcs> orcSGCultureMap = new Dictionary<SG_Orc, HolyOrder_OrcsPlus_Orcs>();

        public double orcDefenceFactor = 2.0;

        public bool isPlayerTurn = false;

        public Dictionary<HolyOrder_OrcsPlus_Orcs, List<ReasonMsg>> influenceGainElder = new Dictionary<HolyOrder_OrcsPlus_Orcs, List<ReasonMsg>>();

        public Dictionary<HolyOrder_OrcsPlus_Orcs, List<ReasonMsg>> influenceGainHuman = new Dictionary<HolyOrder_OrcsPlus_Orcs, List<ReasonMsg>>();

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
            influenceGain = new Dictionary<influenceGainAction, int>
            {
                { influenceGainAction.AgentKill, 8 },
                { influenceGainAction.ArmyKill, 8 },
                { influenceGainAction.BuildFortress, 10 },
                { influenceGainAction.BuildMages, 15 },
                { influenceGainAction.BuildMenagerie, 15 },
                { influenceGainAction.BuildShipyard, 10 },
                { influenceGainAction.CommandeerShips, 10 },
                { influenceGainAction.DevastateIndustry, 10 },
                { influenceGainAction.Expand, 6 },
                { influenceGainAction.Raiding, 5 },
                { influenceGainAction.RazeLocation, 8 },
                { influenceGainAction.RazingLocation, 2 },
                { influenceGainAction.Subjugate, 3}
            };

            menaceGain = new Dictionary<menaceGainAction, int>
            {
                { menaceGainAction.Retreat, -2 }
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
    }
}
