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
    public class AttackController : Controller
    {
        private static string objectName = "Attack";
        private AttackDbContext db = new AttackDbContext();
        private ElementalTypeDbContext db_elemTypes = new ElementalTypeDbContext();


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
            ViewBag.Categories = EnumHelpers.intToEnumNameDict<AttackCategory>();
            ViewBag.AttackMethods = EnumHelpers.intToEnumNameDict<AttackMethod>();
            ViewBag.ElementalTypes = db_elemTypes.GetDict();
            ViewBag.Targets = EnumHelpers.intToEnumNameDict<Target>();
            ViewBag.CurrentItems = db.Attacks;
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
                    Dictionary<string, bool> expectedFields = new Dictionary<string, bool>()
                    {
                        {"Name", true },
                        {"Category", false},
                        {"ElementalTypeID", false},
                        {"AttackMethodId", false},
                        {"Description", false},
                        {"MaxPP", false},
                        {"Power", false},
                        {"Priority", false},
                        {"Target", false},
                        {"Accuracy", false}
                    };
                    Dictionary<int, string> elemTypeDict = db_elemTypes.GetDict();
                    Dictionary<string, int> elemLookupDict = db_elemTypes.GetLookupDict();
                    List<List<String>> cells = LoadDataFromFile_BreakUpString(rawData);
                    List<Dictionary<string, string>> fieldValDicts = LoadDataFromFile_FieldValDicts(cells, expectedFields);
                    List<Attack> loadedAttacks = LoadDataFromFile_ParseToObjects(fieldValDicts, elemLookupDict);
                    LoadDataFromFile_DupeNameCheck(loadedAttacks);
                    LoadDataFromFile_SaveToDatabase(loadedAttacks, elemTypeDict);
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
            ViewBag.Categories = EnumHelpers.intToEnumNameDict<AttackCategory>();
            ViewBag.AttackMethods = EnumHelpers.intToEnumNameDict<AttackMethod>();
            ViewBag.ElementalTypes = db_elemTypes.GetDict();
            ViewBag.Targets = EnumHelpers.intToEnumNameDict<Target>();
            return View();
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(FormCollection collection)
        {
            Attack item = null;
            ViewBag.Message = "";
            try
            {
                item = new Attack();
                item.Name = ValidationHelpers.validateAsString(collection, "Name", true, true, "");
                item.Category = ValidationHelpers.validateAsIntEnum<AttackCategory>(collection, "Category", true, true, (int)AttackCategory.None);
                item.ElementalTypeID = ValidationHelpers.validateAsInt(collection, "ElementalTypeID", true, true, 0);
                item.AttackMethodID = ValidationHelpers.validateAsIntEnum<AttackMethod>(collection, "AttackMethodId", true, true, (int)AttackMethod.None);
                item.Description = ValidationHelpers.validateAsString(collection, "Description", true, false, "");
                item.MaxPP = ValidationHelpers.validateAsInt(collection, "MaxPP", true, true, 1);
                item.Power = ValidationHelpers.validateAsInt(collection, "Power", true, true, 0);
                item.Target = ValidationHelpers.validateAsIntEnum<Target>(collection, "Target", true, true, (int)Target.None);
                item.Priority = ValidationHelpers.validateAsInt(collection, "Priority", true, true, 0);

                item.Accuracy = ValidationHelpers.validateAsInt(collection, "Accuracy", true, true, 100);

                //check name
                if (!db.nameIsAvailable(item.Name))
                {
                    throw new Exception("The " + objectName + " name '" + item.Name + "' is already taken.");
                }
                else
                {
                    //we're good. 
                    db.Attacks.Add(item);
                    db.SaveChanges();
                    ViewBag.Message = "Successfully created the '" + item.Name + "' " + objectName + ".";
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                StringBuilder sb = new StringBuilder("Validation failed for one or more entities.\r\n") ;
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
            ViewBag.Categories = EnumHelpers.intToEnumNameDict<AttackCategory>();
            ViewBag.AttackMethods = EnumHelpers.intToEnumNameDict<AttackMethod>();
            ViewBag.ElementalTypes = db_elemTypes.GetDict();
            ViewBag.Targets = EnumHelpers.intToEnumNameDict<Target>();
            return View();
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, string message = null)
        {
            Attack currentItem = db.Attacks.Find(id);
            string theMessage = message;
            if (currentItem != null)
            {
                ViewBag.CurrentItem = currentItem;
                ViewBag.ElementalTypes = db_elemTypes.GetDict();
                ViewBag.Message = theMessage;
                return View();
            }
            else
            {
                theMessage = "Cannot find item with ID " + id;
            }
            return RedirectToAction("Overview", new { message = theMessage });
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, FormCollection collection)
        {
            string theMessage = "";
            ViewBag.StatStrings = StatsHandler.getNames();
            Attack currentItem = db.Attacks.Find(id);
            ViewBag.CurrentItem = currentItem;
            try
            {
                currentItem.Name = ValidationHelpers.validateAsString(collection, "Name", true, true, "");
                currentItem.Category = ValidationHelpers.validateAsIntEnum<AttackCategory>(collection, "Category", true, true, (int)AttackCategory.None);
                currentItem.ElementalTypeID = ValidationHelpers.validateAsInt(collection, "ElementalTypeID", true, true, 0);
                currentItem.AttackMethodID = ValidationHelpers.validateAsIntEnum<AttackMethod>(collection, "AttackMethodId", true, true, (int)AttackMethod.None);
                currentItem.Description = ValidationHelpers.validateAsString(collection, "Description", true, false, "");
                currentItem.MaxPP = ValidationHelpers.validateAsInt(collection, "MaxPP", true, true, 1);
                currentItem.Power = ValidationHelpers.validateAsInt(collection, "Power", true, true, 0);
                currentItem.Priority = ValidationHelpers.validateAsInt(collection, "Priority", true, true, 0);
                currentItem.Target = ValidationHelpers.validateAsIntEnum<Target>(collection, "Target", true, true, (int)Target.None);
                currentItem.Accuracy = ValidationHelpers.validateAsInt(collection, "Accuracy", true, true, 100);

                //check name
                if (!db.nameIsAvailable(currentItem.Name, id))
                {
                    throw new Exception("The " + objectName + " name '" + currentItem.Name + "' is already taken.");
                }
                else
                {
                    //we're good. 
                    db.SaveChanges();
                    theMessage = "Successfully edited the '" + currentItem.Name + "' " + objectName + ".";
                    return RedirectToAction("Overview", new { message = theMessage });
                }
            }
            catch (Exception ex)
            {
                theMessage = objectName + " Edit failed: \r\n" + ex.Message;
            }
            return RedirectToAction("Edit", new { message = theMessage });
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
            Attack currentItem = db.Attacks.Find(id);
            if (currentItem != null)
            {
                try
                {
                    db.Attacks.Remove(currentItem);
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



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Details(int id, string message = null)
        {
            return helpDetailsOrDelete(id, false, message);
        }



        #region Helper Methods

        private ActionResult helpDetailsOrDelete(int id, bool isDeleting, string message = null)
        {
            string theMessage = message;
            Attack currentItem = db.Attacks.Find(id);
            if (currentItem != null)
            {
                ViewBag.Message = theMessage;
                ViewBag.ElementalType = db_elemTypes.ElementalTypes.Find(currentItem.ElementalTypeID);
                ViewBag.CurrentItem = currentItem;
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
        private List<List<String>> LoadDataFromFile_BreakUpString(string rawData)
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
                    //grab the data, if it matches.
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
        private List<Attack> LoadDataFromFile_ParseToObjects(List<Dictionary<string, string>> rawData, Dictionary<string, int> elemLookupDict)
        {
            List<Attack> retList = new List<Attack>();
            for (int i = 0; i < rawData.Count; i++)
            {
                Attack item = new Attack();
                foreach (KeyValuePair<string, string> kvp in rawData[i])
                {
                    switch (kvp.Key.Trim())
                    {
                        case "Name":
                            item.Name = ValidationHelpers.validateAsString(kvp.Value, "Name", true, "");
                            break;
                        case "Category":
                            item.Category = ValidationHelpers.validateAsStringEnum<AttackCategory>(kvp.Value, "Category", true, (int)AttackCategory.Physical);
                            break;
                        case "ElementalTypeID":
                            item.ElementalTypeID = ValidationHelpers.validateAsStringDict(elemLookupDict, kvp.Value, "ElementalTypeID", true, 0);
                            break;
                        case "AttackMethodId":
                            item.AttackMethodID = ValidationHelpers.validateAsStringEnum<AttackMethod>(kvp.Value, "AttackMethodId", true, (int)AttackMethod.Standard);
                            break;
                        case "Description":
                            item.Description = ValidationHelpers.validateAsString(kvp.Value, "Description", false, "");
                            break;
                        case "MaxPP":
                            item.MaxPP = ValidationHelpers.validateAsInt(kvp.Value, "MaxPP", false, 1);
                            break;
                        case "Power":
                            item.Power = ValidationHelpers.validateAsInt(kvp.Value, "Power", false, 0);
                            break;
                        case "Priority":
                            item.Priority = ValidationHelpers.validateAsInt(kvp.Value, "Priority", false, 0);
                            break;
                        case "Target":
                            item.Target = ValidationHelpers.validateAsStringEnum<Target>(kvp.Value, "Target", true, (int)Target.Opponent);
                            break;
                        case "Accuracy":
                            item.Accuracy = ValidationHelpers.validateAsInt(kvp.Value, "Accuracy", false, 100);
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
        /// Checks for duplicate names in the list of parsed attacks, throwing a meaningful exception if it finds one.
        /// </summary>
        private void LoadDataFromFile_DupeNameCheck(List<Attack> rawData)
        {
            Dictionary<string, int> dictCheck = new Dictionary<string, int>();
            for (int i = 0; i < rawData.Count; i++)
            {
                if (!dictCheck.ContainsKey(rawData[i].Name))
                {
                    dictCheck.Add(rawData[i].Name, i);
                }
                else
                {
                    throw new Exception("Duplicate name '" + rawData[i].Name + "' found in lines " + i + " and " + dictCheck[rawData[i].Name] + ".");
                }
            }
        }



        private void LoadDataFromFile_SaveToDatabase(List<Attack> rawData, Dictionary<int, string> elemTypeDict)
        {
            try
            {
                foreach (Attack att in rawData)
                {
                    //are we adding or editing?
                    Attack edt = db.Attacks.Where(a => a.Name.ToLower() == att.Name.ToLower()).FirstOrDefault();
                    if (edt == null)
                    {
                        //new item; add it.
                        db.Attacks.Add(att);
                    }
                    else
                    {
                        //item exists, edit it;
                        edt.Accuracy = att.Accuracy;
                        if (att.AttackMethodID != (int)AttackMethod.None)
                        {
                            edt.AttackMethodID = att.AttackMethodID;
                        }
                        if (att.Category != (int)AttackCategory.None)
                        {
                            edt.Category = att.Category;
                        }
                        if (!String.IsNullOrWhiteSpace(edt.Description))
                        {
                            edt.Description = att.Description;
                        }
                        if (elemTypeDict.ContainsKey(att.ElementalTypeID))
                        {
                            edt.ElementalTypeID = att.ElementalTypeID;
                        }
                        edt.MaxPP = att.MaxPP;
                        edt.Power = att.Power;
                        edt.Priority = att.Priority;
                        if (att.Target != (int)Target.None)
                        {
                            edt.Target = att.Target;
                        }
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
