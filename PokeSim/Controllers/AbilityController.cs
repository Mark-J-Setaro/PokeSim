using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PokeSim.Models;

namespace PokeSim.Controllers
{

    [Authorize]
    public class AbilityController : Controller
    {
        string objectName = "Ability";
        private AbilityDbContext db = new AbilityDbContext();
        


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
            ViewBag.CurrentItems = db.Abilities;
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
                    Dictionary<string, string> dataDict = LoadDataFromFile_BreakUpString(rawData);
                    LoadDataFromFile_SaveData(dataDict);
                    ViewBag.Message = "Successful";
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
            ViewBag.message = message;
            return View();
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(Ability newItem)
        {
            try
            {
                if (ModelState.IsValid && newItem != null)
                {
                    string newName = newItem.Name;
                    string newDescription = newItem.Description;
                    if (!String.IsNullOrWhiteSpace(newName) && !String.IsNullOrWhiteSpace(newDescription))
                    {
                        newName = newName.Trim();
                        //make sure the item doesn't exist yet.
                        Models.Ability existingItem = db.Abilities.Where(i => i.Name.ToLower() == newItem.Name.ToLower()).FirstOrDefault();
                        if (existingItem == null)
                        {
                            //doesn't exist yet, add it.
                            db.Abilities.Add(newItem);
                            db.SaveChanges();
                            ViewBag.message = "New Ability '" + newItem.Name + "' added successfully.";
                        }
                        else
                        {
                            //already exists.
                            ViewBag.message = "Ability '" + existingItem.Name + "' already exists; ID: " + existingItem.Id;
                        }
                    }
                    else
                    {
                        ViewBag.message = "Failed to add new Ability, name or description is null or whitespace.";
                    }
                }
                else
                {
                    ViewBag.message = "Failed to add new Ability, Model State invalid.";
                }
            }
            catch
            {
                ViewBag.message = "Failed to add new Ability, Model State invalid.";
            }
            return View();
        }
        


        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, string message)
        {
            Ability currentItem = db.Abilities.Find(id);
            string theMessage = message;
            if (currentItem != null)
            {
                ViewBag.Message = "";
                ViewBag.CurrentItem = db.Abilities.Find(id);
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
            ViewBag.Message = "";
            Ability currentItem = db.Abilities.Find(id);
            string theMessage = "";
            if (currentItem != null)
            {
                try
                {
                    ViewBag.CurrentItem = currentItem;
                    //don't allow whitespace;
                    Ability item = new Ability();
                    item.Name = ValidationHelpers.validateAsString(collection, "Name", true, true, null);
                    item.Description = ValidationHelpers.validateAsString(collection, "Description", true, true, null);
                    
                    Ability dupe = db.Abilities.Where(d => d.Name.ToLower() == item.Name.ToLower() && d.Id != id).FirstOrDefault();
                    //is there a duplicate?
                    if (dupe == null)
                    {
                        //nope it's an original name, go ahead and save and return to Overview.
                        currentItem.Name = item.Name;
                        currentItem.Description = item.Description;
                        db.SaveChanges();
                        ViewBag.currentTypes = db.Abilities;
                        return RedirectToAction("Overview");
                    }
                    else
                    {
                        throw new Exception("Duplicate Ability name found: " + dupe.Name + ", ID: " + dupe.Id);
                    }
                }
                catch (Exception ex)
                {
                    theMessage = "Edit failed: \r\n" + ex.Message;
                }
            }
            else
            {
                theMessage = "Cannot find item with ID " + id;
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
            Ability currentItem = db.Abilities.Find(id);
            if (currentItem != null)
            {
                try
                {
                    db.Abilities.Remove(currentItem);
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
            Ability currentItem = db.Abilities.Find(id);
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



        #region Load Data From File Helpers

        private Dictionary<string, string> LoadDataFromFile_BreakUpString(string rawData)
        {
            Dictionary<string, string> retDict = new Dictionary<string, string>();
            string[] lines = rawData.Split('\r');
            for (int i = 0; i < lines.Length; i++)
            {
                //make sure it's not an empty line.
                if (!String.IsNullOrWhiteSpace(lines[i]))
                {
                    string[] splitLine = lines[i].Split('\t');
                    //sanitize
                    for (int check = 0; check < splitLine.Length; check++)
                    {
                        splitLine[check] = splitLine[check].Trim();
                    }
                    //add to the dictionary.
                    if (splitLine.Length == 2)
                    {
                        if (!retDict.ContainsKey(splitLine[0]))
                        {
                            retDict.Add(splitLine[0], splitLine[1]);
                        }
                        else
                        {
                            throw new Exception("Too entries in Ability definition line " + i + ", expected Ability~Description '" + String.Join("~", splitLine) + "'.");
                        }
                    }
                    else
                    {
                        throw new Exception("Duplicate Ability found in line " + i + ", name '" + splitLine[0] + "'.");
                    }
                }
            }
            return retDict;
        }



        private void LoadDataFromFile_SaveData(Dictionary<string, string> data)
        {
            foreach (KeyValuePair<string, string> item in data)
            {
                //Update or Alter.
                Models.Ability entry = db.Abilities.Where(e => e.Name == item.Key).FirstOrDefault();
                if (entry != null)
                {
                    entry.Name = item.Value;
                }
                else
                {
                    entry = new Models.Ability();
                    entry.Name = item.Key;
                    entry.Description = item.Value;
                    db.Abilities.Add(entry);
                }
            }
            db.SaveChanges();
        }

        #endregion

    }
}
