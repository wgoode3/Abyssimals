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
    [Route("shops")]
    public class ShopsController : Controller
    {
        private SpiritContext context;

        public ShopsController(SpiritContext c){
            context = c;
        }

        [HttpGet]
        [Route("")]
        [Route("all")]
        public IActionResult AllShops(){
            ViewBag.User = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            ViewBag.AllShops = context.Shops.Include(shop => shop.User).
                                Where(shop => shop.UserId != HttpContext.Session.GetInt32("UserId")).ToList();
            return View("AllShops");
        }

        [HttpGet]
        [Route("me")]
        public IActionResult MyShop(){
            ViewBag.User = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            //if i have a shop, go to edit shop, otherwise go to create a shop
            ViewBag.MyShop = context.Shops.Include(shop => shop.Products).ThenInclude(listed => listed.Product).
            SingleOrDefault(shop => shop.UserId == HttpContext.Session.GetInt32("UserId"));
            ViewBag.NoMoney = TempData["NoMoney"];
            if(ViewBag.MyShop == null){
                return View("CreateMyShop");
            }
            return View("MyShop");
        }

        [HttpPost]
        [Route("create")]
        public IActionResult CreateShop(Shop shop){
            ViewBag.User = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            if(ModelState.IsValid){
                User CurUser = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
                if(CurUser.SubtractMoney(75)){
                    shop.UserId = CurUser.UserId;
                    context.Add(shop);
                    context.SaveChanges(); 
                }
                else{
                    TempData["NoMoney"] = "I said shops were easy to set up, not free! Come back once you have 75 SG!";
                }
                return RedirectToAction("MyShop");
            }
            else{
                return View("MyShop");
            }
        }

        [HttpGet]
        [Route("{username}")]
        public IActionResult OtherShop(string username){
            //if my id, redirect to shops/me
            ViewBag.User = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            ViewBag.OtherUser = context.GetOneUser(username);
            int OtherUserId = ViewBag.OtherUser.UserId;
            if(ViewBag.OtherUser == null){
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Shop = context.Shops.Include(shop => shop.Products).ThenInclude(listed => listed.Product).
            SingleOrDefault(shop => shop.UserId == OtherUserId);
            if(ViewBag.Shop == null || ViewBag.Shop.UserId == ViewBag.User.UserId){
                return RedirectToAction("MyShop");
            }

            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];
            return View();
        } 

        [HttpPost]
        [Route("{user}/{itemid}")]
        public IActionResult PurchaseItem(string user, int itemid, int amount){
            Console.WriteLine("In PurchaseItem");
            ViewBag.User = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            ListedProduct Item = context.GetOneListedProduct(itemid);
            if(Item == null){
                return RedirectToAction("OtherShop", new{ username = user});
            }

            if(context.PurchaseItem(ViewBag.User, Item, amount)){
                TempData["Success"] = "Thank you for your purchase!";
            }
            else{
                TempData["Error"] = "Something went wrong with your purchase...";
            }
            
            return RedirectToAction("OtherShop", new{ username = user});
        }

        [HttpPost]
        [Route("update")]
        public IActionResult UpdateStock(IDictionary<int, ListedProduct> UpdateProds){
            ViewBag.User = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(ViewBag.User == null){
                return RedirectToAction("Index", "Home");
            }
            foreach(KeyValuePair<int, ListedProduct> Prod in UpdateProds){
                ListedProduct ExistingProduct = context.GetOneListedProduct(Prod.Key);
                int AmountDifference = ExistingProduct.Stock - Prod.Value.Stock;
                if(AmountDifference > 0){
                    Console.WriteLine("Adding " + AmountDifference + " back to inventory!");
                    Inventory AddBack = new Inventory();
                    AddBack.ProductId = ExistingProduct.ProductId;
                    AddBack.UserId = ViewBag.User.UserId;
                    AddBack.Amount = AmountDifference;
                    context.AddToInventory(AddBack);
                }
                if(ExistingProduct.Stock <= 0 || Prod.Value.Stock <= 0){
                    Console.WriteLine("About to delete this product!");
                    context.Remove(ExistingProduct);
                    continue;
                }
                ExistingProduct.Price = Prod.Value.Price;
                ExistingProduct.Stock = Prod.Value.Stock;
            }
            context.SaveChanges();
            return RedirectToAction("MyShop");
        }

        

        [HttpGet]
        [Route("lose")]
        public IActionResult LoseMoney(){
            User CurUser = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(CurUser != null){
                CurUser.SubtractMoney(50);
                context.SaveChanges();  
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("add")]
        public IActionResult AddMoney(){
            User CurUser = context.GetOneUser(HttpContext.Session.GetInt32("UserId"));
            if(CurUser != null){
                CurUser.Balance += 25;
                context.SaveChanges();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}