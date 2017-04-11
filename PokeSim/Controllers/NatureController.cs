using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PokeSim.Models;
using System.Text;


namespace PokeSim.Controllers
{
    [Authorize]
    public class NatureController : Controller
    {
        string objectName = "Nature";
        private NatureDbContext db = new NatureDbContext();
        


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
            ViewBag.CurrentItems = db.Natures;
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
                        {"IncreasedStat", false },
                        {"DecreasedStat", false },
                    };

            ViewBag.Message = "";
            //get the raw string;
            String rawData = collection["RawData"];
            if (!String.IsNullOrWhiteSpace(rawData))
            {
                try
                {
                    List<string> presentFields;
                    Dictionary<string, int> statLookupDict = EnumHelpers.enumNameToIntDict<Stat>();
                    
                    List<List<String>> cells = LoadDataFromFile_BreakUpString(rawData);
                    List<Dictionary<string, string>> fieldValDicts = LoadDataFromFile_FieldValDicts(cells, expectedFields, out presentFields);
                    List<Nature> loadedObjects = LoadDataFromFile_ParseToObjects(fieldValDicts, statLookupDict);
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
            ViewBag.StatStrings = StatsHandler.getNames();
            return View();
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(FormCollection collection)
        {
            string theMessage = null;
            try
            {
                Nature newNature = new Nature();
                newNature.Name = collection["Name"].Trim();
                newNature.IncreasedStat = StatsHandler.getInt(collection["IncreasedStat"]);
                newNature.DecreasedStat = StatsHandler.getInt(collection["DecreasedStat"]);
                //make sure the same name doesn't exist in the database yet;
                Nature dupeNature = db.Natures.Where(n => n.Name.ToLower() == newNature.Name.ToLower()).FirstOrDefault();
                if (dupeNature != null)
                {
                    //problem!
                    theMessage = "You cannot use a duplicate Nature name '" + newNature.Name + "', it is being used by nature with ID: " + dupeNature.Id;
                }
                else
                {
                    //we're good. 
                    db.Natures.Add(newNature);
                    db.SaveChanges();
                    theMessage = "Successfully created the '" + newNature.Name + "' nature.";
                }
            }
            catch(Exception ex)
            {
                theMessage = "Creation failed: \r\n" + ex.Message;
            }
            return RedirectToAction("Create", new { message = theMessage });
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, string message = null)
        {
            string theMessage = message;
            Nature currentItem = db.Natures.Find(id);
            if (currentItem != null)
            {
                ViewBag.StatStrings = StatsHandler.getNames();
                ViewBag.CurrentItem = db.Natures.Find(id);
                ViewBag.Message = theMessage;
                return View();
            }
            else
            {
                theMessage = "Could not find Pokemon Instance with ID " + id;
            }
            return RedirectToAction("Index", new { message = theMessage });
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, FormCollection collection)
        {
            string theMessage = "";
            Nature currentItem = db.Natures.Find(id);
            if (currentItem != null)
            {
                try
                {
                    //don't allow blank;
                    currentItem.Name = ValidationHelpers.validateAsString(collection, "Name", true, true, null);
                    currentItem.IncreasedStat = StatsHandler.getInt(collection["IncreasedStat"]);
                    currentItem.DecreasedStat = StatsHandler.getInt(collection["DecreasedStat"]);
                    if (db.nameIsAvailable(currentItem.Name, currentItem.Id))
                    {
                        db.SaveChanges();
                        theMessage = "Successfully edited " + objectName;
                    }
                    else
                    {
                        theMessage = "Natures cannot have duplicate names.";
                    }
                }
                catch (Exception ex)
                {
                    theMessage = "Edit failed: \r\n" + ex.Message;
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
            ViewBag.Message = "";
            try
            {
                db.Natures.Remove(db.Natures.Where(i => i.Id == id).First());
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ViewBag.Message = "Deletion failed: \r\n" + ex.Message;
                ViewBag.CurrentItem = db.Natures.Find(id);
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
            Nature currentItem = db.Natures.Find(id);
            if (currentItem != null)
            {
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
        private List<Nature> LoadDataFromFile_ParseToObjects(List<Dictionary<string, string>> rawData, Dictionary<string, int> statLookupDict)
        {
            List<Nature> retList = new List<Nature>();
            for (int i = 0; i < rawData.Count; i++)
            {
                Nature item = new Nature();
                foreach (KeyValuePair<string, string> kvp in rawData[i])
                {
                    switch (kvp.Key.Trim())
                    {
                        case "Name":
                            item.Name = ValidationHelpers.validateAsString(kvp.Value, kvp.Key, true, "");
                            break;
                        case "IncreasedStat":
                            item.IncreasedStat = ValidationHelpers.validateAsStringDict(statLookupDict, kvp.Value, kvp.Key, false, (int)Stat.None);
                            break;
                        case "DecreasedStat":
                            item.DecreasedStat = ValidationHelpers.validateAsStringDict(statLookupDict, kvp.Value, kvp.Key, false, (int)Stat.None);
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
        private void LoadDataFromFile_SaveToDatabase(List<Nature> rawData, List<string> presentFields)
        {
            try
            {
                foreach (Nature item in rawData)
                {
                    //check to see if the item already exists;
                    Nature currentItem = db.Natures.Where(i => i.Name == item.Name).FirstOrDefault();
                    if (currentItem != null)
                    {
                        //edit according to what is there.
                        if (presentFields.Contains("Name")) { currentItem.Name = item.Name; }
                        if (presentFields.Contains("IncreasedStat")) { currentItem.IncreasedStat = item.IncreasedStat; }
                        if (presentFields.Contains("DecreasedStat")) { currentItem.DecreasedStat = item.DecreasedStat; }
                    }
                    else
                    {
                        //new item; just add it.
                        db.Natures.Add(item);
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
