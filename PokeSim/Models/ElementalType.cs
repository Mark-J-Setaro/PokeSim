using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace PokeSim.Models
{
    public class ElementalType
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class ElementalTypeDbContext : DbContext
    {
        public bool isIdValid(int id)
        {
            return (ElementalTypes.Find(id) != null);
        }

        public DbSet<ElementalType> ElementalTypes { get; set; }

        public bool nameIsAvailable(string newName, int? existingItemId = null)
        {
            return (ElementalTypes.Where(n => n.Name.ToLower() == newName.ToLower() && n.Id != existingItemId).FirstOrDefault() == null);
        }

        public Dictionary<int, string> GetDict()
        {
            Dictionary<int, string> retDict = new Dictionary<int, string>();
            foreach (ElementalType etype in ElementalTypes)
            {
                retDict.Add(etype.Id, etype.Name);
            }
            return retDict;
        }

        public Dictionary<string, int> GetLookupDict()
        {
            Dictionary<string, int> retDict = new Dictionary<string, int>();
            foreach (ElementalType etype in ElementalTypes)
            {
                retDict.Add(etype.Name, etype.Id);
            }
            return retDict;
        }
    }
}