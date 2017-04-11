using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;


namespace PokeSim.Models
{
    public class PokemonInstance
    {
        //there can be a max of 510 EV's across all of their values.
        public const int MAX_EVS = 510;
        //an individual EV can have a max of 252.
        public const int MAX_EV = 252;
        //each IV can have a max of 31.
        public const int MAX_IV = 31;


        [Key]
        [Display(Name = "ID")]
        public int Id
        {
            get; set;
        }

        [Display(Name = "Owner")]
        public string OwnerId
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        [Display(Name = "Species")]
        public int PokemonBaseId
        {
            get; set;
        }

        [Display(Name = "Gender")]
        public int GenderEnumId
        {
            get; set;
        }

        [Display(Name = "Ability")]
        public int AbilityEnumId
        {
            get; set;
        }

        [Display(Name = "Nature")]
        public int NatureId
        {
            get; set;
        }

        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                if (value < 1)
                {
                    level = 1;
                }
                else if (value > 100)
                {
                    level = 100;
                }
                else
                {
                    level = value;
                }
            }
        }
        private int level;

        public int XP
        {
            get
            {
                return xp;
            }
            set
            {
                if (value < 0)
                {
                    xp = 0;
                }
                else
                {
                    xp = value;
                }
            }
        }
        private int xp;

        [Display(Name = "Attack1")]
        public int AttackID_1
        {
            get
            {
                return attackId_1;
            }
            set
            {
                sanitizeAttacks(out attackId_1, value, attackId_2, attackId_3, attackId_4);
            }
        }
        private int attackId_1;

        [Display(Name = "Attack2")]
        public int AttackID_2
        {
            get
            {
                return attackId_2;
            }
            set
            {
                sanitizeAttacks(out attackId_2, value, attackId_1,  attackId_3, attackId_4);
            }
        }
        private int attackId_2;

        [Display(Name = "Attack3")]
        public int AttackID_3
        {
            get
            {
                return attackId_3;
            }
            set
            {
                sanitizeAttacks(out attackId_3, value, attackId_1, attackId_2, attackId_4);
            }
        }
        private int attackId_3;

        [Display(Name = "Attack4")]
        public int AttackID_4
        {
            get
            {
                return attackId_4;
            }
            set
            {
                sanitizeAttacks(out attackId_4, value, attackId_1, attackId_2, attackId_3);
            }
        }
        private int attackId_4;


        [Display(Name = "IV HP")]
        public int IvHP
        {
            get
            {
                return ivHP;
            }
            set
            {
                int tempVal;
                if (value > MAX_IV) { tempVal = MAX_IV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                ivHP = tempVal;
            }
        }
        private int ivHP;

        [Display(Name = "IV Attack")]
        public int IvAtt
        {
            get
            {
                return ivAtt;
            }
            set
            {
                int tempVal;
                if (value > MAX_IV) { tempVal = MAX_IV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                ivAtt = tempVal;
            }
        }
        private int ivAtt;

        [Display(Name = "IV Defense")]
        public int IvDef
        {
            get
            {
                return ivDef;
            }
            set
            {
                int tempVal;
                if (value > MAX_IV) { tempVal = MAX_IV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                ivDef = tempVal;
            }
        }
        private int ivDef;

        [Display(Name = "IV Special Attack")]
        public int IvSpAtt
        {
            get
            {
                return ivSpAtt;
            }
            set
            {
                int tempVal;
                if (value > MAX_IV) { tempVal = MAX_IV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                ivSpAtt = tempVal;
            }
        }
        private int ivSpAtt;

        [Display(Name = "IV Special Defense")]
        public int IvSpDef
        {
            get
            {
                return ivSpDef;
            }
            set
            {
                int tempVal;
                if (value > MAX_IV) { tempVal = MAX_IV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                ivSpDef = tempVal;
            }
        }
        private int ivSpDef;

        [Display(Name = "IV Speed")]
        public int IvSpeed
        {
            get
            {
                return ivSpeed;
            }
            set
            {
                int tempVal;
                if (value > MAX_IV) { tempVal = MAX_IV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                ivSpeed = tempVal;
            }
        }
        private int ivSpeed;
        
        [Display(Name = "EV HP")]
        public int EvHP
        {
            get
            {
                return evHP;
            }
            set
            {
                int tempVal;
                if (value > MAX_EV) { tempVal = MAX_EV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                sanitizeEvs(out evHP, tempVal, evAtt, evDef, evSpAtt, evSpDef, evSpeed);
                resetEvsIfInvalid();
            }
        }
        private int evHP;

        [Display(Name = "EV Attack")]
        public int EvAtt
        {
            get
            {
                return evAtt;
            }
            set
            {
                int tempVal;
                if (value > MAX_EV) { tempVal = MAX_EV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                sanitizeEvs(out evAtt, tempVal, evHP,  evDef, evSpAtt, evSpDef, evSpeed);
                resetEvsIfInvalid();
            }
        }
        private int evAtt;

        [Display(Name = "EV Defense")]
        public int EvDef
        {
            get
            {
                return evDef;
            }
            set
            {
                int tempVal;
                if (value > MAX_EV) { tempVal = MAX_EV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                sanitizeEvs(out evDef, tempVal, evHP, evAtt, evSpAtt, evSpDef, evSpeed);
                resetEvsIfInvalid();
            }
        }
        private int evDef;

        [Display(Name = "EV Special Attack")]
        public int EvSpAtt
        {
            get
            {
                return evSpAtt;
            }
            set
            {
                int tempVal;
                if (value > MAX_EV) { tempVal = MAX_EV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                sanitizeEvs(out evSpAtt, tempVal, evHP, evAtt, evDef, evSpDef, evSpeed);
                resetEvsIfInvalid();
            }
        }
        private int evSpAtt;

        [Display(Name = "EV Special Defense")]
        public int EvSpDef
        {
            get
            {
                return evSpDef;
            }
            set
            {
                int tempVal;
                if (value > MAX_EV) { tempVal = MAX_EV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                sanitizeEvs(out evSpDef, tempVal, evHP, evAtt, evDef, evSpAtt, evSpeed);
                resetEvsIfInvalid();
            }
        }
        private int evSpDef;

        [Display(Name = "EV Speed")]
        public int EvSpeed
        {
            get
            {
                return evSpeed;
            }
            set
            {
                int tempVal;
                if (value > MAX_EV) { tempVal = MAX_EV; }
                else if (value < 0) { tempVal = 0; }
                else { tempVal = value; }
                sanitizeEvs(out evSpeed, tempVal, evHP, evAtt, evDef, evSpAtt, evSpDef);
                resetEvsIfInvalid();
            }
        }
        private int evSpeed;
        
        private void sanitizeEvs (out int testEv, int newVal, int ev1, int ev2, int ev3, int ev4, int ev5)
        {
            if (newVal + ev1 + ev2 + ev3 + ev4 + ev5 >= MAX_EVS)
            {
                newVal = MAX_EVS - (ev1 + ev2 + ev3 + ev4 + ev5);
            }
            testEv = newVal;
        }

        //if somehow the sanitizeEVs function failed to actually sanitize, this resets all EVs to zero
        private void resetEvsIfInvalid()
        {
            if (evHP + evAtt + evDef + evSpAtt + evSpDef + evSpeed > MAX_EVS)
            {
                evHP = 0;
                evAtt = 0;
                evDef = 0;
                evSpAtt = 0;
                evSpDef = 0;
                evSpeed = 0;
            }
        }

        //Makes sure there are duplicate valid attacks; sets to 0 if the new value is a duplicate of an old one. 
        private void sanitizeAttacks(out int testAttack, int newVal, int att1, int att2, int att3)
        {
            if (newVal > 0 && ( newVal == att1 || newVal == att2 || newVal == att3) )
            {
                newVal = 0;
            }
            testAttack = newVal;
        }
        
        //gets the effective stat value for that pokemon;
        public int getStat( Stat selectedStat, PokemonBaseDbContext theBaseDb, NatureDbContext theNatureDb )
        {
            PokemonBase theBase = theBaseDb.PokemonBases.Find(PokemonBaseId);
            Nature theNature = theNatureDb.Natures.Find(NatureId);
            if (theBase != null && theNature != null)
            {
                return getStat(selectedStat, theBase, theNature);
            }
            else
            {
                return -1;
            }
        }

        public int getStat( Stat selectedStat, PokemonBase theBase, Nature theNature )
        {
            if (theBase != null && theNature != null)
            {
                int bs = 0; int ev = 0; int iv = 0;
                switch (selectedStat)
                {
                    case Stat.HP:
                        bs = theBase.BaseHP;
                        ev = evHP;
                        iv = ivHP;
                        break;
                    case Stat.Attack:
                        bs = theBase.BaseAtt;
                        ev = evAtt;
                        iv = ivAtt;
                        break;
                    case Stat.Defense:
                        bs = theBase.BaseDef;
                        ev = evDef;
                        iv = ivDef;
                        break;
                    case Stat.SpecialAttack:
                        bs = theBase.BaseSpAtt;
                        ev = evSpAtt;
                        iv = ivSpAtt;
                        break;
                    case Stat.SpecialDefense:
                        bs = theBase.BaseSpDef;
                        ev = evSpDef;
                        iv = ivSpDef;
                        break;
                    case Stat.Speed:
                        bs = theBase.BaseSpeed;
                        ev = evSpeed;
                        iv = ivSpeed;
                        break;
                    default:
                        break;
                }

                if (selectedStat == Stat.HP)
                {
                    return getStat_HP(bs, ev, iv);
                }
                else if (selectedStat != Stat.None)
                {
                    double natureMult;
                    if (theNature.IncreasedStat == (int)selectedStat)
                    {
                        natureMult = 1.1;
                    }
                    else if (theNature.DecreasedStat == (int)selectedStat)
                    {
                        natureMult = .9;
                    }
                    else
                    {
                        natureMult = 1;
                    }
                    return getStat_NotHP(bs, ev, iv, natureMult);
                }
                else
                {
                    //selected stat is 'None'...
                    return 0;
                }
            }
            else
            {
                //Nature or Base is invalid.
                return -1;
            }
        }

        private int getStat_HP( int bs, int ev, int iv)
        {
            return (int)Math.Round((((2 * (double)bs) + iv + ((double)ev / 4)) * level / 100) + level + 10);
        }

        private int getStat_NotHP(int bs, int ev, int iv, double natureMult)
        {
            return (int)Math.Round((((((2 * (double)bs) + iv + ((double)ev / 4)) * level / 100) + 5) * natureMult ) );
        }



        public int getDamage(Attack theAtt, PokemonInstance opponent, NatureDbContext natures, PokemonBaseDbContext bases, AttackDbContext attacks, ElementalTypeDbContext elemTypes, ElementalTypeDamageMultiplierDbContext elemComps)
        {
            int damage = 0;
            //is the attack null?
            if (theAtt != null)
            {
                //are the required parameters null?
                if (opponent != null && natures != null && bases != null && attacks != null && elemTypes != null && elemComps != null)
                {
                    //good, we have the required parameter objects.
                    //does it have a category we care about?
                    if (theAtt.Category == (int)AttackCategory.Physical || theAtt.Category == (int)AttackCategory.Special)
                    {
                        //get the bases and types.
                        PokemonBase attBase = bases.PokemonBases.Find(PokemonBaseId);
                        PokemonBase defBase = bases.PokemonBases.Find(opponent.PokemonBaseId);
                        //are the bases both good?
                        if (attBase != null && defBase != null)
                        {
                            ElementalType attType = elemTypes.ElementalTypes.Find(theAtt.ElementalTypeID);
                            ElementalType defType1 = elemTypes.ElementalTypes.Find(defBase.ElemTypeID_1);
                            ElementalType defType2 = elemTypes.ElementalTypes.Find(defBase.ElemTypeID_2);

                            if (attType != null && defType1 != null)
                            {
                                //get the multipliers; first comparison must be valid, but second can be if the element is.
                                double elemMult = 1;
                                ElementalTypeDamageMultiplier comp1 = elemComps.ElementalTypeDamageMultipliers.Where(i => i.ElementalTypeId_Attack == attType.Id && i.ElementalTypeId_Defend == defType1.Id).FirstOrDefault();
                                if (comp1 != null)
                                {
                                    //first comparison is valid, get the second if it exists.
                                    ElementalTypeDamageMultiplier comp2 = null;
                                    if (defType2 != null)
                                    {
                                        comp2 = elemComps.ElementalTypeDamageMultipliers.Where(i => i.ElementalTypeId_Attack == attType.Id && i.ElementalTypeId_Defend == defType2.Id).FirstOrDefault();
                                        //make sure the comparison exists.
                                        if (comp2 != null)
                                        {
                                            elemMult *= comp2.Multiplier;
                                        }
                                        else
                                        {
                                            throw new Exception("Could not find Elemental Type Comparison between (att)" + attType.Name + " and (def)" + defType2.Name);
                                        }
                                    }
                                    elemMult *= comp1.Multiplier;
                                    if (elemMult > 0)
                                    {
                                        //Get the natures.
                                        Nature attNat = natures.Natures.Find(NatureId);
                                        Nature defNat = natures.Natures.Find(opponent.NatureId);
                                        if (theAtt.AttackMethodID == (int)AttackMethod.Level_Damage)
                                        {
                                            damage = Level;
                                        }
                                        else if (theAtt.AttackMethodID == (int)AttackMethod.Pure_Damage)
                                        {
                                            damage = theAtt.Power;
                                        }
                                        else if (theAtt.AttackMethodID == (int)AttackMethod.None)
                                        {
                                            throw new Exception("Invalid AttackMethodEnum " + AttackMethod.None.ToString());
                                        }
                                        else
                                        {
                                            //it's a normal attack of some sort.
                                            int attackStat = getDamage_getAtt(theAtt, opponent, attBase, defBase, attNat, defNat);
                                            int defendStat = getDamage_getDef(theAtt, opponent, defBase, defNat);
                                            double modifier = getDamage_getMod(theAtt, attBase);
                                            damage = (int)Math.Round(((((((2 * (double)Level) / 5) + 2) * theAtt.Power * ((double)attackStat / defendStat) / 50) + 2) * modifier * elemMult));
                                            if (damage < 1) { damage = 1; }
                                        }
                                    }
                                    else
                                    {
                                        //ineffective elemental damage, doesn't do squat.
                                        damage = 0;
                                    }
                                }
                                else
                                {
                                    throw new Exception("Could not find Elemental Type Comparison between (att)" + attType.Name + " and (def)" + defType1.Name);
                                }
                            }
                            else
                            {
                                throw new Exception("Requires valid Attack and Defend types; Att:" + (attType == null ? "UNKNOWN(" + theAtt.ElementalTypeID + ")" : attType.Name) + "; Def:" + (defType1 == null ? "UNKNOWN(" + defBase.ElemTypeID_1 + ")" : defType1.Name));
                            }
                        }
                        else
                        {
                            throw new Exception("Requires valid Attacker and Defender Pokemon Bases; Att:" + (attBase == null ? "UNKNOWN(" + PokemonBaseId + ")" : attBase.Name) + "; Def:" + (defBase == null ? "UNKNOWN(" + opponent.PokemonBaseId + ")" : defBase.Name));
                        }
                    }
                    else
                    {
                        //it's a Status move, doesn't do squat.
                        damage = 0;
                    }
                }
                else
                {
                    throw new Exception("Null parameters: " +
                        (opponent == null ? "opponent; " : "") +
                        (natures == null ? "natures; " : "") +
                        (bases == null ? "bases; " : "") +
                        (attacks == null ? "attacks; " : "") +
                        (elemTypes == null ? "elemTypes; " : "") +
                        (elemComps == null ? "elemComps; " : ""));
                }
            }
            else
            {
                //the attack is null, the instance didn't have an attack in that slot; doesn't do squat.
                damage = 0;
            }
            return damage;
        }



        private int getDamage_getAtt(Attack theAtt, PokemonInstance defInst, PokemonBase attBase, PokemonBase defBase, Nature attNat, Nature defNat)
        {
            int att;
            if ( theAtt.AttackMethodID == (int)AttackMethod.Standard )
            {
                Stat selStat;
                if (theAtt.Category == (int)AttackCategory.Physical) { selStat = Stat.Attack; }
                else { selStat = Stat.SpecialAttack; }
                att = getStat(selStat, attBase, attNat);
            }
            else if (theAtt.AttackMethodID == (int)AttackMethod.SpAtt_To_Def)
            {
                att = getStat(Stat.SpecialAttack, attBase, attNat);
            }
            else if (theAtt.AttackMethodID == (int)AttackMethod.Foul_Play)
            {
                att = defInst.getStat(Stat.Attack, defBase, defNat);
            }
            else if (theAtt.AttackMethodID == (int)AttackMethod.None)
            {
                throw new Exception("Invalid AttackMethodEnum " + AttackMethod.None.ToString());
            }
            else
            {
                throw new Exception("Invalid AttackMethodEnumId " + theAtt.AttackMethodID);
            }
            return att;
        }



        private int getDamage_getDef(Attack theAtt, PokemonInstance defInst, PokemonBase defBase, Nature defNat)
        {
            int def;
            if (theAtt.AttackMethodID == (int)AttackMethod.Standard || theAtt.AttackMethodID == (int)AttackMethod.Foul_Play)
            {
                Stat selStat;
                if (theAtt.Category == (int)AttackCategory.Physical) { selStat = Stat.Defense; }
                else { selStat = Stat.SpecialDefense; }
                def = defInst.getStat(selStat, defBase, defNat);
            }
            else if (theAtt.AttackMethodID == (int)AttackMethod.SpAtt_To_Def)
            {
                def = defInst.getStat(Stat.SpecialDefense, defBase, defNat);
            }
            else if (theAtt.AttackMethodID == (int)AttackMethod.None)
            {
                throw new Exception("Invalid AttackMethodEnum " + AttackMethod.None.ToString());
            }
            else
            {
                throw new Exception("Invalid AttackMethodEnumId " + theAtt.AttackMethodID);
            }
            return def;
        }



        private double getDamage_getMod(Attack theAtt, PokemonBase attBase)
        {
            //Targets;
            double retMod = 1;
            if (theAtt.Target == (int)Target.Opposing_Team || theAtt.Target == (int)Target.Field) { retMod *= .75;  }
            //STAB
            if (theAtt.ElementalTypeID == attBase.ElemTypeID_1 || theAtt.ElementalTypeID == attBase.ElemTypeID_2)
            {
                retMod *= 1.5;
            }
            return retMod;
        }
    }

    public class PokemonInstanceDbContext : DbContext
    {

        public DbSet<PokemonInstance> PokemonInstances
        {
            get; set;
        }

        public bool isIdValid(int id)
        {
            return (PokemonInstances.Find(id) != null);
        }

        public Dictionary<int, PokemonInstance> GetDict()
        {
            Dictionary<int, PokemonInstance> retDict = new Dictionary<int, PokemonInstance>();
            foreach (PokemonInstance item in PokemonInstances)
            {
                retDict.Add(item.Id, item);
            }
            return retDict;
        }

        public Dictionary<string, List<PokemonInstance>> GetUserPokemonDict(bool isAdmin, string ownerId = null)
        {
            Dictionary<string, List<PokemonInstance>> retDict = new Dictionary<string, List<PokemonInstance>>();

            List<PokemonInstance> validItems;
            if (isAdmin)
            { validItems = PokemonInstances.ToList(); }
            else { validItems = PokemonInstances.Where(i => i.OwnerId == ownerId).ToList(); }
            
            foreach (PokemonInstance item in validItems)
            {
                List<PokemonInstance> list;
                if (!retDict.TryGetValue(item.OwnerId, out list))
                {
                    list = new List<PokemonInstance>();
                    retDict.Add(item.OwnerId, list);
                }
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
            return retDict;
        }
    }
}