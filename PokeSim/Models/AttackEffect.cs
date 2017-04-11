using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace PokeSim.Models
{
    public class AttackEffect
    {
        [Key]
        public int Id
        {
            get; set;
        }
        
        /// <summary>
        /// The associated attack's ID.
        /// </summary>
        [Required]
        [Display(Name = "Attack ID")]
        public int AttackId
        {
            get; set;
        }

        /// <summary>
        /// Defines the general effect category.
        /// </summary>
        [Required]
        [Display(Name = "Effect Category")]
        public int EffectCategory
        {
            get; set;
        }

        /// <summary>
        /// Defines the effect from the category specified in EffectCategory
        /// </summary>
        [Required]
        public int Effect
        {
            get; set;
        }
        
        /// <summary>
        /// Based on the Target enum. Ignored for certain EffectCategories, like Weather, Room, Field, etc.
        /// </summary>
        [Display(Name = "Target")]
        public int EffectTarget
        {
            get; set;
        }

        public int Probability
        {
            get; set;
        }

        public int Magnitude
        {
            get; set;
        }
        
    }

    public class AttackEffectDbContext : DbContext
    {
        public DbSet<AttackEffect> AttackEffects
        {
            get; set;
        }

        public bool isIdValid(int id)
        {
            return (AttackEffects.Find(id) != null);
        }

        public Dictionary<int, Dictionary<int, string>> GetDict()
        {
            return EnumHelpers.getEffectNameFromId();
        }

        public Dictionary<string, Dictionary<string, int>> GetLookupDict()
        {
            return EnumHelpers.getEffectIdFromName();
        }

        public Dictionary<int, List<AttackEffect>> GetGroupedDict()
        {
            Dictionary<int, List<AttackEffect>> retDict = new Dictionary<int, List<AttackEffect>>();
            foreach (AttackEffect item in AttackEffects)
            {
                List<AttackEffect> currentList = null;
                if (!retDict.TryGetValue(item.AttackId, out currentList))
                {
                    currentList = new List<AttackEffect>();
                    retDict.Add(item.AttackId, currentList);
                }
                currentList.Add(item);
            }
            return retDict;
        }

    }
}