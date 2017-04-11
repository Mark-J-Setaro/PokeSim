using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace PokeSim.Models
{
    public class Nature
    {
        [Key]
        public int Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        [Display(Name = "Increased Stat")]
        public int IncreasedStat
        {
            get; set;
        }

        [Display(Name = "Decreased Stat")]
        public int DecreasedStat
        {
            get; set;
        }
    }

    public class NatureDbContext : DbContext
    {
        public DbSet<Nature> Natures
        {
            get; set;
        }

        public bool isIdValid(int id)
        {
            return (Natures.Find(id) != null);
        }


        public bool nameIsAvailable(string newName, int? existingItemId = null)
        {
            return (Natures.Where(n => n.Name.ToLower() == newName.ToLower() && n.Id != existingItemId).FirstOrDefault() == null);
        }

        public Dictionary<int, string> GetDict()
        {
            return Natures.Distinct().ToDictionary(n => n.Id, n => n.Name);
        }
        
        public Dictionary<string, int> GetLookupDict()
        {
            return Natures.Distinct().ToDictionary(n => n.Name, n => n.Id);
        }
        
        public Dictionary<int, Nature> GetObjDict()
        {
            return Natures.Distinct().ToDictionary(n => n.Id, n => n);
        }
    }
}