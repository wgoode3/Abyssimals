using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using SpiritMarket.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;

namespace SpiritMarket.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private SpiritContext context;
        private IHostingEnvironment _env;

        public AdminController(SpiritContext c, IHostingEnvironment env){
            context = c;
            _env = env;
        }

        [HttpGet]
        [Route("")]
        public IActionResult AdminHome(){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Success = TempData["AdminMessage"];
            return View("Home");
        }

        

        /*
        Item CRUD
         */

        [HttpGet]
        [Route("item/new")]
        public IActionResult NewItem(){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [Route("item/new")]
        public IActionResult CreateItem(Item p){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }

            if(ModelState.IsValid){
                if(context.GetOneItem(p.Name) != null){
                    ViewBag.NameError = "An item with that name already exists!";
                    return View("NewItem");
                }
                var webRoot = _env.WebRootPath;
                var file = System.IO.Path.Combine(webRoot + "\\images", p.Image);
                Console.WriteLine("File path: " + file);
                if(System.IO.File.Exists(file)){
                    //Console.WriteLine("File exists!");
                    p.IsTradeable = p.IsTradeable ?? false;
                    //p.Image = file;
                    context.Add(p);
                    context.SaveChanges();
                    TempData["AdminMessage"] = $"{p.Name} successfully added to the database!";
                    return RedirectToAction("AdminHome");
                }
                else{
                    Console.WriteLine("File didn't exist");
                    ViewBag.ImageError = "The file couldn't be found. Please check the spelling and file extension!";
                    return View("NewItem");
                }
            }
            return View("NewItem");
        }

        [HttpGet]
        [Route("item/edit")]
        public IActionResult ItemList(){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            ViewBag.AdminMessage = TempData["AdminMessage"];
            ViewBag.AllItems = context.Items.ToList();
            return View();
        }

        [HttpGet]
        [Route("item/edit/{pid}")]
        public IActionResult EditItem(int pid){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Item = context.GetOneItem(pid);
            if(ViewBag.Item == null){
                TempData["AdminMessage"] = $"Item with the requested id {pid} not found!";
                return RedirectToAction("AdminHome");
            }
            return View();
        }

        [HttpPost]
        [Route("item/edit/{pid}")]
        public IActionResult EditItem(Item p, int pid){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            Item original = context.GetOneItem(pid);
            if(original == null){
                TempData["AdminMessage"] = $"Item with the requested id {pid} not found!";
                return RedirectToAction("AdminHome");
            }

            ViewBag.Item = original;
            if(ModelState.IsValid){
                Item existing = context.GetOneItem(p.Name);
                if(existing != null && existing.ItemId != pid){
                    ViewBag.NameError = "An item with that name already exists!";
                    return View("EditItem");
                }
                var webRoot = _env.WebRootPath;
                var file = System.IO.Path.Combine(webRoot + "\\images", p.Image);
                Console.WriteLine("File path: " + file);
                if(System.IO.File.Exists(file)){
                    Console.WriteLine("File exists!");
                    p.IsTradeable = p.IsTradeable ?? false;
                    Console.WriteLine("P Tradeable is " + p.IsTradeable);
                    //p.Image = file;
                    original.Name = p.Name;
                    original.Description = p.Description;
                    original.Image = p.Image;
                    original.IsTradeable = p.IsTradeable;
                    context.SaveChanges();
                    TempData["AdminMessage"] = $"Item #{pid} successfully edited!";
                    return RedirectToAction("ItemList");
                }
                else{
                    Console.WriteLine("File didn't exist");
                    ViewBag.ImageError = "The file couldn't be found. Please check the spelling and file extension!";
                    return View("EditItem");
                }
            }
            Console.WriteLine("IsTradeable is " + p.IsTradeable);
            return View("EditItem");
        }


        /*
        Main Item Type CRUD
         */

        [HttpGet]
        [Route("mainitemtype/edit")]
        public IActionResult AllMainItemTypes(){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            ViewBag.AdminMessage = TempData["AdminMessage"];
            ViewBag.AllMainItemTypes = context.MainItemTypes.ToList();
            return View();
        }

        [HttpGet]
        [Route("mainitemtype/new")]
        public IActionResult NewMainItemType(){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [Route("mainitemtype/new")]
        public IActionResult CreateMainItemType(MainItemType NewType){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            if(ModelState.IsValid){
                MainItemType existing = context.GetOneMainItemType(NewType.Name);
                if(existing != null && existing.MainItemTypeId != NewType.MainItemTypeId){
                    ViewBag.NameError = "A Main Item Type with that name already exists!";
                    return View("NewMainItemType");
                }
                context.Add(NewType);
                context.SaveChanges();
                TempData["AdminMessage"] = $"{NewType.Name} successfully added to the database!";
                return RedirectToAction("AllMainItemTypes");
            }
            return View("NewMainItemType");
        }

        [HttpGet]
        [Route("mainitemtype/edit/{mid}")]
        public IActionResult EditMainItemType(int mid){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            ViewBag.MainItemType = context.GetOneMainItemType(mid);
            if(ViewBag.MainItemType == null){
                TempData["AdminMessage"] = $"Main Item Type with the requested id {mid} not found!";
                return RedirectToAction("AllMainItemTypes");
            }
            return View();
        }

        [HttpPost]
        [Route("mainitemtype/edit/{mid}")]
        public IActionResult EditMainItemType(MainItemType ItemType, int mid){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            MainItemType original = context.GetOneMainItemType(mid);
            if(original == null){
                TempData["AdminMessage"] = $"Main Item Type with the requested id {mid} not found!";
                return RedirectToAction("AllMainItemTypes");
            }

            ViewBag.MainItemType = original;
            if(ModelState.IsValid){
                MainItemType existing = context.GetOneMainItemType(ItemType.Name);
                if(existing != null && existing.MainItemTypeId != mid){
                    ViewBag.NameError = "A Main Item Type with that name already exists!";
                    return View("EditMainItemType");
                }
                original.Name = ItemType.Name;
                original.Description = ItemType.Description;
                context.SaveChanges();
                TempData["AdminMessage"] = $"Main Item Type #{mid} successfully edited!";
                return RedirectToAction("AllMainItemTypes");
            }
            return View();
        }

        [HttpGet]
        [Route("mainitemtype/delete/{mid}")]
        public IActionResult DeleteMainItemType(int mid){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            context.DeleteMainItemType(mid);
            context.SaveChanges();
            TempData["AdminMessage"] = $"Main Item Type #{mid} successfully deleted! I hope you knew what you were doing!";
            return RedirectToAction("AllMainItemTypes");
        }


        /*
        Admin Checking and Editing
         */

        [HttpGet]
        [Route("new")]
        public IActionResult NewAdmin(){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Message = TempData["AdminMessage"];
            return View();
        }

        [HttpPost]
        [Route("giveadmin")]
        public IActionResult GiveAdmin(string username){
            ViewBag.User = HasAccess();
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            User user = context.GetOneUser(username);
            if(user != null){
                if(user.IsAdmin){
                    TempData["AdminMessage"] = $"{username} is already an admin!";
                }
                else{
                    user.IsAdmin = true;
                    context.SaveChanges();
                    TempData["AdminMessage"] = $"{username} is now an admin!";
                }
            }
            else{
                TempData["AdminMessage"] = $"No user with the username \"{username}\" found!";
            }
            return RedirectToAction("NewAdmin");
        }

        public User HasAccess(){
            if(HttpContext.Session.GetInt32("UserId") == null){
                return null;
            }
            User u = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(u == null || !u.IsAdmin){
                return null;
            }
            return u;
        }
    }
}
