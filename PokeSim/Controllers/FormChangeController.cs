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
    public class FormChangeController : Controller
    {
        string objectName = "Form Change";
        FormChangeDbContext db = new FormChangeDbContext();
        PokemonBaseDbContext db_pokemonBases = new PokemonBaseDbContext();


        [AllowAnonymous]
        public ActionResult Index(string message = null)
        {
            ViewBag.Message = message;
            return View();
        }
        


        public ActionResult Overview(string message = null)
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            ViewBag.IsAdmin = isAdmin;
            ViewBag.Message = message;

            Dictionary<int, string> pkmnBaseDict = db_pokemonBases.GetDict();
            ViewBag.PkmnBaseDict = pkmnBaseDict;

            Dictionary<int, Dictionary<int, List<FormChange>>> currentItems = db.Get_Prev_Type_Dict();
            ViewBag.CurrentItems = currentItems;

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
            ViewBag.Message = LoadDataFromFileHelper(collection);
            return View();
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(string message = null)
        {
            ViewBag.Message = message;
            Dictionary<int, string> pokemonBases = db_pokemonBases.GetDict();
            ViewBag.PokemonBases = pokemonBases;
            return View();
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(FormCollection collection, string message = null)
        {
            string theMessage = "";
            try
            {
                FormChange item = new FormChange();
                //set to the collection's inputs.
                setItemToCollectionInputs(item, collection, true);
                //make sure the IDs are valid;
                sanitizeItem(item);
                //we're good. 
                db.FormChanges.Add(item);
                db.SaveChanges();
                theMessage = "Successfully created new " + objectName + " association '" + db_pokemonBases.PokemonBases.Find(item.PokemonBaseId_Prev).Name + "'->'" + db_pokemonBases.PokemonBases.Find(item.PokemonBaseId_Next).Name + "'.";
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
                theMessage = sb.ToString();
            }
            catch (Exception ex)
            {
                theMessage = objectName + " Creation failed: \r\n" + ex.Message;
            }
            return RedirectToAction("Create", new { message = theMessage });
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, string message = null)
        {
            string theMessage = "";
            FormChange currentItem = db.FormChanges.Find(id);
            if (currentItem != null)
            {
                PokemonBase pkmnBasePrev = db_pokemonBases.PokemonBases.Find(currentItem.PokemonBaseId_Prev);
                PokemonBase pkmnBaseNext = db_pokemonBases.PokemonBases.Find(currentItem.PokemonBaseId_Next);
                ViewBag.PkmnBasePrev = pkmnBasePrev;
                ViewBag.PkmnBaseNext = pkmnBaseNext;
                ViewBag.CurrentItem = currentItem;
                ViewBag.Message = theMessage;
                return View();
            }
            else
            {
                theMessage = "Could not identify " + objectName + " with ID " + id;
            }
            return RedirectToAction("Overview", new { message = theMessage });
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, FormCollection collection)
        {
            string theMessage = "";
            FormChange currentItem = db.FormChanges.Find(id);
            if (currentItem != null)
            {
                try
                {
                    setItemToCollectionInputs(currentItem, collection, false);
                    sanitizeItem(currentItem);
                    db.SaveChanges();
                    theMessage = "Successfully edited ID " + currentItem.Id;
                    return RedirectToAction("Overview", new { message = theMessage });
                }
                catch(Exception ex)
                {
                    theMessage = objectName + " Edit failed: \r\n" + ex.Message;
                    ViewBag.Message = theMessage;
                    return View();
                }
            }
            else
            {
                theMessage = "Could not identify " + objectName + " with ID " + id;
            }
            return RedirectToAction("Overview", new { message = theMessage });
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Delete(int id, string message = null)
        {
            return helpDetailsOrDelete(id, true, message);
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Delete(int id, FormCollection collection, string message = null)
        {
            string theMessage = "";
            FormChange currentItem = db.FormChanges.Find(id);
            if (currentItem != null)
            {
                try
                {
                    db.FormChanges.Remove(currentItem);
                    db.SaveChanges();
                    theMessage = "Successfully deleted ID " + currentItem.Id;
                    return RedirectToAction("Overview", new { message = theMessage });
                }
                catch (Exception ex)
                {
                    theMessage = objectName + " Edit failed: \r\n" + ex.Message;
                    ViewBag.Message = theMessage;
                    return View();
                }
            }
            else
            {
                theMessage = "Could not identify " + objectName + " with ID " + id;
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
            string theMessage = "";
            FormChange currentItem = db.FormChanges.Find(id);
            if (currentItem != null)
            {
                PokemonBase pkmnBasePrev = db_pokemonBases.PokemonBases.Find(currentItem.PokemonBaseId_Prev);
                PokemonBase pkmnBaseNext = db_pokemonBases.PokemonBases.Find(currentItem.PokemonBaseId_Next);
                ViewBag.PkmnBasePrev = pkmnBasePrev;
                ViewBag.PkmnBaseNext = pkmnBaseNext;
                ViewBag.CurrentItem = currentItem;
                ViewBag.Message = theMessage;
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
                theMessage = "Could not identify " + objectName + " with ID " + id;
            }
            return RedirectToAction("Overview", new { message = theMessage });
        }



        /// <summary>
        /// Ensures both IDs exist, the IDs are different, the Previous/Next combo is unique, and the form change enum ID matches the enum.
        /// </summary>
        private void sanitizeItem(FormChange item, bool isCreating = false)
        {
            //make sure both ID's exist.
            if (db_pokemonBases.PokemonBases.Find(item.PokemonBaseId_Prev) == null)
            { throw new Exception( "Unknown Previous Pokemon Species ID " + item.PokemonBaseId_Prev ); }

            if (db_pokemonBases.PokemonBases.Find(item.PokemonBaseId_Next) == null)
            { throw new Exception( "Unknown Next Pokemon Species ID " + item.PokemonBaseId_Next ); }

            if (item.PokemonBaseId_Next == item.PokemonBaseId_Prev)
            { throw new Exception("Previous Pokemon Species ID cannot be the same as the Next (" + item.PokemonBaseId_Prev + "/" + item.PokemonBaseId_Next + ").");}
            
            if (db.comboExists(item, isCreating))
            { throw new Exception( "Combination (" + item.PokemonBaseId_Prev + ")/(" + item.PokemonBaseId_Next + ") already exists." ); }

            if (!EnumHelpers.enumContainsInt<FormChangeType>(item.FormChangeEnum))
            { throw new Exception( "Unknown Form Change Enum ID " + item.FormChangeEnum ); }
            
        }



        /// <summary>
        /// Sets the item's values to what's in the form collection.
        /// Will throw exceptions if the expected item is not present, or if the user is not an admin and attempts to modify the 
        /// </summary>
        private void setItemToCollectionInputs(FormChange item, FormCollection collection, bool creating)
        {
            if (creating)
            {
                item.PokemonBaseId_Prev = ValidationHelpers.validateAsInt(collection, "PokemonBaseId_Prev", true, true, 0);
                item.PokemonBaseId_Next = ValidationHelpers.validateAsInt(collection, "PokemonBaseId_Next", true, true, 0);
            }
            item.FormChangeEnum = ValidationHelpers.validateAsIntEnum<FormChangeType>(collection, "FormChangeEnum", true, true, 0);
        }

        #endregion



        #region Load Data From File Helpers

        /// <summary>
        /// Main method; controls the flow, defines what fields should be there, etc.
        /// </summary>
        private string LoadDataFromFileHelper(FormCollection collection)
        {
            string retMessage = "";
            Dictionary<string, bool> expectedFields = new Dictionary<string, bool>()
            {
                {"PokemonBaseId_Previous", true },
                {"PokemonBaseId_Next", true },
                {"FormChangeEnum", true },
            };

            //get the raw string;
            String rawData = collection["RawData"];
            if (!String.IsNullOrWhiteSpace(rawData))
            {
                try
                {
                    Dictionary<string, int> pokemonBaseLookupDict = db_pokemonBases.GetLookupDict();

                    List<string> presentFields;

                    List<List<String>> cells = LoadDataFromFile_BreakUpString(rawData);
                    List<Dictionary<string, string>> fieldValDicts = LoadDataFromFile_FieldValDicts(cells, expectedFields, out presentFields);
                    List<FormChange> loadedObjects = LoadDataFromFile_ParseToObjects(fieldValDicts, pokemonBaseLookupDict);
                    
                    LoadDataFromFile_SaveToDatabase(loadedObjects, presentFields);
                    retMessage = "Data successfully saved to database.";
                }
                catch (Exception ex)
                {
                    retMessage = ex.Message;
                }
            }
            else
            {
                retMessage = "Could not parse, no data in file.";
            }
            return retMessage;
        }



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
            int lengths = int.MinValue;
            for (int i = 0; i < retCells.Count; i++)
            {
                if (lengths == int.MinValue)
                {
                    //initialize and assign;
                    lengths = retCells[i].Count();
                }
                else
                {
                    if (lengths != retCells[i].Count())
                    {
                        throw new Exception("Line " + i + " of file had unexpected length; File Contents:\r\n" + rawData);
                    }
                }
            }
            return retCells;
        }



        /// <summary>
        /// Breaks the cells up into Field/Value dictionaries, with the keys in every dictionary matching the header of their column.
        /// Also checks the headers, ensuring each field is recognized and all of the required fields are present, and throws exceptions if the columns don't match up with the headers.
        /// Returns a list of present fields later so that edited items don't have empty fields pushed in.
        /// </summary>
        private List<Dictionary<string, string>> LoadDataFromFile_FieldValDicts(List<List<string>> rawData, Dictionary<string, bool> expectedFields, out List<string> presentFields)
        {
            List<Dictionary<string, string>> retList = new List<Dictionary<string, string>>();
            presentFields = null;
            for (int i = 0; i < rawData.Count; i++)
            {
                if (i == 0)
                {
                    //grab the headers;
                    presentFields = rawData[i];
                    //make sure the headers are valid; first that each is recognized, and second that the critical fields are present.
                    foreach (string header in presentFields)
                    {
                        //first that each field is valid;
                        if (!expectedFields.ContainsKey(header))
                        {
                            StringBuilder sb = new StringBuilder("Headers contain unrecognized field name '" + header + "', make sure all field names are spelled correctly (case sensitive). Expected fields: ");
                            foreach (KeyValuePair<string, bool> expectedField in expectedFields)
                            {
                                sb.Append(expectedField.Key + " | ");
                            }
                            throw new Exception(sb.ToString());
                        }
                        //now that each required field is there;
                        foreach (KeyValuePair<string, bool> kvp in expectedFields)
                        {
                            if (kvp.Value && !presentFields.Contains(kvp.Key))
                            {
                                StringBuilder sb = new StringBuilder("Headers do not contian required field '" + kvp.Key + "'. Required fields are: .");
                                foreach (KeyValuePair<string, bool> expectedField in expectedFields)
                                {
                                    if (expectedField.Value)
                                    {
                                        sb.Append(expectedField.Key + " | ");
                                    }
                                }
                                sb.Append(";  \r\nExisting fields: " + String.Join(" | ", presentFields));
                                throw new Exception(sb.ToString());
                            }
                        }
                    }
                }
                else
                {
                    //grab the data, if it matches/
                    if (presentFields.Count == rawData[i].Count)
                    {
                        Dictionary<string, string> newLine = new Dictionary<string, string>();
                        for (int c = 0; c < presentFields.Count; c++)
                        {
                            newLine.Add(presentFields[c], rawData[i][c]);
                        }
                        retList.Add(newLine);
                    }
                    else
                    {
                        throw new Exception("Line #" + i + " does not match header format; expected " + presentFields.Count + " items, found " + rawData[i] + ". Contents: <" + String.Join("><", rawData[i]) + ">");
                    }
                }
            }
            return retList;
        }



        /// <summary>
        /// Parses the string values from the file, turning them into objects. Throws exceptions if it finds they aren't properly validated.
        /// </summary>
        private List<FormChange> LoadDataFromFile_ParseToObjects(List<Dictionary<string, string>> rawData, Dictionary<string, int> pokemonBaseLookupDict)
        {
            List<FormChange> retList = new List<FormChange>();
            for (int i = 0; i < rawData.Count; i++)
            {
                FormChange item = new FormChange();
                foreach (KeyValuePair<string, string> kvp in rawData[i])
                {
                    switch (kvp.Key.Trim())
                    {
                        case "PokemonBaseId_Previous":
                            item.PokemonBaseId_Prev = ValidationHelpers.validateAsStringDict(pokemonBaseLookupDict, kvp.Value, kvp.Key, true, 0);
                            break;
                        case "PokemonBaseId_Next":
                            item.PokemonBaseId_Next = ValidationHelpers.validateAsStringDict(pokemonBaseLookupDict, kvp.Value, kvp.Key, true, 0);
                            break;
                        case "FormChangeEnum":
                            item.FormChangeEnum = ValidationHelpers.validateAsIntEnum<FormChangeType>(kvp.Value, kvp.Key, true, -1);
                            break;
                        default:
                            //shouldn't reach here, we checked all the other items.
                            throw new Exception("Unhandled field " + kvp.Key + " in line " + i + ", should have been handled earlier.");
                    }
                }
                retList.Add(item);
            }
            return retList;
        }



        /// <summary>
        /// Saves the new items to the database, updating as necessary.
        /// </summary>
        private void LoadDataFromFile_SaveToDatabase(List<FormChange> rawData, List<string> presentFields)
        {
            try
            {
                foreach (FormChange item in rawData)
                {
                    //check to see if the item already exists;
                    FormChange currentItem = db.FormChanges.Where(i => i.PokemonBaseId_Prev == item.PokemonBaseId_Prev && i.PokemonBaseId_Next == item.PokemonBaseId_Next).FirstOrDefault();
                    
                    if (currentItem != null)
                    {
                        //edit according to what is there.
                        if (presentFields.Contains("PokemonBaseId_Previous")) { currentItem.PokemonBaseId_Prev = item.PokemonBaseId_Prev; }
                        if (presentFields.Contains("PokemonBaseId_Next")) { currentItem.PokemonBaseId_Next = item.PokemonBaseId_Next; }
                        if (presentFields.Contains("FormChangeEnum")) { currentItem.FormChangeEnum = item.FormChangeEnum; }
                    }
                    else
                    {
                        //new item; just add it.
                        db.FormChanges.Add(item);
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
