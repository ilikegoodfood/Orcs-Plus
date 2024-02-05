using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Pr_Orcs_GreatConstruction : Property
    {
        public SG_Orc orcSociety;

        public HolyOrder_Orcs orcCulture;

        public Set_OrcCamp orcCamp;

        public int specialism = 0;

        public double baseCost;

        public double initCost = 10.0;

        public int specialisedNeighbourCount;

        public double heldGold = 0.0;

        public double usedGold;

        public double chargePerGold;

        public double maxExpenditureRate = 5.0;

        public Pr_Orcs_GreatConstruction(Set_OrcCamp camp, SG_Orc orcs, int specialism)
        : base (camp.location)
        {
            orcCamp = camp;
            orcSociety = orcs;
            ModCore.Get().data.orcSGCultureMap.TryGetValue(orcs, out orcCulture);
            this.specialism = specialism;

            baseCost = 2.0 * map.param.ch_orc_buildFortressCostPerNeighbour;
            specialisedNeighbourCount = location.getNeighbours().FindAll(l => l.settlement is Set_OrcCamp camp2 && camp2.specialism > 0).Count;
            chargePerGold = 300.0 / (baseCost + (specialisedNeighbourCount * map.param.ch_orc_buildFortressCostPerNeighbour));

            usedGold = initCost;
            charge = usedGold * chargePerGold;
        }

        public override string getName()
        {
            string result = "Great Construction";
            if (specialism == 1)
            {
                result += " (Fortress)";
            }
            else if (specialism == 2)
            {
                result += " (Mage Camp)";
            }
            else if (specialism == 3)
            {
                result += " (Menagerie)";
            }
            else if (specialism == 5)
            {
                result += " (Shipyard)";
            }

            return result;
        }

        public override string getDesc()
        {
            string desc = "ERROR: Invalid specialisation";

            if (specialism == 1)
            {
                desc = "Multiple sets of walls are being built far outside of the camp's current bounds, themselves surrounded by trenches and spike-pits. The buildings within the camp are being torn down, and the footprint of much larger buildings are taking shape in the dirt.";
            }
            else if (specialism == 2)
            {
                desc = "The centre of the camp has been completely cleared, replaced with the stone base of what looks like some kind of tower. The inner walls of the camp are lined with construction materials, among which many tribal artifacts, staffs, totems and other religious and arcane symbols lie.";
            }
            else if (specialism == 3)
            {
                desc = "Where the centre of the camp once stood, a deep pit cuts into the ground. Its edges are being lined with cages, and a great many partitions and barriers lie waiting, as if the pit will soon host a great many beasts.";
            }
            else if (specialism == 5)
            {
                desc = "The walls of the camp have been torn down on the side of the coast, replaced by a series of warehouses. New walls are being erected to encase the extension, even as wooden piles are driven into the seafloor. Vast amounts of lumber are being gathered in the newly built warehouses.";
            }

            return "The orcs of " + orcSociety.getName() + " are performing extensive construction work in " + orcCamp.getName() + ". " + desc;
        }

        public override Sprite getSprite(World world)
        {
            return world.iconStore.humanColony;
        }

        public override void turnTick()
        {
            if (!location.properties.Contains(this))
            {
                return;
            }

            bool valid = true;
            if (location.settlement != orcCamp)
            {
                Console.WriteLine("Orcs Plus: Great Construction invalid because camp has been destroyed or moved");
                valid = false;
            }
            else if (location.soc != orcSociety)
            {
                Console.WriteLine("Orcs Plus: Great Construction invalid because location is not owned by " + orcSociety.getName());
                valid = false;
            }
            else if (orcCulture == null)
            {
                Console.WriteLine("Orcs Plus: Great Construction invalid because it is not linked to an orc culture");
                valid = false;
            }
            else if (orcCamp.specialism != 0)
            {
                Console.WriteLine("Orcs Plus: Great Construction invalid because camp is already specialised");
                valid = false;
            }
            else if (specialism == 0 || specialism == 4)
            {
                Console.WriteLine("Orcs Plus: Great Construction invalid due to target specialisation");
                valid = false;
            }
            else if (specialism == 2)
            {
                if (!ModCore.GetComLib().checkHasLocus(location))
                {
                    Console.WriteLine("Orcs Plus: Great Construction (mage camp) invalid because there is no geomantic locus present");
                    valid = false;
                }
            }
            else if (specialism == 3)
            {
                if (!location.properties.Any(pr => pr is PrWM_CagedManticore))
                {
                    Console.WriteLine("Orcs Plus: Great Construction (menageire) invalid because there is no caged manticore present");
                    valid = false;
                }
            }
            else if (specialism == 5)
            {
                if (!location.settlement.subs.Any(sub => sub is Sub_Shipwreck shipwreck && !shipwreck.isReinforced()))
                {
                    Console.WriteLine("Orcs Plus: Great Construction (shipyard) invalid because there is no shipwreck present");
                    valid = false;
                }
            }

            if (!valid)
            {
                if (specialism == 3)
                {
                    PrWM_CagedManticore manticore = (PrWM_CagedManticore)location.properties.FirstOrDefault(pr => pr is PrWM_CagedManticore);
                    if (manticore != null)
                    {
                        location.properties.Remove(manticore);
                        location.properties.Add(new PrWM_Manticore(location));
                    }
                }

                location.properties.Remove(this);
                return;
            }

            if (charge >= 300.0)
            {
                charge = 300.0;
                completeConstruction();
                return;
            }
            else if (charge <= 0.0)
            {
                charge = 0.0;
                location.properties.Remove(this);
                return;
            }

            double cost = baseCost + (specialisedNeighbourCount * map.param.ch_orc_buildFortressCostPerNeighbour);

            double newBaseCost = 2 * map.param.ch_orc_buildFortressCostPerNeighbour;
            int newSpecilisedNeighbourCount = location.getNeighbours().FindAll(l => l.settlement is Set_OrcCamp camp2 && camp2.specialism > 0).Count;
            double newChargePerGold = 300.0 / (baseCost + (specialisedNeighbourCount * map.param.ch_orc_buildFortressCostPerNeighbour));

            double newCost = newBaseCost + (newSpecilisedNeighbourCount * map.param.ch_orc_buildFortressCostPerNeighbour);

            if (newCost != cost)
            {
                double newCharge = (usedGold / newCost) * 300.0;
                double deltaCharge = newCharge - charge;

                if (deltaCharge > 0)
                {
                    influences.Add(new ReasonMsg("Cost increased", deltaCharge));
                }
                else if (deltaCharge < 0)
                {
                    influences.Add(new ReasonMsg("Cost decreased", deltaCharge));
                }
            }

            double totalGold = heldGold + usedGold;

            if (totalGold < newCost && orcCulture.plunderValue > 0.0)
            {
                double goldDrawn = Math.Min(newCost - totalGold, orcCulture.plunderValue);
                heldGold += goldDrawn;
                totalGold += goldDrawn;
                orcCulture.spendGold(goldDrawn);
            }

            if (heldGold > 0.0)
            {
                double expenditure = Math.Min(heldGold, maxExpenditureRate);

                heldGold -= expenditure;
                usedGold += expenditure;

                if (expenditure == maxExpenditureRate)
                {
                    influences.Add(new ReasonMsg("Construction (fully funded)", expenditure * newChargePerGold));
                }
                else
                {
                    influences.Add(new ReasonMsg("Construction (partially funded)", expenditure * newChargePerGold));
                }
            }
            else
            {
                usedGold--;
                influences.Add(new ReasonMsg("Construction stalled", -newChargePerGold));
            }

            baseCost = newBaseCost;
            specialisedNeighbourCount = newSpecilisedNeighbourCount;
            chargePerGold = newChargePerGold;
        }

        public void completeConstruction()
        {
            if (specialism == 3)
            {
                PrWM_CagedManticore manticore = (PrWM_CagedManticore)location.properties.FirstOrDefault(pr => pr is PrWM_CagedManticore);
                if (manticore != null)
                {
                    location.properties.Remove(manticore);
                }
                else
                {
                    specialism = 1;
                }
            }
            else if (specialism == 5)
            {
                Sub_Shipwreck shipwreck = (Sub_Shipwreck)orcCamp.subs.FirstOrDefault(sub => sub is Sub_Shipwreck wreck && !wreck.isReinforced());
                if (shipwreck != null)
                {
                    shipwreck.removeWreck();
                }
                else
                {
                    specialism = 4;
                }
            }

            orcCamp.specialism = specialism;

            location.properties.Remove(this);
        }
    }
}
