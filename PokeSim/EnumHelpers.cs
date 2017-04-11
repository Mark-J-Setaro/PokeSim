using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeSim
{

    #region Attack Enums

    /// <summary>
    /// Specially handles damage types; most direct attacks will use the Standard type, but things like Nightshade will use others. 
    /// </summary>
    public enum AttackMethod
    {
        None = 0,
        Standard = 1,
        Level_Damage = 2,
        Pure_Damage = 3,
        Foul_Play = 4,
        SpAtt_To_Def = 5
    }


    /// <summary>
    /// Mutually Exclusive
    /// </summary>
    public enum AttackCategory
    {
        None = 0,
        Physical = 1,
        Special = 2,
        Status = 3
    }

    public enum Target
    {
        None = 0,
        Self = 1,
        Opponent = 2,
        Field = 3,
        Own_Team = 4,
        Opposing_Team = 5,
    }
    

    /// <summary>
    /// List of other attack effect categories
    /// </summary>
    public enum AttackEffectCategory
    {
        None = 0,
        Status = 1,
        Weather = 2,
        Terrain = 3,
        Field = 4,
        Room = 5,
        Entry_Hazard = 6,
        Team_Effect = 7,
        Volatile_Status = 8,
        Volatile_Battle_Status = 9,
        Stat_Change = 10
    }

    /// <summary>
    /// Mutually Exclusive
    /// Affects single pokemon
    /// </summary>
    public enum Status
    {
        None = 0,
        Burn = 1,
        Freeze = 2,
        Paralysis = 3,
        Poison = 4,
        Bad_Poison = 5,
        Sleep = 6
    }

    /// <summary>
    /// Mutually Exclusive
    /// Affects all
    /// </summary>
    public enum Weather
    {
        None = 0,
        Sandstorm = 1,
        Harsh_Sunlight = 2,
        Hail = 3,
        Shadowy_Aura = 4,
        Extremely_Harsh_Sunlight = 5,
        Heavy_Rain = 6,
    }

    /// <summary>
    /// Mutually Exclusive
    /// Affects all
    /// </summary>
    public enum Terrain
    {
        None = 0,
        Electric_Terrain = 1,
        Grassy_Terrain = 2,
        Misty_Terrain = 3,
        Psychic_Terrain = 4
    }

    /// <summary>
    /// Mutually Exclusive
    /// Affects all
    /// </summary>
    public enum Room
    {
        None = 0,
        Trick_Room = 1,
        Magic_Room = 2,
        Wonder_Room = 3,
    }

    /// <summary>
    /// Affects all
    /// </summary>
    public enum Field
    {
        None = 0,
        Gravity = 1,
        Mud_Sport = 2,
        Water_Sport = 3
    }

    public enum EntryHazard
    {
        None = 0,
        Spikes = 1,
        Poison_Spikes = 2,
        Stealth_Rock = 3
    }

    public enum TeamEffect
    {
        None = 0,
        Light_Screen,
        Reflect,
        Tailwind,
    }
    
    ///Affects Target or opposing team
    public enum VolatileStatus
    {
        None = 0,
        Bound,
        Arena_Trapped,
        Cursed,
        Embargo,
        Encore,
        Flinch,
        Heal_Blocked,
        Indentified,
        Infatuated,
        Leech_Seed,
        Nightmare,
        Perish_Song,
        Taunted,
        Telekinesis,
        Torment,
        Yawn,
        Silenced
    }

    ///Affects Self or own team
    public enum VolatileBattleStatus
    {
        None = 0,
        Aqua_Ring,
        Braced,
        Center_of_Attention,
        Defense_Curl,
        Ingrained,
        Magic_Coat,
        Minimized,
        Protected,
        Spikey_Shield,
        Baneful_Bunker,
        Charging,
        Recharging,
        Wide_Guard,
        Quick_Guard,
        Mat_Block,
        Crafty_Guard,
        Substitute,
        Taking_Aim,
        Withdrawing
    }

    ///Affects anyone. 
    public enum StatChange
    {
        None = 0,
        Increase_Attack,
        Increase_Defense,
        Increase_Special_Attack,
        Increase_Special_Defense,
        Increase_Speed,
        Increase_Accuracy,
        Increase_Evasion,
        Sharp_Increase_Attack,
        Sharp_Increase_Defense,
        Sharp_Increase_Special_Attack,
        Sharp_Increase_Special_Defense,
        Sharp_Increase_Speed,
        Sharp_Increase_Accuracy,
        Sharp_Increase_Evasion,
        Decrease_Attack,
        Decrease_Defense,
        Decrease_Special_Attack,
        Decrease_Special_Defense,
        Decrease_Speed,
        Decrease_Accuracy,
        Decrease_Evasion,
        Sharp_Decrease_Attack,
        Sharp_Decrease_Defense,
        Sharp_Decrease_Special_Attack,
        Sharp_Decrease_Special_Defense,
        Sharp_Decrease_Speed,
        Sharp_Decrease_Accuracy,
        Sharp_Decrease_Evasion,
        Increase_All
    }

    public enum Misc
    {
        Recoil
    }

    #endregion

    #region Other Enums

    public enum EggGroup
    {
        Field,
        Humanlike,
        Monster,
        Water1,
        Water2,
        Water3,
        Bug,
        Mineral,
        Amorphous,
        Fairy,
        Grass,
        Dragon,
        Flying,
        Undiscovered,
        Ditto
    }

    public enum GenderType
    {
        Dimorphic,  //Male and Female
        Genderless,  //No gender
        MaleOnly,
        FemaleOnly
    }

    public enum GenderEnum
    {
        Male = 0,
        Female = 1,
        Genderless = 2
    }

    public enum AbilitiesEnum
    {
        First = 0,
        Second,
        Hidden
    }

    public enum FormChangeType
    {
        Evolve = 0,
        MegaEvolve = 1,
        Castform,
        Cherrim,
        Deoxys,
        Burmy,
        Rotom,
        Giratina,
        Shaymin,
        Arceus,
        Darmanitan,
        RevealGlass, //Tornadus, Thundurus, Landorus
        Kyurem,
        Keldeo,
        Meloetta,
        Genesect,
        Aegislash,
        Pumpkaboo,
        Hoopa,
        Oricorio,
        Lycanroc,
        Wishiwashi,
        Silvally,
        Minior,
        Solgaleo,
        Lunala,
        Other = 1000
    }

    


    #endregion
    

    public static class EnumHelpers
    {
        public const string ROLE_ADMIN = "Admin";
        public const int ACCURACY_NEVER_MISS = 1000;

        public static Dictionary<GenderType, List<GenderEnum>> getGenderTypeAssociations()
        {
            return new Dictionary<GenderType, List<GenderEnum>>()
            {
                { GenderType.Dimorphic, new List<GenderEnum>() { GenderEnum.Male, GenderEnum.Female } },
                { GenderType.MaleOnly, new List<GenderEnum>() { GenderEnum.Male } },
                { GenderType.FemaleOnly, new List<GenderEnum>() { GenderEnum.Female } },
                { GenderType.Genderless, new List<GenderEnum>() { GenderEnum.Genderless} }
            };
        }

        public static Dictionary<int, Dictionary<int, string>> getGenderNameFromId()
        {
            Dictionary<int, Dictionary<int, string>> retDict = new Dictionary<int, Dictionary<int, string>>();
            foreach (KeyValuePair<GenderType, List<GenderEnum>> kvp in getGenderTypeAssociations())
            {
                Dictionary<int, string> tempDict = new Dictionary<int, string>();
                retDict.Add((int)kvp.Key, tempDict);
                foreach (GenderEnum ge in kvp.Value)
                {
                    tempDict.Add((int)ge, ge.ToString());
                }
            }
            return retDict;
        }

        public static Dictionary<string, Dictionary<string, int>> getGenderIdFromName()
        {
            Dictionary<string, Dictionary<string, int>> retDict = new Dictionary<string, Dictionary<string, int>>();
            foreach (KeyValuePair<GenderType, List<GenderEnum>> kvp in getGenderTypeAssociations())
            {
                Dictionary<string, int> tempDict = new Dictionary<string, int>();
                retDict.Add(kvp.Key.ToString(), tempDict);
                foreach (GenderEnum ge in kvp.Value)
                {
                    tempDict.Add(ge.ToString(), (int)ge);
                }
            }
            return retDict;
        }
        
        public static Dictionary<string, List<string>> getGenderNames()
        {
            Dictionary<string, List<string>> retDict = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<GenderType, List<GenderEnum>> kvp in getGenderTypeAssociations())
            {
                List<string> tempList = new List<string>();
                retDict.Add(kvp.Key.ToString(), tempList);
                foreach (GenderEnum ge in kvp.Value)
                {
                    tempList.Add(ge.ToString());
                }
            }
            return retDict;
        }

        public static Dictionary<int, string> getAllGenderIdsDict()
        {
            Dictionary<int, string> retDict = new Dictionary<int, string>();
            foreach (GenderEnum ge in enumToList<GenderEnum>())
            {
                retDict.Add((int)ge, ge.ToString());
            }
            return retDict;
        }

        public static Dictionary<string, int> getAllGenderNamesDict()
        {
            Dictionary<string, int> retDict = new Dictionary<string, int>();
            foreach (GenderEnum ge in enumToList<GenderEnum>())
            {
                retDict.Add(ge.ToString(), (int)ge);
            }
            return retDict;
        }
        
        public static Dictionary<int, Dictionary<int, string>> getEffectNameFromId()
        {
            return new Dictionary<int, Dictionary<int, string>>()
            {
                { (int)AttackEffectCategory.None, new Dictionary<int, string>() { { (int)AttackEffectCategory.None, AttackEffectCategory.None.ToString() } } },
                { (int)AttackEffectCategory.Status, intToEnumNameDict<Status>() },
                { (int)AttackEffectCategory.Weather, intToEnumNameDict<Weather>() },
                { (int)AttackEffectCategory.Terrain, intToEnumNameDict<Terrain>() },
                { (int)AttackEffectCategory.Field, intToEnumNameDict<Field>() },
                { (int)AttackEffectCategory.Room, intToEnumNameDict<Room>() },
                { (int)AttackEffectCategory.Entry_Hazard, intToEnumNameDict<EntryHazard>() },
                { (int)AttackEffectCategory.Team_Effect, intToEnumNameDict<TeamEffect>() },
                { (int)AttackEffectCategory.Volatile_Status, intToEnumNameDict<VolatileStatus>() },
                { (int)AttackEffectCategory.Volatile_Battle_Status, intToEnumNameDict<VolatileBattleStatus>() },
                { (int)AttackEffectCategory.Stat_Change, intToEnumNameDict<StatChange>() }
            };
        }

        public static Dictionary<string, Dictionary<string, int>> getEffectIdFromName()
        {
            return new Dictionary<string, Dictionary<string, int>>()
            {
                { AttackEffectCategory.None.ToString(), new Dictionary<string, int>() { { AttackEffectCategory.None.ToString(), (int)AttackEffectCategory.None } } },
                { AttackEffectCategory.Status.ToString(), enumNameToIntDict<Status>() },
                { AttackEffectCategory.Weather.ToString(), enumNameToIntDict<Weather>() },
                { AttackEffectCategory.Terrain.ToString(), enumNameToIntDict<Terrain>() },
                { AttackEffectCategory.Field.ToString(), enumNameToIntDict<Field>() },
                { AttackEffectCategory.Room.ToString(), enumNameToIntDict<Room>() },
                { AttackEffectCategory.Entry_Hazard.ToString(), enumNameToIntDict<EntryHazard>() },
                { AttackEffectCategory.Team_Effect.ToString(), enumNameToIntDict<TeamEffect>() },
                { AttackEffectCategory.Volatile_Status.ToString(), enumNameToIntDict<VolatileStatus>() },
                { AttackEffectCategory.Volatile_Battle_Status.ToString(), enumNameToIntDict<VolatileBattleStatus>() },
                { AttackEffectCategory.Stat_Change.ToString(), enumNameToIntDict<StatChange>() }
            };
        }

        public static Dictionary<string, List<string>> getEffectNames()
        {
            return new Dictionary<string, List<string>>()
            {
                { AttackEffectCategory.None.ToString(), new List<string >(){ AttackEffectCategory.None.ToString() } },
                { AttackEffectCategory.Status.ToString(), enumToNameList<Status>() },
                { AttackEffectCategory.Weather.ToString(), enumToNameList<Weather>() },
                { AttackEffectCategory.Terrain.ToString(), enumToNameList<Terrain>() },
                { AttackEffectCategory.Field.ToString(), enumToNameList<Field>() },
                { AttackEffectCategory.Room.ToString(), enumToNameList<Room>() },
                { AttackEffectCategory.Entry_Hazard.ToString(), enumToNameList<EntryHazard>() },
                { AttackEffectCategory.Team_Effect.ToString(), enumToNameList<TeamEffect>() },
                { AttackEffectCategory.Volatile_Status.ToString(), enumToNameList<VolatileStatus>() },
                { AttackEffectCategory.Volatile_Battle_Status.ToString(), enumToNameList<VolatileBattleStatus>() },
                { AttackEffectCategory.Stat_Change.ToString(), enumToNameList<StatChange>() }
            };
        }
        
        public static Dictionary<string, int> getAllEffectNamesDict()
        {
            Dictionary<string, int> retDict = new Dictionary<string, int>();
            foreach (KeyValuePair<string, Dictionary<string, int>> dictKvp in getEffectIdFromName())
            {
                foreach(KeyValuePair<string, int> entry in dictKvp.Value)
                {
                    if (!retDict.ContainsKey(entry.Key))
                    {
                        retDict.Add(entry.Key, entry.Value);
                    }
                }
            }
            return retDict;
        }
        
        /// <summary>
        /// Checks whether the integer value represents a value in the Enum.
        /// </summary>
        public static bool enumContainsInt<T>(int i, bool throwExceptionIfFalse = false) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(typeof(T).ToString() + " is not an enumerated type");
            if (Enum.GetValues(typeof(T)).Cast<int>().ToArray().Contains(i))
            {
                return true;
            }
            else
            {
                if (throwExceptionIfFalse)
                {
                    throw new Exception("Integer value " + i + " is not Enum type " + typeof(T).ToString());
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Tries to parse an int into enum of type T.
        /// </summary>
        public static T? parseIntToEnum<T>(int testInt) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            { throw new ArgumentException("T is not an Enum type"); }
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(int))
            { throw new ArgumentException("The underlying type of the enum T is not Int32"); }
            try
            {
                T en = (T)(object)(testInt);
                return en;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Turns an enum into a list of strings.
        /// </summary>
        public static List<string> enumToNameList<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            { throw new ArgumentException("T is not an Enum type"); }

            return Enum.GetValues(typeof(T)).Cast<T>().Select(i => i.ToString()).ToList();
        }

        /// <summary>
        /// Gets a complete list of the enums in the type.
        /// </summary>
        public static List<T> enumToList<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            { throw new ArgumentException("T is not an Enum type"); }

            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        /// <summary>
        /// Returns a dictionary if int-to-string for a given enum type.
        /// </summary>
        public static Dictionary<int, string> intToEnumNameDict<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            { throw new ArgumentException("T is not an Enum type"); }
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(int))
            { throw new ArgumentException("The underlying type of the enum T is not Int32"); }

            return Enum.GetValues(typeof(T)).Cast<T>().ToList().Distinct().ToDictionary(i => (int)(object)i, i => i.ToString());
        }
        
        /// <summary>
        /// Returns a dictionary if int-to-enum for a given enum type.
        /// </summary>
        public static Dictionary<int, T> intToEnumDict<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            { throw new ArgumentException("T is not an Enum type"); }
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(int))
            { throw new ArgumentException("The underlying type of the enum T is not Int32"); }

            return Enum.GetValues(typeof(T)).Cast<T>().ToList().Distinct().ToDictionary(i => (int)(object)i, i => i);
        }

        /// <summary>
        /// Creates a String-To-Enum dictionary of enum T.
        /// </summary>
        public static Dictionary<string, T> enumNameToEnumDict<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T is not an Enum type");
            }

            return Enum.GetValues(typeof(T)).Cast<T>().ToList().Distinct().ToDictionary(i => i.ToString(), i => i);
        }

        /// <summary>
        /// Turns an enum into an int-to-string dictionary.
        /// </summary>
        public static Dictionary<string, int> enumNameToIntDict<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            { throw new ArgumentException("T is not an Enum type"); }
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(int))
            { throw new ArgumentException("The underlying type of the enum T is not Int32"); }

            return Enum.GetValues(typeof(T)).Cast<T>().ToList().Distinct().ToDictionary(i => i.ToString(), i => (int)(object)i);
        }

    }
}
