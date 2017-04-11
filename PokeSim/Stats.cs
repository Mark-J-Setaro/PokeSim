using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeSim
{
    public enum Stat
    {
        None=0,
        HP=1,
        Attack=2,
        Defense=3,
        SpecialAttack=4,
        SpecialDefense=5,
        Speed=6
    }

    public static class StatsHandler
    {
        public static String getName(int stat, bool abbreviate = false)
        {
            String ret;
            switch (stat)
            {
                case 1:
                    if (abbreviate) { ret = "HP"; }
                    else { ret = "HP"; }
                    break;
                case 2:
                    if (abbreviate) { ret = "Att"; }
                    else { ret = "Attack"; }
                    break;
                case 3:
                    if (abbreviate) { ret = "Defense"; }
                    else { ret = "Defense"; }
                    break;
                case 4:
                    if (abbreviate) { ret = "SpAtt"; ; }
                    else { ret = "Special Attack"; }
                    break;
                case 5:
                    if (abbreviate) { ret = "SpDef"; }
                    else { ret = "Special Defense"; }
                    break;
                case 6:
                    if (abbreviate) { ret = "Spd"; }
                    else { ret = "Speed"; }
                    break;
                default:
                    ret = "None";
                    break;
            }
            return ret;
        }

        public static String getName(Stat stat, bool abbreviate = false)
        {
            return getName((int)stat, abbreviate);
        }
        
        public static int getInt(string statString)
        {
            int ret;
            switch (statString.ToLower())
            {
                case "hp":
                    ret = 1;
                    break;
                case "attack":
                    ret = 2;
                    break;
                case "att":
                    ret = 2;
                    break;
                case "defense":
                    ret = 3;
                    break;
                case "def":
                    ret = 3;
                    break;
                case "specialattack":
                    ret = 4;
                    break;
                case "special attack":
                    ret = 4;
                    break;
                case "spatt":
                    ret = 4;
                    break;
                case "specialdefense":
                    ret = 5;
                    break;
                case "special defense":
                    ret = 5;
                    break;
                case "spdef":
                    ret = 5;
                    break;
                case "speed":
                    ret = 6;
                    break;
                case "spd":
                    ret = 6;
                    break;
                default:
                    ret = 0;
                    break;
            }
            return ret;
        }
        
        public static Stat getEnum(string statString)
        {
            return (Stat)getInt(statString);
        }
        
        /// <summary>
        /// Returns a string array of normal, Abbreviated, or literal Enum names.
        /// </summary>
        public static string[] getNames( bool abbreviate = false, bool enumOnly = false )
        {
            List<string> retList = new List<string>();
            foreach (Stat e in Enum.GetValues(typeof(Stat)).Cast<Stat>())
            {
                if (enumOnly)
                {
                    retList.Add(e.ToString());
                }
                else
                {
                    retList.Add(getName((int)e, abbreviate));
                }
            }
            return retList.ToArray();
        }

    }
}
