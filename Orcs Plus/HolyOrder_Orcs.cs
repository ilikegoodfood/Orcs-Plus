using Assets.Code;
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

        public List<UM> militaryUnits = new List<UM>();

        public List<UA> agents = new List<UA>();

        public List<UAEN_OrcElder> acolytes = new List<UAEN_OrcElder>();

        public int acolyteSpawnCounter = 0;

        public List<Pr_OrcPlunder> plunder = new List<Pr_OrcPlunder>();

        public double plunderValue;

        public H_Orcs_Intolerance tenet_intolerance;

        public H_Orcs_ShadowWeaving tenet_shadowWeaving;

        public H_Orcs_Industrious tenet_industrious;

        public H_Orcs_Expansionism tenet_expansionism;

        public HolyTenet tenet_god;

        public int vinerva_HealthDuration = 0;

        public bool ophanim_PerfectSociety = false;

        public int civilWar_TargetCampCount = 8;

        public List<MonsterAction> monsterActions = new List<MonsterAction>();

        public HolyOrder_Orcs(Map m, Location l, SG_Orc o)
            : base(m, o.map.locations[(o.capital != -1 ? o.capital : l.index)])
        {
            //Console.WriteLine("OrcsPlus: HolyOrder_Orcs Ctor");
            orcSociety = o;
            capital = o.capital;
            if (!ModCore.Get().data.orcSGCultureMap.ContainsKey(orcSociety))
            {
                ModCore.Get().data.orcSGCultureMap.Add(orcSociety, this);
            }

            genderExclusive = 1 - Eleven.random.Next(3);

            generateName(l);

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
            tenet_intolerance = new H_Orcs_Intolerance(this);
            tenets.Add(tenet_intolerance);
            tenet_shadowWeaving= new H_Orcs_ShadowWeaving(this);
            tenets.Add(tenet_shadowWeaving);
            tenet_industrious = new H_Orcs_Industrious(this);
            tenets.Add(tenet_industrious);
            tenet_expansionism = new H_Orcs_Expansionism(this);
            tenets.Add(tenet_expansionism);

            establishInitialOrcTenentSpread();

            // Function returns immediately. Does not do anything.
            // establishInitialProphecy();

            // Remove half of non-structural tenets at Random for reduced tenets option.
            // Orcs do not have enough tenets, or tenet variety, for this to be viable at this time.
            /*if (map.opt_holyOrderSubsetting)
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
            }*/

            // Add god specific HolyTenets
            if (ModCore.Get().data.godTenetTypes.TryGetValue(map.overmind.god.GetType(), out Type tenetType) && tenetType != null)
            {
                tenet_god = (HolyTenet)Activator.CreateInstance(tenetType, new object[] { this });
                tenets.Add(tenet_god);
            }
            else
            {
                tenet_god = new H_Orcs_ShadowWarriors(this);
                tenets.Add(tenet_god);
            }

            // No divine entities as this is a culture, not a religion. Maybe replace with Ancestors or Heroes later.
            if (map.opt_divineEntities)
            {
                divinity = null;
            }

            // Set map colour, favouring redish colours.
            color = new Color(Math.Min(o.color.r * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f), Math.Min(o.color.g * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f), Math.Min(o.color.b * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f));
            color2 = new Color(Math.Min(o.color2.r * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f), Math.Min(o.color2.g * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f), Math.Min(o.color2.b * (1 + ((Eleven.random.Next(11) + 10) / 100)), 1f));

            updateSeat();
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

        public virtual void establishInitialOrcTenentSpread()
        {
            tenet_intolerance.status = -1;
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
            militaryUnits.Clear();
            agents.Clear();
            acolytes.Clear();
            plunder.Clear();
            plunderValue = 0;

            ModCore.Get().data.getOrcCamps(map, this, out camps, out specializedCamps);
            List<Set_OrcCamp> allCamps = new List<Set_OrcCamp>();
            allCamps.AddRange(camps);
            allCamps.AddRange(specializedCamps);

            foreach (Set_OrcCamp camp in allCamps)
            {
                Sub_Temple temple = (Sub_Temple)camp.subs.FirstOrDefault(sub => sub is Sub_OrcCultureCapital || sub is Sub_OrcTemple);
                if (temple != null)
                {
                    temples.Add(temple);
                }

                List<Pr_OrcPlunder> plunderProperties = camp.location.properties.OfType<Pr_OrcPlunder>().ToList();
                foreach (Pr_OrcPlunder plunderProperty in plunderProperties)
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
                        if (ua is UAEN_OrcElder elder)
                        {
                            acolytes.Add(elder);
                        }
                        else
                        {
                            agents.Add(ua);
                        }
                    }
                    else if (unit is UM um)
                    {
                        militaryUnits.Add(um);
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

            if (militaryUnits != null)
            {
                nWorshippers += militaryUnits.Count;
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

        public bool validateCapital()
        {
            capital = orcSociety.capital;
            if (orcSociety.capital != -1 && (orcSociety.map.locations[orcSociety.capital].soc != orcSociety || !(orcSociety.map.locations[orcSociety.capital].settlement is Set_OrcCamp)))
            {
                orcSociety.capital = -1;
                capital = -1;
            }

            if (orcSociety.capital == -1 && camps.Count > 0)
            {
                int index;
                if (camps.Count > 1)
                {
                    index = camps[Eleven.random.Next(camps.Count)].location.index;
                    orcSociety.capital = index;
                    capital = index;
                    return true;
                }

                index = camps[0].location.index;
                orcSociety.capital = index;
                capital = index;
                return true;
            }

            return false;
        }

        public void updateSeat()
        {
            capital = orcSociety.capital;
            if (capital == -1)
            {
                return;
            }

            if (seat == null || seat.settlement == null || seat.settlement.location.settlement != seat.settlement || !seat.settlement.subs.Contains(seat))
            {
                Set_OrcCamp capitalCamp = map.locations[capital].settlement as Set_OrcCamp;
                if (capitalCamp != null)
                {
                    Sub_OrcTemple temple = (Sub_OrcTemple)capitalCamp.subs.FirstOrDefault(sub => sub is Sub_OrcTemple);
                    if (temple != null)
                    {
                        capitalCamp.subs.Remove(temple);
                        temples.Remove(temple);
                    }

                    seat = new Sub_OrcCultureCapital(capitalCamp, this);
                    capitalCamp.subs.Add(seat);
                    temples.Add(seat);
                }

                return;
            }
            
            if (seat != null && seat.settlement.location.index != capital)
            {
                Set_OrcCamp seatCamp = seat.settlement as Set_OrcCamp;

                temples.Remove(seat);
                seat = null;

                if (seatCamp != null)
                {
                    Sub_OrcTemple temple = new Sub_OrcTemple(seatCamp, this);
                    seatCamp.subs.Add(temple);
                    temples.Add(temple);
                }

                Set_OrcCamp capitalCamp = map.locations[capital].settlement as Set_OrcCamp;
                if (capitalCamp != null)
                {
                    Sub_OrcTemple temple = (Sub_OrcTemple)capitalCamp.subs.FirstOrDefault(sub => sub is Sub_OrcTemple);
                    if (temple != null)
                    {
                        capitalCamp.subs.Remove(temple);
                        temples.Remove(temple);
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

            return inf;
        }

        new public int processIncome(List<ReasonMsg> msgs)
        {
            int income = 0;
            msgs?.Add(new ReasonMsg("Plunder", plunderValue));
            income += (int)Math.Floor(plunderValue);

            return income;
        }

        new public int computeInfluenceHuman(List<ReasonMsg> msgs)
        {
            int inf = 0;

            foreach (ModKernel mod in map.mods)
            {
                inf += mod.adjustHolyInfluenceGood(this, inf, msgs);
            }

            return inf;
        }

        public virtual int getAcolyteCost()
        {
            return costAcolyte + (acolytes.Count * 10);
        }

        public override void turnTick()
        {
            capital = orcSociety.capital;
            updateData();
            validateCapital();
            updateSeat();

            manageMonsterActions();
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
                costAcolyte = 15 + Math.Max(Eleven.random.Next(11), Eleven.random.Next(11));
            }
            costPreach = (int)(10.0 * Math.Pow(Math.Max(0, settlementCount - 5), 0.75));
            costTemple = 50 * (templeCount + 1);

            if (acolyteCount < map.param.holy_maxAcolytes)
            {
                acolyteSpawnCounter++;
                if (acolyteSpawnCounter >= getAcolyteCost())
                {
                    createAcolyte();
                    acolyteSpawnCounter = 0;
                }
            }

            if (militaryUnits.Count > 0 && vinerva_HealthDuration > 0)
            {
                vinerva_HealthDuration--;
                foreach(UM um in militaryUnits)
                {
                    if (um.hp < um.maxHp)
                    {
                        um.hp = Math.Min(um.hp + 2, um.maxHp);
                    }
                }
            }

            if (ophanim_PerfectSociety)
            {
                //Console.WriteLine("OrcsPlus: Orc Society is perfect");
                if (!(tenet_god is H_Orcs_Perfection perfection) || perfection.status >= -1)
                {
                    //Console.WriteLine("OrcsPlus: Orc Society cannot be perfect");
                    ophanim_PerfectSociety = false;
                }
                else
                {
                    bool perfect = false;
                    List<Set_OrcCamp> allCamps = new List<Set_OrcCamp>();
                    allCamps.AddRange(camps);
                    allCamps.AddRange(specializedCamps);
                    foreach (Set_OrcCamp camp in allCamps)
                    {
                        if (camp.location.properties.Any(pr => pr is Pr_Ophanim_Perfection && pr.charge > 299.0))
                        {
                            perfect = true;
                            break;
                        }
                    }

                    if (perfect)
                    {
                        //Console.WriteLine("OrcsPlus: Perfection confirmed");
                        foreach (Set_OrcCamp camp in camps)
                        {
                            Pr_Ophanim_Perfection perfectionLocal = (Pr_Ophanim_Perfection)camp.location.properties.FirstOrDefault(pr => pr is Pr_Ophanim_Perfection);
                            if (perfectionLocal == null)
                            {
                                perfectionLocal = new Pr_Ophanim_Perfection(camp.location);
                                perfectionLocal.influences.Add(new ReasonMsg("Perfect Society", perfectionLocal.perfectionRate));
                                camp.location.properties.Add(perfectionLocal);
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("OrcsPlus: Orc Society is no longer perfect");
                        ophanim_PerfectSociety = false;
                    }
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
                else if (influenceElder < 0)
                {
                    influenceElder = 0;
                }

                if (influenceHuman > influenceHumanReq)
                {
                    influenceHuman = influenceHumanReq;
                }
                else if (influenceHuman < 0)
                {
                    influenceHuman = 0;
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
            capital = orcSociety.capital;
            Location location = null;
            List<Settlement> spawnLocations = new List<Settlement>();
            spawnLocations?.AddRange(camps);
            spawnLocations?.AddRange(specializedCamps);
            spawnLocations?.AddRange(specializedCamps);

            if (spawnLocations.Count > 0)
            {
                location = spawnLocations[Eleven.random.Next(spawnLocations.Count)].location;
            }

            if (location == null && capital != -1)
            {
                location = map.locations[capital];
            }

            if (location == null)
            {
                return;
            }

            UAEN_OrcElder item = new UAEN_OrcElder(location, this, new Person(this, houseOrc));
            location.units.Add(item);
            map.units.Add(item);
            acolytes.Add(item);
            nAcolytes++;
        }

        public void manageShamans()
        {
            if (specializedCamps.Count > 0)
            {
                List<Set_OrcCamp> mageCamps = specializedCamps.FindAll(camp => camp.specialism == 2);
                List<UAEN_OrcShaman> shamans = agents.OfType<UAEN_OrcShaman>().ToList();

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

                    foreach (Set_OrcCamp mageCamp in mageCamps)
                    {
                        if (Eleven.random.Next(4) == 0)
                        {
                            createShaman(mageCamp);
                        }
                    }
                }
            }
        }

        public void createShaman(Set_OrcCamp homeCamp)
        {
            capital = orcSociety.capital;
            UAEN_OrcShaman shaman = new UAEN_OrcShaman(homeCamp.location, orcSociety, new Person(map.soc_neutral, map.soc_neutral.houseOrc));
            homeCamp.location.units.Add(shaman);
            map.units.Add(shaman);
            agents.Add(shaman);
        }

        public void manageMonsterActions()
        {
            capital = orcSociety.capital;
            List<MonsterAction> actionsToRemove = new List<MonsterAction>();

            foreach(MonsterAction action in monsterActions)
            {
                if (action is MA_Orc_HireMercenaries hire && !orcSociety.getNeighbours().Contains(hire.target))
                {
                    actionsToRemove.Add(action);
                }
            }

            foreach (MonsterAction action in actionsToRemove)
            {
                monsterActions.Remove(action);
            }
        }

        public new void receiveFunding(Person other, int delta)
        {
            capital = orcSociety.capital;
            bool playerCanInfluenceFlag = influenceElder >= influenceElderReq;

            if (capital != -1)
            {
                Pr_OrcPlunder capitalPlunder = (Pr_OrcPlunder)map.locations[capital].properties.FirstOrDefault(pr => pr is Pr_OrcPlunder);

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
                Pr_OrcPlunder targetPlunder = (Pr_OrcPlunder)targetCamp.location.properties.FirstOrDefault(pr => pr is Pr_OrcPlunder);
                
                if (targetPlunder == null)
                {
                    targetPlunder = new Pr_OrcPlunder(seat.settlement.location);
                    targetCamp.location.properties.Add(new Pr_OrcPlunder(seat.settlement.location));
                }

                targetPlunder.addGold(delta);
            }

            updateData();

            if (!playerCanInfluenceFlag && influenceElder >= influenceElderReq)
            {
                map.addUnifiedMessage(this, null, "Can Influence Holy Order", "You have enough influence to change the tenets of " + getName() + ", via the holy order screen", UnifiedMessage.messageType.CAN_INFLUENCE_ORDER);
            }
        }

        public bool spendGold(double gold)
        {
            capital = orcSociety.capital;
            updateData();

            if (gold > plunderValue)
            {
                return false;
            }

            while (gold > 0 && plunderValue > 0)
            {
                Pr_OrcPlunder targetPlunder = plunder[Eleven.random.Next(plunder.Count)];

                int pGold = (int)Math.Floor(Math.Min(gold, targetPlunder.getGold()));
                targetPlunder.addGold(-pGold);
                if (targetPlunder.getGold() <= 0)
                {
                    bool isEmpty = true;
                    foreach (Item item in targetPlunder.items)
                    {
                        if (item != null)
                        {
                            isEmpty = false;
                            break;
                        }
                    }

                    if (isEmpty)
                    {
                        targetPlunder.location.properties.Remove(targetPlunder);
                        plunder.Remove(targetPlunder);
                    }
                }

                gold -= pGold;
                plunderValue -= pGold;
            }

            return true;
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

        public override void triggerCivilWar()
        {
            // Ensure all data is up to date.
            capital = orcSociety.capital;
            updateData();
            validateCapital();

            if (seat == null || seat.settlement == null || seat.settlement.location == null || seat.settlement.location.settlement != seat.settlement || !seat.settlement.subs.Contains(seat))
            {
                seat = null;
                updateSeat();
            }

            Location hordeCapital = map.locations[capital];
            Set_OrcCamp capitalCamp = hordeCapital.settlement as Set_OrcCamp;
            List<Set_OrcCamp> capitals = new List<Set_OrcCamp>();

            HashSet<Location> locationSet = new HashSet<Location>();
            List<List<Location>> chunks = new List<List<Location>>();

            // Builds Location lists, named "chunk", where all locations are orc camps or waystations of target social group, are contigous with each other, and where each location is included only once.
            // Uses bredth-first search algorithm.
            foreach(Location loc in map.locations)
            {
                if (loc.soc == orcSociety && loc.settlement is Set_OrcCamp)
                {
                    if (locationSet.Contains(loc))
                    {
                        continue;
                    }

                    List<Location> chunkLocations = new List<Location> { loc };
                    List<Location> outerChunkLocations = new List<Location> { loc };
                    locationSet.Add(loc);
                    int i = 0;

                    while (i < 128)
                    {
                        i++;
                        List<Location> newChunkLocations = new List<Location>();

                        foreach (Location chunkLocation in outerChunkLocations)
                        {
                            foreach( Location neighbour in chunkLocation.getNeighbours())
                            {
                                if (locationSet.Contains(neighbour))
                                {
                                    continue;
                                }

                                if (neighbour.soc == orcSociety || (neighbour.settlement != null && neighbour.settlement.subs.Any(sub => sub is Sub_OrcWaystation waystation && waystation.orcSociety == orcSociety)))
                                {
                                    newChunkLocations.Add(neighbour);
                                    locationSet.Add(neighbour);
                                }
                            }
                        }

                        if (newChunkLocations.Count == 0)
                        {
                            chunks.Add(chunkLocations);
                            break;
                        }

                        chunkLocations.AddRange(newChunkLocations);
                        outerChunkLocations = newChunkLocations;
                    }
                }
            }


            foreach (List<Location> chunk in chunks)
            {
                Set_OrcCamp chunkCapital = null;
                List<Set_OrcCamp> chunkCamps = new List<Set_OrcCamp>();
                List<Set_OrcCamp> chunkFortresses = new List<Set_OrcCamp>();
                List<Set_OrcCamp> chunkSpecialisedCamps = new List<Set_OrcCamp>();
                foreach (Location loc in chunk)
                {
                    if (loc.settlement is Set_OrcCamp camp)
                    {
                        chunkCamps.Add(camp);
                        if (camp.specialism == 1 && camp.specialism == 2 && camp.specialism == 3)
                        {
                            chunkSpecialisedCamps.Add(camp);
                            if (camp.specialism == 1)
                            {
                                chunkFortresses.Add(camp);
                            }
                        }
                    }
                }

                if (chunkCamps.Contains(capitalCamp))
                {
                    chunkCapital = capitalCamp;
                }
                else
                {
                    if (chunkCamps.Count == 1)
                    {
                        chunkCapital = chunkCamps[0];
                    }
                    else if (chunkFortresses.Count > 0)
                    {
                        chunkCapital = chunkFortresses[0];
                        if (chunkFortresses.Count > 1)
                        {
                            chunkCapital = chunkFortresses[Eleven.random.Next(chunkFortresses.Count)];
                        }
                    }
                    else if (chunkSpecialisedCamps.Count > 0)
                    {
                        chunkCapital = chunkSpecialisedCamps[0];
                        if (chunkSpecialisedCamps.Count > 1)
                        {
                            chunkCapital = chunkSpecialisedCamps[Eleven.random.Next(chunkSpecialisedCamps.Count)];
                        }
                    }

                    if (chunkCapital == null)
                    {
                        chunkCapital = chunkCamps[Eleven.random.Next(chunkCamps.Count)];
                    }

                    if (chunkCapital == null)
                    {
                        continue;
                    }
                }

                // Integer division truncates.
                int targetCapitalCount = Math.Max(1, chunkCamps.Count / civilWar_TargetCampCount);
                if (chunks.Count == 1 && targetCapitalCount == 1)
                {
                    targetCapitalCount++;
                }

                List<Set_OrcCamp> chunkCapitals = new List<Set_OrcCamp>();
                if (chunkCapital != null)
                {
                    chunkCapitals.Add(chunkCapital);
                }

                if (targetCapitalCount > chunkCapitals.Count)
                {
                    for (int i = 3; i > 0; i--)
                    {
                        for (int j = targetCapitalCount - chunkCapitals.Count; j > 0; j--)
                        {
                            Set_OrcCamp newRebelCapital = null;
                            List<Set_OrcCamp> availableChunkFortresses = chunkFortresses.FindAll(cf => !chunkCapitals.Any(cc => map.getStepDist(cc.location, cf.location) < i) && map.getStepDist(cf.location, hordeCapital) >= i);
                            List<Set_OrcCamp> availableSpecialisedCamps = chunkSpecialisedCamps.FindAll(csc => !chunkCapitals.Any(cc => map.getStepDist(cc.location, csc.location) < i) && map.getStepDist(csc.location, hordeCapital) >= i);
                            List<Set_OrcCamp> availableChunkCamps = chunkCamps.FindAll(camp => !chunkCapitals.Any(cc => map.getStepDist(cc.location, camp.location) < i) && map.getStepDist(camp.location, hordeCapital) >= i);

                            if (availableChunkFortresses.Count > 0)
                            {
                                newRebelCapital = availableChunkFortresses[0];
                                if (availableChunkFortresses.Count > 1)
                                {
                                    newRebelCapital = availableChunkFortresses[Eleven.random.Next(availableChunkFortresses.Count)];
                                }
                            }
                            else if (availableSpecialisedCamps.Count > 0)
                            {
                                newRebelCapital = availableSpecialisedCamps[0];
                                if (availableSpecialisedCamps.Count > 1)
                                {
                                    newRebelCapital = availableSpecialisedCamps[Eleven.random.Next(availableSpecialisedCamps.Count)];
                                }
                            }
                            else if (availableChunkCamps.Count > 0)
                            {
                                newRebelCapital = availableChunkCamps[Eleven.random.Next(availableChunkCamps.Count)];
                            }

                            if (newRebelCapital != null)
                            {
                                chunkCapitals.Add(newRebelCapital);
                            }
                        }

                        if (chunkCapitals.Count >= targetCapitalCount)
                        {
                            break;
                        }
                    }
                }

                capitals.AddRange(chunkCapitals);
            }

            Dictionary<Set_OrcCamp, List<Location>> hordeLocations = new Dictionary<Set_OrcCamp, List<Location>>();
            Dictionary<Set_OrcCamp, HashSet<Location>> hordeLocationSets = new Dictionary<Set_OrcCamp, HashSet<Location>>();
            Dictionary<Set_OrcCamp, HashSet<Location>> hordeSearchedSets = new Dictionary<Set_OrcCamp, HashSet<Location>>();
            Dictionary<Set_OrcCamp, HashSet<Location>> stepLocations = new Dictionary<Set_OrcCamp, HashSet<Location>>();

            foreach (Set_OrcCamp capital in capitals)
            {
                hordeLocations.Add(capital, new List<Location> { capital.location });
                hordeLocationSets.Add(capital, new HashSet<Location> { capital.location });
                hordeSearchedSets.Add(capital, new HashSet<Location> { capital.location });
                stepLocations.Add(capital, new HashSet<Location> { capital.location });
            }

            for (int i = 1; i < 128; i++)
            {
                Dictionary<Set_OrcCamp, HashSet<Location>> newStepLocations = new Dictionary<Set_OrcCamp, HashSet<Location>>();
                HashSet<Location> conflicts = new HashSet<Location>();

                foreach (Set_OrcCamp capital in capitals)
                {
                    newStepLocations.Add(capital, new HashSet<Location>());
                    foreach (Location loc in stepLocations[capital])
                    {
                        foreach (Location neighbour in loc.getNeighbours())
                        {
                            if (!hordeSearchedSets[capital].Contains(neighbour))
                            {
                                hordeSearchedSets[capital].Add(neighbour);

                                if (!hordeLocationSets.Any(p => p.Value.Contains(neighbour)))
                                {
                                    if (neighbour.soc == orcSociety || (neighbour.settlement != null && neighbour.settlement.subs.Any(sub => sub is Sub_OrcWaystation waystation && waystation.orcSociety == orcSociety)))
                                    {
                                        if (!conflicts.Contains(neighbour) && newStepLocations.Any(p => p.Key != capital && p.Value.Contains(neighbour)))
                                        {
                                            conflicts.Add(neighbour);
                                        }
                                        newStepLocations[capital].Add(neighbour);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (Location conflict in conflicts)
                {
                    List<Set_OrcCamp> conflictCapitals = new List<Set_OrcCamp>();
                    foreach (KeyValuePair<Set_OrcCamp, HashSet<Location>> pair in newStepLocations)
                    {
                        if (pair.Value.Contains(conflict))
                        {
                            conflictCapitals.Add(pair.Key);
                        }
                    }

                    Set_OrcCamp conflictWinner = conflictCapitals[Eleven.random.Next(conflictCapitals.Count)];
                    foreach (Set_OrcCamp capital in conflictCapitals)
                    {
                        if (capital == conflictWinner)
                        {
                            continue;
                        }

                        newStepLocations[capital].Remove(conflict);
                    }
                }

                stepLocations = newStepLocations;

                foreach (KeyValuePair<Set_OrcCamp, HashSet<Location>> pair in stepLocations)
                {
                    hordeLocations[pair.Key].AddRange(pair.Value);
                    hordeLocationSets[pair.Key].UnionWith(pair.Value);
                }

                if (stepLocations.All(p => p.Value.Count == 0))
                {
                    break;
                }
            }

            Dictionary<Set_OrcCamp, Tuple<SG_Orc, HolyOrder_Orcs>> rebelData = new Dictionary<Set_OrcCamp, Tuple<SG_Orc, HolyOrder_Orcs>>();
            hordeLocationSets.Remove(capitalCamp);
            foreach (Set_OrcCamp rebelCapital in capitals)
            {
                if (rebelCapital.locationIndex == capital)
                {
                    continue;
                }

                SG_Orc rebelSociety = new SG_Orc(map, rebelCapital.location);
                ModCore.Get().data.orcSGCultureMap.TryGetValue(rebelSociety, out HolyOrder_Orcs rebelCulture);

                rebelData.Add(rebelCapital, new Tuple<SG_Orc, HolyOrder_Orcs>(rebelSociety, rebelCulture));

                if (orcSociety.canGoUnderground())
                {
                    rebelSociety.canGoUndergroundFlag = true;
                }

                foreach (HolyTenet tenet in rebelCulture.tenets)
                {
                    HolyTenet matching = tenets.FirstOrDefault(t => t.GetType() == tenet.GetType() || tenet.GetType().IsSubclassOf(t.GetType()));
                    if (matching != null)
                    {
                        tenet.status = matching.status;
                    }
                }

                if (ophanim_PerfectSociety)
                {
                    rebelCulture.ophanim_PerfectSociety = true;
                }

                rebelCapital.location.soc = rebelSociety;
                if (rebelCapital.specialism != 1 && rebelCapital.specialism != 2 && rebelCapital.specialism != 3)
                {
                    if (rebelCapital.army != null)
                    {
                        rebelCapital.army.die(map, "Desertion");
                        rebelCapital.army = null;
                    }

                    rebelCapital.specialism = 1;
                }

                if (rebelCapital.army != null)
                {
                    rebelCapital.army.society = rebelSociety;
                    rebelCapital.army.task = null;
                }
                else
                {
                    UM_OrcArmy army = null;
                    if (rebelCapital.specialism == 3)
                    {
                        army = new UM_OrcBeastArmy(rebelCapital.location, rebelSociety, rebelCapital);
                    }
                    else
                    {
                        army = new UM_OrcArmy(rebelCapital.location, rebelSociety, rebelCapital);
                    }

                    rebelCapital.army = army;
                    rebelCapital.location.units.Add(army);
                    rebelCapital.armyRebuildTimer = 0;
                    map.units.Add(army);

                    if (army is UM_OrcBeastArmy bestialHorde)
                    {
                        bestialHorde.updateMaxHP();
                        bestialHorde.hp = bestialHorde.maxHp;
                    }
                    else
                    {
                        army.updateMaxHP();
                        army.hp = army.maxHp;
                    }
                }

                Sub_Temple temple = (Sub_Temple)rebelCapital.subs.FirstOrDefault(sub => sub is Sub_OrcCultureCapital || sub is Sub_OrcTemple);
                rebelCapital.subs.Remove(temple);

                Sub_OrcCultureCapital rebelSeat = new Sub_OrcCultureCapital(rebelCapital, rebelCulture);
                rebelCapital.subs.Add(rebelSeat);
                rebelCulture.seat = rebelSeat;

                for (int i = 1; i < hordeLocations[rebelCapital].Count; i++)
                {
                    Location loc = hordeLocations[rebelCapital][i];
                    if (loc.soc == orcSociety)
                    {
                        loc.soc = rebelSociety;
                    }
                    else if (loc.soc == this)
                    {
                        loc.soc = rebelCulture;
                    }
                    
                    if (loc.settlement != null)
                    {
                        Sub_OrcWaystation waystation = (Sub_OrcWaystation)loc.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation way && way.orcSociety == orcSociety);
                        if (waystation != null)
                        {
                            waystation.orcSociety = rebelSociety;
                        }

                        if (loc.settlement is Set_OrcCamp camp)
                        {
                            rebelCulture.camps.Add(camp);
                            camps.Remove(camp);

                            if (camp.specialism > 0)
                            {
                                rebelCulture.specializedCamps.Add(camp);
                                specializedCamps.Remove(camp);
                            }

                            if (camp.army != null)
                            {
                                camp.army.society = rebelSociety;
                                camp.army.task = null;
                            }

                            temple = (Sub_Temple)camp.subs.FirstOrDefault(sub => sub is Sub_OrcCultureCapital || sub is Sub_OrcTemple);
                            if (temple != null)
                            {
                                camp.subs.Remove(temple);
                                camp.subs.Add(new Sub_OrcTemple(camp, rebelCulture));
                            }
                        }
                    }
                }
            }

            foreach (Unit unit in map.units)
            {
                if (!unit.isDead && !unit.isCommandable() && (unit.society == orcSociety || unit.society == this) && unit.homeLocation != -1)
                {
                    Location homeLocation = map.locations[unit.homeLocation];

                    Set_OrcCamp rebelCapital = null;
                    SG_Orc rebelSociety = null;
                    HolyOrder_Orcs rebelCulture = null;
                    foreach (KeyValuePair<Set_OrcCamp, HashSet<Location>> pair in hordeLocationSets)
                    {
                        if (pair.Value.Contains(homeLocation))
                        {
                            rebelCapital = pair.Key;
                            rebelSociety = rebelData[pair.Key].Item1;
                            rebelCulture = rebelData[pair.Key].Item2;
                            break;
                        }
                    }

                    if (rebelCapital != null)
                    {
                        if (unit.society == orcSociety)
                        {
                            unit.society = rebelSociety;

                            if (unit is UAEN_OrcUpstart upstart)
                            {
                                upstart.task = new Task_Disrupted(1);

                                for(int i = 0; i < upstart.person.items.Length; i++)
                                {
                                    if (upstart.person.items[i] is I_HordeBanner banner && banner.orcs == orcSociety)
                                    {
                                        banner.orcs = rebelSociety;
                                    }
                                }

                                if (rebelSociety.upstart == null)
                                {
                                    rebelSociety.upstart = upstart;
                                }
                            }
                            else
                            {
                                unit.task = null;
                            }
                        }
                        else
                        {
                            unit.society = rebelCulture;
                            unit.task = null;
                        }
                    }
                }
            }

            updateData();
            foreach (KeyValuePair<Set_OrcCamp, Tuple<SG_Orc, HolyOrder_Orcs>> pair in rebelData)
            {
                SG_Orc rebelSociety = pair.Value.Item1;
                HolyOrder_Orcs rebelCulture = pair.Value.Item2;

                rebelCulture.updateData();
                rebelCulture.manageShamans();

                if (rebelSociety.upstart == null)
                {
                    UAEN_OrcUpstart upstart = rebelSociety.placeUpstart(pair.Key.location);
                    upstart.task = new Task_Disrupted(1);
                }

                if (rebelCulture.nAcolytes == 0)
                {
                    rebelCulture.createAcolyte();
                }

                map.declareWar(rebelSociety, orcSociety);
                foreach (KeyValuePair<Set_OrcCamp, Tuple<SG_Orc, HolyOrder_Orcs>> pair2 in rebelData)
                {
                    if (pair2.Value.Item1 == rebelSociety)
                    {
                        continue;
                    }

                    map.declareWar(rebelSociety, pair2.Value.Item1);
                }
            }

            World.log("Civil war triggered for " + orcSociety.getName() + " hostile capitals " + rebelData.Count.ToString());
            map.addUnifiedMessage(orcSociety, null, "Civil War", orcSociety.getName() + " descends into civil war, with remote territories raising their armies against the horde.\n\nRebels and loyalists will now fight to destroy the other in the hopes that they can rebuild.", UnifiedMessage.messageType.CIVIL_WAR, false);
        }

        public override bool isDark()
        {
            return true;
        }
    }
}
