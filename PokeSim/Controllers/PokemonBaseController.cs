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
    public class PokemonBaseController : Controller
    {
        private static string objectName = "Pokemon Species";
        private PokemonBaseDbContext db = new PokemonBaseDbContext();
        private ElementalTypeDbContext db_elementalTypes = new ElementalTypeDbContext();
        private AbilityDbContext db_abilities = new AbilityDbContext();



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
            ViewBag.Elements = db_elementalTypes.GetDict();
            ViewBag.Abilities = db_abilities.GetDict();
            ViewBag.CurrentItems = db.PokemonBases;
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
                        {"Name", true },
                        {"ElemTypeID_1", false },
                        {"ElemTypeID_2", false },
                        {"GenderType", false },
                        {"AbilityID_First", false },
                        {"AbilityID_Second", false },
                        {"AbilityID_Hidden", false },
                        {"EggGroup1", false },
                        {"EggGroup2", false },
                        {"BaseHP", false },
                        {"BaseAtt", false },
                        {"BaseDef", false },
                        {"BaseSpAtt", false },
                        {"BaseSpDef", false },
                        {"BaseSpeed", false },
                    };

            ViewBag.Message = "";
            //get the raw string;
            String rawData = collection["RawData"];
            if (!String.IsNullOrWhiteSpace(rawData))
            {
                try
                {
                    Dictionary<string, int> elemTypeLookupDict = db_elementalTypes.GetLookupDict();
                    Dictionary<string, int> abilityLookupDict = db_abilities.GetLookupDict();
                    List<string> presentFields;

                    List<List<String>> cells = LoadDataFromFile_BreakUpString(rawData);
                    List<Dictionary<string, string>> fieldValDicts = LoadDataFromFile_FieldValDicts(cells, expectedFields, out presentFields);
                    List<PokemonBase> loadedObjects = LoadDataFromFile_ParseToObjects(fieldValDicts, elemTypeLookupDict, abilityLookupDict);
                    LoadDataFromFile_SaveToDatabase(loadedObjects, presentFields);
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
            ViewBag.Elements = db_elementalTypes.GetDict();
            ViewBag.Abilities = db_abilities.GetDict();
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
                PokemonBase item = new PokemonBase();
                Dictionary<int, string> elemTypeDict = db_elementalTypes.GetDict();
                Dictionary<int, string> abilityDict = db_abilities.GetDict();
                Dictionary<string, int> categoryLookupDict = EnumHelpers.getAllEffectNamesDict();
                Dictionary<string, Dictionary<string, int>> effectLookupDict = EnumHelpers.getEffectIdFromName();
                Dictionary<string, int> genderTypeLookupDict = EnumHelpers.enumNameToIntDict<GenderType>();
                
                item.Name = ValidationHelpers.validateAsString(collection, "Name", true, true, "");
                item.ElemTypeID_1 = ValidationHelpers.validateAsIntDict(elemTypeDict, collection, "ElemTypeID_1", true, true, -1);
                item.ElemTypeID_2 = ValidationHelpers.validateAsIntDict(elemTypeDict, collection, "ElemTypeID_2", true, false, -1);
                item.GenderType = ValidationHelpers.validateAsStringEnum<GenderType>(collection, "GenderType", true, true, (int)GenderType.Dimorphic);
                item.AbilityID_First = ValidationHelpers.validateAsIntDict(abilityDict, collection, "AbilityID_First", true, true, -1);
                item.AbilityID_Second = ValidationHelpers.validateAsIntDict(abilityDict, collection, "AbilityID_Second", true, false, -1);
                item.AbilityID_Hidden = ValidationHelpers.validateAsIntDict(abilityDict, collection, "AbilityID_Hidden", true, false, -1);
                item.EggGroup1 = ValidationHelpers.validateAsStringEnum<EggGroup>(collection, "EggGroup1", true, true, -1);
                item.EggGroup2 = ValidationHelpers.validateAsStringEnum<EggGroup>(collection, "EggGroup2", true, false, -1);
                item.BaseHP = ValidationHelpers.validateAsInt(collection, "BaseHP", true, true, 1);
                item.BaseAtt = ValidationHelpers.validateAsInt(collection, "BaseAtt", true, true, 1);
                item.BaseDef = ValidationHelpers.validateAsInt(collection, "BaseDef", true, true, 1);
                item.BaseSpAtt = ValidationHelpers.validateAsInt(collection, "BaseSpAtt", true, true, 1);
                item.BaseSpDef = ValidationHelpers.validateAsInt(collection, "BaseSpDef", true, true, 1);
                item.BaseSpeed = ValidationHelpers.validateAsInt(collection, "BaseSpeed", true, true, 1);

                //make sure it's not a duplicate name;
                if (!db.nameIsAvailable(item.Name))
                {
                    throw new Exception("The name '" + item.Name + "' is already taken.");
                }
                //we're good. 
                db.PokemonBases.Add(item);
                db.SaveChanges();
                ViewBag.Message = "Successfully created new " + objectName + " '" + item.Name + "'.";
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
            ViewBag.Elements = db_elementalTypes.GetDict();
            ViewBag.Abilities = db_abilities.GetDict();
            return View();
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, string message = null)
        {
            ViewBag.Message = message;
            PokemonBase currentItem = db.PokemonBases.Find(id);
            ViewBag.CurrentItem = currentItem;

            ViewBag.Elements = db_elementalTypes.GetDict();
            ViewBag.Abilities = db_abilities.GetDict();

            return View();
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, FormCollection collection)
        {
            ViewBag.Message = "";
            PokemonBase item = db.PokemonBases.Find(id);
            ViewBag.CurrentItem = item;
            
            Dictionary<int, string> elemTypeDict = db_elementalTypes.GetDict();
            Dictionary<int, string> abilityDict = db_abilities.GetDict();
            try
            {
                item.Name = ValidationHelpers.validateAsString(collection, "Name", true, true, "");
                item.ElemTypeID_1 = ValidationHelpers.validateAsIntDict(elemTypeDict, collection, "ElemTypeID_1", true, true, -1);
                item.ElemTypeID_2 = ValidationHelpers.validateAsIntDict(elemTypeDict, collection, "ElemTypeID_2", true, false, -1);
                item.GenderType = ValidationHelpers.validateAsStringEnum<GenderType>(collection, "GenderType", true, true, -1);
                item.AbilityID_First = ValidationHelpers.validateAsIntDict(abilityDict, collection, "AbilityID_First", true, true, -1);
                item.AbilityID_Second = ValidationHelpers.validateAsIntDict(abilityDict, collection, "AbilityID_Second", true, false, -1);
                item.AbilityID_Hidden = ValidationHelpers.validateAsIntDict(abilityDict, collection, "AbilityID_Hidden", true, false, -1);
                item.EggGroup1 = ValidationHelpers.validateAsStringEnum<EggGroup>(collection, "EggGroup1", true, true, -1);
                item.EggGroup2 = ValidationHelpers.validateAsStringEnum<EggGroup>(collection, "EggGroup2", true, false, -1);
                item.BaseHP = ValidationHelpers.validateAsInt(collection, "BaseHP", true, true, 1);
                item.BaseAtt = ValidationHelpers.validateAsInt(collection, "BaseAtt", true, true, 1);
                item.BaseDef = ValidationHelpers.validateAsInt(collection, "BaseDef", true, true, 1);
                item.BaseSpAtt = ValidationHelpers.validateAsInt(collection, "BaseSpAtt", true, true, 1);
                item.BaseSpDef = ValidationHelpers.validateAsInt(collection, "BaseSpDef", true, true, 1);
                item.BaseSpeed = ValidationHelpers.validateAsInt(collection, "BaseSpeed", true, true, 1);

                //make sure it's not a duplicate name;
                if (!db.nameIsAvailable(item.Name, id))
                {
                    throw new Exception("The name '" + item.Name + "' is already taken.");
                }

                //we're good. 
                db.SaveChanges();
                ViewBag.Message = "Successfully edited " + objectName + ".";
                return RedirectToAction("Overview");
            }
            catch (Exception ex)
            {
                ViewBag.Message = objectName + "Edit failed: \r\n" + ex.Message;
            }
            ViewBag.Elements = elemTypeDict;
            ViewBag.Abilities = abilityDict;
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
            ViewBag.Message = "";
            try
            {
                db.PokemonBases.Remove(db.PokemonBases.Where(i => i.Id == id).First());
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Deletion failed: \r\n" + ex.Message;

                PokemonBase currentItem = db.PokemonBases.Find(id);
                ViewBag.CurrentItem = currentItem;

                ViewBag.AbilityFirst = db_abilities.Abilities.Where(i => i.Id == currentItem.AbilityID_First).FirstOrDefault();
                ViewBag.AbilitySecond = db_abilities.Abilities.Where(i => i.Id == currentItem.AbilityID_Second).FirstOrDefault();
                ViewBag.AbilityHidden = db_abilities.Abilities.Where(i => i.Id == currentItem.AbilityID_Hidden).FirstOrDefault();

                ViewBag.Element1 = db_elementalTypes.ElementalTypes.Where(i => i.Id == currentItem.ElemTypeID_1).FirstOrDefault();
                ViewBag.Element2 = db_elementalTypes.ElementalTypes.Where(i => i.Id == currentItem.ElemTypeID_2).FirstOrDefault();

                return View();
            }
        }



        public ActionResult Details(int id, string message = null)
        {
            return helpDetailsOrDelete(id, false, message);
        }



        #region Helper Methods

        private ActionResult helpDetailsOrDelete(int id, bool isDeleting, string message = null)
        {
            string theMessage = message;
            PokemonBase currentItem = db.PokemonBases.Find(id);
            if (currentItem != null)
            {
                ViewBag.CurrentItem = currentItem;

                ViewBag.AbilityFirst = db_abilities.Abilities.Where(i => i.Id == currentItem.AbilityID_First).FirstOrDefault();
                ViewBag.AbilitySecond = db_abilities.Abilities.Where(i => i.Id == currentItem.AbilityID_Second).FirstOrDefault();
                ViewBag.AbilityHidden = db_abilities.Abilities.Where(i => i.Id == currentItem.AbilityID_Hidden).FirstOrDefault();

                ViewBag.Element1 = db_elementalTypes.ElementalTypes.Where(i => i.Id == currentItem.ElemTypeID_1).FirstOrDefault();
                ViewBag.Element2 = db_elementalTypes.ElementalTypes.Where(i => i.Id == currentItem.ElemTypeID_2).FirstOrDefault();
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
        /// Returns a list of present fields later so that edited items don't have empty fields pushed in.
        /// </summary>
        private List<Dictionary<string, string>> LoadDataFromFile_FieldValDicts(List<List<String>> rawData, Dictionary<string, bool> expectedFields, out List<string> presentFields)
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
        private List<PokemonBase> LoadDataFromFile_ParseToObjects(List<Dictionary<string, string>> rawData, Dictionary<string, int> elemTypeLookupDict, Dictionary<string, int> abilityLookupDict)
        {
            List<PokemonBase> retList = new List<PokemonBase>();
            for (int i = 0; i < rawData.Count; i++)
            {
                PokemonBase item = new PokemonBase();
                foreach (KeyValuePair<string, string> kvp in rawData[i])
                {
                    switch (kvp.Key.Trim())
                    {
                        case "Name":
                            item.Name = ValidationHelpers.validateAsString(kvp.Value, kvp.Key, true, "");
                            break;
                        case "ElemTypeID_1":
                            item.ElemTypeID_1 = ValidationHelpers.validateAsStringDict(elemTypeLookupDict, kvp.Value, kvp.Key, true, -1);
                            break;
                        case "ElemTypeID_2":
                            item.ElemTypeID_2 = ValidationHelpers.validateAsStringDict(elemTypeLookupDict, kvp.Value, kvp.Key, false, -1);
                            break;
                        case "GenderType":
                            item.GenderType = ValidationHelpers.validateAsStringEnum<GenderType>(kvp.Value, kvp.Key, true, (int)GenderType.Dimorphic);
                            break;
                        case "AbilityID_First":
                            item.AbilityID_First = ValidationHelpers.validateAsStringDict(abilityLookupDict, kvp.Value, kvp.Key, true, -1);
                            break;
                        case "AbilityID_Second":
                            item.AbilityID_Second = ValidationHelpers.validateAsStringDict(abilityLookupDict, kvp.Value, kvp.Key, false, -1);
                            break;
                        case "AbilityID_Hidden":
                            item.AbilityID_Hidden = ValidationHelpers.validateAsStringDict(abilityLookupDict, kvp.Value, kvp.Key, false, -1);
                            break;
                        case "EggGroup1":
                            item.EggGroup1 = ValidationHelpers.validateAsStringEnum<EggGroup>(kvp.Value, kvp.Key, true, -1);
                            break;
                        case "EggGroup2":
                            item.EggGroup2 = ValidationHelpers.validateAsStringEnum<EggGroup>(kvp.Value, kvp.Key, false, -1);
                            break;
                        case "BaseHP":
                            item.BaseHP = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "BaseAtt":
                            item.BaseAtt = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "BaseDef":
                            item.BaseDef = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "BaseSpAtt":
                            item.BaseSpAtt = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "BaseSpDef":
                            item.BaseSpDef = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "BaseSpeed":
                            item.BaseSpeed = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
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
        private void LoadDataFromFile_SaveToDatabase(List<PokemonBase> rawData, List<string> presentFields)
        {
            try
            {
                foreach (PokemonBase item in rawData)
                {
                    //check to see if the item already exists;
                    PokemonBase currentItem = db.PokemonBases.Where(i => i.Name == item.Name).FirstOrDefault();
                    if (currentItem != null)
                    {
                        //edit according to what is there.
                        if (presentFields.Contains("ElemTypeID_1")) { currentItem.ElemTypeID_1 = item.ElemTypeID_1; }
                        if (presentFields.Contains("ElemTypeID_2")) { currentItem.ElemTypeID_2 = item.ElemTypeID_2; }
                        if (presentFields.Contains("GenderType")) { currentItem.GenderType = item.GenderType; }
                        if (presentFields.Contains("AbilityID_First")) { currentItem.AbilityID_First = item.AbilityID_First; }
                        if (presentFields.Contains("AbilityID_Second")) { currentItem.AbilityID_Second = item.AbilityID_Second; }
                        if (presentFields.Contains("AbilityID_Hidden")) { currentItem.AbilityID_Hidden = item.AbilityID_Hidden; }
                        if (presentFields.Contains("EggGroup1")) { currentItem.EggGroup1 = item.EggGroup1; }
                        if (presentFields.Contains("EggGroup2")) { currentItem.EggGroup2 = item.EggGroup2; }
                        if (presentFields.Contains("BaseHP")) { currentItem.BaseHP = item.BaseHP; }
                        if (presentFields.Contains("BaseAtt")) { currentItem.BaseAtt = item.BaseAtt; }
                        if (presentFields.Contains("BaseDef")) { currentItem.BaseDef = item.BaseDef; }
                        if (presentFields.Contains("BaseSpAtt")) { currentItem.BaseSpAtt = item.BaseSpAtt; }
                        if (presentFields.Contains("BaseSpDef")) { currentItem.BaseSpDef = item.BaseSpDef; }
                        if (presentFields.Contains("BaseSpeed")) { currentItem.BaseSpeed = item.BaseSpeed; }
                    }
                    else
                    {
                        //new item; just add it.
                        db.PokemonBases.Add(item);
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
