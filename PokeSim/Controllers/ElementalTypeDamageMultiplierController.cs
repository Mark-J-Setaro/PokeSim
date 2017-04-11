using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PokeSim.Models;
using Microsoft.AspNet.Identity;
using System.Text;

namespace PokeSim.Controllers
{
    [Authorize]
    public class ElementalTypeDamageMultiplierController : Controller
    {
        string objectName = "Elemental Comparison";
        private Models.ElementalTypeDamageMultiplierDbContext db = new ElementalTypeDamageMultiplierDbContext();
        private Models.ElementalTypeDbContext db_types = new ElementalTypeDbContext();

        

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
            Dictionary<int, string> elemTypeDict = db_types.GetDict();
            ViewBag.elemTypeDict = elemTypeDict;
            ViewBag.CurrentItems = db.ElementalTypeDamageMultipliers;
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
                    Dictionary<string, int> sti_ElemDict = db_types.GetLookupDict();
                    Dictionary<int, string> its_ElemDict = db_types.GetDict();

                    List<List<String>> cells = LoadDataFromFile_BreakUpString(rawData);
                    List<List<double>> vals = LoadDataFromFile_ParseStringsToDoubles(cells, sti_ElemDict);
                    Dictionary<int, Dictionary<int, double>> elemEffDict = LoadDataFromFile_GetValueDict(vals, its_ElemDict);
                    LoadDataFromFile_SaveData(elemEffDict);

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



        public ActionResult ViewTable(string message = null)
        {
            ViewBag.Message = message;
            Dictionary<int, string> elemTypeDict = db_types.GetDict();
            db.initializeByElements(elemTypeDict);
            Dictionary<int, Dictionary<int, KeyValuePair<int, double>>> elemTypeDamgMultDict = db.GetDict(elemTypeDict);
            ViewBag.elemTypeDict = elemTypeDict;
            ViewBag.elemTypeDamgMultDict = elemTypeDamgMultDict;
            return View();
        }

        

        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult EditTable(string message = null)
        {
            ViewBag.Message = message;
            Dictionary<int, string> elemTypeDict = db_types.GetDict();
            db.initializeByElements(elemTypeDict);
            Dictionary<int, Dictionary<int, KeyValuePair<int, double>>> elemTypeDamgMultDict = db.GetDict(elemTypeDict);
            ViewBag.elemTypeDict = elemTypeDict;
            ViewBag.elemTypeDamgMultDict = elemTypeDamgMultDict;
            return View();
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult EditTable(FormCollection collection)
        {
            foreach (string key in collection.AllKeys)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(key, "[Ss][Ee][Ll][_][0-9]+$"))
                {
                    int id = Convert.ToInt32(key.Split('_').Last());
                    double mult = Convert.ToDouble(collection[key]);
                    db.ElementalTypeDamageMultipliers.Find(id).Multiplier = mult;
                }
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(string message = null)
        {
            ViewBag.Message = message;
            Dictionary<int, string> elemTypeDict = db_types.GetDict();
            ViewBag.elemTypeDict = elemTypeDict;
            return View();
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Create(FormCollection collection)
        {
            Dictionary<int, string> elemTypeDict = db_types.GetDict();
            ViewBag.elemTypeDict = elemTypeDict;
            try
            {
                // TODO: Add insert logic here
                Models.ElementalTypeDamageMultiplier newItem = new ElementalTypeDamageMultiplier();
                newItem.ElementalTypeId_Attack = Convert.ToInt32(collection["ElementalTypeId_Attack"]);
                newItem.ElementalTypeId_Defend = Convert.ToInt32(collection["ElementalTypeId_Defend"]);
                newItem.Multiplier = Convert.ToDouble(collection["Multiplier"]);
                //make sure it doesn't already exist.
                Models.ElementalTypeDamageMultiplier oldItem = db.ElementalTypeDamageMultipliers.Where(i => i.ElementalTypeId_Attack == newItem.ElementalTypeId_Attack && i.ElementalTypeId_Defend == newItem.ElementalTypeId_Defend).FirstOrDefault();
                if (oldItem == null)
                {
                    //no type comparison like that exists; create it.
                    db.ElementalTypeDamageMultipliers.Add(newItem);
                    db.SaveChanges();
                    ViewBag.Message = "New Type Comparison '" + elemTypeDict[newItem.ElementalTypeId_Attack] + "/" + elemTypeDict[newItem.ElementalTypeId_Defend] + "/" + newItem.Multiplier + "' created."; 
                }
                else
                {
                    //type comparison already exists.
                    ViewBag.Message = "Type comparison between '" + newItem.ElementalTypeId_Attack + "' and '" + newItem.ElementalTypeId_Defend + "' already exists; ID: " + oldItem.Id;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "There was an error creating the item;\r\n " + ex.Message;
            }
            return View();
        }



        [Authorize(Roles = EnumHelpers.ROLE_ADMIN)]
        public ActionResult Edit(int id, string message = "")
        {
            string theMessage = message;
            ElementalTypeDamageMultiplier currentItem = db.ElementalTypeDamageMultipliers.Find(id);
            if (currentItem != null)
            {
                ElementalType attType = db_types.ElementalTypes.Find(currentItem.ElementalTypeId_Attack);
                ElementalType defType = db_types.ElementalTypes.Find(currentItem.ElementalTypeId_Defend);
                string attTypeStr = null;
                string defTypeStr = null;
                if (attType != null) { attTypeStr = attType.Name; }
                if (defType != null) { defTypeStr = defType.Name; }
                ViewBag.AttTypeStr = attTypeStr;
                ViewBag.DefTypeStr = defTypeStr;
                ViewBag.CurrentItem = currentItem;
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
            ElementalTypeDamageMultiplier currentItem = db.ElementalTypeDamageMultipliers.Find(id);
            if (currentItem != null)
            {
                try
                {
                    currentItem.Multiplier = Convert.ToDouble(collection["Multiplier"]);
                    db.SaveChanges();
                    theMessage = "Successfully edited " + objectName + ".";
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
            ElementalTypeDamageMultiplier currentItem = db.ElementalTypeDamageMultipliers.Find(id);
            if (currentItem != null)
            {
                try
                {
                    db.ElementalTypeDamageMultipliers.Remove(currentItem);
                    db.SaveChanges();
                    theMessage = "Successfully deleted association.";
                    return RedirectToAction("Overview", new { message = theMessage });
                }
                catch (Exception ex)
                {
                    theMessage = "Deletion failed: \r\n" + ex.Message;
                }
            }
            else
            {
                theMessage = "Cannot find item with ID " + id;
            }
            ViewBag.Message = theMessage;
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
            ElementalTypeDamageMultiplier currentItem = db.ElementalTypeDamageMultipliers.Find(id);
            if (currentItem != null)
            {
                ElementalType attType = db_types.ElementalTypes.Find(currentItem.ElementalTypeId_Attack);
                ElementalType defType = db_types.ElementalTypes.Find(currentItem.ElementalTypeId_Defend);
                string attTypeStr = null;
                string defTypeStr = null;
                if (attType != null) { attTypeStr = attType.Name; }
                if (defType != null) { defTypeStr = defType.Name; }
                ViewBag.AttTypeStr = attTypeStr;
                ViewBag.DefTypeStr = defTypeStr;
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
                theMessage = "Could not find " + objectName + " with ID " + id;
            }
            return RedirectToAction("Index", new { message = theMessage });
        }

        #endregion



        #region Load Data From File Helpers

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



        private List<List<double>> LoadDataFromFile_ParseStringsToDoubles(List<List<String>> stringCells, Dictionary<string, int> elemLookupDict)
        {
            List<List<double>> retVals = new List<List<double>>();
            for (int row = 0; row < stringCells.Count(); row++)
            {
                List<double> rowVals = new List<double>();
                for (int col = 0; col < stringCells[row].Count(); col++)
                {
                    if (row == 0 && col == 0)
                    {
                        //origin, doesn't matter.
                        rowVals.Add(-1);
                    }
                    else if (row == 0 || col == 0)
                    {
                        //one of the elemental types;
                        try
                        {
                            double val = elemLookupDict[stringCells[row][col]];
                            rowVals.Add(val);
                        }
                        catch
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (List<String> line in stringCells)
                            {
                                sb.Append("<p>");
                                sb.Append(String.Join(",", line));
                                sb.Append("</p>");
                            }
                            throw new Exception("Could not recognize Elemental Type '" + stringCells[row][col] + "' on " + col + ":" + row + "; Contents: " + sb.ToString());
                        }
                    }
                    else
                    {
                        //just a standard value;
                        try
                        {
                            double val = Convert.ToDouble(stringCells[row][col]);
                            rowVals.Add(val);
                        }
                        catch
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (List<String> line in stringCells)
                            {
                                sb.Append("<p>");
                                sb.Append(String.Join(",", line));
                                sb.Append("</p>");
                            }
                            throw new Exception("Could not recognize double value at '" + stringCells[row][col] + "' on " + col + ":" + row + "; Contents: " + sb.ToString());
                        }
                    }
                }
                retVals.Add(rowVals);
            }
            return retVals;
        }



        private Dictionary<int, Dictionary<int, double>> LoadDataFromFile_GetValueDict(List<List<double>> vals, Dictionary<int, string> elemLookupDict)
        {
            Dictionary<int, Dictionary<int, double>> colDict = new Dictionary<int, Dictionary<int, double>>();
            for (int row = 1; row < vals.Count(); row++)
            {
                Dictionary<int, double> rowVals = new Dictionary<int, double>();
                for (int col = 1; col < vals[row].Count(); col++)
                {
                    double value = vals[row][col];
                    if (!rowVals.ContainsKey((int)vals[0][col]))
                    {
                        rowVals.Add((int)vals[0][col], value);
                    }
                    else
                    {
                        throw new Exception("Duplicate Defend Type found, (" + col + ")" + elemLookupDict[(int)col] + ", value: " + vals[0][col]);
                    }
                }
                if (!colDict.ContainsKey((int)vals[row][0]))
                {
                    colDict.Add((int)vals[row][0], rowVals);
                }
                else
                {
                    throw new Exception("Duplicate Attac Type found, (" + row + ")" + elemLookupDict[(int)row] + ", value: " + vals[row][0]);
                }
            }
            return colDict;
        }



        private void LoadDataFromFile_SaveData(Dictionary<int, Dictionary<int, double>> data)
        {
            foreach (KeyValuePair<int, Dictionary<int, double>> attKvp in data)
            {
                foreach (KeyValuePair<int, double> defKvp in attKvp.Value)
                {
                    ElementalTypeDamageMultiplier entry = db.ElementalTypeDamageMultipliers.Where(e => e.ElementalTypeId_Attack == attKvp.Key && e.ElementalTypeId_Defend == defKvp.Key).FirstOrDefault();
                    if (entry != null)
                    {
                        entry.Multiplier = defKvp.Value;
                    }
                    else
                    {
                        entry = new ElementalTypeDamageMultiplier();
                        entry.ElementalTypeId_Attack = attKvp.Key;
                        entry.ElementalTypeId_Defend = defKvp.Key;
                        entry.Multiplier = defKvp.Value;
                        db.ElementalTypeDamageMultipliers.Add(entry);
                    }
                }
            }
            db.SaveChanges();
        }

        #endregion

    }
}
