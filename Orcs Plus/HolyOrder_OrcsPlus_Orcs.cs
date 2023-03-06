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

        public List<Set_OrcCamp> camps;

        public List<Set_OrcCamp> specializedCamps = new List<Set_OrcCamp>();

        public List<Sub_Temple> temples;

        public List<Unit> units;

        public List<UA> agents;

        public List<UAA> acolytes;

        public List<Pr_OrcPlunder> plunder;

        public double plunderValue;

        public H_OrcsPlus_DrumsOfWar tenet_drumsOfWar;

        public HolyOrder_OrcsPlus_Orcs(Map m, Location l, SG_Orc o) : base(m, l)
        {
            orcSociety = o;
            capital = l.index;

            generateName(l);
            RefreshLocalCaches();
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
            tenet_drumsOfWar = new H_OrcsPlus_DrumsOfWar(this);
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

        public void RefreshLocalCaches()
        {
            camps = null;
            if (ModCore.comLibCache.settlementsBySocialGroupByType.ContainsKey(orcSociety) && ModCore.comLibCache.settlementsBySocialGroupByType[orcSociety] != null && ModCore.comLibCache.settlementsBySocialGroupByType[orcSociety].ContainsKey(typeof(Set_OrcCamp)) && ModCore.comLibCache.settlementsBySocialGroupByType[orcSociety][typeof(Set_OrcCamp)] != null)
            {
                camps = ModCore.comLibCache.settlementsBySocialGroupByType[orcSociety][typeof(Set_OrcCamp)] as List<Set_OrcCamp>;
            }

            specializedCamps.Clear();
            if (ModCore.comLibCache.orcCampBySocialGroupBySpecialism.ContainsKey(orcSociety) && ModCore.comLibCache.orcCampBySocialGroupBySpecialism[orcSociety] != null)
            {
                List<Set_OrcCamp>[] campsBySpecialism = ModCore.comLibCache.orcCampBySocialGroupBySpecialism[orcSociety];
                for (int i = 1; i < campsBySpecialism.Count(); i++)
                {
                    if (campsBySpecialism[i] != null && campsBySpecialism[i].Count > 0)
                    {
                        specializedCamps.AddRange(campsBySpecialism[i]);
                    }
                }
            }

            temples = null;
            if (ModCore.comLibCache.subsettlementsBySocialGroupByType.ContainsKey(orcSociety) && ModCore.comLibCache.subsettlementsBySocialGroupByType[orcSociety] != null && ModCore.comLibCache.subsettlementsBySocialGroupByType[orcSociety].ContainsKey(typeof(Sub_Temple)) && ModCore.comLibCache.subsettlementsBySocialGroupByType[orcSociety][typeof(Sub_Temple)] != null)
            {
                temples = ModCore.comLibCache.subsettlementsBySocialGroupByType[orcSociety][typeof(Sub_Temple)] as List<Sub_Temple>;
            }

            units = null;
            if (ModCore.comLibCache.unitsBySocialGroupByType.ContainsKey(orcSociety) && ModCore.comLibCache.unitsBySocialGroupByType[orcSociety] != null && ModCore.comLibCache.unitsBySocialGroupByType[orcSociety].ContainsKey(typeof(Unit)) && ModCore.comLibCache.unitsBySocialGroupByType[orcSociety][typeof(Unit)] != null)
            {
                units = ModCore.comLibCache.unitsBySocialGroupByType[orcSociety][typeof(Unit)] as List<Unit>;
            }

            agents = null;
            if (ModCore.comLibCache.unitsBySocialGroupByType.ContainsKey(orcSociety) && ModCore.comLibCache.unitsBySocialGroupByType[orcSociety] != null && ModCore.comLibCache.unitsBySocialGroupByType[orcSociety].ContainsKey(typeof(UA)) && ModCore.comLibCache.unitsBySocialGroupByType[orcSociety][typeof(UA)] != null)
            {
                agents = ModCore.comLibCache.unitsBySocialGroupByType[orcSociety][typeof(UA)] as List<UA>;
            }

            acolytes = null;
            if (ModCore.comLibCache.unitsBySocialGroupByType.ContainsKey(this) && ModCore.comLibCache.unitsBySocialGroupByType[this] != null && ModCore.comLibCache.unitsBySocialGroupByType[this].ContainsKey(typeof(UAA)) && ModCore.comLibCache.unitsBySocialGroupByType[this][typeof(UAA)] != null)
            {
                acolytes = ModCore.comLibCache.unitsBySocialGroupByType[this][typeof(UAA)] as List<UAA>;
            }

            plunder = null;
            plunderValue = 0;
            if (ModCore.comLibCache.propertiesBySocialGroupByType.ContainsKey(orcSociety) && ModCore.comLibCache.propertiesBySocialGroupByType[orcSociety] != null && ModCore.comLibCache.propertiesBySocialGroupByType[orcSociety].ContainsKey(typeof(Pr_OrcPlunder)) && ModCore.comLibCache.propertiesBySocialGroupByType[orcSociety][typeof(Pr_OrcPlunder)] != null)
            {
                plunder = ModCore.comLibCache.propertiesBySocialGroupByType[orcSociety][typeof(Pr_OrcPlunder)] as List<Pr_OrcPlunder>;

                foreach (Pr_OrcPlunder p in plunder)
                {
                    plunderValue += p.gold;
                }
            }
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
            nAcolytes = 0;
            nWorshippers = 0;
            nWorshippingRulers = 0;
            nTemples = 0;
            // Counts orc settlements of orcCulture.
            if (camps != null)
            {
                nWorshippers += camps.Count;
            }

            if (temples != null)
            {
                nTemples = temples.Count;
            }

            if (seat != null)
            {
                nTemples++;
            }

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

            msgs?.Add(new ReasonMsg("Elder powers gain influence with orc cultures by defeating them in combat, aiding them, or harming their foes", 0));

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

            msgs?.Add(new ReasonMsg("Humans gain influence with orc cultures by defeating them in combat", 0));

            return inf;
        }

        public override void turnTick()
        {
            RefreshLocalCaches();

            if (seat != null && !seat.settlement.subs.Contains(seat))
            {
                seat = null;
            }

            if (seat != null && seat.settlement.location.settlement != seat.settlement)
            {
                seat = null;
            }

            CreateSeat();

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

            costAcolyte = 200 * Math.Max(0, acolyteCount - 1);
            costPreach = (int)(10.0 * Math.Pow(Math.Max(0, settlementCount - 5), 0.75));
            costTemple = 50 * (templeCount + 1);

            int cash = processIncome(null);
            int expenses = 0;
            double expenditureDemand = 2 + priorityTemples.status;
            int expenditureTemples = (int)((double)priorityTemples.status / expenditureDemand * (double)cash);
            cash -= expenditureTemples;
            expenses += expenditureTemples;

            cashForPreaching = 0;
            cashForTemples += expenditureTemples;
            // Determine the exact amount of OrcPlunder being used this turn.
            if (cashForAcolytes > costAcolyte * 2)
            {
                expenses -= cashForAcolytes - (costAcolyte * 2);
                cashForAcolytes = costAcolyte * 2;
            }
            else if (cashForAcolytes == costAcolyte * 2)
            {
                cashForAcolytes = costAcolyte * 2;
            }
            else if (cashForAcolytes + cash > costAcolyte * 2)
            {
                expenses += (costAcolyte * 2) - cashForAcolytes;
                cashForAcolytes = costAcolyte * 2;
            }
            else
            {
                cashForAcolytes += cash;
                expenses += cash;
            }

            // Remove Orc plunder until the amount has been matched.
            List<Pr_OrcPlunder> drainedPlunders = new List<Pr_OrcPlunder>();
            while (expenses > 0)
            {
                int iteration = 0;
                int index = Eleven.random.Next(plunder.Count());
                double gold = plunder[index].getGold();

                if (gold <= 0)
                {
                    plunder[index].gold = 0;
                    if (!drainedPlunders.Contains(plunder[index]))
                    {
                        drainedPlunders.Add(plunder[index]);
                    }
                    continue;
                }

                if (gold >= expenses)
                {
                    plunder[index].addGold(-expenses);
                    expenses = 0;
                }
                else
                {
                    plunder[index].addGold(-(int)gold);
                    expenses -= (int)gold;
                }

                iteration++;
                if (iteration > 50)
                {
                    break;
                }
            }

            if (drainedPlunders.Count > 0)
            {
                foreach (Pr_OrcPlunder drainedPunder in drainedPlunders)
                {
                    Item[] items = drainedPunder.getItems();
                    bool plunderEmpty = true;

                    foreach (Item item in items)
                    {
                        if (item != null)
                        {
                            plunderEmpty = false;
                        }
                    }

                    if (plunderEmpty)
                    {
                        drainedPunder.location.properties.Remove(drainedPunder);
                    }
                }
            }

            if (acolyteCount < map.param.holy_maxAcolytes && cashForAcolytes >= costAcolyte)
            {
                cashForAcolytes -= costAcolyte;
                createAcolyte();
            }

            if (cashForAcolytes > costAcolyte * 2)
            {
                expenses -= cashForAcolytes - (costAcolyte * 2);
                cashForAcolytes = costAcolyte * 2;
            }

            if (cashForPreaching > costPreach * 2)
            {
                expenses -= cashForPreaching - (costPreach * 2);
                cashForPreaching = costPreach * 2;
            }

            if (cashForTemples > costTemple * 2)
            {
                expenses -= cashForTemples - (costTemple * 2);
                cashForTemples = costTemple * 2;
            }

            if (expenses < 0 && seat != null)
            {
                Pr_OrcPlunder capitalPlunder = null;
                foreach (Property p in seat.settlement.location.properties)
                {
                    if (p is Pr_OrcPlunder)
                    {
                        capitalPlunder = (Pr_OrcPlunder)p;
                        break;
                    }
                }

                if (capitalPlunder == null)
                {
                    capitalPlunder = new Pr_OrcPlunder(seat.settlement.location);
                    seat.settlement.location.properties.Add(capitalPlunder);
                }
                capitalPlunder.addGold(-expenses);
            }

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
            int num = 0;
            Location location = null;
            foreach (Location location2 in map.locations)
            {
                SettlementHuman settlementHuman = location2.settlement as SettlementHuman;
                if (settlementHuman != null && settlementHuman.order == this)
                {
                    foreach (Subsettlement sub in settlementHuman.subs)
                    {
                        Sub_Temple sub_Temple = sub as Sub_Temple;
                        int num2;
                        if (sub_Temple == null || sub_Temple.order != this)
                        {
                            Sub_HolyOrderCapital sub_HolyOrderCapital = sub as Sub_HolyOrderCapital;
                            num2 = ((sub_HolyOrderCapital != null && sub_HolyOrderCapital.order == this) ? 1 : 0);
                        }
                        else
                        {
                            num2 = 1;
                        }

                        if (num2 != 0)
                        {
                            num++;
                            if (Eleven.random.Next(num) == 0)
                            {
                                location = location2;
                            }
                        }
                    }
                }

                if (!(location2.settlement is Set_MinorOther))
                {
                    continue;
                }

                foreach (Subsettlement sub2 in location2.settlement.subs)
                {
                    Sub_Temple sub_Temple2 = sub2 as Sub_Temple;
                    int num3;
                    if (sub_Temple2 == null || sub_Temple2.order != this)
                    {
                        Sub_HolyOrderCapital sub_HolyOrderCapital2 = sub2 as Sub_HolyOrderCapital;
                        num3 = ((sub_HolyOrderCapital2 != null && sub_HolyOrderCapital2.order == this) ? 1 : 0);
                    }
                    else
                    {
                        num3 = 1;
                    }

                    if (num3 != 0)
                    {
                        num++;
                        if (Eleven.random.Next(num) == 0)
                        {
                            location = location2;
                        }
                    }
                }
            }

            if (location == null)
            {
                location = map.locations[0];
            }

            if (location != null)
            {
                UAA item = new UAA(location, this);
                location.units.Add(item);
                map.units.Add(item);
                updateData();
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
