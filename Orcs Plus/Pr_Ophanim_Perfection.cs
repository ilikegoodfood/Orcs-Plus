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

            if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_Perfection perfection)
            {
                if (perfection.status < 0)
                {
                    double threshold = perfection.thresholdMinor;
                    if (perfection.status < -1)
                    {
                        threshold = perfection.thresholdMajor;

                        if (charge > 299.5)
                        {
                            orcCulture.ophanim_PerfectSociety = true;
                        }

                        if (orcCulture.ophanim_PerfectSociety && charge < 300.0)
                        {
                            influences.Add(new ReasonMsg("Perfect Society", Math.Max(0, Math.Min(perfectionRate, 300.0 - charge))));
                        }
                    }

                    if (charge > threshold)
                    {
                        influences.Add(new ReasonMsg("Doubt", Math.Min(0, Math.Max(-5.0, threshold - charge))));
                    }
                    else if (charge < threshold && camp.shadow > 0.0)
                    {
                        influences.Add(new ReasonMsg("Shadow", Math.Max(0, Math.Min(camp.shadow * 2, threshold - charge))));
                    }

                    foreach (Location neighbour in location.getNeighbours())
                    {
                        if (charge >= 100)
                        {
                            if (neighbour.settlement is Set_OrcCamp && neighbour.soc is SG_Orc orcSociety2 && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null && orcCulture2.tenet_god is H_Orcs_Perfection perfection2 && perfection2.status < 0)
                            {
                                double threshold2 = perfection2.thresholdMinor;
                                if (perfection2.status < -1)
                                {
                                    threshold2 = perfection2.thresholdMajor;
                                }

                                Pr_Ophanim_Perfection neighbourPerfection = (Pr_Ophanim_Perfection)neighbour.properties.FirstOrDefault(pr => pr is Pr_Ophanim_Perfection);

                                if (neighbourPerfection == null)
                                {
                                    neighbourPerfection = new Pr_Ophanim_Perfection(neighbour);
                                    neighbour.properties.Add(neighbourPerfection);
                                }

                                if (neighbourPerfection.charge < charge && neighbourPerfection.charge < threshold2)
                                {
                                    neighbourPerfection.influences.Add(new ReasonMsg("Driven to perfection", Math.Min(Math.Min(charge * spreadFactor, threshold2 - neighbourPerfection.charge), charge - neighbourPerfection.charge)));
                                }
                            }
                        }

                        if (charge < threshold)
                        {
                            if (neighbour.settlement is SettlementHuman)
                            {
                                Pr_Opha_Faith neighbouringFaith = (Pr_Opha_Faith)neighbour.properties.FirstOrDefault(pr => pr is Pr_Opha_Faith);
                                if (neighbouringFaith != null)
                                {
                                    if(neighbouringFaith.charge > 100.0 && neighbouringFaith.charge > charge)
                                    {
                                        influences.Add(new ReasonMsg("Driven to perfection", Math.Min(Math.Min(neighbouringFaith.charge * spreadFactor, threshold - charge), neighbouringFaith.charge - charge)));
                                    }

                                    if (neighbouringFaith.charge < 300.0)
                                    {
                                        neighbouringFaith.influences.Add(new ReasonMsg("Neighbouring Faith", 1.0));
                                    }
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

        public override bool hasBackgroundHexView()
        {
            return true;
        }

        public override Sprite getHexBackgroundSprite()
        {
            if (charge > 299.0)
            {
                return map.world.textureStore.loc_icon_ophaPerfect;
            }

            return map.world.textureStore.loc_icon_opha;
        }
    }
}
