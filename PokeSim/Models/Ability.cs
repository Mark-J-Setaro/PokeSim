using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace PokeSim.Models
{
    public class Ability
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

        public string Description
        {
            get; set;
        }
    }

    public class AbilityDbContext : DbContext
    {
        public DbSet<Ability> Abilities
        {
            get; set;
        }

        public bool isIdValid(int id)
        {
            return (Abilities.Find(id) != null);
        }

        public bool nameIsAvailable(string newName, int? existingItemId = null)
        {
            return (Abilities.Where(n => n.Name.ToLower() == newName.ToLower() && n.Id != existingItemId).FirstOrDefault() == null);
        }

        public Dictionary<int, string> GetDict()
        {
            Dictionary<int, string> retDict = new Dictionary<int, string>();
            foreach (Ability item in Abilities)
            {
                retDict.Add(item.Id, item.Name);
            }
            return retDict;
        }

        public Dictionary<string, int> GetLookupDict()
        {
            Dictionary<string, int> retDict = new Dictionary<string, int>();
            foreach (Ability item in Abilities)
            {
                retDict.Add(item.Name, item.Id);
            }
            return retDict;
        }

    }
}