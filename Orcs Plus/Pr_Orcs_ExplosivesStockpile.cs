using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Pr_Orcs_ExplosivesStockpile : Property
    {
        public SG_Orc orcSociety;

        public HolyOrder_Orcs orcCulture;

        public Pr_Orcs_ExplosivesStockpile(Location loc)
            : base(loc)
        {
            orcSociety = loc.soc as SG_Orc;
            orcCulture = loc.soc as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }
            else if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
            }
        }

        public override string getName()
        {
            return "Explosives Stockpile";
        }

        public override string getDesc()
        {
            return "A heavily guarded warehouse stands removed from the rest of the camp, surrounded by it's own small wall and trenches. Within, barrels of explosive black powder line the walls in shelves that climb all the way to the rafters.";
        }

        public override Sprite getSprite(World world)
        {
            return EventManager.getImg("OrcsPlus.Icon_Warehouse.png");
        }

        public override void turnTick()
        {
            if (orcCulture == null || orcCulture.isGone())
            {
                location.properties.Remove(this);
            }

            H_Orcs_SecretsOfDestruction secrets = orcCulture.tenet_god as H_Orcs_SecretsOfDestruction;
            if (secrets == null)
            {
                location.properties.Remove(this);
            }

            if (secrets.status >= 0)
            {
                if (charge > 0.0)
                {
                    influences.Add(new ReasonMsg("Stockpile Abandonned", -5.0));
                }
                else
                {
                    location.properties.Remove(this);
                }
            }
            else if (charge < 300.0)
            {
                double industryCharge = 0.0;
                Pr_OrcishIndustry industry = (Pr_OrcishIndustry)location.properties.FirstOrDefault(pr => pr is Pr_OrcishIndustry);
                if (industry != null)
                {
                    industryCharge += industry.charge;
                }

                foreach (Location neighbour in location.getNeighbours())
                {
                    if ((neighbour.soc == orcSociety || neighbour.soc == orcCulture) && neighbour.settlement is Set_OrcCamp)
                    {
                        industry = (Pr_OrcishIndustry)location.properties.FirstOrDefault(pr => pr is Pr_OrcishIndustry);
                        if (industry != null)
                        {
                            industryCharge += industry.charge;
                        }
                    }
                }

                double industryPerExplosive = 50.0;
                if (secrets.status < -1)
                {
                    industryPerExplosive = 30.0;
                }

                double deltaCharge = Math.Floor(industryCharge / industryPerExplosive);
                if (deltaCharge > 0.0)
                {
                    influences.Add(new ReasonMsg("Explosives Production", deltaCharge));
                }
            }
            else
            {
                charge = 300.0;
            }
        }
    }
}
