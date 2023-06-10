using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;
using UnityEngine;

namespace Orcs_Plus
{
    public class UAEN_OrcShaman : UAEN
    {
        public Rt_Orcs_SacrificialSite sacrificalSite;

        public UAEN_OrcShaman(Location loc, SocialGroup sg, Person p)
            : base(loc, sg, p)
        {
            p.stat_might = 2 + Eleven.random.Next(2);
            p.stat_intrigue = 2;
            p.stat_lore = 3 + Eleven.random.Next(3);
            p.stat_command = 3;
            p.hasSoul = false;
            p.species = map.species_orc;

            T_ArcaneKnowledge knowledge = new T_ArcaneKnowledge();
            knowledge.level = 0;
            p.receiveTrait(knowledge);

            T_MasteryDeath deathMastery = new T_MasteryDeath();
            deathMastery.level = 2;
            p.receiveTrait(deathMastery);

            sacrificalSite = new Rt_Orcs_SacrificialSite(loc, this);
            rituals.Add(sacrificalSite);
        }

        public override void turnTick(Map map)
        {
            base.turnTick(map);
            person.XP += map.param.socialGroup_orc_upstartXPPerTurn;
            if (person.skillPoints > 0)
            {
                spendSkillPoint();
            }
        }

        public new List<Unit> getVisibleUnits()
        {
            List<Unit> result = new List<Unit>();

            // Do Stuff

            return result;
        }

        public override double getDisruptUtility(Unit c, List<ReasonMsg> reasons)
        {
            double val = -10000.0;
            reasons?.Add(new ReasonMsg("Does not disrupt agents", val));
            return -10000.0;
        }

        public override double getAttackUtility(Unit c, List<ReasonMsg> reasons, bool includeDangerousFoe = true)
        {
            double val = -10000.0;
            reasons?.Add(new ReasonMsg("Does not attack agents", val));
            return -10000.0;
        }

        public override double getBodyguardUtility(Unit c, List<ReasonMsg> reasons)
        {
            double val = -10000.0;
            reasons?.Add(new ReasonMsg("Does not guard agents", val));
            return -10000.0;
        }

        public override void turnTickAI()
        {
            ModCore.core.comLibAI.turnTickAI(this);
        }

        public override bool definesName()
        {
            return true;
        }

        public override string getName()
        {
            return "Orc Shaman";
        }

        public override Sprite getPortraitBackground()
        {
            return map.world.iconStore.standardBack;
        }

        public override Sprite getPortraitForeground()
        {
            return EventManager.getImg("OrcsPlus.Foreground_OrcShaman.png");
        }

        public override Sprite getPortraitForegroundAlt()
        {
            return EventManager.getImg("OrcsPlus.Foreground_OrcShaman.png");
        }

        public override void spendSkillPoint()
        {
            aiGrowthTargetTags.Remove(Tags.INTRIGUE);
            if (aiGrowthTargetTags.ContainsKey(Tags.LORE))
            {
                aiGrowthTargetTags[Tags.LORE] += 20;
                if (map.automatic && map.overmind.autoAI.currentMode == Overmind_Automatic.aiMode.MAGIC)
                {
                    aiGrowthTargetTags[Tags.LORE] += 200;
                }
            }

            if (aiGrowthTargetTags.ContainsKey(Tags.INTRIGUE))
            {
                aiGrowthTargetTags[Tags.INTRIGUE] -= 20;
            }

            base.spendSkillPoint();
            person.skillPoints--;
        }

        public override int[] getPositiveTags()
        {
            int[] tags = new int[2] { Tags.ORC, Tags.UNDEAD };
            int[] pTags = person.getTags();

            int[] result = new int[pTags.Length + tags.Length];

            for (int i = 0; i < tags.Length; i++)
            {
                result[i] = tags[i];
            }
            for (int i = 0; i < pTags.Length; i++)
            {
                result[tags.Length + i] = pTags[i];
            }

            return result;
        }
    }
}
