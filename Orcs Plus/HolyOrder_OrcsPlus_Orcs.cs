using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SortedDictionaryProvider;

namespace Orcs_Plus
{
    public class HolyOrder_OrcsPlus_Orcs : HolyOrder
    {
        public SG_Orc orcSociety;

        public bool Infiltrated;

        public List<Set_OrcCamp> camps = new List<Set_OrcCamp>();

        public List<Set_OrcCamp> specializedCamps = new List<Set_OrcCamp>();

        public List<Sub_Temple> temples = new List<Sub_Temple>();

        public List<Unit> units = new List<Unit>();

        public List<UA> agents = new List<UA>();

        public List<UAA> acolytes = new List<UAA>();

        public int acolyteSpawnCounter = 0;

        public List<Pr_OrcPlunder> plunder = new List<Pr_OrcPlunder>();

        public double plunderValue;

        public H_OrcsPlus_Intolerance tenet_drumsOfWar;

        public HolyOrder_OrcsPlus_Orcs(Map m, Location l, SG_Orc o) : base(m, l)
        {
            orcSociety = o;
            capital = l.index;

            generateName(l);
            CreateSeat();

            genderExclusive = -1;
            priorityPreach.status = 0;
            priorityTemples.status = 0;
            elderInflueenceBias = 0.8;
            humanInfluenceBias = 1.2;

            // Remove all base HolyTenets by wiping list.
            tenets = new List<HolyTenet>();

            // Add required base HolyTenets but does not add them to list.
            priorityPreach = new H_Preachers(this);

            // Add required base HolyTenets to new list.
            tenet_alignment = new H_Alignment(this);
            tenets.Add(tenet_alignment);
            tenet_drumsOfWar = new H_OrcsPlus_Intolerance(this);
            tenets.Add(tenet_drumsOfWar);
            tenet_dogmatic = new H_Dogmantic(this);
            tenets.Add(tenet_dogmatic);

            // Add new HolyTenets to list.

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
            }

            // Set map colour, favouring redish colours.
            color = new Color((float)(Eleven.random.NextDouble() * 0.75 + 0.25), (float)(Eleven.random.NextDouble() * 0.75 + 0.25), (float)(Eleven.random.NextDouble() * 0.25));
            color2 = new Color((float)(Eleven.random.NextDouble() * 0.75 + 0.25), (float)(Eleven.random.NextDouble() * 0.75 + 0.25), (float)(Eleven.random.NextDouble() * 0.25));
        }

        public override bool checkIsGone()
        {
            if (cachedGone || orcSociety == null)
            {
                return true;
            }

            cachedGone = orcSociety.isGone();
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
            return map.world.textureStore.loc_evil_orcFort;
        }

        public override Sprite getTempleIcon()
        {
            return map.world.iconStore.orcDefences;
        }

        public override void establishInitialProphecy()
        {
            return;
        }

        public override int[] getTags()
        {
            int[] newTags = new int[1] { Tags.RELIGION };
            int[] tags = base.getTags().Concat(newTags).ToArray();
            return tags;
        }

        new public void humanAIExpenditure()
        {
            HolyTenet holyTenet = null;
            if (tenet_alignment.status < -1)
            {
                holyTenet = tenet_alignment;
            }
            else
            {
                int status = 0;
                int priority = 0;
                int randomSelectorNegative = 0;
                int randomSelectorPositive = 0;
                HolyTenet holyTenet2 = null;
                foreach (HolyTenet tenet in tenets)
                {
                    if (tenet.structuralTenet() || tenet is H_Alignment)
                    {
                        continue;
                    }

                    if (tenet.status < 0)
                    {
                        priority++;
                        randomSelectorNegative++;
                        if (Eleven.random.Next(randomSelectorNegative) == 0)
                        {
                            holyTenet = tenet;
                        }
                    }
                    else if (tenet.status > 0)
                    {
                        status += tenet.status;
                    }
                    else if (tenet.status >= 0 && tenet.getMaxPositiveInfluence() > tenet.status)
                    {
                        randomSelectorPositive++;
                        if (Eleven.random.Next(randomSelectorPositive) == 0)
                        {
                            holyTenet2 = tenet;
                        }
                    }
                }

                if (priority == 0 && tenet_alignment.status < 2)
                {
                    holyTenet = tenet_alignment;
                }
                else if (holyTenet == null && status < 1 && holyTenet2 != null)
                {
                    holyTenet = holyTenet2;
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
            camps.Clear();
            specializedCamps.Clear();
            temples.Clear();
            units.Clear();
            agents.Clear();
            acolytes.Clear();
            plunder.Clear();
            plunderValue = 0;

            ModCore.data.getOrcCamps(map, this, out camps, out specializedCamps);

            if (seat == null || seat.settlement == null || seat.settlement.location?.settlement != seat.settlement || !seat.settlement.subs.Contains(seat))
            {
                seat = null;
                CreateSeat();
            }

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
                if (unit.society != null && (unit.society == orcSociety || unit.society == this))
                {
                    UA ua = unit as UA;

                    if (ua == null)
                    {
                        units.Add(unit);
                    }
                    else
                    {
                        UAA uaa = ua as UAA;

                        if (uaa == null)
                        {
                            agents.Add(ua);
                        }
                        else
                        {
                            acolytes.Add(uaa);
                        }
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
            if (seat == null)
            {
                Set_OrcCamp capitalCamp = map.locations[capital].settlement as Set_OrcCamp;
                if (capitalCamp == null || !(capitalCamp is Set_OrcCamp))
                {
                    //Console.WriteLine("OrcsPlus: Choosing seat of orc culture.");
                    
                    if (specializedCamps.Count == 0)
                    {
                        if (camps.Count == 0)
                        {
                            orcSociety.checkIsGone();
                            checkIsGone();
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

                seat = new Sub_OrcsPlus_OrcCultureCapital(capitalCamp, this);
                capitalCamp.subs.Add(seat);

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

            bool playerCanInfluenceFlag = influenceElder >= influenceElderReq;
            updateData();

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
                UAA item = new UAA(location, this);
                location.units.Add(item);
                map.units.Add(item);
                updateData();
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

                if (allCamps.Count == 0)
                {
                    Console.WriteLine("CommunityLib: ERROR: Tried to give gold to dead orc culture.");
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
            return "Warrior ";
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

        public override void newTempleCreated(Sub_Temple sub_Temple)
        {
            base.newTempleCreated(sub_Temple);
            // Settlement settlement = sub_Temple.settlement;
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
