using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Data.Entity.Validation;
using System.Diagnostics;

using PokeSim.Models;
using System.Text;

namespace PokeSim.Controllers
{
    [Authorize]
    public class AttackEffectController : Controller
    {
        private static string objectName = "Attack Effect";
        private AttackEffectDbContext db = new AttackEffectDbContext();
        private AttackDbContext db_attacks = new AttackDbContext();



        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult Overview(string message = null)
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            ViewBag.IsAdmin = isAdmin;
            ViewBag.Message = message;
            ViewBag.Attacks = db_attacks.GetDict();
            ViewBag.CurrentItems = db.GetGroupedDict();
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
            Dictionary<string, bool> expectedFields = new Dictionary<string, bool>()
                    {
                        {"AttackId", true },
                        {"EffectCategory", true},
                        {"Effect", true},
                        {"Target", false},
                        {"Probability", false},
                        {"Magnitude", false}
                    };

            ViewBag.Message = "";
            //get the raw string;
            String rawData = collection["RawData"];
            if (!String.IsNullOrWhiteSpace(rawData))
            {
                try
                {
                    Dictionary<int, string> attackDict = db_attacks.GetDict();
                    Dictionary<string, int> attackLookupDict = db_attacks.GetLookupDict();
                    Dictionary<string, int> effectLookupDict = EnumHelpers.getAllEffectNamesDict();

                    List<List<String>> cells = LoadDataFromFile_BreakUpString(rawData);
                    List<Dictionary<string, string>> fieldValDicts = LoadDataFromFile_FieldValDicts(cells, expectedFields);
                    List<AttackEffect> loadedAttacks = LoadDataFromFile_ParseToObjects(fieldValDicts, attackLookupDict, effectLookupDict);
                    LoadDataFromFile_SaveToDatabase(loadedAttacks);
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
        public ActionResult Create(string message = null)
        {
            ViewBag.Message = message;
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
                AttackEffect item = new AttackEffect();
                Dictionary<int, string> attackDict = db_attacks.GetDict();
                Dictionary<string, int> categoryLookupDict = EnumHelpers.getAllEffectNamesDict();
                Dictionary<string, Dictionary<string, int>> effectLookupDict = EnumHelpers.getEffectIdFromName();

                item.AttackId = ValidationHelpers.validateAsIntDict(attackDict, collection, "AttackId", true, true, 0);
                item.EffectCategory = ValidationHelpers.validateAsStringEnum<AttackEffectCategory>(collection, "EffectCategory", true, true, (int)AttackEffectCategory.None);
                item.Effect = ValidationHelpers.validateAsStringDict(effectLookupDict[((AttackEffectCategory)item.EffectCategory).ToString()], collection, "Effect", true, true, (int)AttackEffectCategory.None);
                item.EffectTarget = ValidationHelpers.validateAsIntEnum<Target>(collection, "EffectTarget", true, true, (int)Target.None);
                item.Probability = ValidationHelpers.validateAsInt(collection, "Probability", true, true, -1);
                item.Magnitude = ValidationHelpers.validateAsInt(collection, "Magnitude", true, true, 0);

                //we're good. 
                db.AttackEffects.Add(item);
                db.SaveChanges();
                ViewBag.Message = "Successfully created new " + objectName + ".";
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
            ViewBag.Attacks = db_attacks.GetDict();
            return View();
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, string message = null)
        {
            AttackEffect currentItem = db.AttackEffects.Find(id);
            string theMessage = message;
            if (currentItem != null)
            {
                ViewBag.Message = theMessage;
                ViewBag.CurrentItem = currentItem;
                ViewBag.Attacks = db_attacks.GetDict();
                return View();
            }
            else
            {
                theMessage = "Cannot find " + objectName + " with ID " + id;
            }
            return RedirectToAction("Overview", new { message = theMessage });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, FormCollection collection)
        {
            string theMessage = "";
            AttackEffect item = db.AttackEffects.Find(id);
            if (item != null)
            {

                try
                {
                    Dictionary<int, string> attackDict = db_attacks.GetDict();
                    Dictionary<string, int> categoryLookupDict = EnumHelpers.getAllEffectNamesDict();
                    Dictionary<string, Dictionary<string, int>> effectLookupDict = EnumHelpers.getEffectIdFromName();

                    item.AttackId = ValidationHelpers.validateAsIntDict(attackDict, collection, "AttackId", true, true, 0);
                    item.EffectCategory = ValidationHelpers.validateAsStringEnum<AttackEffectCategory>(collection, "EffectCategory", true, true, (int)AttackEffectCategory.None);
                    item.Effect = ValidationHelpers.validateAsStringDict(effectLookupDict[((AttackEffectCategory)item.EffectCategory).ToString()], collection, "Effect", true, true, (int)AttackEffectCategory.None);
                    item.EffectTarget = ValidationHelpers.validateAsIntEnum<Target>(collection, "EffectTarget", true, true, (int)Target.None);
                    item.Probability = ValidationHelpers.validateAsInt(collection, "Probability", true, true, -1);
                    item.Magnitude = ValidationHelpers.validateAsInt(collection, "Magnitude", true, true, 0);

                    //we're good. 
                    db.SaveChanges();
                    theMessage = "Successfully created new " + objectName + ".";
                }
                catch (Exception ex)
                {
                    theMessage = objectName + "Edit failed: \r\n" + ex.Message;
                }
            }
            else
            {
                theMessage = "Cannot find " + objectName + " with ID " + id;
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
        public ActionResult Delete(int id, FormCollection collection)
        {
            string theMessage = "";
            AttackEffect currentItem = db.AttackEffects.Find(id);
            if (currentItem != null)
            {
                try
                {
                    db.AttackEffects.Remove(currentItem);
                    db.SaveChanges();
                    theMessage = "Deleted item successfully.";
                }
                catch (Exception ex)
                {
                    theMessage = "Deletion failed: \r\n" + ex.Message;
                    return View();
                }
            }
            else
            {
                theMessage = "Could not identify item with ID " + id;
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
            AttackEffect currentItem = db.AttackEffects.Find(id);
            if (currentItem != null)
            {
                ViewBag.Message = theMessage;
                ViewBag.CurrentItem = currentItem;
                ViewBag.Attacks = db_attacks.Attacks.Find(currentItem.AttackId);
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
            return RedirectToAction("Index", new { message = theMessage });
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
        /// </summary>
        private List<Dictionary<string, string>> LoadDataFromFile_FieldValDicts(List<List<String>> rawData, Dictionary<string, bool> expectedFields)
        {
            List<Dictionary<string, string>> retList = new List<Dictionary<string, string>>();
            List<string> headers = null;
            for (int i = 0; i < rawData.Count; i++)
            {
                if (i == 0)
                {
                    //grab the headers;
                    headers = rawData[i];
                    //make sure the headers are valid; first that each is recognized, and second that the critical fields are present.
                    foreach (string header in headers)
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
                            if (kvp.Value && !headers.Contains(kvp.Key))
                            {
                                StringBuilder sb = new StringBuilder("Headers do not contian required field '" + kvp.Key + "'. Required fields are: .");
                                foreach (KeyValuePair<string, bool> expectedField in expectedFields)
                                {
                                    if (expectedField.Value)
                                    {
                                        sb.Append(expectedField.Key + " | ");
                                    }
                                }
                                sb.Append(";  \r\nExisting fields: " + String.Join(" | ", headers));
                                throw new Exception(sb.ToString());
                            }
                        }
                    }
                }
                else
                {
                    //grab the data, if it matches/
                    if (headers.Count == rawData[i].Count)
                    {
                        Dictionary<string, string> newLine = new Dictionary<string, string>();
                        for (int c = 0; c < headers.Count; c++)
                        {
                            newLine.Add(headers[c], rawData[i][c]);
                        }
                        retList.Add(newLine);
                    }
                    else
                    {
                        throw new Exception("Line #" + i + " does not match header format; expected " + headers.Count + " items, found " + rawData[i] + ". Contents: <" + String.Join("><", rawData[i]) + ">");
                    }
                }
            }
            return retList;
        }



        /// <summary>
        /// Parses the string values from the file, turning them into objects. Throws exceptions if it finds they aren't properly validated.
        /// </summary>
        private List<AttackEffect> LoadDataFromFile_ParseToObjects(List<Dictionary<string, string>> rawData, Dictionary<string, int> attackLookupDict, Dictionary<string, int> effectLookupDict)
        {
            List<AttackEffect> retList = new List<AttackEffect>();
            for (int i = 0; i < rawData.Count; i++)
            {
                AttackEffect item = new AttackEffect();
                foreach (KeyValuePair<string, string> kvp in rawData[i])
                {
                    switch (kvp.Key.Trim())
                    {
                        case "AttackId":
                            item.AttackId = ValidationHelpers.validateAsStringDict(attackLookupDict, kvp.Value, kvp.Key, true, 0);
                            break;
                        case "EffectCategory":
                            item.EffectCategory = ValidationHelpers.validateAsStringEnum<AttackEffectCategory>(kvp.Value, kvp.Key, true, (int)AttackEffectCategory.None);
                            break;
                        case "Effect":
                            item.Effect = ValidationHelpers.validateAsStringDict(effectLookupDict, kvp.Value, kvp.Key, true, (int)AttackEffectCategory.None);
                            break;
                        case "EffectTarget":
                            item.EffectTarget = ValidationHelpers.validateAsStringEnum<Target>(kvp.Value, kvp.Key, false, (int)Target.None);
                            break;
                        case "Probability":
                            item.Probability = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, false, -1);
                            break;
                        case "Magnitude":
                            item.Magnitude = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, false, 0);
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
        private void LoadDataFromFile_SaveToDatabase(List<AttackEffect> rawData)
        {
            try
            {
                foreach (AttackEffect att in rawData)
                {
                    //new item; add it.
                    db.AttackEffects.Add(att);
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
