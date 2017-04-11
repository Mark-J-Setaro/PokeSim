using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PokeSim.Models;
using System.Text;
using System.Data.Entity.Validation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;



namespace PokeSim.Controllers
{
    [Authorize]
    public class PokemonInstanceController : Controller
    {
        private static string objectName = "Pokemon Instance";
        private PokemonInstanceDbContext db = new PokemonInstanceDbContext();
        private PokemonBaseDbContext db_pokemonBase = new PokemonBaseDbContext();
        private FormChangeDbContext db_formChanges = new FormChangeDbContext();
        private AbilityDbContext db_abilities = new AbilityDbContext();
        private NatureDbContext db_natures = new NatureDbContext();
        private AttackDbContext db_attacks = new AttackDbContext();
        private LearnableAttackDbContext db_learnables = new LearnableAttackDbContext();
        private ApplicationDbContext db_users = new ApplicationDbContext();
        private ElementalTypeDbContext db_elemTypes = new ElementalTypeDbContext();
        private ElementalTypeDamageMultiplierDbContext db_elemComps = new ElementalTypeDamageMultiplierDbContext();
            
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

            if (isAdmin)
            { 
                Dictionary<string, List<PokemonInstance>> currentItems = db.GetUserPokemonDict(true);
                ViewBag.CurrentItems = currentItems;
                Dictionary<string, ApplicationUser> userDict = db_users.Users.Distinct().ToDictionary(i => i.Id, i => i);
                ViewBag.UserDict = userDict;
            }
            else
            {
                string userId = User.Identity.GetUserId();
                Dictionary<string, List<PokemonInstance>> currentItems = db.GetUserPokemonDict(false, userId);
                ViewBag.CurrentItems = currentItems;
                ApplicationUser user = db_users.Users.Where(i => i.Id == userId).FirstOrDefault();
                Dictionary<string, ApplicationUser> userDict = new Dictionary<string, ApplicationUser>() { { userId, user } };
                ViewBag.UserDict = userDict;
            }

            ViewBag.PokemonBaseDict = db_pokemonBase.getIntObjDict();
            ViewBag.NatureDict = db_natures.GetObjDict();
            ViewBag.AttackDict = db_attacks.GetDict();
            ViewBag.AbilityDict = db_abilities.GetDict();

            return View();
        }
        


        public ActionResult LoadDataFromFile(string message = null)
        {
            ViewBag.Message = message;
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoadDataFromFile(FormCollection collection)
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            ViewBag.IsAdmin = isAdmin;
            ViewBag.Message = LoadDataFromFileHelper(collection, isAdmin);
            return View();
        }



        public ActionResult Create(string message = "")
        {
            //don't care if it's admin here, we only care if they're signed in - and that's handled by the Authorize attribute.
            ViewBag.Message = message;
            ViewBag.PokemonBaseDict = db_pokemonBase.GetDict();
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection)
        {
            //get the pokemon base.
            try
            {
                int selectedPokemonBaseId = ValidationHelpers.validateAsInt(collection, "PokemonBaseID", true, true, -1);
                return RedirectToAction("CreateDetail", new { pkmnBaseId = selectedPokemonBaseId });
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Could not identify pokemon species; " + ex.Message;
                ViewBag.PokemonBaseDict = db_pokemonBase.GetDict();
                return View();
            }
        }
        


        public ActionResult CreateDetail(int pkmnBaseId, string message = "")
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            //make sure the ID is good.
            PokemonBase selectedPokemonBase = db_pokemonBase.PokemonBases.Find(pkmnBaseId);
            if (selectedPokemonBase != null)
            {
                //good, we found the PokemonBase. Make sure it has a valid Gender Type.
                Dictionary<int, string> genders;
                if (EnumHelpers.getGenderNameFromId().TryGetValue(selectedPokemonBase.GenderType, out genders))
                {
                    //good, valid gender type. Gather the necessary info and pass it along.
                    ViewBag.Genders = genders;
                    ViewBag.SelectedPokemonBase = selectedPokemonBase;
                    ViewBag.AbilityDict = new Dictionary<AbilitiesEnum, Ability>()
                    {
                        { AbilitiesEnum.First , db_abilities.Abilities.Find(selectedPokemonBase.AbilityID_First) },
                        { AbilitiesEnum.Second , db_abilities.Abilities.Find(selectedPokemonBase.AbilityID_Second) },
                        { AbilitiesEnum.Hidden , db_abilities.Abilities.Find(selectedPokemonBase.AbilityID_Hidden) },
                    };
                    ViewBag.Natures = db_natures.GetDict();
                    ViewBag.Attacks = db_attacks.GetDict(db_learnables.getPkmnBaseAttackList(pkmnBaseId).Select(x => x.AttackId).ToList());
                    ViewBag.Message = message;
                    ViewBag.IsAdmin = isAdmin;
                    if (isAdmin)
                    {
                        Dictionary<string, ApplicationUser> userDict = db_users.Users.Distinct().ToDictionary(i => i.Id, i => i);
                        ViewBag.UserDict = userDict;
                    }
                    else
                    {
                        ViewBag.UserDict = null;
                    }
                    return View("CreateDetail");
                }
                else
                {
                    return RedirectToAction("Index", new { message = "No valid Gender Type found for the species '" + selectedPokemonBase.Name + "'." });
                }
            }
            else
            {
                return RedirectToAction("Index", new { message = "Invalid pokemon base ID '" + pkmnBaseId + "'." });
            }
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDetail(FormCollection collection)
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string theMessage = "";
            try
            {
                PokemonInstance item = new PokemonInstance();
                
                //get the rest of the form's info and put it into the item.
                setItemToCollectionInputs(item, collection, true, isAdmin);

                //make sure the item is valid.
                sanitizeItem(item);

                //we're good. 
                db.PokemonInstances.Add(item);
                db.SaveChanges();
                theMessage = "Successfully created new " + objectName + " '" + item.Name + "'.";
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
            return RedirectToAction("Index", new { message = theMessage });
        }
        


        public ActionResult Edit(int id, string message = "")
        {
            string theMessage = "";
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string userId = User.Identity.GetUserId();
            PokemonInstance currentItem = db.PokemonInstances.Find(id);
            if (currentItem != null)
            {
                if (isAdmin || userId == currentItem.OwnerId)
                {
                    //make sure the Pokemon Base is valid;
                    PokemonBase currentItemBase = db_pokemonBase.PokemonBases.Find(currentItem.PokemonBaseId);
                    if (currentItemBase != null)
                    {
                        Dictionary<int, string> genders;
                        if (EnumHelpers.getGenderNameFromId().TryGetValue(currentItemBase.GenderType, out genders))
                        {
                            ViewBag.CurrentItem = currentItem;
                            ViewBag.Genders = genders;
                            ViewBag.SelectedPokemonBase = currentItemBase;
                            ViewBag.AbilityDict = new Dictionary<AbilitiesEnum, Ability>()
                            {
                                { AbilitiesEnum.First , db_abilities.Abilities.Find(currentItemBase.AbilityID_First) },
                                { AbilitiesEnum.Second , db_abilities.Abilities.Find(currentItemBase.AbilityID_Second) },
                                { AbilitiesEnum.Hidden , db_abilities.Abilities.Find(currentItemBase.AbilityID_Hidden) },
                            };
                            ViewBag.Natures = db_natures.GetDict();
                            ViewBag.Attacks = db_attacks.GetDict(db_learnables.getPkmnBaseAttackList(currentItem.PokemonBaseId).Select(x => x.AttackId).ToList());
                            ViewBag.Message = message;
                            ViewBag.IsAdmin = isAdmin;
                            if (isAdmin)
                            {
                                Dictionary<string, ApplicationUser> userDict = db_users.Users.Distinct().ToDictionary(i => i.Id, i => i);
                                ViewBag.UserDict = userDict;
                            }
                            else
                            {
                                ViewBag.UserDict = null;
                            }
                            return View();
                        }
                        else
                        {
                            theMessage = "No valid Gender Type found for the species '" + currentItemBase.Name + "'.";
                        }
                    }
                    else
                    {
                        theMessage = "Cannot identify Pokemon Species ID " + currentItem.PokemonBaseId;
                    }
                }
                else
                {
                    theMessage = "You do not have permission to edit this Pokemon Instance.";
                }
            }
            else
            {
                theMessage = "Could not find Pokemon Instance with ID " + id;
            }
            return RedirectToAction("Index", new { message = theMessage });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, FormCollection collection)
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string theMessage = "";
            //make sure the pokemon Base ID is good.

            //make sure the pokemon instance is good;
            PokemonInstance item = db.PokemonInstances.Find(id);
            if (item != null)
            {
                //good, we found the PokemonBase.
                try
                {
                    //grab the data from the collection and save.
                    setItemToCollectionInputs(item, collection, false, isAdmin);

                    //check the validity.
                    sanitizeItem(item);

                    //we're good. 
                    db.SaveChanges();
                    theMessage = "Successfully edited " + objectName + " '(" + item.Id + ")" + item.Name + "'.";
                    return RedirectToAction("Details", new { id = id, message = theMessage });
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
                    theMessage = objectName + " Edit failed: \r\n" + ex.Message;
                }
                
            }
            else
            {
                theMessage = "Invalid pokemon instance ID '" + id + "'.";
            }
            return RedirectToAction("Index", new { message = theMessage });
        }

        

        public ActionResult EditSpecies(int id, string message = "")
        {
            string theMessage = "";
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string userId = User.Identity.GetUserId();
            PokemonInstance currentItem = db.PokemonInstances.Find(id);
            if (currentItem != null)
            {
                if (isAdmin || userId == currentItem.OwnerId)
                {
                    ViewBag.CurrentItem = currentItem;
                    Dictionary<int, PokemonBase> pokemonBaseDict = db_pokemonBase.getIntObjDict();
                    ViewBag.PokemonBaseDict = pokemonBaseDict;
                    return View();
                }
                else
                {
                    theMessage = "You do not have permission to edit this Pokemon Instance.";
                }
            }
            else
            {
                theMessage = "Could not find Pokemon Instance with ID " + id;
            }
            return RedirectToAction("Index", new { message = theMessage });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSpecies(int id, FormCollection collection)
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string theMessage = "";
            //make sure the pokemon Base ID is good.

            //make sure the pokemon instance is good;
            PokemonInstance item = db.PokemonInstances.Find(id);
            if (item != null)
            {
                //good, we found the item.
                try
                {
                    //grab the data from the collection and save.
                    int newBaseId = ValidationHelpers.validateAsInt(collection, "PokemonBaseId", true, true, 0);
                    PokemonBase newBase = db_pokemonBase.PokemonBases.Find(newBaseId);
                    //make sure the new base exists
                    if (newBase != null && newBase.Id != item.PokemonBaseId)
                    {
                        //make sure it's not the same as the old base.
                        if (newBase.Id != item.PokemonBaseId)
                        {
                            Dictionary<int, string> validGenders;
                            if (EnumHelpers.getGenderNameFromId().TryGetValue(newBase.GenderType, out validGenders))
                            {
                                //sanitize the gender;
                                if (!validGenders.ContainsKey(item.GenderEnumId))
                                {
                                    item.GenderEnumId = validGenders.First().Key;
                                }
                                List<LearnableAttack> validAttacks = db_learnables.getPkmnBaseAttackList(newBase.Id);
                                if (validAttacks.Where(i => i.AttackId == item.AttackID_1).FirstOrDefault() == null) { item.AttackID_1 = 0; }
                                if (validAttacks.Where(i => i.AttackId == item.AttackID_2).FirstOrDefault() == null) { item.AttackID_2 = 0; }
                                if (validAttacks.Where(i => i.AttackId == item.AttackID_3).FirstOrDefault() == null) { item.AttackID_3 = 0; }
                                if (validAttacks.Where(i => i.AttackId == item.AttackID_4).FirstOrDefault() == null) { item.AttackID_4 = 0; }
                                
                                //and change the species
                                item.PokemonBaseId = newBaseId;

                                //we're good. 
                                db.SaveChanges();
                                theMessage = "Successfully edited species to " + newBase.Name;
                                return RedirectToAction("Details", new { id = id, message = theMessage });
                            }
                            else
                            {
                                throw new Exception("New species does not have a valid Gender Type Id " + newBase.GenderType);
                            }
                        }
                        else
                        {
                            throw new Exception("New species is the same as the old species.");
                        }
                    }
                    else
                    {
                        throw new Exception("Cannot identify Pokemon Species ID " + newBaseId);
                    }
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
                    theMessage = "Species Edit failed: \r\n" + ex.Message;
                }
            }
            else
            {
                theMessage = "Invalid pokemon instance ID '" + id + "'.";
            }
            return RedirectToAction("Index", new { message = theMessage });
        }



        public ActionResult EditForm(int id, string message = "")
        {
            string theMessage = "";
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string userId = User.Identity.GetUserId();
            PokemonInstance currentItem = db.PokemonInstances.Find(id);
            if (currentItem != null)
            {
                if (isAdmin || userId == currentItem.OwnerId)
                {
                    ViewBag.CurrentItem = currentItem;
                    ViewBag.OldBase = db_pokemonBase.PokemonBases.Find(currentItem.PokemonBaseId);
                    List<PokemonBase> invalidNextForms = new List<PokemonBase>();
                    List<PokemonBase> nextForms = db_formChanges.getNextFormsList(currentItem, db_pokemonBase, invalidNextForms);
                    ViewBag.InvalidNextForms = invalidNextForms;
                    ViewBag.NextForms = nextForms;
                    return View();
                }
                else
                {
                    theMessage = "You do not have permission to edit this Pokemon Instance.";
                }
            }
            else
            {
                theMessage = "Could not find Pokemon Instance with ID " + id;
            }
            return RedirectToAction("Index", new { message = theMessage });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditForm(int id, FormCollection collection)
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string theMessage = "";
            //make sure the pokemon Base ID is good.

            //make sure the pokemon instance is good;
            PokemonInstance item = db.PokemonInstances.Find(id);
            if (item != null)
            {
                //good, we found the item.
                try
                {
                    //grab the data from the collection and save.
                    int newBaseId = ValidationHelpers.validateAsInt(collection, "PokemonBaseId", true, true, 0);
                    PokemonBase newBase = db_pokemonBase.PokemonBases.Find(newBaseId);
                    //make sure the new base exists
                    if (newBase != null)
                    {
                        //make sure it's not the same as the old base.
                        if (newBase.Id != item.PokemonBaseId)
                        {
                            //good, now make sure it's a valid form change;
                            if (db_formChanges.FormChanges.Where(i =>
                            (i.PokemonBaseId_Prev == item.PokemonBaseId && i.PokemonBaseId_Next == newBase.Id) ||
                            (i.PokemonBaseId_Next == item.PokemonBaseId && i.PokemonBaseId_Prev == newBase.Id)).FirstOrDefault() != null)
                            {
                                //looks like a valid form change.
                                item.PokemonBaseId = newBaseId;
                                //we're good. 
                                db.SaveChanges();
                                theMessage = "Successfully edited " + objectName + " '(" + item.Id + ")" + item.Name + "' species to " + newBase.Name;
                                return RedirectToAction("Details", new { id = id, message = theMessage });
                            }
                            else
                            {
                                string oldBaseName = "UNKNOWN";
                                PokemonBase oldBase = db_pokemonBase.PokemonBases.Find(item.PokemonBaseId);
                                if (oldBase != null) { oldBaseName = oldBase.Name; }
                                throw new Exception("Unknown Form Change from (" + item.PokemonBaseId + ")" + oldBaseName + " to (" + newBase.Id + ")" + newBase.Name + ".");
                            }
                        }
                        else
                        {
                            throw new Exception("New species is the same as the old species.");
                        }
                    }
                    else
                    {
                        throw new Exception("Cannot identify Pokemon Species ID " + newBaseId);
                    }
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
                    theMessage = "Species Edit failed: \r\n" + ex.Message;
                }
            }
            else
            {
                theMessage = "Invalid pokemon instance ID '" + id + "'.";
            }
            return RedirectToAction("Index", new { message = theMessage });
        }



        public ActionResult Delete(int id, string message = "")
        {
            return helpDetailsOrDelete(id, true, message);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, FormCollection collection)
        {
            PokemonInstance currentItem = db.PokemonInstances.Find(id);
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string ownerId = User.Identity.GetUserId();
            string theMessage = "";
            if (currentItem != null)
            {
                if (isAdmin || ownerId == currentItem.OwnerId)
                {
                    try
                    {
                        db.PokemonInstances.Remove(currentItem);
                        db.SaveChanges();
                        theMessage = "Successfully edited " + objectName + " '(" + currentItem.Id + ")" + currentItem.Name + "'.";
                        return RedirectToAction("Overview", new { message = theMessage });
                    }
                    catch (Exception ex)
                    {
                        theMessage = "Deletion failed: \r\n" + ex.Message;
                    }
                }
                else
                {
                    theMessage = "You do not own this instance.";
                }
            }
            else
            {
                theMessage = "Could not find Pokemon Instance with ID " + id;
            }
            return RedirectToAction("Index", new { message = theMessage });
        }


        
        public ActionResult Details(int id, string message = "")
        {
            return helpDetailsOrDelete(id, false, message);
        }



        public ActionResult Compare( int? instAttId = null, int? instDefId = null, string message = null  )
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string ownerId = User.Identity.GetUserId();
            string theMessage = "";
            Dictionary<string, List<PokemonInstance>> availableItems;
            ViewBag.IsAdmin = isAdmin;
            if (isAdmin)
            {
                availableItems = db.GetUserPokemonDict(true);
                ViewBag.AvailableItems = availableItems;
            }
            else
            {
                availableItems = db.GetUserPokemonDict(false, ownerId);
                ViewBag.AvailableItems = availableItems;
            }
            PokemonInstance instAtt = null;
            PokemonInstance instDef = null;
            if (instAttId != null && instDefId != null )
            {
                instDef = db.PokemonInstances.Find(instDefId);
                instAtt = db.PokemonInstances.Find(instAttId);
            }

            if (instAtt != null && instDef != null)
            {
                if (isAdmin || (instAtt.OwnerId == ownerId && instDef.OwnerId == ownerId))
                {
                    ViewBag.InstAtt = instAtt;
                    ViewBag.InstDef = instDef;
                    if (instAtt != null && instDef != null)
                    {
                        try
                        {
                            Attack att1 = db_attacks.Attacks.Find(instAtt.AttackID_1);
                            Attack att2 = db_attacks.Attacks.Find(instAtt.AttackID_2);
                            Attack att3 = db_attacks.Attacks.Find(instAtt.AttackID_3);
                            Attack att4 = db_attacks.Attacks.Find(instAtt.AttackID_4);
                            List<KeyValuePair<Attack, int>> attDmgList = new List<KeyValuePair<Attack, int>>();

                            attDmgList.Add(new KeyValuePair<Attack, int>(att1, instAtt.getDamage(att1, instDef, db_natures, db_pokemonBase, db_attacks, db_elemTypes, db_elemComps)));
                            attDmgList.Add(new KeyValuePair<Attack, int>(att2, instAtt.getDamage(att2, instDef, db_natures, db_pokemonBase, db_attacks, db_elemTypes, db_elemComps)));
                            attDmgList.Add(new KeyValuePair<Attack, int>(att3, instAtt.getDamage(att3, instDef, db_natures, db_pokemonBase, db_attacks, db_elemTypes, db_elemComps)));
                            attDmgList.Add(new KeyValuePair<Attack, int>(att4, instAtt.getDamage(att4, instDef, db_natures, db_pokemonBase, db_attacks, db_elemTypes, db_elemComps)));
                            ViewBag.AttDmgList = attDmgList;
                        }
                        catch (Exception ex)
                        {
                            theMessage = "Encountered an error trying to parse the data; " + ex.Message;
                        }
                    }
                }
                else
                {
                    theMessage = "You do not own these instances.";
                }
            }
            ViewBag.Message = theMessage;
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Compare(FormCollection collection)
        {
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string ownerId = User.Identity.GetUserId();
            string theMessage = "";
            try
            {
                int attId = ValidationHelpers.validateAsInt( collection, "AttackingPokemonInstanceId", true, true, -1 );
                int defId = ValidationHelpers.validateAsInt(collection, "DefendingPokemonInstanceId", true, true, -1);
                return RedirectToAction("Compare", new { instAttId = attId, instDefId = defId, message = theMessage });
            }
            catch (Exception ex)
            {
                theMessage = "Couldn't parse from form collection; " + ex.Message;
            }
            return RedirectToAction("Compare", new { instAtt = -1, instDef = -1, message = theMessage });
        }



        #region Helper Methods

        private ActionResult helpDetailsOrDelete(int id, bool isDeleting, string message = "")
        {
            PokemonInstance currentItem = db.PokemonInstances.Find(id);
            bool isAdmin = ValidationHelpers.isUserAdmin(this);
            string ownerId = User.Identity.GetUserId();
            string theMessage = "";
            if (currentItem != null)
            {
                if (isAdmin || ownerId == currentItem.OwnerId)
                {
                    PokemonBase pokemonBase = db_pokemonBase.PokemonBases.Find(currentItem.PokemonBaseId);
                    //select the appropriate ability based on the pokemon base. Default to the first ability if the current one is invalid.
                    if (pokemonBase != null)
                    {
                        int abilId;
                        switch (currentItem.AbilityEnumId)
                        {
                            case (int)AbilitiesEnum.First:
                                abilId = pokemonBase.AbilityID_First;
                                break;
                            case (int)AbilitiesEnum.Second:
                                abilId = pokemonBase.AbilityID_Second;
                                break;
                            case (int)AbilitiesEnum.Hidden:
                                abilId = pokemonBase.AbilityID_Hidden;
                                break;
                            default:
                                abilId = -1;
                                break;
                        }
                        Ability ability = db_abilities.Abilities.Find(abilId);
                        if (ability == null && currentItem.AbilityEnumId != (int)AbilitiesEnum.First)
                        {
                            ability = db_abilities.Abilities.Find(pokemonBase.AbilityID_First);
                        }
                        ViewBag.Ability = ability;
                    }
                    else
                    {
                        ViewBag.Ability = null;
                    }
                    ViewBag.CurrentItem = currentItem;
                    ViewBag.Owner = db_users.Users.Find(ownerId);
                    ViewBag.PokemonBase = pokemonBase;
                    ViewBag.Nature = db_natures.Natures.Find(currentItem.NatureId);
                    ViewBag.Att1 = db_attacks.Attacks.Find(currentItem.AttackID_1);
                    ViewBag.Att2 = db_attacks.Attacks.Find(currentItem.AttackID_2);
                    ViewBag.Att3 = db_attacks.Attacks.Find(currentItem.AttackID_3);
                    ViewBag.Att4 = db_attacks.Attacks.Find(currentItem.AttackID_4);
                    ViewBag.Message = message;
                    if (isDeleting)
                    {
                        return View("Delete");
                    }
                    else
                    {
                        return View("Details");
                    }
                }
                else
                {
                    theMessage = "You do not own this instance.";
                }
            }
            else
            {
                theMessage = "Could not find " + objectName + " with ID " + id;
            }
            return RedirectToAction("Index", new { message = theMessage });
        }




        private void sanitizeItem(PokemonInstance item)
        {
            if (!db_natures.isIdValid(item.NatureId)) { throw new Exception("Nature ID '" + item.NatureId + "' is invalid."); }
            PokemonBase species = db_pokemonBase.PokemonBases.Find(item.PokemonBaseId);
            if (species == null) { throw new Exception("Pokemon Species ID '" + item.PokemonBaseId + "' is invalid."); }
            Dictionary<int, string> validGenders;
            if (!EnumHelpers.getGenderNameFromId().TryGetValue(species.GenderType, out validGenders))
            {
                throw new Exception("Pokemon Species '(" + species.Id + ")" + species.Name + "' has invalid Gender Type ID " + species.GenderType);
            }
            else
            {
                if (!validGenders.ContainsKey(item.GenderEnumId))
                {
                    throw new Exception("Pokemon Instance '(" + species.Name + ")" + item.Name + "' has invalid Gender Enum ID " + item.GenderEnumId);
                }
            }
            Attack att1 = db_attacks.Attacks.Find(item.AttackID_1);
            Attack att2 = db_attacks.Attacks.Find(item.AttackID_2);
            Attack att3 = db_attacks.Attacks.Find(item.AttackID_3);
            Attack att4 = db_attacks.Attacks.Find(item.AttackID_4);
            //now check the attacks.
            if (att1 != null && !db_learnables.comboExists(species.Id, att1.Id))
            {
                throw new Exception("(Att1)Pokemon species '(" + species.Id + ")" + species.Name + "' cannot learn move (" + att1.Id + ")" + att1.Name);
            }
            if (att2 != null && !db_learnables.comboExists(species.Id, att2.Id))
            {
                throw new Exception("(Att2)Pokemon species '(" + species.Id + ")" + species.Name + "' cannot learn move (" + att2.Id + ")" + att2.Name);
            }
            if (att3 != null && !db_learnables.comboExists(species.Id, att3.Id))
            {
                throw new Exception("(Att3)Pokemon species '(" + species.Id + ")" + species.Name + "' cannot learn move (" + att3.Id + ")" + att3.Name);
            }
            if (att4 != null && !db_learnables.comboExists(species.Id, att4.Id))
            {
                throw new Exception("(Att4)Pokemon species '(" + species.Id + ")" + species.Name + "' cannot learn move (" + att4.Id + ")" + att4.Name);
            }
        }




        /// <summary>
        /// Sets the item's values to what's in the form collection.
        /// Will throw exceptions if the expected item is not present, or if the user is not an admin and attempts to modify the 
        /// </summary>
        private void setItemToCollectionInputs(PokemonInstance item, FormCollection collection, bool creating, bool isAdmin)
        {
            //are they an admin?
            if (isAdmin)
            {
                //yes, just set it to what's in the collection.
                item.OwnerId = ValidationHelpers.validateAsString(collection, "OwnerId", true, true, "");
            }
            else
            {
                //nope - are we creating?
                if (creating)
                {
                    //yep, just set its ID to the signed in user.
                    item.OwnerId = User.Identity.GetUserId();
                }
                else
                {
                    //we're editing; does the item's OwnerID match the user?
                    if (item.OwnerId != User.Identity.GetUserId())
                    {
                        throw new Exception("You do not have permission to modify this Pokemon Instance.");
                    }
                }
            }

            if (creating)
            {
                item.PokemonBaseId = ValidationHelpers.validateAsInt(collection, "PokemonBaseId", true, true, -1);
            }
            
            item.Name = ValidationHelpers.validateAsString(collection, "Name", true, true, "");
            item.GenderEnumId = ValidationHelpers.validateAsInt(collection, "GenderEnumId", true, true, -1);
            item.AbilityEnumId = ValidationHelpers.validateAsIntEnum<AbilitiesEnum>(collection, "AbilityEnumId", true, true, -1);
            item.NatureId = ValidationHelpers.validateAsInt(collection, "NatureId", true, true, -1);
            item.Level = ValidationHelpers.validateAsInt(collection, "Level", true, true, 1);
            item.XP = ValidationHelpers.validateAsInt(collection, "XP", true, true, 0);

            item.AttackID_1 = ValidationHelpers.validateAsInt(collection, "AttackID_1", true, true, 0);
            item.AttackID_2 = ValidationHelpers.validateAsInt(collection, "AttackID_2", true, true, 0);
            item.AttackID_3 = ValidationHelpers.validateAsInt(collection, "AttackID_3", true, true, 0);
            item.AttackID_4 = ValidationHelpers.validateAsInt(collection, "AttackID_4", true, true, 0);

            item.EvHP = ValidationHelpers.validateAsInt(collection, "EvHP", true, true, 0);
            item.EvAtt = ValidationHelpers.validateAsInt(collection, "EvAtt", true, true, 0);
            item.EvDef = ValidationHelpers.validateAsInt(collection, "EvDef", true, true, 0);
            item.EvSpAtt = ValidationHelpers.validateAsInt(collection, "EvSpAtt", true, true, 0);
            item.EvSpDef = ValidationHelpers.validateAsInt(collection, "EvSpDef", true, true, 0);
            item.EvSpeed = ValidationHelpers.validateAsInt(collection, "EvSpeed", true, true, 0);

            item.IvHP = ValidationHelpers.validateAsInt(collection, "IvHP", true, true, 0);
            item.IvAtt = ValidationHelpers.validateAsInt(collection, "IvAtt", true, true, 0);
            item.IvDef = ValidationHelpers.validateAsInt(collection, "IvDef", true, true, 0);
            item.IvSpAtt = ValidationHelpers.validateAsInt(collection, "IvSpAtt", true, true, 0);
            item.IvSpDef = ValidationHelpers.validateAsInt(collection, "IvSpDef", true, true, 0);
            item.IvSpeed = ValidationHelpers.validateAsInt(collection, "IvSpeed", true, true, 0);
        }
        



        #endregion

        #region Load Data From File Helpers

        /// <summary>
        /// Main method; controls the flow, defines what fields should be there, etc.
        /// </summary>
        private string LoadDataFromFileHelper(FormCollection collection, bool isAdmin)
        {
            string retMessage = "";
            Dictionary<string, bool> expectedFields = new Dictionary<string, bool>()
            {
                {"Name", true },
                {"PokemonBaseId", false },
                {"Gender", false },
                {"AbilityEnumId", false },
                {"NatureId", false },
                {"Level", false },
                {"XP", false },

                {"IvHP", false },
                {"IvAtt", false },
                {"IvDef", false },
                {"IvSpAtt", false },
                {"IvSpDef", false },
                {"IvSpeed", false },

                {"EvHP", false },
                {"EvAtt", false },
                {"EvDef", false },
                {"EvSpAtt", false },
                {"EvSpDef", false },
                {"EvSpeed", false },
            };
            if (isAdmin)
            {
                expectedFields.Add("OwnerId", true);
            }

            //get the raw string;
            String rawData = collection["RawData"];
            if (!String.IsNullOrWhiteSpace(rawData))
            {
                try
                {
                    Dictionary<string, int> pokemonBaseLookupDict = db_pokemonBase.GetLookupDict();
                    Dictionary<string, int> genderLookupDict = EnumHelpers.getAllGenderNamesDict();
                    Dictionary<string, int> natureLookupDict = db_natures.GetLookupDict();
                    Dictionary<string, int> attackLookupDict = db_attacks.GetLookupDict();

                    List<string> presentFields;

                    List<List<String>> cells = LoadDataFromFile_BreakUpString(rawData);
                    List<Dictionary<string, string>> fieldValDicts = LoadDataFromFile_FieldValDicts(cells, expectedFields, out presentFields);
                    List<PokemonInstance> loadedObjects = LoadDataFromFile_ParseToObjects(fieldValDicts, pokemonBaseLookupDict, genderLookupDict, natureLookupDict, attackLookupDict, isAdmin);

                    string OwnerId = null;
                    if (!isAdmin)
                    {
                        OwnerId = User.Identity.GetUserId();
                        if (String.IsNullOrWhiteSpace(OwnerId))
                        {
                            throw new Exception("You must sign in to use this feature.");
                        }
                    }
                    LoadDataFromFile_SaveToDatabase(loadedObjects, presentFields, isAdmin);
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
        private List<PokemonInstance> LoadDataFromFile_ParseToObjects(List<Dictionary<string, string>> rawData, Dictionary<string, int> pokemonBaseLookupDict, Dictionary<string, int> genderLookupDict, Dictionary<string, int> natureLookupDict, Dictionary<string, int> attackLookupDict, bool isAdmin)
        {
            List<PokemonInstance> retList = new List<PokemonInstance>();
            string userId = User.Identity.GetUserId();
            for (int i = 0; i < rawData.Count; i++)
            {
                PokemonInstance item = new PokemonInstance();
                foreach (KeyValuePair<string, string> kvp in rawData[i])
                {
                    if (!isAdmin)
                    {
                        item.OwnerId = userId;
                    }
                    switch (kvp.Key.Trim())
                    {
                        case "Name":
                            item.Name = ValidationHelpers.validateAsString(kvp.Value, kvp.Key, true, "");
                            break;
                        case "OwnerId":
                            item.Name = ValidationHelpers.validateAsString(kvp.Value, kvp.Key, true, "");
                            break;
                        case "PokemonBaseId":
                            item.PokemonBaseId = ValidationHelpers.validateAsStringDict(pokemonBaseLookupDict, kvp.Value, kvp.Key, true, -1);
                            break;
                        case "Gender":
                            item.GenderEnumId = ValidationHelpers.validateAsStringDict(genderLookupDict, kvp.Value, kvp.Key, false, -1);
                            break;
                        case "AbilityEnumId":
                            item.AbilityEnumId = ValidationHelpers.validateAsStringEnum<AbilitiesEnum>(kvp.Value, kvp.Key, false, (int)AbilitiesEnum.First);
                            break;
                        case "NatureId":
                            item.NatureId = ValidationHelpers.validateAsStringDict(natureLookupDict, kvp.Value, kvp.Key, true, -1);
                            break;
                        case "Level":
                            item.Level = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, false, 1);
                            break;
                        case "XP":
                            item.XP = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, false, 0);
                            break;
                        case "AttackId_1":
                            item.AttackID_1 = ValidationHelpers.validateAsStringDict(attackLookupDict, kvp.Value, kvp.Key, false, -1);
                            break;
                        case "AttackId_2":
                            item.AttackID_2 = ValidationHelpers.validateAsStringDict(attackLookupDict, kvp.Value, kvp.Key, false, -1);
                            break;
                        case "AttackId_3":
                            item.AttackID_3 = ValidationHelpers.validateAsStringDict(attackLookupDict, kvp.Value, kvp.Key, false, -1);
                            break;
                        case "AttackId_4":
                            item.AttackID_4 = ValidationHelpers.validateAsStringDict(attackLookupDict, kvp.Value, kvp.Key, false, -1);
                            break;
                        case "IvHP":
                            item.IvHP = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "IvAtt":
                            item.IvAtt = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "IvDef":
                            item.IvDef = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "IvSpAtt":
                            item.IvSpAtt = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "IvSpDef":
                            item.IvSpDef = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "IvSpeed":
                            item.IvSpeed = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "EvHP":
                            item.EvHP = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "EvAtt":
                            item.EvAtt = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "EvDef":
                            item.EvDef = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "EvSpAtt":
                            item.EvSpAtt = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "EvSpDef":
                            item.EvSpDef = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
                            break;
                        case "EvSpeed":
                            item.EvSpeed = ValidationHelpers.validateAsInt(kvp.Value, kvp.Key, true, 1);
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
        private void LoadDataFromFile_SaveToDatabase(List<PokemonInstance> rawData, List<string> presentFields, bool isAdmin)
        {
            try
            {
                foreach (PokemonInstance item in rawData)
                {
                    //get the current user ID;
                    string userId = User.Identity.GetUserId();

                    //check to see if the item already exists;
                    PokemonInstance currentItem = db.PokemonInstances.Where(i => i.OwnerId == item.OwnerId && i.Name == item.Name).FirstOrDefault();
                    if (!isAdmin)
                    {
                        currentItem.OwnerId = item.OwnerId;
                    }
                    if (currentItem != null)
                    {
                        //edit according to what is there.
                        if (presentFields.Contains("Name")) { currentItem.Name = item.Name; }
                        if (presentFields.Contains("OwnerId")) { currentItem.OwnerId = item.OwnerId; }
                        if (presentFields.Contains("PokemonBaseId")) { currentItem.PokemonBaseId = item.PokemonBaseId; }
                        if (presentFields.Contains("Gender")) { currentItem.GenderEnumId = item.GenderEnumId; }
                        if (presentFields.Contains("AbilityEnumId")) { currentItem.AbilityEnumId = item.AbilityEnumId; }
                        if (presentFields.Contains("NatureId")) { currentItem.NatureId = item.NatureId; }
                        if (presentFields.Contains("Level")) { currentItem.Level = item.Level; }
                        if (presentFields.Contains("XP")) { currentItem.XP = item.XP; }

                        if (presentFields.Contains("AttackId_1")) { currentItem.AttackID_1 = item.AttackID_1; }
                        if (presentFields.Contains("AttackId_2")) { currentItem.AttackID_2 = item.AttackID_2; }
                        if (presentFields.Contains("AttackId_3")) { currentItem.AttackID_3 = item.AttackID_3; }
                        if (presentFields.Contains("AttackId_4")) { currentItem.AttackID_4 = item.AttackID_4; }

                        if (presentFields.Contains("IvHP")) { currentItem.IvHP = item.IvHP; }
                        if (presentFields.Contains("IvAtt")) { currentItem.IvAtt = item.IvAtt; }
                        if (presentFields.Contains("IvDef")) { currentItem.IvDef = item.IvDef; }
                        if (presentFields.Contains("IvSpAtt")) { currentItem.IvSpAtt = item.IvSpAtt; }
                        if (presentFields.Contains("IvSpDef")) { currentItem.IvSpDef = item.IvSpDef; }
                        if (presentFields.Contains("IvSpeed")) { currentItem.IvSpeed = item.IvSpeed; }

                        if (presentFields.Contains("EvHP")) { currentItem.EvHP = item.EvHP; }
                        if (presentFields.Contains("EvAtt")) { currentItem.EvAtt = item.EvAtt; }
                        if (presentFields.Contains("EvDef")) { currentItem.EvDef = item.EvDef; }
                        if (presentFields.Contains("EvSpAtt")) { currentItem.EvSpAtt = item.EvSpAtt; }
                        if (presentFields.Contains("EvSpDef")) { currentItem.EvSpDef = item.EvSpDef; }
                        if (presentFields.Contains("EvSpeed")) { currentItem.EvSpeed = item.EvSpeed; }
                    }
                    else
                    {
                        //new item; just add it.
                        db.PokemonInstances.Add(item);
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
