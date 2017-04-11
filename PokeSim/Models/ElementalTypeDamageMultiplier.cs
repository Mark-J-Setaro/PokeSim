using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace PokeSim.Models
{
    public class ElementalTypeDamageMultiplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Attack Type")]
        public int ElementalTypeId_Attack { get; set; }

        [Required]
        [Display(Name = "Defend Type")]
        public int ElementalTypeId_Defend { get; set; }

        //Limit to 0, .5, 1, and 2
        [Required]
        public double Multiplier
        {
            get
            {
                return multiplier;
            }
            set
            {
                if (value >= 1.5) { multiplier = 2; }
                else if (value >= .75) { multiplier = 1; }
                else if (value >= .25) { multiplier = .5; }
                else { multiplier = 0; }
            }
        }
        private double multiplier;
    }

    public class ElementalTypeDamageMultiplierDbContext : DbContext
    {
        public bool isIdValid(int id)
        {
            return (ElementalTypeDamageMultipliers.Find(id) != null);
        }

        public DbSet<ElementalTypeDamageMultiplier> ElementalTypeDamageMultipliers
        {
            get; set;
        }

        public bool comboIsAvailable(int attType, int defType, int? existingItemId = null)
        {
            return (ElementalTypeDamageMultipliers.Where(n => n.ElementalTypeId_Attack == attType && n.ElementalTypeId_Defend == defType && n.Id != existingItemId).FirstOrDefault() == null);
        }

        /// <summary>
        /// Gets a meaningful dictionary of elemental effectiveness, with the format 'attackTypeID', 'defendTypeId', and outputs a KVP of the ETDM ID and multiplier.
        /// Pass in the ElementalType's dictionary to limit the returning dictionary to only those values.
        /// </summary>
        public Dictionary<int, Dictionary<int, KeyValuePair< int, double >>> GetDict( Dictionary<int, string> elementalDict = null )
        {
            Dictionary<int, Dictionary<int, KeyValuePair<int, double>>> retDict = new Dictionary<int, Dictionary<int, KeyValuePair<int, double>>>();
            foreach (ElementalTypeDamageMultiplier etype in ElementalTypeDamageMultipliers)
            {
                if (elementalDict == null || ( elementalDict.ContainsKey(etype.ElementalTypeId_Attack) && elementalDict.ContainsKey(etype.ElementalTypeId_Defend ) ) )
                {
                    Dictionary<int, KeyValuePair<int, double>> defDict;
                    if (!retDict.ContainsKey(etype.ElementalTypeId_Attack))
                    {
                        defDict = new Dictionary<int, KeyValuePair<int, double>>();
                        retDict.Add(etype.ElementalTypeId_Attack, defDict);
                    }
                    else
                    {
                        defDict = retDict[etype.ElementalTypeId_Attack];
                    }
                    if (!defDict.ContainsKey(etype.ElementalTypeId_Defend))
                    {
                        defDict.Add(etype.ElementalTypeId_Defend, new KeyValuePair<int, double>(etype.Id, etype.Multiplier));
                    }
                    else
                    {
                        //it should not reach here.
                        throw new Exception("Found duplicate ElementalTypeDamageMultipliers in database, ID's " + retDict[etype.ElementalTypeId_Attack][etype.ElementalTypeId_Defend].Key + " and " + etype.Id + ", please delete one of these or the Elemental Effectiveness tables, forms, etc, may display unexpected behavior.");
                    }
                }
            }
            return retDict;
        }
        
        public void initializeByElements(Dictionary<int, string> elemDict)
        {
            Dictionary<int, string> elemTypeDict = new Dictionary<int, string>();
            Dictionary<int, Dictionary<int, KeyValuePair<int, double>>> elemTypeDamgMultDict = new Dictionary<int, Dictionary<int, KeyValuePair<int, double>>>();
            //first, fill up the database as needed with blank entries, according to what should be there.
            foreach (KeyValuePair<int, string> attType in elemDict)
            {
                if (!String.IsNullOrWhiteSpace(attType.Value))
                {
                    foreach (KeyValuePair<int, string> defType in elemDict)
                    {
                        if (!String.IsNullOrWhiteSpace(defType.Value))
                        {

                            ElementalTypeDamageMultiplier match = ElementalTypeDamageMultipliers.Where(i => i.ElementalTypeId_Attack == attType.Key && i.ElementalTypeId_Defend == defType.Key).FirstOrDefault();
                            if (match == null)
                            {
                                //couldn't find it; make it.
                                ElementalTypeDamageMultiplier newElem = new ElementalTypeDamageMultiplier();
                                newElem.ElementalTypeId_Attack = attType.Key;
                                newElem.ElementalTypeId_Defend = defType.Key;
                                newElem.Multiplier = 1;
                                ElementalTypeDamageMultipliers.Add(newElem);
                            }
                        }
                    }
                }
            }
            SaveChanges();
        }

    }
}
