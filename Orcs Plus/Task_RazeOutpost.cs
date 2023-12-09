using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class Task_RazeOutpost : Assets.Code.Task
    {
        public Pr_HumanOutpost outpost = null;

        public override string getShort()
        {
            return "Razing Outpost";
        }

        public override string getLong()
        {
            return "This army is razing an outpost at this location, preventing positive growth and causing negative growth equal to 10 times the army's hp.";
        }

        public override void turnTick(Unit unit)
        {
            if (unit == null || unit.location == null)
            {
                return;
            }

            UM um = unit as UM;
            if (um == null)
            {
                return;
            }

            outpost = unit.location.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
            if (outpost == null || outpost.charge <= 0.0)
            {
                unit.task = null;
                return;
            }

            // Remove all positive growth factor. Outposts don't grow while under attaack.
            List<ReasonMsg> positiveInfluences = new List<ReasonMsg>();
            foreach (ReasonMsg influence in outpost.influences)
            {
                if (influence.value > 0.0)
                {
                    positiveInfluences.Add(influence);
                }
            }
            if (positiveInfluences.Count > 0)
            {
                foreach (ReasonMsg influence in positiveInfluences)
                {
                    outpost.influences.Remove(influence);
                }
            }

            // Add negative growth factor.
            outpost.influences.Add(new ReasonMsg("Under Attack", unit.hp * -10));

            // Cause death at outpost
            Pr_Death death = outpost.location.properties.OfType<Pr_Death>().FirstOrDefault();
            if (death == null)
            {
                death = new Pr_Death(outpost.location);
                outpost.location.properties.Add(death);
            }
            Property.addToProperty("Militray Action", Property.standardProperties.DEATH, 2.0, outpost.location);

            Society society = um.society as Society;
            bool sgMenace = society == null;
            if (um.society == um.map.soc_dark || (society != null && (society.isDarkEmpire || society.isOphanimControlled)))
            {
                sgMenace = true;
            }

            if (sgMenace)
            {
                if (outpost.parent is Society && um.map.burnInComplete)
                {
                    um.map.hintSystem.popHint(HintSystem.hintType.SOCIALGROUP_MENACE);
                    um.society.menace += um.map.param.society_menaceGainFromRaze * um.map.difficultyMult_growWithDifficulty;
                }

                um.addMenace((double)um.map.param.um_menaceGainFromRaze * um.map.difficultyMult_growWithDifficulty);
            }

            if (outpost.parent != null && (outpost.parent == um.map.soc_dark || outpost.parent.isDarkEmpire || outpost.parent.isOphanimControlled))
            {
                outpost.parent.menace -= um.map.param.um_menaceLostFromRaze;
                if (outpost.parent.menace < 0.0)
                {
                    outpost.parent.menace = 0.0;
                }
            }
        }
    }
}
