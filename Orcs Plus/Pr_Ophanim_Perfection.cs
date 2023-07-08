using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Pr_Ophanim_Perfection : Property
    {
        public double spreadFactor = 0.02;

        public double perfectionRate = 30.0;

        public Pr_Ophanim_Perfection(Location location)
            : base(location)
        {
            charge = 0.0;
        }

        public override string getName()
        {
            return "Perfection";
        }

        public override string getDesc()
        {
            return "Ophanim's will drives the orcs of this camp to aspire and work towards the ordered perfection described by Ophanim's will.";
        }

        public override Sprite getSprite(World world)
        {
            return world.iconStore.ophanimLight;
        }

        public override Sprite hexViewSprite()
        {
            return World.staticMap.world.iconStore.ophanimLightFaded;
        }

        public override Property.standardProperties getPropType()
        {
            return Property.standardProperties.OTHER;
        }

        public override void turnTick()
        {
            Set_OrcCamp camp = location.settlement as Set_OrcCamp;
            SG_Orc orcSociety = location.soc as SG_Orc;

            if (camp == null || orcSociety == null)
            {
                location.properties.Remove(this);
                return;
            }

            if (charge > 300.0)
            {
                charge = 300.0;
            }

            Pr_OrcDefences defences = location.properties.OfType<Pr_OrcDefences>().FirstOrDefault();
            if (charge > 0.0 && defences == null)
            {
                defences = new Pr_OrcDefences(location);
                defences.charge = 0.0;
                location.properties.Add(defences);
            }

            if (ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_Perfection perfection)
            {
                if (orcCulture.ophanim_PerfectSociety && perfection.status < -1)
                {
                    if (charge < 300.0)
                    {
                        influences.Add(new ReasonMsg("Perfect Society", Math.Min(perfectionRate, 300.0 - charge)));
                    }
                }
                else if (perfection.status < 0)
                {
                    double threshold = perfection.thresholdMinor;
                    if (perfection.status < -1)
                    {
                        threshold = perfection.thresholdMajor;

                        if (charge >= 300.0)
                        {
                            orcCulture.ophanim_PerfectSociety = true;
                        }
                    }

                    if (charge > threshold)
                    {
                        influences.Add(new ReasonMsg("Doubt", Math.Max(-5.0, threshold - charge)));
                    }
                    else if (charge < threshold && camp.shadow > 0.0)
                    {
                        influences.Add(new ReasonMsg("Shadow", Math.Min(camp.shadow * 2, threshold - charge)));
                    }

                    foreach (Location neighbour in location.getNeighbours())
                    {
                        if (charge >= 100)
                        {
                            if (neighbour.settlement is Set_OrcCamp && neighbour.soc is SG_Orc orcSociety2 && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null && orcCulture2.tenet_god is H_Orcs_Perfection perfection2 && perfection2.status < 0)
                            {
                                double threshold2 = perfection2.thresholdMinor;
                                if (perfection2.status < -1)
                                {
                                    threshold2 = perfection2.thresholdMajor;
                                }

                                Pr_Ophanim_Perfection neighbourPerfection = neighbour.properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();

                                if (neighbourPerfection == null)
                                {
                                    neighbourPerfection = new Pr_Ophanim_Perfection(neighbour);
                                    neighbour.properties.Add(neighbourPerfection);
                                }

                                if (neighbourPerfection.charge < charge && neighbourPerfection.charge < threshold2)
                                {
                                    neighbourPerfection.influences.Add(new ReasonMsg("Driven to perfection", Math.Min(charge * spreadFactor, threshold - neighbourPerfection.charge)));
                                }
                            }
                        }

                        if (charge < threshold)
                        {
                            if (neighbour.settlement is SettlementHuman)
                            {
                                Pr_Opha_Faith neighbouringFaith = neighbour.properties.OfType<Pr_Opha_Faith>().FirstOrDefault();
                                if (neighbouringFaith != null && neighbouringFaith.charge > 100.0)
                                {
                                    influences.Add(new ReasonMsg("Driven to perfection", Math.Min(charge * spreadFactor, threshold - neighbouringFaith.charge)));
                                }
                            }
                        }
                    }
                }
            }
            else if (charge <= 0.0)
            {
                location.properties.Remove(this);
            }
            else
            {
                influences.Add(new ReasonMsg("Doubt", Math.Max(-5.0, -charge)));
            }
        }
    }
}
