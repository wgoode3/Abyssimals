using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SpiritMarket.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SpiritMarket.Controllers
{
    [Route("inventory")]
    public class InventoryController : Controller
    {
        private SpiritContext context;

        public InventoryController(SpiritContext c){
            context = c;
        }

        [HttpGet]
        [Route("")]
        public IActionResult DisplayInventory(){
            ViewBag.User = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Inventory = context.Users.Include(user => user.Items).ThenInclude(i => i.Product).
                                SingleOrDefault(user => user.UserId == HttpContext.Session.GetInt32("UserId")).Items;
            ViewBag.Error = TempData["Error"];
            ViewBag.Success = TempData["Success"];
            return View();
        }

        [HttpPost]
        [Route("add")]
        public IActionResult AddItemToShop(int InventoryId, int Amount, decimal Price){
            Inventory Item = context.Inventories.Include(inventory => inventory.Product).SingleOrDefault(inventory => inventory.InventoryId == InventoryId);
            if(Item == null){
                TempData["Error"] = "It appears as though that item isn't in your inventory! Did you already do something with it?";
                return RedirectToAction("DisplayInventory");
            }
            Shop UserShop = context.Shops.Include(shop => shop.Products).
                            SingleOrDefault(Shop=> Shop.UserId == HttpContext.Session.GetInt32("UserId"));
            if(UserShop == null){
                TempData["Error"] = "Hey! It looks like you don't have a shop! Why don't you go make one now?";
                return RedirectToAction("DisplayInventory");
            }
            if(context.AddToShop(Item, UserShop, Amount, Price)){
                TempData["Success"] = Item.Product.Name + " x" + Amount + " was added to your shop!";
            }
            else{
                TempData["Error"] = "Something went wrong when trying to add " + Item.Product.Name + " to your shop.";
            }
            return RedirectToAction("DisplayInventory");
        }

        [HttpGet]
        [Route("get/starterkit")]
        public IActionResult GetStarterKit(){
            ViewBag.User = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            User current = ViewBag.User;
            Inventory TestLeaf = new Inventory();
            TestLeaf.Amount = 3;
            TestLeaf.ProductId = 1;
            TestLeaf.UserId = current.UserId;

            Inventory TestEssence = new Inventory();
            TestEssence.Amount = 2;
            TestEssence.ProductId = 7;
            TestEssence.UserId = current.UserId;

            Inventory TestPetal = new Inventory();
            TestPetal.Amount = 5;
            TestPetal.ProductId = 2;
            TestPetal.UserId = current.UserId;
            context.AddToInventory(TestLeaf);
            context.AddToInventory(TestEssence);
            context.AddToInventory(TestPetal);
            return RedirectToAction("DisplayInventory");
        }
    }
}