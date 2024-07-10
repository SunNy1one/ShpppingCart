using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using System.Diagnostics;

namespace ShoppingCart.Controllers
{
    public class SoftwareController : Controller
    {
        private readonly DatabaseContext db;
        public SoftwareController(DatabaseContext db)
        {
            this.db = db;
        }
        public IActionResult Index(User user)
        {
            ViewData["username"] = user.username;
            foreach(var k in HttpContext.Session.Keys)
            {
                var v = HttpContext.Session.GetString(k);
            }
            return View(db.GetAllSoftware());
        }

        public IActionResult Reviews()
        {
            return View();
        }

        public IActionResult ViewCart_2()
        {
            return View(new List<ShoppingcartModel>() { new ShoppingcartModel("1", (decimal)1.0, (decimal)1.0, (decimal)1.0, "/images/numerics.jpg", "item") });
        }

        [HttpPost]
        public IActionResult Search(string searchString)
        {
            var result = db.Search(searchString);
            ViewData["search-result"] = result;
            return View("Index", result);
        }

        

        [HttpGet]
        public IActionResult AddToCart([FromQuery] string softwareId)
        {
            int cartCount = HttpContext.Session.GetInt32("CartCount") ?? 0;
            HttpContext.Session.SetInt32("CartCount", cartCount + 1);
            HttpContext.Session.SetString($"soft{cartCount + 1}", softwareId);
            
            int count = 1;
            foreach(var k in HttpContext.Session.Keys.Where((k)=>k.StartsWith("soft")))
            {
                count++;
            }
            return View("Index", db.GetAllSoftware());
        }

        public IActionResult EditQuantity(string todo, string softwareId)
        {
            List<string> softKeys = HttpContext.Session.Keys.Where((k) => k.StartsWith("soft")).ToList();
            int softwareKey = 0;
            for(int i = 0; i < softKeys.Count; i++)
            {
                if (HttpContext.Session.GetString(softKeys[i]) == softwareId)
                {
                    softwareKey = i + 1;
                    break;
                }
            }
            int totalSoftware = GetCartCount();
            if (todo == "decrease")
            {
                HttpContext.Session.Remove($"soft{softwareKey}");
                HttpContext.Session?.SetInt32("CartCount", totalSoftware - 1);

            } else if(todo == "increase")
            {
                HttpContext.Session.SetString($"soft{totalSoftware + 1}",softwareId);
                HttpContext.Session?.SetInt32("CartCount", totalSoftware + 1);
            }
            return View();
        }

        private int GetCartCount()
        {
            return HttpContext.Session.GetInt32("CartCount") ?? 0;
        }

        [HttpGet]
        public IActionResult ViewCart(string? softwareToPurchase)
        {
            softwareToPurchase ??= "";
            
            List<string> softwareStrings = softwareToPurchase.Split(",").Where((s) => !string.IsNullOrEmpty(s)).ToList();
            PurchaseCart pc = BuildPurchaseCart(softwareStrings);
            
            return View("ViewCart", pc);
        }

        private PurchaseCart BuildPurchaseCart(List<string> softwareStrings)
        {
            PurchaseCart pc;
            string? username = HttpContext.Session.GetString("username");
            List<Software> distinctSoftwares = db.GetSoftwares(softwareStrings);
            if (username == null)
            {
                pc = new PurchaseCart();
            }
            else
            {
                pc = new PurchaseCart(username);
            }
            foreach (string sws in softwareStrings)
            {
                Software? sw = distinctSoftwares.FirstOrDefault((s) => s.softwareId == sws);
                if (sw != null)
                {
                    pc.softwareList.Add(sw);
                }
            }
            return pc;
        }

        private PurchaseCart BuildPurchaseCart()
        {
            IEnumerable<string> sessionKeys = HttpContext.Session.Keys.Where((k) => k.StartsWith("soft"));
            List<string> softwareStrings = new List<string>();
            foreach(string k in sessionKeys)
            {
                softwareStrings.Add(HttpContext.Session.GetString(k)!);
            }
            return BuildPurchaseCart(softwareStrings);
        }

        public IActionResult Checkout()
        {
            // Check log in status
            ISession session = HttpContext.Session;
            string? username = session.GetString("username");
            if (username == null)
            {
                HttpContext.Session.SetString("checking-out", "true");
                return RedirectToAction("Index", "Login");
            } else
            {
                PurchaseCart pc = BuildPurchaseCart();
                if (db.Checkout(pc))
                {
                    ClearCart();
                    return View("PastPurchase", db.GetPastPurchase2(username).OrderByDescending(p => p.lastdateOfPurchase));
                } else
                {
                    return View("ViewCart", pc);
                }
            }
        }

        public IActionResult PastPurchase()
        {
            ISession session = HttpContext.Session;
            string? username = session.GetString("username");
            if (username != null)
            {
                var purchases = db.GetPastPurchase2(username).OrderByDescending(p => p.lastdateOfPurchase);

                return View(purchases);
            }
            return View();
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove("checking-out");
            foreach(var k in HttpContext.Session.Keys.Where((k) => k.StartsWith("soft")))
            {
                HttpContext.Session.Remove(k);
            }
        }
    }
}
