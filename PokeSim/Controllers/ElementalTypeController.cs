using PokeSim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using Microsoft.AspNet.Identity;


namespace PokeSim.Controllers
{
    [Authorize]
    public class ElementalTypeController : Controller
    {
        string objectName = "Elemental Type";
        private Models.ElementalTypeDbContext db = new ElementalTypeDbContext();
        


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
            ViewBag.CurrentItems = db.ElementalTypes;
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
            string rawData = collection["RawData"];
            if (!string.IsNullOrWhiteSpace(rawData))
            {
                try
                {
                    //split it into lines, trim the lines, make sure they're distinct.
                    string[] lines = rawData.Split('\r');
                    Dictionary<string, string> sanitizerDictionary = new Dictionary<string, string>();
                    foreach(string line in lines) 
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            string key = line.Trim().ToLower();
                            string val = line.Trim();
                            if (!sanitizerDictionary.ContainsKey(key))
                            {
                                sanitizerDictionary.Add(key, val);
                            }
                        }
                    }
                    
                    foreach (KeyValuePair<string, string> kvp in sanitizerDictionary)
                    {
                        if (db.ElementalTypes.Where(i => i.Name.ToLower() == kvp.Key).FirstOrDefault() == null)
                        {
                            ElementalType newType = new ElementalType();
                            newType.Name = kvp.Value;
                            db.ElementalTypes.Add(newType);
                        }
                    }
                    db.SaveChanges();
                    ViewBag.Message = "Data successfully saved to database.";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Could not parse, encountered exception: \r\n" + ex.Message;
                }
            }
            else
            {
                ViewBag.Message = "Could not parse, no data in file.";
            }
            return View();
        }



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



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(string message = null)
        {
            ViewBag.message = message;
            return View();
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(Models.ElementalType elementalType)
        {
            try
            {
                // TODO: Add insert logic here
                if (ModelState.IsValid && elementalType != null)
                {
                    string newName = elementalType.Name;
                    if (!String.IsNullOrWhiteSpace(newName))
                    {
                        newName = newName.Trim();
                        //make sure the item doesn't exist yet.
                        Models.ElementalType existingItem = db.ElementalTypes.Where(i => i.Name.ToLower() == elementalType.Name.ToLower()).FirstOrDefault();
                        if (existingItem == null)
                        {
                            //doesn't exist yet, add it.
                            db.ElementalTypes.Add(elementalType);
                            db.SaveChanges();
                            ViewBag.message = "New Type '" + elementalType.Name + "' added successfully.";
                        }
                        else
                        {
                            //already exists.
                            ViewBag.message = "Type '" + existingItem.Name + "' already exists; ID: " + existingItem.Id;
                        }
                    }
                    else
                    {
                        ViewBag.message = "Failed to add new type, object null." ;
                    }
                }
                else
                {
                    ViewBag.message = "Failed to add new type, Model State invalid." ;
                }
            }
            catch
            {
                ViewBag.message = "Failed to add new type, Model State invalid.";
            }
            return View();
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, string message = null)
        {
            ElementalType currentItem = db.ElementalTypes.Find(id);
            string theMessage = message;
            if (currentItem != null)
            {
                ViewBag.Message = theMessage;
                ViewBag.CurrentItem = currentItem;
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
            ElementalType currentItem = db.ElementalTypes.Find(id);
            string theMessage = "";
            if (currentItem != null)
            {
                try
                {
                    currentItem.Name = ValidationHelpers.validateAsString(collection, "Name", true, true, null);
                    if (db.nameIsAvailable(currentItem.Name, currentItem.Id))
                    {
                        db.SaveChanges();
                        theMessage = objectName + " '" + currentItem.Name + "' edited successfully.";
                    }
                    else
                    {
                        theMessage = "The name " + currentItem.Name + " is already taken.";
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
            string theMessage = "";
            ElementalType currentItem = db.ElementalTypes.Find(id);
            if (currentItem != null)
            {
                try
                {
                    db.ElementalTypes.Remove(currentItem);
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
            ElementalType currentItem = db.ElementalTypes.Find(id);
            if (currentItem != null)
            {
                ViewBag.Message = theMessage;
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

    }
}
