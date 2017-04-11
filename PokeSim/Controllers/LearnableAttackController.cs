using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PokeSim.Models;
using System.Text;
using System.Data.Entity.Validation;


namespace PokeSim.Controllers
{
    [Authorize]
    public class LearnableAttackController : Controller
    {
        private static string objectName = "Pokemon Species";
        private LearnableAttackDbContext db = new LearnableAttackDbContext();
        private PokemonBaseDbContext db_pkmnBases = new PokemonBaseDbContext();
        private AttackDbContext db_attacks = new AttackDbContext();



        [AllowAnonymous]
        public ActionResult Overview(string message = null)
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            ViewBag.IsAdmin = isAdmin;
            ViewBag.Message = message;
            ViewBag.PokemonBases = db_pkmnBases.GetDict();
            ViewBag.Attacks = db_attacks.GetDict();
            ViewBag.CurrentItems = db.getPkmnBaseToAttackDict();
            return View();
        }



        [AllowAnonymous]
        public ActionResult Index(string message = null)
        {
            ViewBag.Message = message;
            return View();
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult LoadDataFromFile(string message = null)
        {
            ViewBag.Message = message;
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult LoadDataFromFile(FormCollection collection)
        {
            ViewBag.Message = "";
            //get the raw string;
            String rawData = collection["RawData"];
            if (!String.IsNullOrWhiteSpace(rawData))
            {
                try
                {
                    Dictionary<string, int> pokemonBaseLookupDict = db_pkmnBases.GetLookupDict();
                    Dictionary<string, int> attackLookupDict = db_attacks.GetLookupDict();
                    
                    List<List<String>> cells = LoadDataFromFile_BreakUpString(rawData);
                    Dictionary<string, List<string>> fieldValDicts = LoadDataFromFile_FieldValDicts(cells);
                    List<LearnableAttack> loadedObjects = LoadDataFromFile_ParseToObjects(fieldValDicts, pokemonBaseLookupDict, attackLookupDict);
                    LoadDataFromFile_SaveToDatabase(loadedObjects);
                    ViewBag.Message = "Data successfully saved to database.";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                }
            }
            else
            {
                ViewBag.Message = "Could not parse, no data in file.";
            }
            return View();
        }
        


        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(string message = null )
        {
            ViewBag.Message = message;
            ViewBag.PokemonBases = db_pkmnBases.GetDict();
            ViewBag.Attacks = db_attacks.GetDict();
            return View();
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(FormCollection collection)
        {
            ViewBag.Message = "";
            try
            {
                LearnableAttack item = new LearnableAttack();
                item.PokemonBaseId = ValidationHelpers.validateAsInt(collection, "PokemonBaseId", true, true, -1);
                item.AttackId = ValidationHelpers.validateAsInt(collection, "AttackId", true, true, -1);
                //make sure the IDs are valid;
                validateItem(item);
                //good, they exist; get the associated base/attack.
                PokemonBase pkmnBase = db_pkmnBases.PokemonBases.Find(item.PokemonBaseId);
                Attack att = db_attacks.Attacks.Find(item.AttackId);
                //make sure it's not a duplicate.
                if (db.comboExists(item.PokemonBaseId, item.AttackId))
                {
                    throw new Exception("The combination '" + att.Name + "':'" + pkmnBase.Name + "' already exists.");
                }
                //we're good. 
                db.LearnableAttacks.Add(item);
                db.SaveChanges();
                ViewBag.Message = "Successfully created new Pokemon Species/Attack association '" + att.Name + "':'" + pkmnBase.Name + "'.";
            }
            catch (DbEntityValidationException dbEx)
            {
                StringBuilder sb = new StringBuilder("Validation failed for one or more entities.\r\n");
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        sb.Append("Property: " + validationError.PropertyName + " ; Error: " + validationError.ErrorMessage + "\r\n");
                    }
                }
                ViewBag.Message = sb.ToString();
            }
            catch (Exception ex)
            {
                ViewBag.Message = objectName + "Creation failed: \r\n" + ex.Message;
            }
            ViewBag.PokemonBases = db_pkmnBases.GetDict();
            ViewBag.Attacks = db_attacks.GetDict();
            return View();
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Delete(int id, string message = null)
        {
            return helpDetailsOrDelete(id, true, message);
        }


        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Delete(int id, FormCollection collection)
        {
            string theMessage = "";
            LearnableAttack currentItem = db.LearnableAttacks.Find(id);
            if (currentItem != null)
            {
                try
                {
                    db.LearnableAttacks.Remove(currentItem);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    theMessage = "Deletion failed: \r\n" + ex.Message;
                }
            }
            else
            {
                theMessage = "Could not find " + objectName + " with ID " + id;
            }
            return RedirectToAction("Overview", new { message = theMessage });
        }



        public ActionResult Details(int id, string message = null)
        {
            return helpDetailsOrDelete(id, false, message);
        }



        #region Helper Methods

        private ActionResult helpDetailsOrDelete(int id, bool isDeleting, string message = null)
        {
            string theMessage = message;
            LearnableAttack currentItem = db.LearnableAttacks.Find(id);
            if (currentItem != null)
            {
                ViewBag.CurrentItem = currentItem;

                PokemonBase pkmnBase = db_pkmnBases.PokemonBases.Find(currentItem.PokemonBaseId);
                if (pkmnBase != null){ViewBag.PokemonBaseName = pkmnBase.Name;}
                else{ViewBag.PokemonBaseName = null;}
                Attack pkmnAttack = db_attacks.Attacks.Find(currentItem.AttackId);
                if (pkmnAttack != null){ViewBag.AttackName = pkmnAttack.Name;}
                else{ViewBag.AttackName = null;}
                if (isDeleting)
                {
                    return View("Delete");
                }
                else
                {
                    bool isAdmin = ValidationHelpers.isUserAdmin(this);
                    ViewBag.IsAdmin = isAdmin;
                    return View("Details");
                }
            }
            else
            {
                theMessage = "Could not find " + objectName + " with ID " + id;
            }
            return RedirectToAction("Overview", new { message = theMessage });
        }


        /// <summary>
        /// Throws exceptions if the item has ID's that don't exist in the appropriate database.
        /// </summary>
        private void validateItem(LearnableAttack item)
        {
            if (!db_attacks.isIdValid(item.AttackId)) { throw new Exception("Invalid Attack ID '" + item.AttackId + "'."); }
            if (!db_pkmnBases.isIdValid(item.PokemonBaseId)) { throw new Exception("Invalid Pokemon Species ID '" + item.PokemonBaseId + "'."); }
        }


        #endregion


        #region Load Data From File Helpers

        /// <summary>
        /// Breaks the raw data into individual, trimmed 'cells' of string values. Throws out empty lines.
        /// Throws an exception if all of the rows don't have the same number of cells.
        /// </summary>
        private List<List<string>> LoadDataFromFile_BreakUpString(string rawData)
        {
            List<List<String>> retCells = new List<List<String>>();
            string[] lines = rawData.Split('\r');
            foreach (string line in lines)
            {
                //make sure it's not an empty line.
                if (!String.IsNullOrWhiteSpace(line))
                {
                    string[] splitLine = line.Split('\t');
                    for (int i = 0; i < splitLine.Length; i++)
                    {
                        splitLine[i] = splitLine[i].Trim();
                    }
                    retCells.Add(splitLine.ToList());
                }
            }
            return retCells;
        }



        /// <summary>
        /// Breaks the cells up into Field/Value dictionaries, with the keys in every dictionary matching the header of their column.
        /// Also checks the headers, ensuring each field is recognized and all of the required fields are present, and throws exceptions if the columns don't match up with the headers.
        /// Returns a list of present fields later so that edited items don't have empty fields pushed in.
        /// </summary>
        private Dictionary<string, List<string>> LoadDataFromFile_FieldValDicts(List<List<String>> rawData)
        {
            Dictionary<string, List<string>> retDict = new Dictionary<string, List<string>>();
            foreach (List<string> line in rawData)
            {
                //grab the first item;
                string PokemonBaseName = line[0];
                //initialize the entry.
                List<string> AttackNames;
                if (!retDict.TryGetValue(PokemonBaseName, out AttackNames))
                {
                    AttackNames = new List<string>();
                    retDict.Add(PokemonBaseName, AttackNames);
                }
                //add each attack name to the list, if it isn't there yet.
                for (int i = 1; i < line.Count; i++)
                {
                    if (!AttackNames.Contains(line[i]))
                    {
                        AttackNames.Add(line[i]);
                    }
                }
            }
            return retDict;
        }



        /// <summary>
        /// Parses the string values from the file, turning them into objects. Throws exceptions if it finds they aren't properly validated.
        /// </summary>
        private List<LearnableAttack> LoadDataFromFile_ParseToObjects(Dictionary<string, List<string>> rawData, Dictionary<string, int> pokemonBaseLookupDict, Dictionary<string, int> attackLookupDict)
        {
            List<LearnableAttack> retList = new List<LearnableAttack>();
            foreach (KeyValuePair<string, List<string>> entry in rawData)
            {
                //is the attack valid?
                int pokemonBaseId = ValidationHelpers.validateAsStringDict(pokemonBaseLookupDict, entry.Key, "PokemonBaseId", true, -1);
                foreach (string attackName in entry.Value)
                {
                    int attackId = ValidationHelpers.validateAsStringDict(attackLookupDict, attackName, "AttackId", true, -1);
                    LearnableAttack newItem = new LearnableAttack();
                    newItem.PokemonBaseId = pokemonBaseId;
                    newItem.AttackId = attackId;
                    retList.Add(newItem);
                }
            }
            return retList;
        }



        /// <summary>
        /// Saves the new items to the database, updating as necessary.
        /// </summary>
        private void LoadDataFromFile_SaveToDatabase(List<LearnableAttack> rawData)
        {
            try
            {
                foreach (LearnableAttack item in rawData)
                {
                    //check to see if the item already exists; if it does, do not add it.
                    if (db.LearnableAttacks.Where(i => i.PokemonBaseId == item.PokemonBaseId && i.AttackId == item.AttackId).FirstOrDefault() == null)
                    {
                        db.LearnableAttacks.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Problem transferring " + objectName + " to database; " + ex.Message);
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Problem saving changes to " + objectName + " table to database; " + ex.Message);
            }
        }

        #endregion

    }
}
