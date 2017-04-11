using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace PokeSim.Models
{
    public class LearnableAttack
    {
        [Key]
        public int Id
        {
            get; set;
        }

        [Display(Name = "Pokemon Species")]
        public int PokemonBaseId
        {
            get; set;
        }

        [Display(Name = "Attack")]
        public int AttackId
        {
            get; set;
        }
    }

    public class LearnableAttackDbContext : DbContext
    {
        public DbSet<LearnableAttack> LearnableAttacks
        {
            get; set;
        }
        
        public bool isIdValid(int id)
        {
            return (LearnableAttacks.Find(id) != null);
        }

        /// <summary>
        /// Checks to see if the pokemon base is already associated with the given attack.
        /// </summary>
        public bool comboExists(int pokemonBaseId, int attackId)
        {
            return (LearnableAttacks.Where(i => i.PokemonBaseId == pokemonBaseId && i.AttackId == attackId).FirstOrDefault() != null);
        }


        /// <summary>
        /// Gets a list of attacks associated with the given pokemon base, if there are any.
        /// </summary>
        public List<LearnableAttack> getPkmnBaseAttackList(int pokemonBaseId)
        {
            return LearnableAttacks.Where(i => i.PokemonBaseId == pokemonBaseId).ToList();
        }

        /// <summary>
        /// Gets a list of the pokemon bases associated with the given attack, if there are any.
        /// </summary>
        public List<LearnableAttack> getAttacksPkmnBaseList(int attackId) 
        {
            return LearnableAttacks.Where(i => i.AttackId == attackId).ToList();
        }

        /// <summary>
        /// Gets a dictionary of Pokemon Base Ids and their associated Attack Id lists
        /// </summary>
        public Dictionary<int, Dictionary<int, LearnableAttack>> getPkmnBaseToAttackDict()
        {
            Dictionary<int, Dictionary<int, LearnableAttack>> retDict = new Dictionary<int, Dictionary<int, LearnableAttack>>();
            foreach (LearnableAttack item in LearnableAttacks)
            {
                Dictionary<int, LearnableAttack> currDict;
                if (!retDict.TryGetValue(item.PokemonBaseId, out currDict))
                {
                    currDict = new Dictionary<int, LearnableAttack>();
                    retDict.Add(item.PokemonBaseId, currDict);
                }
                if (!currDict.ContainsKey(item.AttackId))
                {
                    currDict.Add(item.AttackId, item);
                }
            }
            return retDict;
        }


        /// <summary>
        /// Gets a dictionary of Attack Ids and the Ids of the Pokemon Bases that can learn them.
        /// </summary>
        public Dictionary<int, Dictionary<int, LearnableAttack>> getAttackToPkmnBaseDict()
        {
            Dictionary<int, Dictionary<int, LearnableAttack>> retDict = new Dictionary<int, Dictionary<int, LearnableAttack>>();
            foreach (LearnableAttack item in LearnableAttacks)
            {
                Dictionary<int, LearnableAttack> currDict;
                if (!retDict.TryGetValue(item.AttackId, out currDict))
                {
                    currDict = new Dictionary<int, LearnableAttack>();
                    retDict.Add(item.AttackId, currDict);
                }
                if (!currDict.ContainsKey(item.PokemonBaseId))
                {
                    currDict.Add(item.PokemonBaseId, item);
                }
            }
            return retDict;
        }
    }
}