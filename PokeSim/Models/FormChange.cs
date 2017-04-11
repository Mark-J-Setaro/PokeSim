using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;


namespace PokeSim.Models
{
    public class FormChange
    {
        [Key]
        public int Id
        {
            get; set;
        }

        [Display(Name = "Previous Form")]
        public int PokemonBaseId_Prev
        {
            get
            {
                return pokemonBaseId_Prev;
            }
            set
            {
                sanitize( out pokemonBaseId_Prev, value, pokemonBaseId_Next );
            }
        }
        private int pokemonBaseId_Prev;

        [Display(Name = "Next Form")]
        public int PokemonBaseId_Next
        {
            get
            {
                return pokemonBaseId_Next;
            }
            set
            {
                sanitize( out pokemonBaseId_Next, value, pokemonBaseId_Prev );
            }
        }
        private int pokemonBaseId_Next;

        [Display(Name = "Change Type")]
        public int FormChangeEnum
        {
            get; set;
        }

        private void sanitize( out int theVal, int newVal, int otherVal )
        {
            if (newVal > 0 && newVal == otherVal)
            {
                newVal = 0;
            }
            theVal = newVal;
        }
    }



    public class FormChangeDbContext : DbContext
    {
        public DbSet<FormChange> FormChanges
        {
            get; set;
        }

        public bool isIdValid(int id)
        {
            return (FormChanges.Find(id) != null);
        }

        public bool comboExists(FormChange item, bool isCreating = false)
        {
            return comboExists(item.Id, item.PokemonBaseId_Prev, item.PokemonBaseId_Next, isCreating);
        }

        /// <summary>
        /// Returns a dictionary of Previous ID, Form Change Type, and Next Form list. 
        /// </summary>
        public Dictionary<int, Dictionary<int, List<FormChange>>> Get_Prev_Type_Dict()
        {
            Dictionary<int, Dictionary<int, List<FormChange>>> retDict = new Dictionary<int, Dictionary<int, List<FormChange>>>();
            foreach(FormChange item in FormChanges)
            {
                Dictionary<int, List<FormChange>> changeTypeDict;
                if (!retDict.TryGetValue(item.PokemonBaseId_Prev, out changeTypeDict))
                {
                    changeTypeDict = new Dictionary<int, List<FormChange>>();
                    retDict.Add(item.PokemonBaseId_Prev, changeTypeDict);
                }
                List<FormChange> nextFormList;
                if (!changeTypeDict.TryGetValue(item.FormChangeEnum, out nextFormList))
                {
                    nextFormList = new List<FormChange>();
                    changeTypeDict.Add(item.FormChangeEnum, nextFormList);
                }
                nextFormList.Add(item);
            }
            return retDict;
        }

        /// <summary>
        /// Checks to see if the given pokemon base can evolve into another base.
        /// </summary>
        public bool comboExists(int itemId, int pokemonBaseId_Previous, int pokemonBaseId_Next, bool isCreating = false)
        {
            return (FormChanges.Where(
                i => i.PokemonBaseId_Prev == pokemonBaseId_Previous &&  
                i.PokemonBaseId_Next == pokemonBaseId_Next && 
                (isCreating || i.Id != itemId ))
                .FirstOrDefault() != null);
        }
        
        /// <summary>
        /// Gets a list of possible next forms.
        /// </summary>
        public List<FormChange> getNextFormsList(int pokemonBaseId)
        {
            return FormChanges.Where(i => i.PokemonBaseId_Prev == pokemonBaseId).ToList();
        }
        
        public List<PokemonBase> getNextFormsList(PokemonInstance pokemonInstance, PokemonBaseDbContext db_pkmnBases, List<PokemonBase> restrictedForms = null)
        {
            List<PokemonBase> retList = new List<PokemonBase>();

            List<FormChange> possibleForms = getNextFormsList(pokemonInstance.PokemonBaseId);
            
            PokemonBase currentBase = db_pkmnBases.PokemonBases.Find(pokemonInstance.PokemonBaseId);
            if (currentBase != null)
            {
                GenderEnum currentGender;
                if (EnumHelpers.intToEnumDict<GenderEnum>().TryGetValue(pokemonInstance.GenderEnumId, out currentGender))
                {
                    Dictionary<int, GenderType> genderTypeDict = EnumHelpers.intToEnumDict<GenderType>();
                    foreach (FormChange pf in possibleForms)
                    {
                        PokemonBase pb = db_pkmnBases.PokemonBases.Find(pf.PokemonBaseId_Next);
                        if (pb != null)
                        {
                            GenderType pbgt;
                            if (genderTypeDict.TryGetValue(pb.GenderType, out pbgt))
                            {
                                bool match = false;
                                switch (pbgt)
                                {
                                    case GenderType.Dimorphic:
                                        if (currentGender == GenderEnum.Male || currentGender == GenderEnum.Female) { match = true; }
                                        break;
                                    case GenderType.MaleOnly:
                                        if (currentGender == GenderEnum.Male) { match = true; }
                                        break;
                                    case GenderType.FemaleOnly:
                                        if (currentGender == GenderEnum.Female) { match = true; }
                                        break;
                                    case GenderType.Genderless:
                                        if (currentGender == GenderEnum.Genderless) { match = true; }
                                        break;
                                    default:
                                        break;
                                }
                                if (match)
                                {
                                    retList.Add(pb);
                                }
                                else
                                {
                                    if (restrictedForms != null)
                                    {
                                        restrictedForms.Add(pb);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Pokemon Instance's has invalid gender ID " + pokemonInstance.GenderEnumId);
                }
            }
            else
            {
                throw new Exception("Pokemon Instance's has invalid species ID " + pokemonInstance.PokemonBaseId);
            }
            return retList;
        }
       
    }

    
}