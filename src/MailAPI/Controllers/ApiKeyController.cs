using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MailAPI.Models;

namespace MailAPI.Controllers
{
    public class ApiKeyController : Controller
    {
        #region Fields
        private ApiContext _db;
        #endregion Fields

        public ApiKeyController(ApiContext context)
        {
            _db = context;
        }

        private string getHash(string key)
        {
            SHA256 sha256 = SHA256.Create();
            var SHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            return BitConverter.ToString(SHash).Replace("-", "").ToLower();
        }
      
        
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }
        
        //GET: Api/Create - Display form to create Key
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //POST: Api/Create
        [HttpPost]
        public async Task<IActionResult> Create(ApiKey newKey)
        {
            newKey.ActiveStatus = true;
            newKey.TimeStamp = DateTime.Now;
            newKey.Uses = 0;
            string tempKey = newKey.Key;

            if (ModelState.IsValid)
            {
                //SALT THAT KEY!
                newKey.Key = getHash(tempKey + newKey.Salt);

                try
                {
                    _db.ApiKeys.Add(newKey);
                    await _db.SaveChangesAsync();
                    TempData["Result"]= "Success! Key created!";
                    TempData["UserKey"] = tempKey;
                }
                catch (Exception ex)
                {
                    // Move to central error handling ex.Message;
                    TempData["Result"] = "Key could not be created! " + ex.InnerException;
                }
            }

            return RedirectToAction("List");
        }

        [HttpGet]
        //Gets the list of issued keys
        public async Task<IActionResult> List()
        {

            List<ApiKey> Keys = await _db.ApiKeys.ToListAsync();

            //Check for blanks
            if(Keys.Count == 0)
            {
                Keys.Add(new ApiKey
                {
                    ApiKeyId = 0,
                    Key = "12345678-1234-1234-1234-123456789abc",
                    ActiveStatus = false,
                    AssociatedApplication = "No associated applications yet!",
                    TimeStamp = DateTime.Now,
                    Uses = 0
                });
            };
            ViewBag.Message = TempData["Result"] ?? "No action";
            ViewBag.UserKey = TempData["UserKey"] ?? "Not a new key";

            return View(Keys);
        }

        [HttpPost]
        public async Task<IActionResult> Revoke(int ApiKeyId)
        {

            ApiKey key = await _db.ApiKeys.Where(x => x.ApiKeyId == ApiKeyId).FirstOrDefaultAsync();
            try
            {
                key.ActiveStatus = false;
                _db.Entry(key).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Result"] = ex.InnerException;
            }


            return RedirectToAction("List");
        }

        //Internal Validation Check
        public async Task<bool> Validate(string associatedApplication, string key)
        {
            //Assume failure
            bool result = false;

            try
            {
                ApiKey checkedKey = await _db.ApiKeys.Where(x => x.AssociatedApplication.Equals(associatedApplication) && x.ActiveStatus == true).FirstOrDefaultAsync();

                //Get the comparison value to check against
                string submittedComparisonHash = getHash(key + checkedKey.Salt);

                //Does the password match? Do we need to activate lockout?
                result = checkedKey.Key.Equals(submittedComparisonHash) ? true : false;

                //Increment counter
                if (result == true)
                {
                    checkedKey.Uses++;

                    _db.Entry(checkedKey).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                }

                //Before we were checking to see if the passwords matched, but what calls this is asking "Activate Lockout"? So if the passwords match, it's true here, but we return false so lockout isn't activated.
                return !result;
            }
            catch
            {
                return result;
            }
           

        }

    }
}
