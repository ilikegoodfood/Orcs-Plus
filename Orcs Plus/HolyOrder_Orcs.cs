﻿using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Orcs_Plus
{
    public class HolyOrder_Orcs : HolyOrder
    {
        public SG_Orc orcSociety;

        public House houseOrc;

        public List<Set_OrcCamp> camps = new List<Set_OrcCamp>();

        public List<Set_OrcCamp> specializedCamps = new List<Set_OrcCamp>();

        public List<Sub_Temple> temples = new List<Sub_Temple>();

        public List<Unit> units = new List<Unit>();

        public List<UA> agents = new List<UA>();

        public List<UAA> acolytes = new List<UAA>();

        public int acolyteSpawnCounter = 0;

        public List<Pr_OrcPlunder> plunder = new List<Pr_OrcPlunder>();

        public double plunderValue;

        public H_Intolerance tenet_intolerance;

        public H_ShadowWeaving tenet_shadowWeaving;

        public HolyOrder_Orcs(Map m, Location l, SG_Orc o) : base(m, l)
        {
            //Console.WriteLine("OrcsPlus: HolyOrder_Orcs Ctor");
            orcSociety = o;
            capital = l.index;

            genderExclusive = 1 - Eleven.random.Next(3);

            generateName(l);
            updateData();
            CreateSeat();

            houseOrc = new House(map);

            priorityPreach.status = 0;
            priorityTemples.status = 0;
            elderInflueenceBias = 0.8;
            humanInfluenceBias = 0.8;

            // Remove all base HolyTenets by wiping list.
            tenets = new List<HolyTenet>();

            // Add required base HolyTenets but does not add them to list.

            // Add required base HolyTenets to new list.
            tenet_alignment = new H_Alignment(this);
            tenets.Add(tenet_alignment);
            tenet_dogmatic = new H_Dogmantic(this);
            tenets.Add(tenet_dogmatic);

            // Add new HolyTenets to list.
            tenet_intolerance = new H_Intolerance(this);
            tenets.Add(tenet_intolerance);
            tenet_shadowWeaving= new H_ShadowWeaving(this);
            tenets.Add(tenet_shadowWeaving);

            // Function returns immediately. Does not do anything.
            // establishInitialProphecy();

            // Remove half of non-structural tenets at Random for reduced tenets option.
            if (map.opt_holyOrderSubsetting)
            {
                List<HolyTenet> tenets_NonStructural = new List<HolyTenet>();
                foreach (HolyTenet tenet in tenets)
                {
                    if (!(tenet is H_Alignment) && !tenet.structuralTenet())
                    {
                        tenets_NonStructural.Add(tenet);
                    }
                }

                while (tenets_NonStructural.Count > tenets.Count / 2)
                {
                    tenets_NonStructural.RemoveAt(Eleven.random.Next(tenets_NonStructural.Count));
                }

                foreach (HolyTenet item in tenets_NonStructural)
                {
                    tenets.Remove(item);
                }
            }

            // Add god specific HolyTenets

            // No divine entities as this is a culture, not a religion. Maybe replace with Ancestors or Heroes later.
            if (map.opt_divineEntities)
            {
                divinity = null;
                /*divinity = new DivineEntity(map, this);
                divinity.name = "Ancestors of the " + orcSociety.getName();
                divinity.desire = new D_Blank(map, divinity);*/

            }

            // Set map colour, favouring redish colours.
            color = new Color(Math.Min(o.color.r * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f), Math.Min(o.color.g * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f), Math.Min(o.color.b * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f));
            color2 = new Color(Math.Min(o.color2.r * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f), Math.Min(o.color2.g * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f), Math.Min(o.color2.b * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f));
        }

        public override bool checkIsGone()
        {
            if (cachedGone)
            {
                return cachedGone;
            }

            if (orcSociety == null || orcSociety.checkIsGone())
            {
                cachedGone = true;
            }

            return cachedGone;
        }

        public override void establishInitialTenentSpread()
        {
            return;
        }

        public override string generateSubParentName(Settlement settlement)
        {
            return " Great Hall of " + settlement.location.soc.getName();
        }

        public override Sprite getLocationSpriteTemple()
        {
            return EventManager.getImg("OrcsPlus.Icon_GreatHall.png");
        }

        public override Sprite getTempleIcon()
        {
            return EventManager.getImg("OrcsPlus.Icon_GreatHall.png");
        }

        public override void establishInitialProphecy()
        {
            return;
        }

        public override int[] getTags()
        {
            int[] newTags = new int[] { Tags.ORC };
            int[] tags = base.getTags().Concat(newTags).ToArray();
            return tags;
        }

        new public void humanAIExpenditure()
        {
            HolyTenet holyTenet = null;
            if (tenet_alignment.status < 0)
            {
                holyTenet = tenet_alignment;
            }
            else if (tenet_intolerance.status < tenet_intolerance.getMaxPositiveInfluence() && tenet_intolerance.status < tenet_alignment.status)
            {
                holyTenet= tenet_intolerance;
            }
            else if (tenet_shadowWeaving.status < tenet_shadowWeaving.getMaxPositiveInfluence() && tenet_shadowWeaving.status < tenet_alignment.status)
            {
                holyTenet = tenet_shadowWeaving;
            }
            else if (tenet_alignment.status < tenet_alignment.getMaxPositiveInfluence())
            {
                holyTenet = tenet_alignment;
            }
            else
            {
                List<HolyTenet> holyTenets = tenets.FindAll(t => t.status < 0);

                if (holyTenets.Count == 0)
                {
                    holyTenets = tenets.FindAll(t => t.status < t.getMaxPositiveInfluence());
                }

                if (holyTenets.Count > 0)
                {
                    holyTenet = holyTenets[Eleven.random.Next(holyTenets.Count)];
                }
            }

            if (holyTenet != null)
            {
                influenceHuman = 0;
                if (holyTenet.status < holyTenet.getMaxPositiveInfluence())
                {
                    holyTenet.status++;
                    map.addUnifiedMessage(this, null, "Holy Order Influence", "Humanity has had enough of an influence on the Holy Order \"" + getName() + "\" to change the tenets of its faith, altering the doctrine its believers will follow. The Tenet " + holyTenet.getName() + " has been influenced positively, and is now at " + holyTenet.status, UnifiedMessage.messageType.HUMANITY_CHANGES_TENET, force: true);
                }
            }
        }

        new public void updateData()
        {
            //Console.WriteLine("OrcsPlus: Update HolyOrder_Orcs data");
            camps.Clear();
            specializedCamps.Clear();
            temples.Clear();
            units.Clear();
            agents.Clear();
            acolytes.Clear();
            plunder.Clear();
            plunderValue = 0;

            ModCore.core.data.getOrcCamps(map, this, out camps, out specializedCamps);
            List<Set_OrcCamp> allCamps = new List<Set_OrcCamp>();
            allCamps.AddRange(camps);
            allCamps.AddRange(specializedCamps);

            foreach (Set_OrcCamp camp in allCamps)
            {
                Sub_Temple temple = camp.subs.OfType<Sub_Temple>().FirstOrDefault();
                if (temple != null)
                {
                    temples.Add(temple);
                }

                Pr_OrcPlunder plunderProperty = camp.location.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();
                if (plunderProperty != null)
                {
                    plunder.Add(plunderProperty);
                    plunderValue += plunderProperty.getGold();
                }
            }

            foreach (Unit unit in map.units)
            {
                if (!unit.isDead && unit.society != null && (unit.society == orcSociety || unit.society == this))
                {
                    if (unit is UA ua)
                    {
                        if (ua is UAA uaa)
                        {
                            acolytes.Add(uaa);
                        }
                        else
                        {
                            agents.Add(ua);
                        }
                    }
                    else
                    {
                        units.Add(unit);
                    }
                }
            }

            nAcolytes = 0;
            nWorshippers = 0;
            nWorshippingRulers = 0;
            nTemples = 0;

            // Counts orc settlements of orcCulture.
            nWorshippers += allCamps.Count;
            nTemples = temples.Count;

            if (units != null)
            {
                nWorshippers += units.Count;
            }
            if (agents != null)
            {
                nWorshippingRulers += agents.Count;
            }
            if (acolytes != null)
            {
                nAcolytes += acolytes.Count;
            }
        }

        public void CreateSeat()
        {
            //Console.WriteLine("OrcsPlus: Create HolOrder_Orcs Seat");
            if (seat == null)
            {
                Set_OrcCamp capitalCamp = map.locations[capital].settlement as Set_OrcCamp;
                if (capitalCamp == null || !(capitalCamp is Set_OrcCamp) || capitalCamp.location.soc != orcSociety)
                {
                    //Console.WriteLine("OrcsPlus: Choosing seat of orc culture.");
                    if (temples.Count > 0)
                    {
                        capitalCamp = temples[Eleven.random.Next(temples.Count)].settlement as Set_OrcCamp;
                    }
                    else if (specializedCamps.Count == 0)
                    {
                        if (camps.Count == 0)
                        {
                            return;
                        }

                        if (camps.Count == 1)
                        {
                            capitalCamp = camps[0];
                            capital = capitalCamp.location.index;
                        }
                        else
                        {
                            capitalCamp = camps[Eleven.random.Next(camps.Count())];
                            capital = capitalCamp.location.index;
                        }
                    }
                    else if (specializedCamps.Count == 1)
                    {
                        capitalCamp = specializedCamps[0];
                        capital = capitalCamp.location.index;
                    }
                    else
                    {
                        capitalCamp = specializedCamps[Eleven.random.Next(specializedCamps.Count())];
                        capital = capitalCamp.location.index;
                    }
                }

                if (capitalCamp != null)
                {
                    Sub_OrcTemple temple = capitalCamp.subs.OfType<Sub_OrcTemple>().FirstOrDefault();
                    if (temple != null)
                    {
                        capitalCamp.subs.Remove(temple);
                    }

                    seat = new Sub_OrcCultureCapital(capitalCamp, this);
                    capitalCamp.subs.Add(seat);
                    temples.Add(seat);
                }
            }
        }

        new public int computeInfluenceDark(List<ReasonMsg> msgs)
        {
            int inf = 0;

            foreach (ModKernel mod in map.mods)
            {
                inf += mod.adjustHolyInfluenceDark(this, inf, msgs);
            }

            msgs?.Add(new ReasonMsg("Gain influence with orc cultures by defeating them in combat, aiding them, or harming their foes", 0));

            return inf;
        }

        new public int processIncome(List<ReasonMsg> msgs)
        {
            msgs?.Add(new ReasonMsg("Plunder", plunderValue));
            return (int)Math.Floor(plunderValue);
        }

        new public int computeInfluenceHuman(List<ReasonMsg> msgs)
        {
            int inf = 0;

            foreach (ModKernel mod in map.mods)
            {
                inf += mod.adjustHolyInfluenceGood(this, inf, msgs);
            }

            msgs?.Add(new ReasonMsg("Gain influence with orc cultures by defeating them in combat", 0));

            return inf;
        }

        public override void turnTick()
        {
            updateData();

            if (seat == null || seat.settlement == null || seat.settlement.location?.settlement != seat.settlement || !seat.settlement.subs.Contains(seat))
            {
                seat = null;
                CreateSeat();
            }

            manageShamans();

            bool playerCanInfluenceFlag = influenceElder >= influenceElderReq;

            int acolyteCount = 0;
            if (acolytes != null)
            {
                acolyteCount = acolytes.Count;
            }

            int templeCount = 0;
            if (temples != null)
            {
                templeCount = temples.Count;
            }
            int settlementCount = 0;
            if (camps != null)
            {
                settlementCount = camps.Count;
            }

            if (acolyteCount < map.param.holy_maxAcolytes && acolyteSpawnCounter == 0)
            {
                costAcolyte = acolyteCount * (40 + Eleven.random.Next(11) + Eleven.random.Next(11));
            }
            costPreach = (int)(10.0 * Math.Pow(Math.Max(0, settlementCount - 5), 0.75));
            costTemple = 50 * (templeCount + 1);

            if (acolyteCount < map.param.holy_maxAcolytes)
            {
                acolyteSpawnCounter++;
                if (acolyteSpawnCounter > costAcolyte)
                {
                    createAcolyte();
                    acolyteSpawnCounter = 0;
                }
            }

            processIncome(null);

            if (map.burnInComplete)
            {
                influenceElder += computeInfluenceDark(null);
                influenceHuman += computeInfluenceHuman(null);
                if (influenceElder > influenceElderReq)
                {
                    influenceElder = influenceElderReq;
                }

                if (influenceHuman > influenceHumanReq)
                {
                    influenceHuman = influenceHumanReq;
                }
            }

            if (influenceHuman >= influenceHumanReq)
            {
                humanAIExpenditure();
            }

            if (!playerCanInfluenceFlag && influenceElder >= influenceElderReq)
            {
                map.addUnifiedMessage(this, null, "Can Influence Holy Order", "You have enough influence to change the tenets of " + getName() + ", via the holy order screen", UnifiedMessage.messageType.CAN_INFLUENCE_ORDER);
            }
            // map.addUnifiedMessage(this, null, "Income Breakdown", "Reserves: " + reserves + " cashForAcolytes: " + cashForAcolytes + " cashForTemples: " + cashForTemples + " cashForPreaching: " + cashForPreaching + " Influence Elder: " + influenceElder + "/" + influenceElderReq + " Influence Human " + influenceHuman + "/" + influenceHumanReq, "Test Message: Orc Culture", true);
        }

        new public void createAcolyte()
        {
            Location location = null;
            List<Settlement> spawnLocations = new List<Settlement>();
            spawnLocations?.AddRange(camps);
            spawnLocations?.AddRange(specializedCamps);
            spawnLocations?.AddRange(specializedCamps);

            if (spawnLocations.Count > 0)
            {
                location = spawnLocations[Eleven.random.Next(spawnLocations.Count)].location;
            }

            if (location == null)
            {
                if (seat != null)
                {
                    location = map.locations[capital];
                }
            }

            if (location != null)
            {
                UAA_OrcElder item = new UAA_OrcElder(location, this, new Person(this, houseOrc));
                location.units.Add(item);
                map.units.Add(item);
                acolytes.Add(item);
                nAcolytes++;
            }
        }

        public void manageShamans()
        {
            if (specializedCamps.Count > 0)
            {
                List<Set_OrcCamp> mageCamps = new List<Set_OrcCamp>();
                foreach (Set_OrcCamp specializedCamp in specializedCamps)
                {
                    if (specializedCamp.specialism == 2)
                    {
                        mageCamps.Add(specializedCamp);
                    }
                }

                List<UAEN_OrcShaman> shamans = new List<UAEN_OrcShaman>();
                foreach(UA agent in agents)
                {
                    if (agent is UAEN_OrcShaman shaman)
                    {
                        shamans.Add(shaman);
                    }
                }

                if (mageCamps.Count > 0)
                {
                    if (shamans.Count > 0)
                    {
                        foreach (UAEN_OrcShaman shaman in shamans)
                        {
                            if (shaman.homeLocation != -1 && map.locations[shaman.homeLocation].settlement is Set_OrcCamp mageCamp && mageCamp.specialism == 2)
                            {
                                mageCamps.Remove(mageCamp);
                            }
                        }
                    }

                    if (mageCamps.Count > 0)
                    {
                        foreach(Set_OrcCamp mageCamp in mageCamps)
                        {
                            if (Eleven.random.Next(4) == 0)
                            {
                                createShaman(mageCamp);
                            }
                        }
                    }
                }
            }
        }

        public void createShaman(Set_OrcCamp mageCamp)
        {
            Location location = mageCamp.location;

            if (location != null)
            {
                UAEN_OrcShaman shaman = new UAEN_OrcShaman(location, orcSociety, new Person(map.soc_neutral, map.soc_neutral.houseOrc));
                location.units.Add(shaman);
                map.units.Add(shaman);
                agents.Add(shaman);
            }
        }

        public new void receiveFunding(Person other, int delta)
        {
            if (seat != null)
            {
                Pr_OrcPlunder capitalPlunder = seat.settlement.location.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();

                if (capitalPlunder == null)
                {
                    capitalPlunder = new Pr_OrcPlunder(seat.settlement.location);
                    seat.settlement.location.properties.Add(capitalPlunder);
                }

                capitalPlunder.addGold(delta);
            }
            else
            {
                List<Set_OrcCamp> allCamps = new List<Set_OrcCamp>();
                allCamps.AddRange(camps);
                allCamps.AddRange(specializedCamps);
                allCamps.AddRange(specializedCamps);

                if (isGone() || allCamps.Count == 0)
                {
                    Console.WriteLine("CommunityLib: ERROR: Tried to give gold to a dead orc culture.");
                    return;
                }

                Set_OrcCamp targetCamp = allCamps[Eleven.random.Next(allCamps.Count)];
                Pr_OrcPlunder targetPlunder = targetCamp.location.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();
                
                if (targetPlunder == null)
                {
                    targetPlunder = new Pr_OrcPlunder(seat.settlement.location);
                    targetCamp.location.properties.Add(new Pr_OrcPlunder(seat.settlement.location));
                }

                targetPlunder.addGold(delta);
                updateData();
            }
        }

        public bool spendGold(double gold)
        {
            updateData();
            
            List<Pr_OrcPlunder> emptyPlunder = new List<Pr_OrcPlunder>();

            if (gold > plunderValue)
            {
                return false;
            }

            foreach (Pr_OrcPlunder p in plunder)
            {
                int pGold = (int)Math.Floor(Math.Min(gold, p.getGold()));
                p.addGold(-pGold);
                if (p.getGold() <= 0)
                {
                    bool isEmpty = true;
                    foreach (Item item in p.items)
                    {
                        if (p != null)
                        {
                            isEmpty = false;
                            break;
                        }

                        if (isEmpty)
                        {
                            emptyPlunder.Add(p);
                        }
                    }
                }

                gold -= pGold;
            }

            foreach (Pr_OrcPlunder p in emptyPlunder)
            {
                p.location.properties.Remove(p);
            }

            updateData();

            if (gold <= 0.0)
            {
                return true;
            }
            else
            {
                return true;
            }
        }

        public override bool hasNormalDiplomacy()
        {
            return false;
        }

        public override string getTempleName()
        {
            return "Great Hall";
        }

        public override string getTitle(Person person)
        {
            return "Elder ";
        }

        public override void generateName(Location cap)
        {
            List<string> list = new List<string>();

            if (genderExclusive == -1)
            {
                list.Add("Clanswomen of ");
                list.Add("Sisters of ");
                list.Add("Daughters of ");
                list.Add("Mothers of ");
                list.Add("Sisterhood of ");
            }

            if (genderExclusive == 1)
            {
                list.Add("Brothers of ");
                list.Add("Sons of ");
                list.Add("Fathers of ");
                list.Add("Clansmen of ");
                list.Add("Brotherhood of ");
            }

            list.Add("Warriors of ");
            list.Add("People of ");
            list.Add("Horde of ");
            list.Add("Clansfolk of ");
            list.Add("Children of ");

            int num = Eleven.random.Next(list.Count);
            string text = list[num];

            name = text + cap.shortName;
        }

        public override bool replacePortraits()
        {
            return false;
        }

        public override Sprite getBackgroundSprite()
        {
            return EventManager.getImg("OrcsPlus.Background_OrcCulture.png");
        }

        public override Sprite getPortrait(Person person)
        {
            return map.world.textureStore.evil_orcUpstart;
        }

        public override void locationStolen(HolyOrder order, UA u)
        {

        }

        public override bool isDark()
        {
            return true;
        }
    }
}
