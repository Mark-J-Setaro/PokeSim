using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace PokeSim.Models
{
    public class PokemonBase
    {
        [Key]
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        [Display(Name = "First Type")]
        public int ElemTypeID_1
        {
            get;
            set;
        }

        [Display(Name = "Second Type")]
        public int ElemTypeID_2
        {
            get;
            set;
        }

        [Display(Name = "Gender Type")]
        public int GenderType
        {
            get;
            set;
        }

        [Display(Name = "First Ability")]
        public int AbilityID_First
        {
            get;
            set;
        }

        [Display(Name = "Second Ability")]
        public int AbilityID_Second
        {
            get;
            set;
        }

        [Display(Name = "Hidden Ability")]
        public int AbilityID_Hidden
        {
            get;
            set;
        }
        
        [Display(Name = "Egg Group 1")]
        public int EggGroup1
        {
            get; set;
        }

        [Display(Name = "Egg Group 2")]
        public int EggGroup2
        {
            get; set;
        }

        [Display(Name = "Base HP")]
        public int BaseHP
        {
           get
            {
                return baseHP;
            }
            set
            {
                int tempVal;
                if (value < 1)
                {
                    tempVal = 1;
                }
                else if (value > 255)
                {
                    tempVal = 255;
                }
                else
                {
                    tempVal = value;
                }
                baseHP = tempVal;
            }
        }
        private int baseHP;
        
        [Display(Name = "Base Attack")]
        public int BaseAtt
        {
            get
            {
                return baseAtt;
            }
            set
            {
                int tempVal;
                if (value < 1)
                {
                    tempVal = 1;
                }
                else if (value > 255)
                {
                    tempVal = 255;
                }
                else
                {
                    tempVal = value;
                }
                baseAtt = tempVal;
            }
        }
        private int baseAtt;

        [Display(Name = "Base Defense")]
        public int BaseDef
        {
            get
            {
                return baseDef;
            }
            set
            {
                int tempVal;
                if (value < 1)
                {
                    tempVal = 1;
                }
                else if (value > 255)
                {
                    tempVal = 255;
                }
                else
                {
                    tempVal = value;
                }
                baseDef = tempVal;
            }
        }
        private int baseDef;


        [Display(Name = "Base Special Attack")]
        public int BaseSpAtt
        {
            get
            {
                return baseSpAtt;
            }
            set
            {
                int tempVal;
                if (value < 1)
                {
                    tempVal = 1;
                }
                else if (value > 255)
                {
                    tempVal = 255;
                }
                else
                {
                    tempVal = value;
                }
                baseSpAtt = tempVal;
            }
        }
        private int baseSpAtt;

        [Display(Name = "Base Special Defense")]
        public int BaseSpDef
        {
            get
            {
                return baseSpDef;
            }
            set
            {
                int tempVal;
                if (value < 1)
                {
                    tempVal = 1;
                }
                else if (value > 255)
                {
                    tempVal = 255;
                }
                else
                {
                    tempVal = value;
                }
                baseSpDef = tempVal;
            }
        }
        private int baseSpDef;

        [Display(Name = "Base Speed")]
        public int BaseSpeed
        {
            get
            {
                return baseSpeed;
            }
            set
            {
                int tempVal;
                if (value < 1)
                {
                    tempVal = 1;
                }
                else if (value > 255)
                {
                    tempVal = 255;
                }
                else
                {
                    tempVal = value;
                }
                baseSpeed = tempVal;
            }
        }
        private int baseSpeed;
    }

    public class PokemonBaseDbContext : DbContext
    {
        public DbSet<PokemonBase> PokemonBases
        {
            get; set;
        }

        public bool isIdValid(int id)
        {
            return (PokemonBases.Find(id) != null);
        }

        public bool nameIsAvailable(string newName, int? existingItemId = null)
        {
            return (PokemonBases.Where(n => n.Name.ToLower() == newName.ToLower() && n.Id != existingItemId).FirstOrDefault() == null);
        }

        public Dictionary<int, string> GetDict()
        {
            Dictionary<int, string> retDict = new Dictionary<int, string>();
            foreach (PokemonBase item in PokemonBases)
            {
                retDict.Add(item.Id, item.Name);
            }
            return retDict;
        }

        public Dictionary<string, int> GetLookupDict()
        {
            Dictionary<string, int> retDict = new Dictionary<string, int>();
            foreach (PokemonBase item in PokemonBases)
            {
                retDict.Add(item.Name, item.Id);
            }
            return retDict;
        }

        public Dictionary<int, PokemonBase> getIntObjDict()
        {
            Dictionary<int, PokemonBase> retDict = new Dictionary<int, PokemonBase>();
            foreach (PokemonBase item in PokemonBases)
            {
                retDict.Add(item.Id, item);
            }
            return retDict;
        }

    }
}
