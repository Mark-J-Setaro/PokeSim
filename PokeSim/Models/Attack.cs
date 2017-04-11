using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;


namespace PokeSim.Models
{

    public class Attack
    {

        [Key]
        public int Id
        {
            get; set;
        }

        [Required]
        public string Name
        {
            get; set;
        }

        [Required]
        [Display(Name = "Attack Type")]
        public int ElementalTypeID
        {
            get; set;
        }

        /// <summary>
        /// Matches AttackHelpers.AttackCategory
        /// </summary>
        [Required]
        public int Category
        {
            get
            {
                return category;
            }
            set
            {
                if (value > (int)AttackCategory.Status)
                {
                    category = (int)AttackCategory.Status;
                }
                else if (value < (int)AttackCategory.None)
                {
                    category = (int)AttackCategory.None;
                }
                else
                {
                    category = value;
                }
            }
        }
        private int category;

        public string Description
        {
            get; set;
        }
        
        [Display(Name = "Attack Method")]
        public int AttackMethodID
        {
            get; set;
        }

        [Required]
        [Display(Name = "Max PP")]
        [MinVal(1)]
        public int MaxPP
        {
            get
            {
                return maxpp;
            }
            set
            {
                if (value < 1) { maxpp = 1; }
                else { maxpp = value; }
            }
        }
        private int maxpp;

        [Required]
        [RangeValue(0, 1000)]
        public int Power
        {
            get
            {
                return power;
            }
            set
            {
                if (value < 0)
                {
                    power = 0;
                }
                else
                {
                    power = value;
                }
            }
        }
        private int power;

        /// <summary>
        /// Limited to -7 to 5
        /// </summary>
        [RangeValue(-7, 5)]
        public int Priority
        {
            get
            {
                return priority;
            }
            set
            {
                if (value > 5)
                {
                    priority = 5;
                }
                else if (value < -7)
                {
                    priority = -7;
                }
                else
                {
                    priority = value;
                }
            }
        }
        private int priority;


        /// <summary>
        /// Value must be 1-100, or AttackHelpers.ACCURACY_NEVER_MISS for always-hit.
        /// </summary>
        [RangeWithExcepts(1, 100, new int[] { EnumHelpers.ACCURACY_NEVER_MISS })]
        public int Accuracy
        {
            get
            {
                return accuracy;
            }
            set
            {
                if (value == EnumHelpers.ACCURACY_NEVER_MISS)
                {
                    accuracy = EnumHelpers.ACCURACY_NEVER_MISS;
                }
                else if (value > 100)
                {
                    accuracy = 100;
                }
                else if (value < 1)
                {
                    accuracy = 1;
                }
                else
                {
                    accuracy = value;
                }
            }
        }
        private int accuracy;


        /// <summary>
        /// Matches AttackHelpers.Target
        /// </summary>
        public int Target
        {
            get; set;
        }


    }

    public class AttackDbContext : DbContext
    {
        public DbSet<Attack> Attacks
        {
            get; set;
        }

        public bool isIdValid(int id)
        {
            return (Attacks.Find(id) != null);
        }

        public bool nameIsAvailable(string newName, int? existingItemId = null)
        {
            return (Attacks.Where(n => n.Name.ToLower() == newName.ToLower() && n.Id != existingItemId).FirstOrDefault() == null);
        }

        public Dictionary<int, String> GetDict()
        {
            Dictionary<int, String> retDict = new Dictionary<int, String>();
            foreach (Attack att in Attacks)
            {
                retDict.Add(att.Id, att.Name);
            }
            return retDict;
        }

        public Dictionary<int, string> GetDict(List<int> attackIds)
        {
            Dictionary<int, String> retDict = new Dictionary<int, String>();
            foreach (int attackId in attackIds)
            {
                if (!retDict.ContainsKey(attackId))
                {
                    Attack att = Attacks.Find(attackId);
                    if (att != null)
                    {
                        retDict.Add(attackId, att.Name);
                    }
                }
            }
            return retDict;
        }

        public Dictionary<String, int> GetLookupDict()
        {
            Dictionary<String, int> retDict = new Dictionary<String, int>();
            foreach (Attack att in Attacks)
            {
                retDict.Add(att.Name, att.Id);
            }
            return retDict;
        }
    }



}